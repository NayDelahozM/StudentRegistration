using StudentRegistration.Application.DTOs.Inscripcion;
using StudentRegistration.Domain.Common;


namespace StudentRegistration.Application.Interfaces
{
    public interface IInscripcionService
    {
        Task<Result> ValidateInscripcionAsync(int estudianteId, List<int> materiaIds);
        Task<Result<IEnumerable<InscripcionDto>>> InscribirAsync(CreateInscripcionDto dto);
        Task<Result> CancelarAsync(int inscripcionId);
        Task<Result<IEnumerable<MateriaDisponibleDto>>> GetMateriasDisponiblesAsync(int estudianteId);
        Task<Result<InscripcionDto>> GetInscripcionByIdAsync(int inscripcionId);
        Task<Result<IEnumerable<InscripcionDto>>> GetAllAsync();
    }
}
