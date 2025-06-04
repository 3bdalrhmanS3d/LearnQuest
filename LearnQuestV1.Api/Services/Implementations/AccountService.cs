using AutoMapper;
using LearnQuestV1.Api.DTOs.Users.Request;
using LearnQuestV1.Api.DTOs.Users.Response;
using LearnQuestV1.Api.Services.Interfaces;
using LearnQuestV1.Core.Interfaces;
using LearnQuestV1.Core.Models;
using LearnQuestV1.Core.Enums;
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

        public async Task SignupAsync(SignupRequestDto input)
        {
            // 1. تأكد إنّ المستخدم غير موجود
            var usersFound = await _uow.Users.FindAsync(u => u.EmailAddress == input.EmailAddress);
            var existingUser = usersFound.FirstOrDefault();

            if (existingUser != null)
            {
                // إذا لديه على الأقل سجل واحد للتحقق:
                var lastVerification = existingUser.AccountVerifications
                    .OrderByDescending(av => av.Date)
                    .FirstOrDefault();

                if (lastVerification != null && lastVerification.CheckedOK)
                {
                    throw new InvalidOperationException("User already exists and is verified.");
                }

                if (lastVerification != null)
                {
                    var timeSince = DateTime.UtcNow - lastVerification.Date;
                    if (timeSince.TotalMinutes < 30)
                        throw new InvalidOperationException(
                            $"A verification code was already sent. Wait {30 - (int)timeSince.TotalMinutes} minutes."
                        );

                    // إنشاء رمز جديد
                    var newCode = GenerateVerificationCode();
                    lastVerification.Code = newCode;
                    lastVerification.Date = DateTime.UtcNow;

                    _uow.AccountVerifications.Update(lastVerification);
                }
                else
                {
                    // لم يوجد أيّ سجلّ تحقق سابق
                    var newVerif = new AccountVerification
                    {
                        UserId = existingUser.UserId,
                        Code = GenerateVerificationCode(),
                        CheckedOK = false,
                        Date = DateTime.UtcNow
                    };
                    await _uow.AccountVerifications.AddAsync(newVerif);
                }

                await _uow.SaveAsync();
                _emailQueueService.QueueResendEmail(
                    existingUser.EmailAddress,
                    existingUser.FullName,
                    existingUser.AccountVerifications.OrderByDescending(av => av.Date).First().Code
                );
                throw new InvalidOperationException("User already exists. Please verify your email.");
            }

            // 2. إنشاء مستخدم جديد
            var newUser = new User
            {
                FullName = $"{input.FirstName} {input.LastName}",
                EmailAddress = input.EmailAddress,
                PasswordHash = HashPassword(input.Password),
                CreatedAt = DateTime.UtcNow,
                IsSystemProtected = false,
                IsActive = false,
                IsDeleted = false,
                Role = UserRole.RegularUser,
                ProfilePhoto = "/uploads/profile-pictures/default_user.webp"
            };

            await _uow.Users.AddAsync(newUser);
            await _uow.SaveAsync();

            // 3. إنشاء أول سجلّ تحقق للمستخدم الجديد
            var verificationCode = GenerateVerificationCode();
            var accountVerification = new AccountVerification
            {
                UserId = newUser.UserId,
                Code = verificationCode,
                CheckedOK = false,
                Date = DateTime.UtcNow
            };
            await _uow.AccountVerifications.AddAsync(accountVerification);
            await _uow.SaveAsync();

            // 4. أضف رسالة إلى قائمة الانتظار لإرسال الإيميل
            _emailQueueService.QueueEmail(newUser.EmailAddress, newUser.FullName, verificationCode);

            // 5. ضع الكوكِي في الرد
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(100)
            };
            _httpContextAccessor.HttpContext!.Response.Cookies
                .Append("EmailForVerification", newUser.EmailAddress, cookieOptions);
        }

        public async Task VerifyAccountAsync(VerifyAccountRequestDto input)
        {
            // 1) Get HttpContext
            var httpCtx = _httpContextAccessor.HttpContext
                          ?? throw new InvalidOperationException("HTTP context is unavailable.");

            // 2) Read "EmailForVerification" cookie
            if (!httpCtx.Request.Cookies.TryGetValue("EmailForVerification", out var email))
                throw new InvalidOperationException("Verification email not found. Please register again.");

            // 3) Find the user by email
            var usersFound = await _uow.Users.FindAsync(u => u.EmailAddress == email);
            var user = usersFound.FirstOrDefault() ?? throw new InvalidOperationException("User not found.");

            // 4) Fetch all verification records for this user
            var verifications = await _uow.AccountVerifications.FindAsync(av => av.UserId == user.UserId);
            //    (IBaseRepo<AccountVerification>.FindAsync returns IEnumerable<AccountVerification>)
            var lastVerif = verifications
                            .OrderByDescending(av => av.Date)
                            .FirstOrDefault() ?? throw new InvalidOperationException("Verification details missing.");

            // 5) Check if codes match
            if (!string.Equals(lastVerif.Code, input.VerificationCode, StringComparison.Ordinal))
                throw new InvalidOperationException("Invalid verification code.");

            // 6) Check expiration (30 minutes)
            if (lastVerif.Date.AddMinutes(30) < DateTime.UtcNow)
                throw new InvalidOperationException("Verification code expired.");

            // 7) Activate account and mark verification as successful
            user.IsActive = true;
            lastVerif.CheckedOK = true;

            // 8) Update user and verification record
            _uow.Users.Update(user);
            _uow.AccountVerifications.Update(lastVerif);

            // 9) Commit to database
            await _uow.SaveAsync();

            // 10) Delete the cookie
            httpCtx.Response.Cookies.Delete("EmailForVerification");
        }

        public async Task ResendVerificationCodeAsync()
        {
            var httpCtx = _httpContextAccessor.HttpContext!;
            if (!httpCtx.Request.Cookies.TryGetValue("EmailForVerification", out var email))
                throw new InvalidOperationException("Verification email not found.");

            var user = (await _uow.Users.FindAsync(u => u.EmailAddress == email)).FirstOrDefault();
            if (user == null)
                throw new InvalidOperationException("User not found.");

            var lastVerif = user.AccountVerifications.OrderByDescending(av => av.Date).FirstOrDefault();
            if (lastVerif == null)
                throw new InvalidOperationException("No verification details to resend.");

            var since = DateTime.UtcNow - lastVerif.Date;
            if (since.TotalMinutes < 2)
                throw new InvalidOperationException("Please wait at least 2 minutes before resending.");

            var newCode = GenerateVerificationCode();
            lastVerif.Code = newCode;
            lastVerif.Date = DateTime.UtcNow;

            _uow.AccountVerifications.Update(lastVerif);
            await _uow.SaveAsync();

            _emailQueueService.QueueResendEmail(user.EmailAddress, user.FullName, newCode);
        }

        public async Task<SigninResponseDto> SigninAsync(SigninRequestDto input)
        {
            var email = input.Email;
            var failedMap = _failedLoginTracker.GetFailedAttempts();
            if (failedMap.TryGetValue(email, out var data) && data.LockoutEnd > DateTime.UtcNow)
            {
                var remaining = data.LockoutEnd - DateTime.UtcNow;
                throw new InvalidOperationException($"Too many failed attempts. Try again after {remaining:mm\\:ss}.");
            }

            var user = (await _uow.Users.FindAsync(u => u.EmailAddress == email)).FirstOrDefault();
            if (user == null || !VerifyPassword(input.Password, user.PasswordHash))
            {
                _failedLoginTracker.RecordFailedAttempt(email);
                if (failedMap.TryGetValue(email, out var attemptData) && attemptData.Attempts >= 5)
                {
                    _failedLoginTracker.LockUser(email);
                    throw new InvalidOperationException("Too many failed login attempts. Locked for 15 minutes.");
                }
                throw new InvalidOperationException("Invalid login credentials.");
            }

            // تأكد من وجود سجلّ تحقق:
            var lastVerif = user.AccountVerifications.OrderByDescending(av => av.Date).FirstOrDefault();
            if (lastVerif != null && !lastVerif.CheckedOK)
            {
                var newCode = GenerateVerificationCode();
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

            // سجلّ زيارة المستخدم
            var visit = new UserVisitHistory
            {
                UserId = user.UserId,
                LastVisit = DateTime.UtcNow
            };
            await _uow.UserVisitHistories.AddAsync(visit);
            await _uow.SaveAsync();

            var tokenDuration = input.RememberMe ? TimeSpan.FromDays(30) : TimeSpan.FromHours(3);
            var jwt = GenerateAccessToken(
                user.UserId.ToString(),
                user.EmailAddress,
                user.FullName,
                user.Role,
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

        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto input)
        {
            var rt = (await _uow.RefreshTokens.FindAsync(r => r.Token == input.OldRefreshToken && !r.IsRevoked))
                     .FirstOrDefault();
            if (rt == null || rt.ExpiryDate < DateTime.UtcNow)
                throw new InvalidOperationException("Invalid or expired refresh token.");

            rt.IsRevoked = true;
            _uow.RefreshTokens.Update(rt);

            var user = await _uow.Users.GetByIdAsync(rt.UserId);
            var newJwt = GenerateAccessToken(
                user.UserId.ToString(),
                user.EmailAddress,
                user.FullName,
                user.Role
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

        public async Task<AutoLoginResponseDto> AutoLoginAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new InvalidOperationException("Auto-login failed: missing cookies.");

            var user = (await _uow.Users.FindAsync(u => u.EmailAddress == email && !u.IsDeleted))
                       .FirstOrDefault();
            if (user == null || !VerifyPassword(password, user.PasswordHash))
                throw new InvalidOperationException("Invalid credentials.");

            var lastVerif = user.AccountVerifications.OrderByDescending(av => av.Date).FirstOrDefault();
            if (lastVerif != null && !lastVerif.CheckedOK)
                throw new InvalidOperationException("Your account is not verified.");
            if (user.IsDeleted)
                throw new InvalidOperationException("This account has been deleted.");
            if (!user.IsActive)
                throw new InvalidOperationException("Your account is not activated.");

            var jwt = GenerateAccessToken(
                user.UserId.ToString(),
                user.EmailAddress,
                user.FullName,
                user.Role
            );
            return new AutoLoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                Expiration = jwt.ValidTo,
                Role = user.Role.ToString()
            };
        }

        public async Task ForgetPasswordAsync(ForgetPasswordRequestDto input)
        {
            var user = (await _uow.Users.FindAsync(u => u.EmailAddress == input.Email)).FirstOrDefault();
            if (user == null)
                throw new InvalidOperationException("User does not exist with the provided email.");

            var code = GenerateVerificationCode();
            var resetLink = $"https://yourfrontend.com/reset-password?email={user.EmailAddress}&code={code}";

            _emailQueueService.QueueEmail(user.EmailAddress, user.FullName, code, resetLink);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto input)
        {
            var user = (await _uow.Users.FindAsync(u => u.EmailAddress == input.Email)).FirstOrDefault();
            if (user == null)
                throw new InvalidOperationException("Invalid email or verification code.");

            var lastVerif = user.AccountVerifications.OrderByDescending(av => av.Date).FirstOrDefault();
            if (lastVerif == null)
                throw new InvalidOperationException("Verification details missing.");

            if (lastVerif.Code != input.Code || lastVerif.Date.AddMinutes(30) < DateTime.UtcNow)
                throw new InvalidOperationException("Invalid or expired verification code.");

            user.PasswordHash = HashPassword(input.NewPassword);
            _uow.Users.Update(user);
            await _uow.SaveAsync();
        }

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

        #region Helpers

        private JwtSecurityToken GenerateAccessToken(
            string userId, string email, string fullName, UserRole role, TimeSpan? customExpiry = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Role, role.ToString())
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JWT:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JWT:ValidIss"],
                audience: _config["JWT:ValidAud"],
                claims: claims,
                expires: DateTime.UtcNow.Add(customExpiry ?? TimeSpan.FromHours(1)),
                signingCredentials: creds
            );
            return token;
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash) || !storedHash.Contains(":"))
                return false;
            var parts = storedHash.Split(':');
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[1]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(storedHashBytes.Length);

            return CryptographicOperations.FixedTimeEquals(hash, storedHashBytes);
        }

        private string GenerateVerificationCode()
        {
            byte[] bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            int code = BitConverter.ToInt32(bytes, 0) % 900000 + 100000;
            return Math.Abs(code).ToString();
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        #endregion
    }
}
