namespace StudentRegistration.Application.DTOs.Auth
{
    public class RegisterRequestDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        // Fix Problema #3: Agregar campos para crear Estudiante durante el registro
        public string Nombre { get; set; }
        public string Apellido { get; set; }
    }
}
