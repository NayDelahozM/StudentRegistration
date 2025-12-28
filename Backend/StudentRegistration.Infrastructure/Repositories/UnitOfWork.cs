using Microsoft.EntityFrameworkCore.Storage;
using StudentRegistration.Domain.Interfaces;
using StudentRegistration.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace StudentRegistration.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;
        
        private IEstudianteRepository _estudiantes;
        private IInscripcionRepository _inscripciones;
        private IMateriaRepository _materias;
        private IProfesorRepository _profesores;
        private IUsuarioRepository _usuarios;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEstudianteRepository Estudiantes =>
            _estudiantes ??= new EstudianteRepository(_context);

        public IInscripcionRepository Inscripciones =>
            _inscripciones ??= new InscripcionRepository(_context);

        public IMateriaRepository Materias =>
            _materias ??= new MateriaRepository(_context);

        public IProfesorRepository Profesores =>
            _profesores ??= new ProfesorRepository(_context);

        public IUsuarioRepository Usuarios =>
            _usuarios ??= new UsuarioRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
