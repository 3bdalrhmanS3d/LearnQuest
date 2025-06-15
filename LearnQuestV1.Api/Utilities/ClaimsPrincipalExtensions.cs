
namespace LearnQuestV1.Api.Utilities
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Extension methods for user authentication and identity
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// Gets the current user ID from JWT claims
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

                if (string.IsNullOrEmpty(userIdClaim))
                    return null;

                return int.TryParse(userIdClaim, out var userId) ? userId : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the current user's email from JWT claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>User email if found, null otherwise</returns>
        public static string? GetCurrentUserEmail(this ClaimsPrincipal user)
        {
            try
            {
                return user?.FindFirst(ClaimTypes.Email)?.Value
                    ?? user?.FindFirst("email")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the current user's role from JWT claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>User role if found, null otherwise</returns>
        public static string? GetCurrentUserRole(this ClaimsPrincipal user)
        {
            try
            {
                return user?.FindFirst(ClaimTypes.Role)?.Value
                    ?? user?.FindFirst("role")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the current user's full name from JWT claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>User full name if found, null otherwise</returns>
        public static string? GetCurrentUserFullName(this ClaimsPrincipal user)
        {
            try
            {
                return user?.FindFirst(ClaimTypes.Name)?.Value
                    ?? user?.FindFirst("name")?.Value
                    ?? user?.FindFirst("fullName")?.Value;
            }
            catch
            {
                return null;
            }
        }

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
                return user?.IsInRole(role) ?? false;
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
            return user.HasRole("Admin");
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
            return user.HasRole("RegularUser");
        }

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
                    ? authHeader[7..]
                    : authHeader;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets comprehensive user information from claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal from controller</param>
        /// <returns>User information object</returns>
        public static UserInfo GetUserInfo(this ClaimsPrincipal user)
        {
            return new UserInfo
            {
                UserId = user.GetCurrentUserId(),
                Email = user.GetCurrentUserEmail(),
                FullName = user.GetCurrentUserFullName(),
                Role = user.GetCurrentUserRole(),
                IsAdmin = user.IsAdmin(),
                IsInstructor = user.IsInstructor(),
                IsRegularUser = user.IsRegularUser()
            };
        }
    }

    /// <summary>
    /// User information container
    /// </summary>
    public class UserInfo
    {
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsInstructor { get; set; }
        public bool IsRegularUser { get; set; }

        public bool IsAuthenticated => UserId.HasValue;
        public bool IsValid => IsAuthenticated && !string.IsNullOrEmpty(Email);
    }
}

