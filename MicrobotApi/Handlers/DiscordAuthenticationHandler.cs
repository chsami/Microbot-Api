using System.Security.Claims;
using System.Text.Encodings.Web;
using MicrobotApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MicrobotApi.Handlers;

public class DiscordAuthenticationHandler(
    IOptionsMonitor<CustomAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    DiscordService discordService)
    : AuthenticationHandler<CustomAuthenticationOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Extract your credentials from the request
        var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.Fail("No token provided.");
        }
        
        // Validate the token by making a request to Discord's API or using your method
        var userInfo = await discordService.GetUserInfo(token);
        if (userInfo != null)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userInfo.Id) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
       

        return AuthenticateResult.Fail("Invalid token.");
    }
}

public class CustomAuthenticationOptions : AuthenticationSchemeOptions
{
    // Any custom options here
}