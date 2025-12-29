using System;
using CryptoBCrypt = BCrypt.Net.BCrypt;

namespace StudentRegistration.Domain.Common
{
    /// <summary>
    /// Secure password hasher using BCrypt (adaptive hashing with automatic salt).
    /// BCrypt is industry-standard and recommended for password storage.
    /// Work factor: 12 (provides good security vs performance balance).
    /// </summary>
    public static class PasswordHasher
    {
        private const int WorkFactor = 12;

        /// <summary>
        /// Hash a password using BCrypt with automatic salt generation.
        /// </summary>
        public static string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            return CryptoBCrypt.HashPassword(password, WorkFactor);
        }

        /// <summary>
        /// Verify a password against a hash. Supports both BCrypt and legacy SHA256+Base64.
        /// Legacy format is automatically verified but not rehashed (caller should handle rehashing if needed).
        /// </summary>
        public static bool Verify(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (string.IsNullOrWhiteSpace(hash))
                return false;

            // Check if it's a BCrypt hash (starts with $2a$, $2b$, or $2y$)
            if (hash.StartsWith("$2"))
            {
                return CryptoBCrypt.Verify(password, hash);
            }

            // Legacy SHA256+Base64 format - verify
            if (IsLegacyHash(password, hash))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a password matches a legacy SHA256+Base64 hash.
        /// </summary>
        private static bool IsLegacyHash(string password, string hash)
        {
            try
            {
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                var legacyHash = Convert.ToBase64String(bytes);
                return legacyHash == hash;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if a hash needs to be upgraded to BCrypt.
        /// </summary>
        public static bool NeedsRehash(string hash)
        {
            return !hash.StartsWith("$2");
        }
    }
}
