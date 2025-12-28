using System;
using System.Security.Cryptography;
using System.Text;

namespace StudentRegistration.Domain.Common
{
    /// <summary>
    /// Simple deterministic hasher (SHA256 + Base64) used for the technical test/demo.
    /// NOTE: For production you should use a salted adaptive hash (e.g., PBKDF2/bcrypt/Argon2).
    /// </summary>
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public static bool Verify(string password, string hash)
            => Hash(password) == hash;
    }
}
