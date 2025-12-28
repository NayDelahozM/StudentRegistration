using System.Collections.Generic;

namespace StudentRegistration.Application.DTOs.Inscripcion
{
    public class CreateInscripcionDto
    {
        public int EstudiantId { get; set; }
        public List<int> MateriaIds { get; set; }
    }
}
