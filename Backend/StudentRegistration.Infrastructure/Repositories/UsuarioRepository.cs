using Microsoft.EntityFrameworkCore;
using StudentRegistration.Domain.Entities;
using StudentRegistration.Domain.Interfaces;
using System.Threading.Tasks;

namespace StudentRegistration.Infrastructure.Repositories
{
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(DbContext context) : base(context) { }

        public async Task<Usuario> GetByUsernameAsync(string username)
        {
            return await _context.Set<Usuario>()
                .Include(u => u.Estudiante)
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
        }

        public async Task<Usuario> GetByEmailAsync(string email)
        {
            return await _context.Set<Usuario>()
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Set<Usuario>()
                .AnyAsync(u => u.Username == username && !u.IsDeleted);
        }
    }
}
