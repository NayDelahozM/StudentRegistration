namespace StudentRegistration.Application.DTOs.Auth
{
    public class RegisterRequestDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
    }
}
