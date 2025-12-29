using StudentRegistration.Domain.Entities;

namespace StudentRegistration.Domain.Interfaces
{
    /// <summary>
    /// Interface for password hashing (abstraction for Clean Architecture).
    /// Implementation is in Infrastructure layer.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hash a password securely.
        /// </summary>
        string Hash(Usuario usuario, string password);

        /// <summary>
        /// Verify a password against a hash.
        /// Supports both new format (PBKDF2) and legacy format (BCrypt).
        /// </summary>
        bool Verify(Usuario usuario, string password, string hash);

        /// <summary>
        /// Check if a hash needs to be upgraded to the new format.
        /// </summary>
        bool NeedsRehash(string hash);
    }
}
