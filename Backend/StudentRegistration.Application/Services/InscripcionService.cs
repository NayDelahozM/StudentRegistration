using AutoMapper;
using StudentRegistration.Application.DTOs.Inscripcion;
using StudentRegistration.Application.Interfaces;
using StudentRegistration.Domain.Common;
using StudentRegistration.Domain.Entities;
using StudentRegistration.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistration.Application.Services
{
    public class InscripcionService : IInscripcionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private const int MAX_MATERIAS = 3;

        public InscripcionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result> ValidateInscripcionAsync(int estudianteId, List<int> materiaIds)
        {
            var errors = new List<string>();

            var estudiante = await _unitOfWork.Estudiantes.GetByIdAsync(estudianteId);
            if (estudiante == null)
            {
                return Result.Failure("Estudiante no encontrado");
            }

            if (materiaIds == null || !materiaIds.Any())
            {
                errors.Add("Debe seleccionar al menos una materia");
                return Result.Failure(errors);
            }

            if (materiaIds.Count > MAX_MATERIAS)
            {
                errors.Add($"No puede seleccionar más de {MAX_MATERIAS} materias");
                return Result.Failure(errors);
            }

            var inscripcionesActuales = await _unitOfWork.Inscripciones.CountByEstudianteAsync(estudianteId);
            if (inscripcionesActuales + materiaIds.Count > MAX_MATERIAS)
            {
                errors.Add($"Ya tiene {inscripcionesActuales} materia(s). Máximo: {MAX_MATERIAS}");
                return Result.Failure(errors);
            }

            var profesoresAsignados = new HashSet<int>();
            var inscripcionesExistentes = await _unitOfWork.Inscripciones.GetByEstudianteAsync(estudianteId);
            
            foreach (var i in inscripcionesExistentes)
            {
                profesoresAsignados.Add(i.ProfesorId);
            }

            foreach (var materiaId in materiaIds)
            {
                if (await _unitOfWork.Inscripciones.ExisteInscripcionAsync(estudianteId, materiaId))
                {
                    errors.Add($"Ya está inscrito en la materia con ID {materiaId}");
                    continue;
                }

                var materia = await _unitOfWork.Materias.GetMateriaConProfesorAsync(materiaId);
                if (materia == null)
                {
                    errors.Add($"Materia con ID {materiaId} no encontrada");
                    continue;
                }

                var profesorMateria = materia.ProfesorMaterias.FirstOrDefault();
                if (profesorMateria == null)
                {
                    errors.Add($"La materia {materia.Nombre} no tiene profesor asignado");
                    continue;
                }

                if (profesoresAsignados.Contains(profesorMateria.ProfesorId))
                {
                    errors.Add($"Ya tiene una materia con el profesor {profesorMateria.Profesor.Nombre} {profesorMateria.Profesor.Apellido}");
                }
                else
                {
                    profesoresAsignados.Add(profesorMateria.ProfesorId);
                }
            }

            if (errors.Any())
            {
                return Result.Failure(errors);
            }

            return Result.Success("Validación exitosa");
        }

        public async Task<Result<IEnumerable<InscripcionDto>>> InscribirAsync(CreateInscripcionDto dto)
        {
            var validacion = await ValidateInscripcionAsync(dto.EstudianteId, dto.MateriaIds);
            if (!validacion.IsSuccess)
            {
                return Result<IEnumerable<InscripcionDto>>.Failure(validacion.Message, validacion.Errors);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var materiaId in dto.MateriaIds)
                {
                    var materia = await _unitOfWork.Materias.GetMateriaConProfesorAsync(materiaId);
                    var profesorMateria = materia.ProfesorMaterias.First();

                    var inscripcion = new Inscripcion
                    {
                        EstudianteId = dto.EstudianteId,
                        MateriaId = materiaId,
                        ProfesorId = profesorMateria.ProfesorId
                    };

                    await _unitOfWork.Inscripciones.AddAsync(inscripcion);
                }

                // CommitAsync already calls SaveChangesAsync internally
                await _unitOfWork.CommitAsync();

                var inscripciones = await _unitOfWork.Inscripciones.GetByEstudianteAsync(dto.EstudianteId);
                var dtos = _mapper.Map<IEnumerable<InscripcionDto>>(inscripciones);

                return Result<IEnumerable<InscripcionDto>>.Success(dtos, "Inscripción realizada exitosamente");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Result> CancelarAsync(int inscripcionId)
        {
            var inscripcion = await _unitOfWork.Inscripciones.GetByIdAsync(inscripcionId);

            if (inscripcion == null)
            {
                return Result.Failure("Inscripción no encontrada");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _unitOfWork.Inscripciones.DeleteAsync(inscripcion);

                // CommitAsync hace SaveChangesAsync internamente
                await _unitOfWork.CommitAsync();

                return Result.Success("Inscripción cancelada exitosamente");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Result<IEnumerable<MateriaDisponibleDto>>> GetMateriasDisponiblesAsync(int estudianteId)
        {
            var todasLasMaterias = await _unitOfWork.Materias.GetMateriasConProfesoresAsync();
            var inscripciones = await _unitOfWork.Inscripciones.GetByEstudianteAsync(estudianteId);
            
            var profesoresAsignados = inscripciones.Select(i => i.ProfesorId).ToHashSet();
            var materiasInscritas = inscripciones.Select(i => i.MateriaId).ToHashSet();

            var resultado = todasLasMaterias.Select(m =>
            {
                var pm = m.ProfesorMaterias.FirstOrDefault();
                var disponible = pm != null && 
                                !materiasInscritas.Contains(m.MateriaId) && 
                                !profesoresAsignados.Contains(pm.ProfesorId);

                string motivo = "";
                if (materiasInscritas.Contains(m.MateriaId))
                    motivo = "Ya inscrito";
                else if (pm != null && profesoresAsignados.Contains(pm.ProfesorId))
                    motivo = $"Ya tiene materia con {pm.Profesor.Nombre} {pm.Profesor.Apellido}";

                return new MateriaDisponibleDto
                {
                    MateriaId = m.MateriaId,
                    Nombre = m.Nombre,
                    Codigo = m.Codigo,
                    Creditos = m.Creditos,
                    ProfesorId = pm?.ProfesorId ?? 0,
                    ProfesorNombre = pm != null ? $"{pm.Profesor.Nombre} {pm.Profesor.Apellido}" : "Sin profesor",
                    Disponible = disponible,
                    MotivoNoDisponible = motivo
                };
            });

            return Result<IEnumerable<MateriaDisponibleDto>>.Success(resultado);
        }

        public async Task<Result<InscripcionDto>> GetInscripcionByIdAsync(int inscripcionId)
        {
            var inscripcion = await _unitOfWork.Inscripciones.GetByIdAsync(inscripcionId);

            if (inscripcion == null)
            {
                return Result<InscripcionDto>.Failure("Inscripción no encontrada");
            }

            var dto = _mapper.Map<InscripcionDto>(inscripcion);
            return Result<InscripcionDto>.Success(dto);
        }

        public async Task<Result<IEnumerable<InscripcionDto>>> GetAllAsync()
        {
            var inscripciones = await _unitOfWork.Inscripciones.GetAllWithRelationsAsync();

            var dtos = inscripciones.Select(i => new InscripcionDto
            {
                InscripcionId = i.InscripcionId,
                EstudianteId = i.EstudianteId,
                EstudianteNombre = $"{i.Estudiante.Nombre} {i.Estudiante.Apellido}",
                EstudianteEmail = i.Estudiante.Email,
                MateriaId = i.MateriaId,
                MateriaNombre = i.Materia.Nombre,
                MateriaCode = i.Materia.Codigo,
                ProfesorId = i.ProfesorId,
                ProfesorNombre = $"{i.Profesor.Nombre} {i.Profesor.Apellido}",
                FechaInscripcion = i.CreatedAt
            });

            return Result<IEnumerable<InscripcionDto>>.Success(dtos);
        }
    }
}
