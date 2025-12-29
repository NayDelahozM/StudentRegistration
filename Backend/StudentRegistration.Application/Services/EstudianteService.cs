using AutoMapper;
using StudentRegistration.Application.DTOs.Estudiante;
using StudentRegistration.Application.DTOs.Inscripcion;
using StudentRegistration.Application.Interfaces;
using StudentRegistration.Domain.Common;
using StudentRegistration.Domain.Entities;
using StudentRegistration.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistration.Application.Services
{
    public class EstudianteService : IEstudianteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EstudianteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // Fix Problema #8: GetAll ahora devuelve DTO simplificado (solo nombre/apellido) para cumplir requisito de negocio
        public async Task<Result<IEnumerable<EstudianteSummaryDto>>> GetAllAsync()
        {
            var estudiantes = await _unitOfWork.Estudiantes.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<EstudianteSummaryDto>>(estudiantes);
            return Result<IEnumerable<EstudianteSummaryDto>>.Success(dtos);
        }

        public async Task<Result<EstudianteDto>> GetByIdAsync(int id)
        {
            var estudiante = await _unitOfWork.Estudiantes.GetWithInscripcionesAsync(id);
            
            if (estudiante == null)
            {
                return Result<EstudianteDto>.Failure("Estudiante no encontrado");
            }

            var dto = _mapper.Map<EstudianteDto>(estudiante);
            return Result<EstudianteDto>.Success(dto);
        }

        public async Task<Result<EstudianteDto>> CreateAsync(CreateEstudianteDto dto)
        {
            if (await _unitOfWork.Estudiantes.EmailExistsAsync(dto.Email))
            {
                return Result<EstudianteDto>.Failure("El email ya está registrado");
            }

            var estudiante = _mapper.Map<Estudiante>(dto);
            await _unitOfWork.Estudiantes.AddAsync(estudiante);
            await _unitOfWork.SaveChangesAsync();

            var result = await _unitOfWork.Estudiantes.GetWithInscripcionesAsync(estudiante.EstudiantId);
            var resultDto = _mapper.Map<EstudianteDto>(result);

            return Result<EstudianteDto>.Success(resultDto, "Estudiante creado exitosamente");
        }

        public async Task<Result<EstudianteDto>> UpdateAsync(int id, UpdateEstudianteDto dto)
        {
            var estudiante = await _unitOfWork.Estudiantes.GetByIdAsync(id);
            
            if (estudiante == null)
            {
                return Result<EstudianteDto>.Failure("Estudiante no encontrado");
            }

            if (await _unitOfWork.Estudiantes.EmailExistsAsync(dto.Email, id))
            {
                return Result<EstudianteDto>.Failure("El email ya está registrado");
            }

            _mapper.Map(dto, estudiante);
            estudiante.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.Estudiantes.UpdateAsync(estudiante);
            await _unitOfWork.SaveChangesAsync();

            var result = await _unitOfWork.Estudiantes.GetWithInscripcionesAsync(id);
            var resultDto = _mapper.Map<EstudianteDto>(result);

            return Result<EstudianteDto>.Success(resultDto, "Estudiante actualizado exitosamente");
        }

        public async Task<Result> DeleteAsync(int id)
        {
            var estudiante = await _unitOfWork.Estudiantes.GetByIdAsync(id);
            
            if (estudiante == null)
            {
                return Result.Failure("Estudiante no encontrado");
            }

            estudiante.IsDeleted = true;
            estudiante.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.Estudiantes.UpdateAsync(estudiante);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Estudiante eliminado exitosamente");
        }

        public async Task<Result<IEnumerable<CompañeroClaseDto>>> GetCompañerosAsync(int estudianteId)
        {
            var inscripciones = await _unitOfWork.Inscripciones.GetByEstudianteAsync(estudianteId);

            if (!inscripciones.Any())
            {
                return Result<IEnumerable<CompañeroClaseDto>>.Success(new List<CompañeroClaseDto>());
            }

            // Obtener todos los IDs de materia del estudiante
            var materiaIds = inscripciones.Select(i => i.MateriaId).ToList();

            // Obtener todas las inscripciones de esas materias en una sola consulta
            var todasLasInscripciones = await _unitOfWork.Inscripciones.GetByMateriasAsync(materiaIds);

            // Filtrar: Excluir al estudiante mismo y procesar en memoria
            var companeros = todasLasInscripciones
                .Where(i => i.EstudiantId != estudianteId)
                .Select(i => new CompañeroClaseDto
                {
                    EstudianteNombre = $"{i.Estudiante.Nombre} {i.Estudiante.Apellido}",
                    MateriaNombre = i.Materia.Nombre
                })
                .ToList();

            return Result<IEnumerable<CompañeroClaseDto>>.Success(companeros);
        }
    }
}
