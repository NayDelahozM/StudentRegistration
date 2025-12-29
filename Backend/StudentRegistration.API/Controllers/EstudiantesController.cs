using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRegistration.Application.DTOs.Estudiante;
using StudentRegistration.Application.Interfaces;
using StudentRegistration.API.Helpers;
using System.Threading.Tasks;

namespace StudentRegistration.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EstudiantesController : ControllerBase
    {
        private readonly IEstudianteService _estudianteService;

        public EstudiantesController(IEstudianteService estudianteService)
        {
            _estudianteService = estudianteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _estudianteService.GetAllAsync();
            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!AuthorizationHelper.CanAccessStudentData(HttpContext, id))
            {
                return StatusCode(403, new { message = "No tienes permiso para ver los datos de este estudiante" });
            }

            var result = await _estudianteService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(result.Data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateEstudianteDto dto)
        {
            var result = await _estudianteService.CreateAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data.EstudianteId }, result.Data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEstudianteDto dto)
        {
            var result = await _estudianteService.UpdateAsync(id, dto);

            if (!result.IsSuccess)
            {
                return result.Message.Contains("no encontrado")
                    ? NotFound(new { message = result.Message })
                    : BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _estudianteService.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(new { message = result.Message });
            }

            return NoContent();
        }

        [HttpGet("{id}/companeros")]
        public async Task<IActionResult> GetCompañeros(int id)
        {
            if (!AuthorizationHelper.CanAccessStudentData(HttpContext, id))
            {
                return StatusCode(403, new { message = "No tienes permiso para ver los compañeros de este estudiante" });
            }

            var result = await _estudianteService.GetCompañerosAsync(id);
            return Ok(result.Data);
        }
    }
}