using StudentRegistration.Domain.Entities;

namespace StudentRegistration.Application.Interfaces
{
    /// <summary>
    /// Service interface for JWT token generation
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generate a JWT token for the given user
        /// </summary>
        /// <param name="usuario">User entity to generate token for</param>
        /// <returns>JWT token string</returns>
        string GenerateToken(Usuario usuario);
    }
}
