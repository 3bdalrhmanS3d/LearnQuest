using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace LearnQuestV1.Api.Utilities
{
    /// <summary>
    /// Enhanced security helpers for authentication operations
    /// </summary>
    public static class AuthHelpers
    {
        // Security constants
        private const int PBKDF2_ITERATIONS = 100000; // Increased from 10,000
        private const int SALT_SIZE = 32; // Increased from 16
        private const int HASH_SIZE = 64; // Increased from 32
        private const int MIN_JWT_KEY_LENGTH = 256; // bits
        private const int VERIFICATION_CODE_LENGTH = 6;
        private const int MIN_VERIFICATION_CODE = 100000;
        private const int MAX_VERIFICATION_CODE = 999999;

        /// <summary>
        /// Generates a JWT access token with enhanced security validation
        /// </summary>
        public static JwtSecurityToken GenerateAccessToken(
            string userId,
            string email,
            string fullName,
            UserRole role,
            IConfiguration config,
            string profilePicture = "/uploads/profile-pictures/default.png",
            TimeSpan? customExpiry = null)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name cannot be empty", nameof(fullName));

            // Validate JWT configuration
            var secretKey = config["JWT:SecretKey"];
            var issuer = config["JWT:ValidIss"];
            var audience = config["JWT:ValidAud"];

            ValidateJwtConfiguration(secretKey, issuer, audience);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, fullName),
                new Claim("profilePicture", profilePicture),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim("jti", Guid.NewGuid().ToString()), // JWT ID for tracking
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiry = customExpiry ?? TimeSpan.FromHours(1);
            if (expiry > TimeSpan.FromDays(1)) // Max 1 days
                throw new ArgumentException("Token expiry cannot exceed 1 days");

            return new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(expiry),
                signingCredentials: creds
            );
        }

        /// <summary>
        /// Enhanced password verification with timing attack protection
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (string.IsNullOrWhiteSpace(storedHash) || !storedHash.Contains(':'))
                return false;

            try
            {
                var parts = storedHash.Split(':', 2);
                if (parts.Length != 2)
                    return false;

                var salt = Convert.FromBase64String(parts[0]);
                var storedHashBytes = Convert.FromBase64String(parts[1]);

                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    PBKDF2_ITERATIONS,
                    HashAlgorithmName.SHA256);

                var computedHash = pbkdf2.GetBytes(storedHashBytes.Length);

                // Use constant-time comparison to prevent timing attacks
                return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
            }
            catch (Exception)
            {
                // Always return false for any parsing/crypto errors
                // Don't reveal the specific error to prevent information leakage
                return false;
            }
        }

        /// <summary>
        /// Enhanced password hashing with stronger parameters
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            // Validate password strength
            if (!IsPasswordStrong(password))
                throw new ArgumentException("Password does not meet security requirements", nameof(password));

            var salt = new byte[SALT_SIZE];
            RandomNumberGenerator.Fill(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                PBKDF2_ITERATIONS,
                HashAlgorithmName.SHA256);

            var hash = pbkdf2.GetBytes(HASH_SIZE);

            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Generates a cryptographically secure verification code
        /// </summary>
        public static string GenerateVerificationCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            int code;

            do
            {
                rng.GetBytes(bytes);
                code = Math.Abs(BitConverter.ToInt32(bytes, 0));
            }
            while (code < MIN_VERIFICATION_CODE || code > MAX_VERIFICATION_CODE);

            return code.ToString();
        }

        /// <summary>
        /// Validates password strength
        /// </summary>
        public static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Minimum requirements:
            // - At least 8 characters
            // - At least one uppercase letter
            // - At least one lowercase letter
            // - At least one digit
            // - At least one special character

            if (password.Length < 8)
                return false;

            if (!password.Any(char.IsUpper))
                return false;

            if (!password.Any(char.IsLower))
                return false;

            if (!password.Any(char.IsDigit))
                return false;

            if (!password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)))
                return false;

            // Check for common weak patterns
            if (HasCommonWeakPatterns(password))
                return false;

            return true;
        }

        /// <summary>
        /// Generates a secure random token for various purposes
        /// </summary>
        public static string GenerateSecureToken(int lengthInBytes = 32)
        {
            var bytes = new byte[lengthInBytes];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Validates JWT configuration security
        /// </summary>
        private static void ValidateJwtConfiguration(string? secretKey, string? issuer, string? audience)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("JWT secret key is not configured");

            if (string.IsNullOrWhiteSpace(issuer))
                throw new InvalidOperationException("JWT issuer is not configured");

            if (string.IsNullOrWhiteSpace(audience))
                throw new InvalidOperationException("JWT audience is not configured");

            // Validate secret key strength
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            if (keyBytes.Length * 8 < MIN_JWT_KEY_LENGTH)
                throw new InvalidOperationException($"JWT secret key must be at least {MIN_JWT_KEY_LENGTH} bits");
        }

        /// <summary>
        /// Checks for common weak password patterns
        /// </summary>
        private static bool HasCommonWeakPatterns(string password)
        {
            // Check for sequential characters (123, abc, etc.)
            var sequential = new[]
            {
                "123", "234", "345", "456", "567", "678", "789",
                "abc", "bcd", "cde", "def", "efg", "fgh", "ghi"
            };

            var lowerPassword = password.ToLower();
            if (sequential.Any(seq => lowerPassword.Contains(seq)))
                return true;

            // Check for repeated characters (aaa, 111, etc.)
            if (Regex.IsMatch(password, @"(.)\1{2,}"))
                return true;

            // Check for common weak passwords
            var commonWeak = new[]
            {
                "password", "123456", "qwerty", "admin", "letmein"
            };

            if (commonWeak.Any(weak => lowerPassword.Contains(weak)))
                return true;

            return false;
        }

        /// <summary>
        /// Gets password strength score (0-100)
        /// </summary>
        public static int GetPasswordStrengthScore(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return 0;

            int score = 0;

            // Length scoring
            if (password.Length >= 8) score += 20;
            if (password.Length >= 12) score += 10;
            if (password.Length >= 16) score += 10;

            // Character diversity
            if (password.Any(char.IsUpper)) score += 15;
            if (password.Any(char.IsLower)) score += 15;
            if (password.Any(char.IsDigit)) score += 15;
            if (password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c))) score += 15;

            // Penalty for weak patterns
            if (HasCommonWeakPatterns(password)) score -= 30;

            return Math.Max(0, Math.Min(100, score));
        }
    }
}