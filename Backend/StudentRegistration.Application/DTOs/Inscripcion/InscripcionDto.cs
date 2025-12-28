using System;

namespace StudentRegistration.Application.DTOs.Inscripcion
{
    public class InscripcionDto
    {
        public int InscripcionId { get; set; }
        public int MateriaId { get; set; }
        public string MateriaNombre { get; set; }
        public string MateriaCode { get; set; }
        public int ProfesorId { get; set; }
        public string ProfesorNombre { get; set; }
        public DateTime FechaInscripcion { get; set; }
    }
}
