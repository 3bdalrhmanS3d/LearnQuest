using AutoMapper;
using LearnQuestV1.Api.Configuration;
using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Api.Utilities;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Models.UserManagement;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace LearnQuestV1.Api.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailQueueService _emailQueueService;
        private readonly IFailedLoginTracker _failedLoginTracker;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly SecuritySettings _securitySettings;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IUnitOfWork uow,
            IEmailQueueService emailQueueService,
            IFailedLoginTracker failedLoginTracker,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration config,
            IMapper mapper,
            IOptions<SecuritySettings> securitySettings,
            ILogger<AccountService> logger)
        {
            _uow = uow;
            _emailQueueService = emailQueueService;
            _failedLoginTracker = failedLoginTracker;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _mapper = mapper;
            _securitySettings = securitySettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user or resends a verification code if the user already exists but is not yet verified.
        /// Enhanced with proper transaction management and configuration-based timeouts.
        /// </summary>
        public async Task SignupAsync(SignupRequestDto input)
        {
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                // 1. Check if user already exists with proper includes
                var existingUser = await GetUserWithVerificationsAsync(input.EmailAddress);

                if (existingUser != null)
                {
                    await HandleExistingUserSignupAsync(existingUser);
                    await transaction.CommitAsync();
                    return;
                }

                // 2. Create new user with verification in transaction
                await CreateNewUserAsync(input);
                await transaction.CommitAsync();

                _logger.LogInformation("New user registered: {Email}", input.EmailAddress);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Verifies a user's account using a code and email (no cookie dependency).
        /// Enhanced with better error handling and security logging.
        /// </summary>
        public async Task VerifyAccountAsync(VerifyAccountRequestDto input)
        {
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                // 1. Find user with verifications using email from request
                var user = await GetUserWithVerificationsAsync(input.Email);
                if (user == null)
                    throw new InvalidOperationException("User not found or verification session invalid.");

                // 2. Validate verification code
                var lastVerification = user.AccountVerifications
                    .OrderByDescending(av => av.Date)
                    .FirstOrDefault();

                ValidateVerificationCode(lastVerification, input.VerificationCode);

                // 3. Activate account
                user.IsActive = true;
                lastVerification!.CheckedOK = true;

                _uow.Users.Update(user);
                _uow.AccountVerifications.Update(lastVerification);

                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();

                // 4. Send welcome email
                _emailQueueService.QueueWelcomeEmail(user.EmailAddress, user.FullName);

                _logger.LogInformation("Account verified successfully: {Email}", input.Email);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Verifies user account using verification token from email link
        /// </summary>
        /// 
        // By Swagger 
        // https://localhost:7217/api/Auth/verify-account/ + token 
        
        public async Task VerifyAccountByTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("Invalid verification token.");

            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                // Decode the token to get email and code
                var decodedData = AuthHelpers.DecodeVerificationToken(token);
                if (decodedData == null)
                    throw new InvalidOperationException("Invalid or expired verification token.");

                var user = await GetUserWithVerificationsAsync(decodedData.Email);
                if (user == null)
                    throw new InvalidOperationException("User not found.");

                var lastVerification = user.AccountVerifications
                    .OrderByDescending(av => av.Date)
                    .FirstOrDefault();

                ValidateVerificationCode(lastVerification, decodedData.Code);

                // Activate account
                user.IsActive = true;
                lastVerification!.CheckedOK = true;

                _uow.Users.Update(user);
                _uow.AccountVerifications.Update(lastVerification);

                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send welcome email
                _emailQueueService.QueueWelcomeEmail(user.EmailAddress, user.FullName);

                _logger.LogInformation("Account verified successfully via token: {Email}", decodedData.Email);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Resends verification code with configurable cooldown period.
        /// </summary>
        public async Task ResendVerificationCodeAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("Email is required.");

            var user = await GetUserWithVerificationsAsync(email);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            var lastVerification = user.AccountVerifications
                .OrderByDescending(av => av.Date)
                .FirstOrDefault();

            if (lastVerification == null)
                throw new InvalidOperationException("No verification record found.");

            // Check cooldown period from configuration
            var elapsed = DateTime.UtcNow - lastVerification.Date;
            var cooldownMinutes = _securitySettings.Verification.ResendCodeCooldownMinutes;

            if (elapsed.TotalMinutes < cooldownMinutes)
            {
                var waitMinutes = cooldownMinutes - (int)elapsed.TotalMinutes;
                throw new InvalidOperationException(
                    $"Please wait {waitMinutes} minute(s) before requesting a new code.");
            }

            // Generate new code
            lastVerification.Code = AuthHelpers.GenerateVerificationCode();
            lastVerification.Date = DateTime.UtcNow;

            _uow.AccountVerifications.Update(lastVerification);
            await _uow.SaveChangesAsync();

            // Generate verification token for email link
            var verificationToken = AuthHelpers.GenerateVerificationToken(user.EmailAddress, lastVerification.Code);
            var verificationLink = $"{_config["FrontendUrl"]}/verify-account?token={verificationToken}";

            _emailQueueService.QueueVerificationEmail(user.EmailAddress, user.FullName, lastVerification.Code, verificationLink);

            _logger.LogInformation("Verification code resent: {Email}", email);
        }

        /// <summary>
        /// Enhanced signin with better security checks and transaction management.
        /// </summary>
        public async Task<SigninResponseDto> SigninAsync(SigninRequestDto input)
        {
            var email = input.Email;

            // 1. Check lockout status
            await CheckAccountLockoutAsync(email);

            // 2. Find user with verifications
            var user = await GetUserWithVerificationsAsync(email);

            // 3. Validate credentials
            if (user == null || !AuthHelpers.VerifyPassword(input.Password, user.PasswordHash))
            {
                await HandleFailedLoginAsync(email);
                throw new InvalidOperationException("Invalid login credentials.");
            }

            // 4. Check account status
            await ValidateAccountStatusAsync(user);

            // 5. Reset failed attempts and create session
            _failedLoginTracker.ResetFailedAttempts(email);

            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                // 6. Log visit
                await LogUserVisitAsync(user.UserId);

                // 7. Generate tokens
                var (accessToken, refreshToken) = await GenerateTokensAsync(user);

                await transaction.CommitAsync();

                _logger.LogInformation("Successful login: {Email}", email);

                return new SigninResponseDto
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(accessToken),
                    Expiration = accessToken.ValidTo,
                    Role = user.Role.ToString(),
                    RefreshToken = refreshToken.Token,
                    UserId = user.UserId
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Enhanced refresh token with better validation.
        /// </summary>
        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto input)
        {
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                // 1. Validate and revoke old token
                var refreshToken = await ValidateAndRevokeRefreshTokenAsync(input.OldRefreshToken);

                // 2. Get user
                var user = await _uow.Users.GetByIdAsync(refreshToken.UserId);
                if (user == null || user.IsDeleted || !user.IsActive)
                    throw new InvalidOperationException("User account is not available.");

                // 3. Generate new tokens
                var (newAccessToken, newRefreshToken) = await GenerateTokensAsync(user);

                await transaction.CommitAsync();

                return new RefreshTokenResponseDto
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                    Expiration = newAccessToken.ValidTo,
                    RefreshToken = newRefreshToken.Token,
                    UserId = user.UserId
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Enhanced password reset request with better error handling.
        /// </summary>
        public async Task ForgetPasswordAsync(ForgetPasswordRequestDto input)
        {
            // Always process the request to prevent email enumeration
            var user = await GetUserWithVerificationsAsync(input.Email);

            if (user != null && user.IsActive && !user.IsDeleted)
            {
                // Generate and store verification code
                var code = AuthHelpers.GenerateVerificationCode();
                var verification = new AccountVerification
                {
                    UserId = user.UserId,
                    Code = code,
                    CheckedOK = false,
                    Date = DateTime.UtcNow
                };

                await _uow.AccountVerifications.AddAsync(verification);
                await _uow.SaveChangesAsync();

                // Send reset email
                var resetLink = $"{_config["FrontendUrl"]}/reset-password?email={user.EmailAddress}&code={code}";
                _emailQueueService.QueuePasswordResetEmail(user.EmailAddress, user.FullName, code, resetLink);

                _logger.LogInformation("Password reset requested: {Email}", input.Email);
            }
            else
            {
                _logger.LogWarning("Password reset requested for non-existent user: {Email}", input.Email);
            }

            // Always return success for security
        }

        /// <summary>
        /// Enhanced password reset with better validation.
        /// </summary>
        public async Task ResetPasswordAsync(ResetPasswordRequestDto input)
        {
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                var user = await GetUserWithVerificationsAsync(input.Email);
                if (user == null)
                    throw new InvalidOperationException("Invalid reset request.");

                var verification = user.AccountVerifications
                    .OrderByDescending(av => av.Date)
                    .FirstOrDefault();

                ValidateVerificationCode(verification, input.Code);

                // Update password
                user.PasswordHash = AuthHelpers.HashPassword(input.NewPassword);
                verification!.CheckedOK = true; // Mark as used

                _uow.Users.Update(user);
                _uow.AccountVerifications.Update(verification);

                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send password changed notification
                _emailQueueService.QueuePasswordChangedEmail(user.EmailAddress, user.FullName);

                _logger.LogInformation("Password reset successful: {Email}", input.Email);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Enhanced logout with better token validation.
        /// </summary>
        public async Task LogoutAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new InvalidOperationException("Invalid token format.");

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(accessToken);

                // Only blacklist if token is still valid
                if (jwtToken.ValidTo > DateTime.UtcNow)
                {
                    var blacklisted = new BlacklistToken
                    {
                        Token = accessToken,
                        ExpiryDate = jwtToken.ValidTo
                    };

                    await _uow.BlacklistTokens.AddAsync(blacklisted);
                    await _uow.SaveChangesAsync();
                }

                _logger.LogInformation("User logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout process");
                throw new InvalidOperationException("Logout failed.");
            }
        }

        #region Private Helper Methods

        private async Task<User?> GetUserWithVerificationsAsync(string email)
        {
            var users = await _uow.Users.FindAsync(u => u.EmailAddress == email);
            var user = users.FirstOrDefault();

            if (user != null)
            {
                // Load account verifications separately to avoid N+1 query
                var verifications = await _uow.AccountVerifications.FindAsync(av => av.UserId == user.UserId);
                user.AccountVerifications = verifications.ToList();
            }

            return user;
        }

        private async Task HandleExistingUserSignupAsync(User existingUser)
        {
            var lastVerification = existingUser.AccountVerifications
                .OrderByDescending(av => av.Date)
                .FirstOrDefault();

            if (lastVerification?.CheckedOK == true)
                throw new InvalidOperationException("User already exists and is verified.");

            if (lastVerification != null)
            {
                var elapsed = DateTime.UtcNow - lastVerification.Date;
                var cooldownMinutes = _securitySettings.Verification.ResendCodeCooldownMinutes;

                if (elapsed.TotalMinutes < cooldownMinutes)
                {
                    var waitMinutes = cooldownMinutes - (int)elapsed.TotalMinutes;
                    throw new InvalidOperationException(
                        $"A verification code was already sent. Please wait {waitMinutes} minute(s).");
                }

                // Generate new code
                lastVerification.Code = AuthHelpers.GenerateVerificationCode();
                lastVerification.Date = DateTime.UtcNow;
                _uow.AccountVerifications.Update(lastVerification);
            }
            else
            {
                // Create first verification
                var newVerification = new AccountVerification
                {
                    UserId = existingUser.UserId,
                    Code = AuthHelpers.GenerateVerificationCode(),
                    CheckedOK = false,
                    Date = DateTime.UtcNow
                };
                await _uow.AccountVerifications.AddAsync(newVerification);
            }

            await _uow.SaveChangesAsync();

            // Send email with both code and link
            var codeToSend = existingUser.AccountVerifications
                .OrderByDescending(av => av.Date)
                .First().Code;

            var verificationToken = AuthHelpers.GenerateVerificationToken(existingUser.EmailAddress, codeToSend);
            var verificationLink = $"{_config["FrontendUrl"]}/verify-account?token={verificationToken}";

            _emailQueueService.QueueVerificationEmail(existingUser.EmailAddress, existingUser.FullName, codeToSend, verificationLink);

            throw new InvalidOperationException("User already exists. Please verify your email.");
        }

        private async Task CreateNewUserAsync(SignupRequestDto input)
        {
            // Create user
            var newUser = new User
            {
                FullName = $"{input.FirstName} {input.LastName}",
                EmailAddress = input.EmailAddress,
                PasswordHash = AuthHelpers.HashPassword(input.Password),
                CreatedAt = DateTime.UtcNow,
                IsSystemProtected = false,
                IsActive = false,
                IsDeleted = false,
                Role = UserRole.RegularUser,
                ProfilePhoto = "/uploads/profile-pictures/default_user.webp"
            };

            await _uow.Users.AddAsync(newUser);
            await _uow.SaveChangesAsync();

            // Create verification
            var verificationCode = AuthHelpers.GenerateVerificationCode();
            var accountVerification = new AccountVerification
            {
                UserId = newUser.UserId,
                Code = verificationCode,
                CheckedOK = false,
                Date = DateTime.UtcNow
            };

            await _uow.AccountVerifications.AddAsync(accountVerification);
            await _uow.SaveChangesAsync();

            // Generate verification token for email link
            var verificationToken = AuthHelpers.GenerateVerificationToken(newUser.EmailAddress, verificationCode);
            var verificationLink = $"{_config["FrontendUrl"]}/verify-account?token={verificationToken}";

            // Queue email with both code and link
            _emailQueueService.QueueVerificationEmail(newUser.EmailAddress, newUser.FullName, verificationCode, verificationLink);
        }

        private void ValidateVerificationCode(AccountVerification? verification, string inputCode)
        {
            if (verification == null)
                throw new InvalidOperationException("Verification details missing.");

            if (!string.Equals(verification.Code, inputCode, StringComparison.Ordinal))
                throw new InvalidOperationException("Invalid verification code.");

            var expiryMinutes = _securitySettings.Verification.CodeExpiryMinutes;
            if (verification.Date.AddMinutes(expiryMinutes) < DateTime.UtcNow)
                throw new InvalidOperationException("Verification code expired.");
        }

        private async Task CheckAccountLockoutAsync(string email)
        {
            var failedMap = _failedLoginTracker.GetFailedAttempts();
            if (failedMap.TryGetValue(email, out var data) && data.LockoutEnd > DateTime.UtcNow)
            {
                var remaining = data.LockoutEnd - DateTime.UtcNow;
                throw new InvalidOperationException($"Account locked. Try again after {remaining:mm\\:ss}.");
            }
        }

        private async Task HandleFailedLoginAsync(string email)
        {
            _failedLoginTracker.RecordFailedAttempt(email);
            var failedMap = _failedLoginTracker.GetFailedAttempts();

            if (failedMap.TryGetValue(email, out var attemptData) &&
                attemptData.Attempts >= _securitySettings.Lockout.MaxFailedAttempts)
            {
                _failedLoginTracker.LockUser(email);
                throw new InvalidOperationException("Too many failed login attempts. Account locked.");
            }
        }

        private async Task ValidateAccountStatusAsync(User user)
        {
            var lastVerification = user.AccountVerifications
                .OrderByDescending(av => av.Date)
                .FirstOrDefault();

            if (lastVerification?.CheckedOK != true)
            {
                var newCode = AuthHelpers.GenerateVerificationCode();
                lastVerification!.Code = newCode;
                lastVerification.Date = DateTime.UtcNow;

                _uow.AccountVerifications.Update(lastVerification);
                await _uow.SaveChangesAsync();

                var verificationToken = AuthHelpers.GenerateVerificationToken(user.EmailAddress, newCode);
                var verificationLink = $"{_config["FrontendUrl"]}/verify-account?token={verificationToken}";

                _emailQueueService.QueueVerificationEmail(user.EmailAddress, user.FullName, newCode, verificationLink);

                throw new InvalidOperationException("Your account is not verified. A new code has been sent.");
            }

            if (user.IsDeleted)
                throw new InvalidOperationException("This account has been deleted. Contact support.");

            if (!user.IsActive)
                throw new InvalidOperationException("Your account is not activated.");
        }

        private async Task LogUserVisitAsync(int userId)
        {
            var visit = new UserVisitHistory
            {
                UserId = userId,
                LastVisit = DateTime.UtcNow
            };
            await _uow.UserVisitHistories.AddAsync(visit);
            await _uow.SaveChangesAsync();
        }

        private async Task<(JwtSecurityToken accessToken, RefreshToken refreshToken)> GenerateTokensAsync(User user)
        {
            var accessToken = AuthHelpers.GenerateAccessToken(
                user.UserId.ToString(),
                user.EmailAddress,
                user.FullName,
                user.Role,
                _config,
                user.ProfilePhoto!,
                TimeSpan.FromHours(_securitySettings.Token.AccessTokenExpiryHours)
            );

            var refreshToken = new RefreshToken
            {
                Token = AuthHelpers.GenerateSecureToken(),
                ExpiryDate = DateTime.UtcNow.AddDays(_securitySettings.Token.RefreshTokenExpiryDays),
                UserId = user.UserId
            };

            await _uow.RefreshTokens.AddAsync(refreshToken);
            await _uow.SaveChangesAsync();

            return (accessToken, refreshToken);
        }

        private async Task<RefreshToken> ValidateAndRevokeRefreshTokenAsync(string tokenString)
        {
            var refreshToken = (await _uow.RefreshTokens.FindAsync(r => r.Token == tokenString && !r.IsRevoked))
                .FirstOrDefault();

            if (refreshToken == null || refreshToken.ExpiryDate < DateTime.UtcNow)
                throw new InvalidOperationException("Invalid or expired refresh token.");

            refreshToken.IsRevoked = true;
            _uow.RefreshTokens.Update(refreshToken);
            await _uow.SaveChangesAsync();

            return refreshToken;
        }

        #endregion
    }
}