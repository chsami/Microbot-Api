using MicrobotApi.Services;

namespace MicrobotApi.middleware;

public class DiscordAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DiscordService _discordService;

    public DiscordAuthorizationMiddleware(RequestDelegate next, DiscordService discordService)
    {
        _next = next;
        _discordService = discordService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (token == null || !await ValidateToken(token))
        {
            context.Response.StatusCode = 401; // Unauthorized
            return;
        }

        await _next(context);
    }

    private async Task<bool> ValidateToken(string token)
    {
        // Validate the token by making a request to Discord's API or using your method
        var userInfo = await _discordService.GetUserInfo(token);
        return userInfo != null;
    }
}