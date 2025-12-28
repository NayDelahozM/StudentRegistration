using Microsoft.EntityFrameworkCore;
using StudentRegistration.Domain.Entities;
using StudentRegistration.Domain.Interfaces;
using System.Threading.Tasks;

namespace StudentRegistration.Infrastructure.Repositories
{
    public class ProfesorRepository : Repository<Profesor>, IProfesorRepository
    {
        public ProfesorRepository(DbContext context) : base(context) { }

        public async Task<Profesor> GetWithMateriasAsync(int id)
        {
            return await _context.Set<Profesor>()
                .Include(p => p.ProfesorMaterias)
                    .ThenInclude(pm => pm.Materia)
                .FirstOrDefaultAsync(p => p.ProfesorId == id && !p.IsDeleted);
        }
    }
}
