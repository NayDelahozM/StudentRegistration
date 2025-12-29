using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using StudentRegistration.Domain.Entities;

namespace StudentRegistration.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
    }

    public interface IEstudianteRepository : IRepository<Estudiante>
    {
        Task<Estudiante> GetWithInscripcionesAsync(int id);
        Task<IEnumerable<Estudiante>> GetAllWithInscripcionesAsync();
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task<Estudiante> GetByEmailAsync(string email);
    }

    public interface IInscripcionRepository : IRepository<Inscripcion>
    {
        Task<IEnumerable<Inscripcion>> GetByEstudianteAsync(int estudianteId);
        Task<IEnumerable<Inscripcion>> GetByMateriaAsync(int materiaId);
        Task<IEnumerable<Inscripcion>> GetByMateriasAsync(List<int> materiaIds);
        Task<int> CountByEstudianteAsync(int estudianteId);
        Task<bool> TieneProfesorAsync(int estudianteId, int profesorId);
        Task<bool> ExisteInscripcionAsync(int estudianteId, int materiaId);
        Task<IEnumerable<Inscripcion>> GetAllWithRelationsAsync();
    }

    public interface IMateriaRepository : IRepository<Materia>
    {
        Task<IEnumerable<Materia>> GetMateriasConProfesoresAsync();
        Task<Materia> GetMateriaConProfesorAsync(int materiaId);
    }

    public interface IProfesorRepository : IRepository<Profesor>
    {
        Task<Profesor> GetWithMateriasAsync(int id);
    }

    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario> GetByUsernameAsync(string username);
        Task<Usuario> GetByEmailAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
    }

    public interface IUnitOfWork : IDisposable
    {
        IEstudianteRepository Estudiantes { get; }
        IInscripcionRepository Inscripciones { get; }
        IMateriaRepository Materias { get; }
        IProfesorRepository Profesores { get; }
        IUsuarioRepository Usuarios { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
