using StudentRegistration.Application.Common;
using StudentRegistration.Application.DTOs.Estudiante;
using StudentRegistration.Application.DTOs.Inscripcion;
using StudentRegistration.Domain.Common;

namespace StudentRegistration.Application.Interfaces
{
    public interface IEstudianteService
    {
        // Fix Problema #8: GetAll ahora devuelve DTO simplificado (solo nombre/apellido) para cumplir requisito de negocio
        Task<Result<IEnumerable<EstudianteSummaryDto>>> GetAllAsync();
        Task<Result<PaginatedList<EstudianteSummaryDto>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
        Task<Result<EstudianteDto>> GetByIdAsync(int id);
        Task<Result<EstudianteDto>> CreateAsync(CreateEstudianteDto dto);
        Task<Result<EstudianteDto>> UpdateAsync(int id, UpdateEstudianteDto dto);
        Task<Result> DeleteAsync(int id);
        Task<Result<IEnumerable<CompañeroClaseDto>>> GetCompañerosAsync(int estudianteId);
    }
}
