using System;

namespace StudentRegistration.Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public DateTime Expiration { get; set; }
    }
}
