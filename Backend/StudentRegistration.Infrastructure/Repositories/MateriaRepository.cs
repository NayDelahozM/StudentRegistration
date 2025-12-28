using Microsoft.EntityFrameworkCore;
using StudentRegistration.Domain.Entities;
using StudentRegistration.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistration.Infrastructure.Repositories
{
    public class MateriaRepository : Repository<Materia>, IMateriaRepository
    {
        public MateriaRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<Materia>> GetMateriasConProfesoresAsync()
        {
            return await _context.Set<Materia>()
                .Where(m => !m.IsDeleted)
                .Include(m => m.ProfesorMaterias)
                    .ThenInclude(pm => pm.Profesor)
                .ToListAsync();
        }

        public async Task<Materia> GetMateriaConProfesorAsync(int materiaId)
        {
            return await _context.Set<Materia>()
                .Include(m => m.ProfesorMaterias)
                    .ThenInclude(pm => pm.Profesor)
                .FirstOrDefaultAsync(m => m.MateriaId == materiaId && !m.IsDeleted);
        }
    }
}
