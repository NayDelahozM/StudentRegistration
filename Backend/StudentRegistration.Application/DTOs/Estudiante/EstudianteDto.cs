using System;
using System.Collections.Generic;
using StudentRegistration.Application.DTOs.Inscripcion;

namespace StudentRegistration.Application.DTOs.Estudiante
{
    public class EstudianteDto
    {
        public int EstudiantId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreCompleto => $"{Nombre} {Apellido}";
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Direccion { get; set; }
        public bool Activo { get; set; }
        public int CreditosTotales { get; set; }
        public List<InscripcionDto> Inscripciones { get; set; }
    }
}
