using System.Collections.Generic;

namespace StudentRegistration.Domain.Entities
{
    public class Profesor : BaseEntity
    {
        public int ProfesorId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public virtual ICollection<ProfesorMateria> ProfesorMaterias { get; set; }
        public virtual ICollection<Inscripcion> Inscripciones { get; set; }

        public Profesor()
        {
            ProfesorMaterias = new HashSet<ProfesorMateria>();
            Inscripciones = new HashSet<Inscripcion>();
        }
    }
}
