using System;

namespace StudentRegistration.Application.DTOs.Estudiante
{
    public class UpdateEstudianteDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Direccion { get; set; }
        public bool Activo { get; set; }
    }
}
