namespace StudentRegistration.Domain.Entities
{
    public class Inscripcion : BaseEntity
    {
        public int InscripcionId { get; set; }
        public int EstudianteId { get; set; }
        public int MateriaId { get; set; }
        public int ProfesorId { get; set; }
        public virtual Estudiante Estudiante { get; set; }
        public virtual Materia Materia { get; set; }
        public virtual Profesor Profesor { get; set; }
    }
}
