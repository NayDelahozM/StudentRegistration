using System.Collections.Generic;

namespace StudentRegistration.Domain.Entities
{
    public class Materia : BaseEntity
    {
        public int MateriaId { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public int Creditos { get; set; } = 3;
        public string Descripcion { get; set; }
        public virtual ICollection<ProfesorMateria> ProfesorMaterias { get; set; }
        public virtual ICollection<Inscripcion> Inscripciones { get; set; }

        public Materia()
        {
            ProfesorMaterias = new HashSet<ProfesorMateria>();
            Inscripciones = new HashSet<Inscripcion>();
        }
    }
}
