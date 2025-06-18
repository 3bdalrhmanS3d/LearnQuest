// AutoLoginService.cs
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.UserManagement;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class AutoLoginService : IAutoLoginService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;

        public AutoLoginService(IUnitOfWork uow, IConfiguration config)
        {
            _uow = uow;
            _config = config;
        }

        public async Task<string> CreateAutoLoginTokenAsync(int userId)
        {
            // Generate secure random token
            var tokenBytes = new byte[32];
            RandomNumberGenerator.Fill(tokenBytes);
            var token = Convert.ToBase64String(tokenBytes);

            // Create auto-login token (reuse RefreshToken table for simplicity)
            var autoLoginToken = new RefreshToken
            {
                Token = $"auto_{token}",
                UserId = userId,
                ExpiryDate = DateTime.UtcNow.AddDays(30), // 30 days for auto-login
                IsRevoked = false
            };

            // Revoke any existing auto-login tokens for this user
            await RevokeAutoLoginTokenAsync(userId);

            await _uow.RefreshTokens.AddAsync(autoLoginToken);
            await _uow.SaveChangesAsync();

            return autoLoginToken.Token;
        }

        public async Task<AutoLoginResponseDto> AutoLoginFromTokenAsync(string autoLoginToken)
        {
            if (string.IsNullOrEmpty(autoLoginToken) || !autoLoginToken.StartsWith("auto_"))
                throw new InvalidOperationException("Invalid auto-login token.");

            var token = await _uow.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == autoLoginToken && !rt.IsRevoked);

            if (token == null || token.ExpiryDate < DateTime.UtcNow)
                throw new InvalidOperationException("Auto-login token expired or invalid.");

            var user = await _uow.Users.GetByIdAsync(token.UserId);
            if (user == null || user.IsDeleted || !user.IsActive)
                throw new InvalidOperationException("User account is not available.");

            // Check if user is verified
            var lastVerification = (await _uow.AccountVerifications
                .FindAsync(av => av.UserId == user.UserId))
                .OrderByDescending(av => av.Date)
                .FirstOrDefault();

            if (lastVerification == null || !lastVerification.CheckedOK)
                throw new InvalidOperationException("Account is not verified.");

            // Generate new JWT
            var jwt = AuthHelpers.GenerateAccessToken(
                user.UserId.ToString(),
                user.EmailAddress,
                user.FullName,
                user.Role,
                _config,
                user.ProfilePhoto!, 
                TimeSpan.FromHours(3) // Standard 3-hour token
            );

            return new AutoLoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                Expiration = jwt.ValidTo,
                Role = user.Role.ToString()
            };
        }

        public async Task RevokeAutoLoginTokenAsync(int userId)
        {
            var existingTokens = await _uow.RefreshTokens
                .FindAsync(rt => rt.UserId == userId && rt.Token.StartsWith("auto_") && !rt.IsRevoked);

            foreach (var token in existingTokens)
            {
                token.IsRevoked = true;
                _uow.RefreshTokens.Update(token);
            }

            await _uow.SaveChangesAsync();
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _uow.RefreshTokens
                .FindAsync(rt => rt.Token.StartsWith("auto_") && rt.ExpiryDate < DateTime.UtcNow);

            foreach (var token in expiredTokens)
            {
                _uow.RefreshTokens.Remove(token);
            }

            await _uow.SaveChangesAsync();
        }
    }
}