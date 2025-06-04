using LearnQuestV1.Core.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LearnQuestV1.Api.Utilities
{
    /// <summary>
    /// Contains static helper methods for token generation, password hashing/verification,
    /// and random verification‐code generation.
    /// </summary>
    public class AuthHelpers
    {
        /// <summary>
        /// Generates a JWT access token for the given user information.
        /// </summary>
        public static JwtSecurityToken GenerateAccessToken(
            string userId,
            string email,
            string fullName,
            UserRole role,
            IConfiguration config,
            TimeSpan? customExpiry = null)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Role, role.ToString())
            };

            // Read secret key from configuration
            var secretKey = config["JWT:SecretKey"];
            var issuer = config["JWT:ValidIss"];
            var audience = config["JWT:ValidAud"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(customExpiry ?? TimeSpan.FromHours(1)),
                signingCredentials: creds
            );
        }

        /// <summary>
        /// Verifies a plain‐text password against a stored salted hash ("salt:hash").
        /// Returns true if they match.
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash) || !storedHash.Contains(":"))
                return false;

            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt;
            byte[] hashBytes;
            try
            {
                salt = Convert.FromBase64String(parts[0]);
                hashBytes = Convert.FromBase64String(parts[1]);
            }
            catch
            {
                return false;
            }

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] computedHash = pbkdf2.GetBytes(hashBytes.Length);

            return CryptographicOperations.FixedTimeEquals(computedHash, hashBytes);
        }

        /// <summary>
        /// Generates a 6‐digit random verification code (string). Range: 100000–999999.
        /// </summary>
        public static string GenerateVerificationCode()
        {
            byte[] bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            int raw = BitConverter.ToInt32(bytes, 0) % 900000 + 100000;
            return Math.Abs(raw).ToString();
        }

        /// <summary>
        /// Produces a salted SHA‐256 hash of the plain‐text password in the format "salt:hash".
        /// </summary>
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }
    }
}
