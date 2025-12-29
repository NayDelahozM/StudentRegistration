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
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly Domain.Interfaces.IPasswordHasher _passwordHasher;

        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, IMapper mapper, Domain.Interfaces.IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var usuario = await _unitOfWork.Usuarios.GetByUsernameAsync(request.Username);

            if (usuario == null || !VerifyPassword(usuario, request.Password, usuario.PasswordHash))
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

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var estudiante = new Estudiante
                {
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    Email = request.Email,
                    Telefono = string.Empty,
                    Direccion = string.Empty,
                    Activo = true
                };

                await _unitOfWork.Estudiantes.AddAsync(estudiante);

                var usuario = new Usuario
                {
                    Username = request.Username,
                    Email = request.Email,
                    Rol = "Estudiante",
                    PasswordHash = string.Empty
                };

                usuario.PasswordHash = HashPassword(usuario, request.Password);

                await _unitOfWork.Usuarios.AddAsync(usuario);

                usuario.Estudiante = estudiante;

                await _unitOfWork.CommitAsync();

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
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Result<LoginResponseDto>.Failure($"Error al crear el usuario y estudiante: {ex.Message}");
            }
        }

        private string HashPassword(Usuario usuario, string password)
        {
            return _passwordHasher.Hash(usuario, password);
        }

        private bool VerifyPassword(Usuario usuario, string password, string hash)
        {
            return _passwordHasher.Verify(usuario, password, hash);
        }
    }
}