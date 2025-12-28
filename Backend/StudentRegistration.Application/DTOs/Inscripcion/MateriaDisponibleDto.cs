namespace StudentRegistration.Application.DTOs.Inscripcion
{
    public class MateriaDisponibleDto
    {
        public int MateriaId { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public int Creditos { get; set; }
        public int ProfesorId { get; set; }
        public string ProfesorNombre { get; set; }
        public bool Disponible { get; set; }
        public string MotivoNoDisponible { get; set; }
    }
}
