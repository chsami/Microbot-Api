using System.Security.Claims;

namespace MicrobotApi.Extensions;

public static class UserExtension
{
    public static string? GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        return userId;
    }
}