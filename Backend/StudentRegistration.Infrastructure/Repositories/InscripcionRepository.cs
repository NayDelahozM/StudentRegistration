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
                .Where(i => i.EstudiantId == estudianteId && !i.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inscripcion>> GetByMateriaAsync(int materiaId)
        {
            return await _context.Set<Inscripcion>()
                .Include(i => i.Estudiante)
                .Include(i => i.Profesor)
                .Where(i => i.MateriaId == materiaId && !i.IsDeleted)
                .ToListAsync();
        }

        public async Task<int> CountByEstudianteAsync(int estudianteId)
        {
            return await _context.Set<Inscripcion>()
                .CountAsync(i => i.EstudiantId == estudianteId && !i.IsDeleted);
        }

        public async Task<bool> TieneProfesorAsync(int estudianteId, int profesorId)
        {
            return await _context.Set<Inscripcion>()
                .AnyAsync(i => i.EstudiantId == estudianteId && 
                              i.ProfesorId == profesorId && 
                              !i.IsDeleted);
        }

        public async Task<bool> ExisteInscripcionAsync(int estudianteId, int materiaId)
        {
            return await _context.Set<Inscripcion>()
                .AnyAsync(i => i.EstudiantId == estudianteId && 
                              i.MateriaId == materiaId && 
                              !i.IsDeleted);
        }
    }
}
