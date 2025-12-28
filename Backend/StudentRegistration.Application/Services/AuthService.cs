using AutoMapper;
using StudentRegistration.Application.DTOs.Auth;
using StudentRegistration.Application.Interfaces;
using StudentRegistration.Domain.Common;
using StudentRegistration.Domain.Entities;
using StudentRegistration.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace StudentRegistration.Application.Services
{
    public interface IJwtService
    {
        string GenerateToken(Usuario usuario);
    }

    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;

        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _mapper = mapper;
        }

        public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var usuario = await _unitOfWork.Usuarios.GetByUsernameAsync(request.Username);
            
            if (usuario == null || !VerifyPassword(request.Password, usuario.PasswordHash))
            {
                return Result<LoginResponseDto>.Failure("Usuario o contraseña incorrectos");
            }

            var token = _jwtService.GenerateToken(usuario);
            var expiration = DateTime.UtcNow.AddHours(24);

            var response = new LoginResponseDto
            {
                Token = token,
                Username = usuario.Username,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Expiration = expiration
            };

            return Result<LoginResponseDto>.Success(response, "Login exitoso");
        }

        public async Task<Result<LoginResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            if (await _unitOfWork.Usuarios.UsernameExistsAsync(request.Username))
            {
                return Result<LoginResponseDto>.Failure("El nombre de usuario ya existe");
            }

            // Fix Problema #3: Usar transacción para crear Usuario y Estudiante atómicamente
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Crear el estudiante primero
                var estudiante = new Estudiante
                {
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    Email = request.Email,
                    Telefono = string.Empty, // Campo opcional, se inicializa vacío
                    Direccion = string.Empty, // Campo opcional, se inicializa vacío
                    Activo = true
                };

                await _unitOfWork.Estudiantes.AddAsync(estudiante);
                await _unitOfWork.SaveChangesAsync();

                // Crear el usuario asociado al estudiante
                var usuario = new Usuario
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password),
                    Rol = "Estudiante",
                    EstudiantId = estudiante.EstudiantId // Asociar con el estudiante creado
                };

                await _unitOfWork.Usuarios.AddAsync(usuario);
                await _unitOfWork.SaveChangesAsync();

                // Commit de la transacción
                await _unitOfWork.CommitAsync();

                // Generar token (ahora incluirá el claim studentId)
                var token = _jwtService.GenerateToken(usuario);
                var response = new LoginResponseDto
                {
                    Token = token,
                    Username = usuario.Username,
                    Email = usuario.Email,
                    Rol = usuario.Rol,
                    Expiration = DateTime.UtcNow.AddHours(24)
                };

                return Result<LoginResponseDto>.Success(response, "Registro exitoso");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                return Result<LoginResponseDto>.Failure("Error al crear el usuario y estudiante. Por favor intente nuevamente.");
            }
        }

        private string HashPassword(string password)
        {
            return PasswordHasher.Hash(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return PasswordHasher.Verify(password, hash);
        }
}
}
