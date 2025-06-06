using LearnQuestV1.Core.Models;
using System.Security.Claims;

namespace LearnQuestV1.Api.Utilities
{
    public static class ClaimsPrincipalExtensions
    {

        /// <summary>
        /// Attempts to read the current user’s ID from the NameIdentifier claim.
        /// Returns null if the claim is missing or malformed.
        /// </summary>
        public static int? GetCurrentUserId(this ClaimsPrincipal user)
        {
            if (user == null) return null;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            return int.TryParse(userIdClaim, out var userId)
                ? (int?)userId
                : null;
        }
    }
}
