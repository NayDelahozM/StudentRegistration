using Microsoft.EntityFrameworkCore;
using StudentRegistration.Domain.Entities;
using StudentRegistration.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistration.Infrastructure.Repositories
{
    public class InscripcionRepository : Repository<Inscripcion>, IInscripcionRepository
    {
        public InscripcionRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<Inscripcion>> GetByEstudianteAsync(int estudianteId)
        {
            return await _context.Set<Inscripcion>()
                .Include(i => i.Materia)
                .Include(i => i.Profesor)
                .Include(i => i.Estudiante)
                .Where(i => i.EstudianteId == estudianteId && !i.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Inscripcion>> GetByMateriaAsync(int materiaId)
        {
            return await _context.Set<Inscripcion>()
                .Include(i => i.Estudiante)
                .Include(i => i.Profesor)
                .Where(i => i.MateriaId == materiaId && !i.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Inscripcion>> GetByMateriasAsync(List<int> materiaIds)
        {
            return await _context.Set<Inscripcion>()
                .Include(i => i.Estudiante)
                .Include(i => i.Profesor)
                .Include(i => i.Materia)
                .Where(i => materiaIds.Contains(i.MateriaId) && !i.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountByEstudianteAsync(int estudianteId)
        {
            return await _context.Set<Inscripcion>()
                .CountAsync(i => i.EstudianteId == estudianteId && !i.IsDeleted);
        }

        public async Task<int> GetCreditosByEstudianteAsync(int estudianteId)
        {
            return await _context.Set<Inscripcion>()
                .Where(i => i.EstudianteId == estudianteId && !i.IsDeleted)
                .Include(i => i.Materia)
                .SumAsync(i => i.Materia.Creditos);
        }

        public async Task<bool> TieneProfesorAsync(int estudianteId, int profesorId)
        {
            return await _context.Set<Inscripcion>()
                .AnyAsync(i => i.EstudianteId == estudianteId && 
                              i.ProfesorId == profesorId && 
                              !i.IsDeleted);
        }

        public async Task<bool> ExisteInscripcionAsync(int estudianteId, int materiaId)
        {
            return await _context.Set<Inscripcion>()
                .AnyAsync(i => i.EstudianteId == estudianteId &&
                              i.MateriaId == materiaId &&
                              !i.IsDeleted);
        }

        public async Task<IEnumerable<Inscripcion>> GetAllWithRelationsAsync()
        {
            return await _context.Set<Inscripcion>()
                .Include(i => i.Estudiante)
                .Include(i => i.Materia)
                .Include(i => i.Profesor)
                .Where(i => !i.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(string EstudianteNombre, string MateriaNombre)[]> GetCompañerosByMateriasAsync(List<int> materiaIds, int excludeEstudianteId)
        {
            // Optimized query with direct SQL projection (no N+1 problem)
            var result = await _context.Set<Inscripcion>()
                .Where(i => materiaIds.Contains(i.MateriaId) && i.EstudianteId != excludeEstudianteId && !i.IsDeleted)
                .Select(i => new
                {
                    EstudianteNombre = i.Estudiante.Nombre + " " + i.Estudiante.Apellido,
                    MateriaNombre = i.Materia.Nombre
                })
                .AsNoTracking()
                .ToArrayAsync();

            // Convert anonymous type to ValueTuple array
            return result.Select(x => (x.EstudianteNombre, x.MateriaNombre)).ToArray();
        }
    }
}
