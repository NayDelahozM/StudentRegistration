using AutoMapper;
using StudentRegistration.Application.Common;
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

        public async Task<Result<IEnumerable<EstudianteSummaryDto>>> GetAllAsync()
        {
            var estudiantes = await _unitOfWork.Estudiantes.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<EstudianteSummaryDto>>(estudiantes);
            return Result<IEnumerable<EstudianteSummaryDto>>.Success(dtos);
        }

        /// <summary>
        /// Get paginated list of students (optimized for 100k+ records)
        /// </summary>
        public async Task<Result<PaginatedList<EstudianteSummaryDto>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
        {
            var query = await _unitOfWork.Estudiantes.GetAsQueryableAsync();
            var count = query.Count();
            var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var dtos = _mapper.Map<List<EstudianteSummaryDto>>(items);
            var paginatedList = new PaginatedList<EstudianteSummaryDto>(dtos, count, pageNumber, pageSize);
            return Result<PaginatedList<EstudianteSummaryDto>>.Success(paginatedList);
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

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var estudiante = _mapper.Map<Estudiante>(dto);
                await _unitOfWork.Estudiantes.AddAsync(estudiante);

                await _unitOfWork.CommitAsync();

                var result = await _unitOfWork.Estudiantes.GetWithInscripcionesAsync(estudiante.EstudianteId);
                var resultDto = _mapper.Map<EstudianteDto>(result);

                return Result<EstudianteDto>.Success(resultDto, "Estudiante creado exitosamente");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
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

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                _mapper.Map(dto, estudiante);
                estudiante.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Estudiantes.UpdateAsync(estudiante);

                await _unitOfWork.CommitAsync();

                var result = await _unitOfWork.Estudiantes.GetWithInscripcionesAsync(id);
                var resultDto = _mapper.Map<EstudianteDto>(result);

                return Result<EstudianteDto>.Success(resultDto, "Estudiante actualizado exitosamente");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Result> DeleteAsync(int id)
        {
            var estudiante = await _unitOfWork.Estudiantes.GetByIdAsync(id);

            if (estudiante == null)
            {
                return Result.Failure("Estudiante no encontrado");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                estudiante.IsDeleted = true;
                estudiante.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Estudiantes.UpdateAsync(estudiante);

                await _unitOfWork.CommitAsync();

                return Result.Success("Estudiante eliminado exitosamente");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Result<IEnumerable<CompaneroClaseDto>>> GetCompañerosAsync(int estudianteId)
        {
            var inscripciones = await _unitOfWork.Inscripciones.GetByEstudianteAsync(estudianteId);

            if (!inscripciones.Any())
            {
                return Result<IEnumerable<CompaneroClaseDto>>.Success(new List<CompaneroClaseDto>());
            }

            var materiaIds = inscripciones.Select(i => i.MateriaId).ToList();

            var companerosData = await _unitOfWork.Inscripciones.GetCompañerosByMateriasAsync(materiaIds, estudianteId);

            var companeros = companerosData.Select(c => new CompaneroClaseDto
            {
                EstudianteNombre = c.EstudianteNombre,
                MateriaNombre = c.MateriaNombre
            });

            return Result<IEnumerable<CompaneroClaseDto>>.Success(companeros);
        }
    }
}