using System;
using System.Collections.Generic;

namespace StudentRegistration.Domain.Entities
{
    public class Estudiante : BaseEntity
    {
        public int EstudiantId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Direccion { get; set; }
        public bool Activo { get; set; } = true;
        public virtual ICollection<Inscripcion> Inscripciones { get; set; }

        public Estudiante()
        {
            Inscripciones = new HashSet<Inscripcion>();
        }
    }
}
