using Microsoft.AspNetCore.Identity;
using StudentRegistration.Domain.Entities;
using StudentRegistration.Domain.Interfaces;
using System;
using CryptoBCrypt = BCrypt.Net.BCrypt;

namespace StudentRegistration.Infrastructure.Common
{
    /// <summary>
    /// Password hasher implementation that supports both ASP.NET Core Identity PasswordHasher (PBKDF2)
    /// and legacy BCrypt hashes for backward compatibility.
    /// New passwords are always hashed with ASP.NET Core Identity PasswordHasher.
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private static readonly IPasswordHasher<Usuario> _identityHasher = new PasswordHasher<Usuario>();

        /// <summary>
        /// Hash a password using ASP.NET Core Identity PasswordHasher (PBKDF2-HMAC-SHA256).
        /// This is the recommended approach for password storage in ASP.NET Core applications.
        /// </summary>
        public string Hash(Usuario usuario, string password)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            return _identityHasher.HashPassword(usuario, password);
        }

        /// <summary>
        /// Verify a password against a hash. Supports both:
        /// 1. ASP.NET Core Identity hashes (PBKDF2-HMAC-SHA256) - format: AQAAAAIAAYagAAAAE...
        /// 2. Legacy BCrypt hashes - format: $2a$, $2b$, or $2y$
        /// </summary>
        public bool Verify(Usuario usuario, string password, string hash)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (string.IsNullOrWhiteSpace(hash))
                return false;

            // Try ASP.NET Core Identity hash first (new format)
            if (!hash.StartsWith("$"))
            {
                var result = _identityHasher.VerifyHashedPassword(usuario, hash, password);
                if (result == PasswordVerificationResult.Success)
                    return true;

                if (result == PasswordVerificationResult.SuccessRehashNeeded)
                    return true;
            }

            // Check if it's a legacy BCrypt hash (starts with $2a$, $2b$, or $2y$)
            if (hash.StartsWith("$2"))
            {
                try
                {
                    return CryptoBCrypt.Verify(password, hash);
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a hash needs to be upgraded to the new ASP.NET Core Identity format.
        /// Legacy BCrypt hashes should be rehashed on next successful login.
        /// </summary>
        public bool NeedsRehash(string hash)
        {
            return hash.StartsWith("$2");
        }
    }
}
