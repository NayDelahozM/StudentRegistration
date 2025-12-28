using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public InscripcionesController(IInscripcionService inscripcionService)
        {
            _inscripcionService = inscripcionService;
        }

        [HttpPost("validar")]
        public async Task<IActionResult> Validar([FromBody] CreateInscripcionDto dto)
        {
            // Fix Problema #2: Validar autorización por estudiante
            if (!AuthorizationHelper.CanAccessStudentData(HttpContext, dto.EstudiantId))
            {
                return StatusCode(403, new { message = "No tienes permiso para validar inscripciones de este estudiante" });
            }

            var result = await _inscripcionService.ValidateInscripcionAsync(dto.EstudiantId, dto.MateriaIds);

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
            // Fix Problema #2: Validar autorización por estudiante
            if (!AuthorizationHelper.CanAccessStudentData(HttpContext, dto.EstudiantId))
            {
                return StatusCode(403, new { message = "No tienes permiso para inscribir a este estudiante" });
            }

            var result = await _inscripcionService.InscribirAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return CreatedAtAction(nameof(Validar), result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var result = await _inscripcionService.CancelarAsync(id);
            
            if (!result.IsSuccess)
            {
                return NotFound(new { message = result.Message });
            }

            return NoContent();
        }

        [HttpGet("materias-disponibles/{estudianteId}")]
        public async Task<IActionResult> GetMateriasDisponibles(int estudianteId)
        {
            // Fix Problema #2: Validar autorización por estudiante
            if (!AuthorizationHelper.CanAccessStudentData(HttpContext, estudianteId))
            {
                return StatusCode(403, new { message = "No tienes permiso para ver las materias disponibles de este estudiante" });
            }

            var result = await _inscripcionService.GetMateriasDisponiblesAsync(estudianteId);
            return Ok(result.Data);
        }
    }
}
