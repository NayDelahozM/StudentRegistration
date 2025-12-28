using System;

namespace StudentRegistration.Application.DTOs.Estudiante
{
    public class CreateEstudianteDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Direccion { get; set; }
    }
}
