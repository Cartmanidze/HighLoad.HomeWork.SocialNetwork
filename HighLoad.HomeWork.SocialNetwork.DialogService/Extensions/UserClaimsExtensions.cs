using System.Security.Claims;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Extensions;

internal static class UserClaimsExtensions
{
    internal static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var claimValue = user.FindFirst("UserId")?.Value;
        if (Guid.TryParse(claimValue, out var userId))
            return userId;
        return null;
    }
}