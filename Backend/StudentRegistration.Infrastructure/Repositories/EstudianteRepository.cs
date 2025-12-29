using Microsoft.EntityFrameworkCore;
using StudentRegistration.Domain.Entities;
using StudentRegistration.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistration.Infrastructure.Repositories
{
    public class EstudianteRepository : Repository<Estudiante>, IEstudianteRepository
    {
        public EstudianteRepository(DbContext context) : base(context) { }

        public async Task<Estudiante> GetWithInscripcionesAsync(int id)
        {
            return await _context.Set<Estudiante>()
                .Include(e => e.Inscripciones)
                    .ThenInclude(i => i.Materia)
                .Include(e => e.Inscripciones)
                    .ThenInclude(i => i.Profesor)
                .FirstOrDefaultAsync(e => e.EstudianteId == id && !e.IsDeleted);
        }

        public async Task<IEnumerable<Estudiante>> GetAllWithInscripcionesAsync()
        {
            return await _context.Set<Estudiante>()
                .Where(e => !e.IsDeleted)
                .Include(e => e.Inscripciones)
                    .ThenInclude(i => i.Materia)
                .Include(e => e.Inscripciones)
                    .ThenInclude(i => i.Profesor)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var query = _context.Set<Estudiante>().Where(e => e.Email == email && !e.IsDeleted);
            
            if (excludeId.HasValue)
            {
                query = query.Where(e => e.EstudianteId != excludeId.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task<Estudiante> GetByEmailAsync(string email)
        {
            return await _context.Set<Estudiante>()
                .FirstOrDefaultAsync(e => e.Email == email && !e.IsDeleted);
        }

        public async Task<IQueryable<Estudiante>> GetAsQueryableAsync()
        {
            // Return Task for consistency with async pattern, though the operation is synchronous
            return await Task.FromResult(
                _context.Set<Estudiante>()
                    .Where(e => !e.IsDeleted)
                    .OrderBy(e => e.EstudianteId)
            );
        }
    }
}
