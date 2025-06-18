using System.Security.Claims;

namespace LearnQuestV1.Api.Utilities
{
    /// <summary>
    /// Enhanced extension methods for ClaimsPrincipal with comprehensive user information extraction
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        // =====================================================
        // Core User Information
        // =====================================================

        /// <summary>
        /// Gets the current user's ID from JWT token claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>User ID if found, null otherwise</returns>
        public static int? GetCurrentUserId(this ClaimsPrincipal user)
        {
            try
            {
                var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user?.FindFirst("sub")?.Value
                    ?? user?.FindFirst("userId")?.Value
                    ?? user?.FindFirst("id")?.Value;

                return int.TryParse(userIdClaim, out int userId) ? userId : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the current user's email from JWT token claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>User email if found, null otherwise</returns>
        public static string? GetCurrentUserEmail(this ClaimsPrincipal user)
        {
            try
            {
                return user?.FindFirst(ClaimTypes.Email)?.Value
                    ?? user?.FindFirst("email")?.Value
                    ?? user?.FindFirst("userEmail")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the current user's full name from JWT token claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>User full name if found, null otherwise</returns>
        public static string? GetCurrentUserFullName(this ClaimsPrincipal user)
        {
            try
            {
                return user?.FindFirst(ClaimTypes.Name)?.Value
                    ?? user?.FindFirst("name")?.Value
                    ?? user?.FindFirst("fullName")?.Value
                    ?? user?.FindFirst("displayName")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the current user's username from JWT token claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>Username if found, null otherwise</returns>
        public static string? GetCurrentUsername(this ClaimsPrincipal user)
        {
            try
            {
                return user?.FindFirst("preferred_username")?.Value
                    ?? user?.FindFirst("username")?.Value
                    ?? user?.FindFirst("login")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the current user's primary role from JWT token claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>Primary role if found, null otherwise</returns>
        public static string? GetCurrentUserRole(this ClaimsPrincipal user)
        {
            try
            {
                return user?.FindFirst(ClaimTypes.Role)?.Value
                    ?? user?.FindFirst("role")?.Value
                    ?? user?.FindFirst("userRole")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets all roles for the current user from JWT token claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>List of user roles</returns>
        public static IEnumerable<string> GetCurrentUserRoles(this ClaimsPrincipal user)
        {
            try
            {
                if (user == null) return Enumerable.Empty<string>();

                var roles = user.FindAll(ClaimTypes.Role)
                    .Concat(user.FindAll("role"))
                    .Concat(user.FindAll("roles"))
                    .Select(c => c.Value)
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Distinct()
                    .ToList();

                return roles;
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        // =====================================================
        // Role Checking Methods
        // =====================================================

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <param name="role">Role to check for</param>
        /// <returns>True if user has the role, false otherwise</returns>
        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(role)) return false;
                return user?.IsInRole(role) ?? false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the current user has any of the specified roles
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <param name="roles">Roles to check for</param>
        /// <returns>True if user has any of the roles, false otherwise</returns>
        public static bool HasAnyRole(this ClaimsPrincipal user, params string[] roles)
        {
            try
            {
                if (roles == null || roles.Length == 0) return false;
                return roles.Any(role => user.HasRole(role));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the current user has all of the specified roles
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <param name="roles">Roles to check for</param>
        /// <returns>True if user has all roles, false otherwise</returns>
        public static bool HasAllRoles(this ClaimsPrincipal user, params string[] roles)
        {
            try
            {
                if (roles == null || roles.Length == 0) return false;
                return roles.All(role => user.HasRole(role));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the current user is an admin
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>True if user is admin, false otherwise</returns>
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.HasAnyRole("Admin", "Administrator", "SuperAdmin");
        }

        /// <summary>
        /// Checks if the current user is an instructor
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>True if user is instructor, false otherwise</returns>
        public static bool IsInstructor(this ClaimsPrincipal user)
        {
            return user.HasRole("Instructor");
        }

        /// <summary>
        /// Checks if the current user is a regular user
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>True if user is regular user, false otherwise</returns>
        public static bool IsRegularUser(this ClaimsPrincipal user)
        {
            return user.HasRole("RegularUser") || user.HasRole("User");
        }

        /// <summary>
        /// Checks if the current user is an instructor or admin
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>True if user is instructor or admin, false otherwise</returns>
        public static bool IsInstructorOrAdmin(this ClaimsPrincipal user)
        {
            return user.IsInstructor() || user.IsAdmin();
        }

        // =====================================================
        // Token and Session Information
        // =====================================================

        /// <summary>
        /// Gets the JWT token from the authorization header
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <returns>JWT token without Bearer prefix, null if not found</returns>
        public static string? GetJwtToken(this HttpContext httpContext)
        {
            try
            {
                var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader))
                    return null;

                return authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authHeader.Substring("Bearer ".Length).Trim()
                    : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the session ID from JWT token claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>Session ID if found, null otherwise</returns>
        public static string? GetSessionId(this ClaimsPrincipal user)
        {
            try
            {
                return user?.FindFirst("sessionId")?.Value
                    ?? user?.FindFirst("sid")?.Value
                    ?? user?.FindFirst("session")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the token expiration time from JWT token claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>Expiration time if found, null otherwise</returns>
        public static DateTime? GetTokenExpiration(this ClaimsPrincipal user)
        {
            try
            {
                var expClaim = user?.FindFirst("exp")?.Value;
                if (string.IsNullOrEmpty(expClaim) || !long.TryParse(expClaim, out long exp))
                    return null;

                return DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if the current token is expired
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>True if token is expired, false otherwise</returns>
        public static bool IsTokenExpired(this ClaimsPrincipal user)
        {
            try
            {
                var expiration = user.GetTokenExpiration();
                return expiration.HasValue && expiration.Value <= DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }

        // =====================================================
        // Extended User Information
        // =====================================================

        /// <summary>
        /// Gets the user's phone number from JWT token claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>Phone number if found, null otherwise</returns>
        public static string? GetPhoneNumber(this ClaimsPrincipal user)
        {
            try
            {
                return user?.FindFirst(ClaimTypes.MobilePhone)?.Value
                    ?? user?.FindFirst("phone_number")?.Value
                    ?? user?.FindFirst("phoneNumber")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the user's profile picture URL from JWT token claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>Profile picture URL if found, null otherwise</returns>
        public static string? GetProfilePictureUrl(this ClaimsPrincipal user)
        {
            try
            {
                return user?.FindFirst("picture")?.Value
                    ?? user?.FindFirst("profilePicture")?.Value
                    ?? user?.FindFirst("avatar")?.Value;
            }
            catch
            {
                return null;
            }
        }

        ///// <summary>
        ///// Gets the user's department from JWT token claims
        ///// </summary>
        ///// <param name="user">ClaimsPrincipal from controller</param>
        ///// <returns>Department if found, null otherwise</returns>
        //public static string? GetDepartment(this ClaimsPrincipal user)
        //{
        //    try
        //    {
        //        return user?.FindFirst("department")?.Value
        //            ?? user?.FindFirst("dept")?.Value
        //            ?? user?.FindFirst(ClaimTypes.Department)?.Value;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        /// <summary>
        /// Gets the user's organization from JWT token claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>Organization if found, null otherwise</returns>
        public static string? GetOrganization(this ClaimsPrincipal user)
        {
            try
            {
                return user?.FindFirst("organization")?.Value
                    ?? user?.FindFirst("org")?.Value
                    ?? user?.FindFirst("company")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a custom claim value from JWT token claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <param name="claimType">Claim type to retrieve</param>
        /// <returns>Claim value if found, null otherwise</returns>
        public static string? GetCustomClaim(this ClaimsPrincipal user, string claimType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(claimType)) return null;
                return user?.FindFirst(claimType)?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets all custom claims as a dictionary
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>Dictionary of claim types and values</returns>
        public static Dictionary<string, string> GetAllClaims(this ClaimsPrincipal user)
        {
            try
            {
                if (user?.Claims == null) return new Dictionary<string, string>();

                return user.Claims
                    .GroupBy(c => c.Type)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First().Value
                    );
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        // =====================================================
        // Validation and Security Methods
        // =====================================================

        /// <summary>
        /// Validates if the user has a valid identity
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>True if user has valid identity, false otherwise</returns>
        public static bool HasValidIdentity(this ClaimsPrincipal user)
        {
            try
            {
                return user?.Identity?.IsAuthenticated == true && user.GetCurrentUserId().HasValue;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates if the user belongs to a specific organization
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <param name="organizationId">Organization ID to check</param>
        /// <returns>True if user belongs to organization, false otherwise</returns>
        public static bool BelongsToOrganization(this ClaimsPrincipal user, string organizationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(organizationId)) return false;

                var userOrgId = user.GetCustomClaim("organizationId") ?? user.GetCustomClaim("orgId");
                return string.Equals(userOrgId, organizationId, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the user can access a specific resource based on ownership or admin privileges
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <param name="resourceOwnerId">ID of the resource owner</param>
        /// <returns>True if user can access resource, false otherwise</returns>
        public static bool CanAccessResource(this ClaimsPrincipal user, int resourceOwnerId)
        {
            try
            {
                var currentUserId = user.GetCurrentUserId();
                return user.IsAdmin() || (currentUserId.HasValue && currentUserId.Value == resourceOwnerId);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets user information summary for logging and auditing
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>User summary information</returns>
        public static UserSummaryInfo GetUserSummary(this ClaimsPrincipal user)
        {
            try
            {
                return new UserSummaryInfo
                {
                    UserId = user.GetCurrentUserId(),
                    Email = user.GetCurrentUserEmail(),
                    FullName = user.GetCurrentUserFullName(),
                    Username = user.GetCurrentUsername(),
                    PrimaryRole = user.GetCurrentUserRole(),
                    AllRoles = user.GetCurrentUserRoles().ToList(),
                    IsAuthenticated = user?.Identity?.IsAuthenticated ?? false,
                    SessionId = user.GetSessionId(),
                    TokenExpiration = user.GetTokenExpiration(),
                    IsTokenExpired = user.IsTokenExpired()
                };
            }
            catch
            {
                return new UserSummaryInfo
                {
                    IsAuthenticated = false,
                    IsTokenExpired = true
                };
            }
        }
    }

    /// <summary>
    /// User summary information for logging and auditing
    /// </summary>
    public class UserSummaryInfo
    {
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public string? PrimaryRole { get; set; }
        public List<string> AllRoles { get; set; } = new List<string>();
        public bool IsAuthenticated { get; set; }
        public string? SessionId { get; set; }
        public DateTime? TokenExpiration { get; set; }
        public bool IsTokenExpired { get; set; }

        /// <summary>
        /// Gets a display name for the user
        /// </summary>
        public string DisplayName => FullName ?? Username ?? Email ?? $"User {UserId}" ?? "Unknown User";

        /// <summary>
        /// Gets the primary role or "Unknown" if not found
        /// </summary>
        public string RoleDisplay => PrimaryRole ?? (AllRoles.FirstOrDefault()) ?? "Unknown";

        /// <summary>
        /// Checks if the user is an admin
        /// </summary>
        public bool IsAdmin => AllRoles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                                                r.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ||
                                                r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Checks if the user is an instructor
        /// </summary>
        public bool IsInstructor => AllRoles.Any(r => r.Equals("Instructor", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Gets a formatted string representation of the user
        /// </summary>
        public override string ToString()
        {
            return $"{DisplayName} ({RoleDisplay}) - ID: {UserId}";
        }
    }
}