namespace StudentRegistration.Application.DTOs.Estudiante
{
    /// <summary>
    /// DTO simplificado para lista pública de estudiantes.
    /// Fix Problema #8: Solo expone información no sensible (nombre, apellido) para cumplir requisito de negocio.
    /// </summary>
    public class EstudianteSummaryDto
    {
        public int EstudiantId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
