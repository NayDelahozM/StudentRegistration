using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentRegistration.Application.DTOs.Inscripcion;
using StudentRegistration.Application.Interfaces;
using StudentRegistration.API.Helpers;
using System.Threading.Tasks;

namespace StudentRegistration.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InscripcionesController : ControllerBase
    {
        private readonly IInscripcionService _inscripcionService;
        private readonly ILogger<InscripcionesController> _logger;

        public InscripcionesController(IInscripcionService inscripcionService, ILogger<InscripcionesController> logger)
        {
            _inscripcionService = inscripcionService;
            _logger = logger;
        }

        [HttpPost("validar")]
        public async Task<IActionResult> Validar([FromBody] CreateInscripcionDto dto)
        {
            // SEGURIDAD: Validar que el estudiante del JWT coincida con el del request
            // PREVIENE: Estudiantes inscribiéndose a otros estudiantes (IDOR/authorization bypass)
            if (!AuthorizationHelper.CanAccessStudentData(HttpContext, dto.EstudianteId))
            {
                return StatusCode(403, new { message = "No tienes permiso para validar inscripciones de este estudiante" });
            }

            var result = await _inscripcionService.ValidateInscripcionAsync(dto.EstudianteId, dto.MateriaIds);

            return Ok(new
            {
                isValid = result.IsSuccess,
                message = result.Message,
                errors = result.Errors
            });
        }

        [HttpPost]
        public async Task<IActionResult> Inscribir([FromBody] CreateInscripcionDto dto)
        {
            // SEGURIDAD: Validar que el estudiante del JWT coincida con el del request
            // PREVIENE: Estudiantes inscribiéndose a otros estudiantes (IDOR/authorization bypass)
            if (!AuthorizationHelper.CanAccessStudentData(HttpContext, dto.EstudianteId))
            {
                _logger.LogWarning("Usuario {Username} intentó inscribir al estudiante {EstudianteId} sin permiso",
                    User.Identity?.Name, dto.EstudianteId);
                return StatusCode(403, new { message = "No tienes permiso para inscribir a este estudiante" });
            }

            var result = await _inscripcionService.InscribirAsync(dto);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Inscripción fallida para estudiante {EstudianteId}: {Message}",
                    dto.EstudianteId, result.Message);
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            _logger.LogInformation("Estudiante {EstudianteId} se inscribió exitosamente en {Count} materias: {Materias}",
                dto.EstudianteId, dto.MateriaIds.Count, string.Join(", ", dto.MateriaIds));

            return CreatedAtAction(nameof(Validar), result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancelar(int id)
        {
            // NECESARIO: Obtener inscripción primero para extraer EstudianteId y validar autorización
            var inscripciones = await _inscripcionService.GetInscripcionByIdAsync(id);

            if (!inscripciones.IsSuccess || inscripciones.Data == null)
            {
                return NotFound(new { message = "Inscripción no encontrada" });
            }

            // SEGURIDAD: Validar que el estudiante del JWT coincida con el de la inscripción
            // PREVIENE: Estudiantes cancelando inscripciones de otros estudiantes
            var inscripcion = inscripciones.Data;
            if (!AuthorizationHelper.CanAccessStudentData(HttpContext, inscripcion.EstudianteId))
            {
                _logger.LogWarning("Usuario {Username} intentó cancelar inscripción {InscripcionId} del estudiante {EstudianteId} sin permiso",
                    User.Identity?.Name, id, inscripcion.EstudianteId);
                return StatusCode(403, new { message = "No tienes permiso para cancelar esta inscripción" });
            }

            var result = await _inscripcionService.CancelarAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(new { message = result.Message });
            }

            _logger.LogInformation("Inscripción {InscripcionId} cancelada exitosamente para estudiante {EstudianteId}",
                id, inscripcion.EstudianteId);

            return NoContent();
        }

        [HttpGet("todas")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _inscripcionService.GetAllAsync();
            return Ok(result.Data);
        }

        [HttpGet("materias-disponibles/{estudianteId}")]
        public async Task<IActionResult> GetMateriasDisponibles(int estudianteId)
        {
            // SEGURIDAD: Validar que el estudiante del JWT coincida con el del request
            if (!AuthorizationHelper.CanAccessStudentData(HttpContext, estudianteId))
            {
                return StatusCode(403, new { message = "No tienes permiso para ver las materias disponibles de este estudiante" });
            }

            var result = await _inscripcionService.GetMateriasDisponiblesAsync(estudianteId);
            return Ok(result.Data);
        }
    }
}
