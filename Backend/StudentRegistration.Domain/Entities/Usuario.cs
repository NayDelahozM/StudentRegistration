namespace StudentRegistration.Domain.Entities
{
    public class Usuario : BaseEntity
    {
        public int UsuarioId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Rol { get; set; }
        public int? EstudiantId { get; set; }
        public virtual Estudiante Estudiante { get; set; }
    }
}
