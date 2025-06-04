using AutoMapper;
using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Enums;
using LearnQuestV1.Api.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

        public AccountService(
            IUnitOfWork uow,
            IEmailQueueService emailQueueService,
            IFailedLoginTracker failedLoginTracker,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration config,
            IMapper mapper)
        {
            _uow = uow;
            _emailQueueService = emailQueueService;
            _failedLoginTracker = failedLoginTracker;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _mapper = mapper;
        }

        /// <summary>
        /// Registers a new user or resends a verification code if the user already exists but is not yet verified.
        /// Throws InvalidOperationException if the user is already verified.
        /// </summary>
        public async Task SignupAsync(SignupRequestDto input)
        {
            // 1. Check if user already exists by email
            var existingUser = (await _uow.Users.FindAsync(u => u.EmailAddress == input.EmailAddress))
                               .FirstOrDefault();

            if (existingUser != null)
            {
                // Find the most recent verification record
                var lastVerification = existingUser.AccountVerifications
                    .OrderByDescending(av => av.Date)
                    .FirstOrDefault();

                // If already verified, we cannot register again
                if (lastVerification != null && lastVerification.CheckedOK)
                    throw new InvalidOperationException("User already exists and is verified.");

                if (lastVerification != null)
                {
                    // If it has been less than 30 minutes since last code, reject
                    var elapsed = DateTime.UtcNow - lastVerification.Date;
                    if (elapsed.TotalMinutes < 30)
                    {
                        var waitMinutes = 30 - (int)elapsed.TotalMinutes;
                        throw new InvalidOperationException(
                            $"A verification code was already sent. Please wait {waitMinutes} minute(s)."
                        );
                    }

                    // Generate and save a new code
                    lastVerification.Code = AuthHelpers.GenerateVerificationCode();
                    lastVerification.Date = DateTime.UtcNow;
                    _uow.AccountVerifications.Update(lastVerification);
                }
                else
                {
                    // Create the first verification record
                    var newVerification = new AccountVerification
                    {
                        UserId = existingUser.UserId,
                        Code = AuthHelpers.GenerateVerificationCode(),
                        CheckedOK = false,
                        Date = DateTime.UtcNow
                    };
                    await _uow.AccountVerifications.AddAsync(newVerification);
                }

                await _uow.SaveAsync();

                // Queue an email with the (new) verification code
                var codeToSend = existingUser.AccountVerifications
                    .OrderByDescending(av => av.Date)
                    .First().Code;
                _emailQueueService.QueueResendEmail(
                    existingUser.EmailAddress,
                    existingUser.FullName,
                    codeToSend
                );

                throw new InvalidOperationException("User already exists. Please verify your email.");
            }

            // 2. Create a new user
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
            await _uow.SaveAsync();

            // 3. Create the first verification record
            var verificationCode = AuthHelpers.GenerateVerificationCode();
            var accountVerification = new AccountVerification
            {
                UserId = newUser.UserId,
                Code = verificationCode,
                CheckedOK = false,
                Date = DateTime.UtcNow
            };
            await _uow.AccountVerifications.AddAsync(accountVerification);
            await _uow.SaveAsync();

            // 4. Queue the email with the verification code
            _emailQueueService.QueueEmail(newUser.EmailAddress, newUser.FullName, verificationCode);

            // 5. Set a cookie to track which email is awaiting verification
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(100)
            };
            _httpContextAccessor.HttpContext!
                .Response
                .Cookies
                .Append("EmailForVerification", newUser.EmailAddress, cookieOptions);
        }

        /// <summary>
        /// Verifies a user's account using a code stored in a cookie.
        /// Throws InvalidOperationException if cookie is missing or code is invalid/expired.
        /// </summary>
        public async Task VerifyAccountAsync(VerifyAccountRequestDto input)
        {
            var httpCtx = _httpContextAccessor.HttpContext
                          ?? throw new InvalidOperationException("HTTP context is unavailable.");

            // 1) Read "EmailForVerification" cookie
            if (!httpCtx.Request.Cookies.TryGetValue("EmailForVerification", out var email))
                throw new InvalidOperationException("Verification email not found. Please register again.");

            // 2) Find the user by email
            var user = (await _uow.Users.FindAsync(u => u.EmailAddress == email))
                        .FirstOrDefault()
                       ?? throw new InvalidOperationException("User not found.");

            // 3) Fetch all verification records for this user
            var verifications = await _uow.AccountVerifications.FindAsync(av => av.UserId == user.UserId);
            var lastVerif = verifications
                            .OrderByDescending(av => av.Date)
                            .FirstOrDefault()
                          ?? throw new InvalidOperationException("Verification details missing.");

            // 4) Check code match
            if (!string.Equals(lastVerif.Code, input.VerificationCode, StringComparison.Ordinal))
                throw new InvalidOperationException("Invalid verification code.");

            // 5) Check expiration (30 minutes)
            if (lastVerif.Date.AddMinutes(30) < DateTime.UtcNow)
                throw new InvalidOperationException("Verification code expired.");

            // 6) Activate account and mark verification as successful
            user.IsActive = true;
            lastVerif.CheckedOK = true;
            _uow.Users.Update(user);
            _uow.AccountVerifications.Update(lastVerif);

            await _uow.SaveAsync();

            // 7) Delete the cookie
            httpCtx.Response.Cookies.Delete("EmailForVerification");
        }

        /// <summary>
        /// Resends a fresh verification code if at least 2 minutes have passed since last send.
        /// Throws InvalidOperationException if cookie is missing or user not found.
        /// </summary>
        public async Task ResendVerificationCodeAsync()
        {
            var httpCtx = _httpContextAccessor.HttpContext!;
            if (!httpCtx.Request.Cookies.TryGetValue("EmailForVerification", out var email))
                throw new InvalidOperationException("Verification email not found.");

            var user = (await _uow.Users.FindAsync(u => u.EmailAddress == email))
                       .FirstOrDefault()
                      ?? throw new InvalidOperationException("User not found.");

            var lastVerif = user.AccountVerifications.OrderByDescending(av => av.Date).FirstOrDefault();
            if (lastVerif == null)
                throw new InvalidOperationException("No verification details to resend.");

            var elapsed = DateTime.UtcNow - lastVerif.Date;
            if (elapsed.TotalMinutes < 2)
                throw new InvalidOperationException("Please wait at least 2 minutes before resending.");

            lastVerif.Code = AuthHelpers.GenerateVerificationCode();
            lastVerif.Date = DateTime.UtcNow;
            _uow.AccountVerifications.Update(lastVerif);
            await _uow.SaveAsync();

            _emailQueueService.QueueResendEmail(user.EmailAddress, user.FullName, lastVerif.Code);
        }

        /// <summary>
        /// Authenticates a user and issues a JWT + refresh token.
        /// Throws InvalidOperationException on invalid credentials, account status, or lockout.
        /// </summary>
        public async Task<SigninResponseDto> SigninAsync(SigninRequestDto input)
        {
            var email = input.Email;
            var failedMap = _failedLoginTracker.GetFailedAttempts();
            if (failedMap.TryGetValue(email, out var data) && data.LockoutEnd > DateTime.UtcNow)
            {
                var remaining = data.LockoutEnd - DateTime.UtcNow;
                throw new InvalidOperationException($"Too many failed attempts. Try again after {remaining:mm\\:ss}.");
            }

            var user = (await _uow.Users.FindAsync(u => u.EmailAddress == email))
                       .FirstOrDefault();

            if (user == null || !AuthHelpers.VerifyPassword(input.Password, user.PasswordHash))
            {
                _failedLoginTracker.RecordFailedAttempt(email);
                if (failedMap.TryGetValue(email, out var attemptData) && attemptData.Attempts >= 5)
                {
                    _failedLoginTracker.LockUser(email);
                    throw new InvalidOperationException("Too many failed login attempts. Locked for 15 minutes.");
                }
                throw new InvalidOperationException("Invalid login credentials.");
            }

            // Check latest verification record
            var lastVerif = user.AccountVerifications.OrderByDescending(av => av.Date).FirstOrDefault();
            if (lastVerif != null && !lastVerif.CheckedOK)
            {
                var newCode = AuthHelpers.GenerateVerificationCode();
                lastVerif.Code = newCode;
                lastVerif.Date = DateTime.UtcNow;
                _uow.AccountVerifications.Update(lastVerif);
                await _uow.SaveAsync();
                _emailQueueService.QueueResendEmail(user.EmailAddress, user.FullName, newCode);

                throw new InvalidOperationException("Your account is not verified. A new code has been sent.");
            }

            if (user.IsDeleted)
                throw new InvalidOperationException("This account has been deleted. Contact support.");

            if (!user.IsActive)
                throw new InvalidOperationException("Your account is not activated.");

            _failedLoginTracker.ResetFailedAttempts(email);

            // Log user visit
            var visit = new UserVisitHistory
            {
                UserId = user.UserId,
                LastVisit = DateTime.UtcNow
            };
            await _uow.UserVisitHistories.AddAsync(visit);
            await _uow.SaveAsync();

            var tokenDuration = input.RememberMe ? TimeSpan.FromDays(30) : TimeSpan.FromHours(3);
            var jwt = AuthHelpers.GenerateAccessToken(
                user.UserId.ToString(),
                user.EmailAddress,
                user.FullName,
                user.Role,
                _config,
                tokenDuration
            );

            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                UserId = user.UserId
            };
            await _uow.RefreshTokens.AddAsync(refreshToken);
            await _uow.SaveAsync();

            if (input.RememberMe)
            {
                var cookieOpts = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(30)
                };
                var httpCtx = _httpContextAccessor.HttpContext!;
                httpCtx.Response.Cookies.Append("UserEmail", user.EmailAddress, cookieOpts);
                httpCtx.Response.Cookies.Append("UserPassword", input.Password, cookieOpts);
            }

            return new SigninResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                Expiration = jwt.ValidTo,
                Role = user.Role.ToString(),
                RefreshToken = refreshToken.Token
            };
        }

        /// <summary>
        /// Exchanges an old refresh token for a new JWT + new refresh token.
        /// Throws InvalidOperationException if token is invalid or expired.
        /// </summary>
        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto input)
        {
            var rt = (await _uow.RefreshTokens.FindAsync(r => r.Token == input.OldRefreshToken && !r.IsRevoked))
                     .FirstOrDefault();

            if (rt == null || rt.ExpiryDate < DateTime.UtcNow)
                throw new InvalidOperationException("Invalid or expired refresh token.");

            rt.IsRevoked = true;
            _uow.RefreshTokens.Update(rt);

            var user = await _uow.Users.GetByIdAsync(rt.UserId);
            var newJwt = AuthHelpers.GenerateAccessToken(
                user.UserId.ToString(),
                user.EmailAddress,
                user.FullName,
                user.Role,
                _config
            );

            var newRefresh = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                UserId = user.UserId
            };
            await _uow.RefreshTokens.AddAsync(newRefresh);
            await _uow.SaveAsync();

            return new RefreshTokenResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(newJwt),
                Expiration = newJwt.ValidTo,
                RefreshToken = newRefresh.Token
            };
        }

        /// <summary>
        /// Automatically logs in a user from cookies containing email/password.
        /// Throws InvalidOperationException if credentials or account status is invalid.
        /// </summary>
        public async Task<AutoLoginResponseDto> AutoLoginAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new InvalidOperationException("Auto-login failed: missing cookies.");

            var user = (await _uow.Users.FindAsync(u => u.EmailAddress == email && !u.IsDeleted))
                       .FirstOrDefault();

            if (user == null || !AuthHelpers.VerifyPassword(password, user.PasswordHash))
                throw new InvalidOperationException("Invalid credentials.");

            var lastVerif = user.AccountVerifications.OrderByDescending(av => av.Date).FirstOrDefault();
            if (lastVerif != null && !lastVerif.CheckedOK)
                throw new InvalidOperationException("Your account is not verified.");

            if (user.IsDeleted)
                throw new InvalidOperationException("This account has been deleted.");

            if (!user.IsActive)
                throw new InvalidOperationException("Your account is not activated.");

            var jwt = AuthHelpers.GenerateAccessToken(
                user.UserId.ToString(),
                user.EmailAddress,
                user.FullName,
                user.Role,
                _config
            );

            return new AutoLoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                Expiration = jwt.ValidTo,
                Role = user.Role.ToString()
            };
        }

        /// <summary>
        /// Initiates a password reset by sending a code and link to the user's email.
        /// Throws InvalidOperationException if the email is not registered.
        /// </summary>
        public async Task ForgetPasswordAsync(ForgetPasswordRequestDto input)
        {
            var user = (await _uow.Users.FindAsync(u => u.EmailAddress == input.Email))
                       .FirstOrDefault();

            if (user == null)
                throw new InvalidOperationException("User does not exist with the provided email.");

            var code = AuthHelpers.GenerateVerificationCode();
            var resetLink = $"https://yourfrontend.com/reset-password?email={user.EmailAddress}&code={code}";

            _emailQueueService.QueueEmail(user.EmailAddress, user.FullName, code, resetLink);
        }

        /// <summary>
        /// Resets the user's password after verifying the provided code.
        /// Throws InvalidOperationException if code is invalid or expired.
        /// </summary>
        public async Task ResetPasswordAsync(ResetPasswordRequestDto input)
        {
            var user = (await _uow.Users.FindAsync(u => u.EmailAddress == input.Email))
                       .FirstOrDefault();

            if (user == null)
                throw new InvalidOperationException("Invalid email or verification code.");

            var lastVerif = user.AccountVerifications.OrderByDescending(av => av.Date).FirstOrDefault();
            if (lastVerif == null)
                throw new InvalidOperationException("Verification details missing.");

            if (lastVerif.Code != input.Code || lastVerif.Date.AddMinutes(30) < DateTime.UtcNow)
                throw new InvalidOperationException("Invalid or expired verification code.");

            user.PasswordHash = AuthHelpers.HashPassword(input.NewPassword);
            _uow.Users.Update(user);
            await _uow.SaveAsync();
        }

        /// <summary>
        /// Logs out a user by blacklisting their access token.
        /// Throws InvalidOperationException if token format is invalid.
        /// </summary>
        public async Task LogoutAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new InvalidOperationException("Invalid token format.");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);

            if (jwtToken.ValidTo > DateTime.UtcNow)
            {
                var blacklisted = new BlacklistToken
                {
                    Token = accessToken,
                    ExpiryDate = jwtToken.ValidTo
                };
                await _uow.BlacklistTokens.AddAsync(blacklisted);
                await _uow.SaveAsync();
            }
        }

        
    }
}
