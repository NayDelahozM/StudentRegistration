namespace StudentRegistration.Domain.Entities
{
    public class ProfesorMateria : BaseEntity
    {
        public int ProfesorMateriaId { get; set; }
        public int ProfesorId { get; set; }
        public int MateriaId { get; set; }
        public virtual Profesor Profesor { get; set; }
        public virtual Materia Materia { get; set; }
    }
}
