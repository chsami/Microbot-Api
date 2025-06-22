using System.Security.Claims;
using MicrobotApi.Database;
using MicrobotApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MicrobotApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly DiscordService _discordService;
    private readonly MicrobotContext _microbotContext;

    public AuthController(IConfiguration configuration, DiscordService discordService, MicrobotContext microbotContext)
    {
        _configuration = configuration;
        _discordService = discordService;
        _microbotContext = microbotContext;
    }

    [HttpGet("discord/user")]
    public async Task<IActionResult> DiscordUserInfo([FromQuery] String code)
    {
        if (!string.IsNullOrWhiteSpace(code))
        {
            var clientId = _configuration["Discord:ClientId"] ?? string.Empty;
            var clientSecret = _configuration["Discord:ClientSecret"] ?? string.Empty;
            var redirectUri = _configuration["Discord:RedirectUri"] ?? string.Empty;
            var tokenResponse = await _discordService.GetToken(clientId, clientSecret, code, redirectUri);

            if (tokenResponse == null)
                return BadRequest("Invalid code!");

            var userInfo = await _discordService.GetUserInfo(tokenResponse.Access_Token);

            if (userInfo == null)
                return BadRequest("userinfo is empty");

            var discordUser = await _microbotContext.DiscordUsers.FirstOrDefaultAsync(x => x.DiscordId == userInfo.Id);

            if (discordUser == null)
            {
                _microbotContext.Users.Add(new DiscordUser()
                {
                    DiscordId = userInfo.Id,
                    Token = tokenResponse.Access_Token,
                    RefreshToken = tokenResponse.Refresh_Token,
                    TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.Expires_In),
                });
                await _microbotContext.SaveChangesAsync();
            }

            return Ok(tokenResponse.Access_Token);

        }

        return BadRequest("Code is missing!");
    }

    [HttpGet("userinfo")]
    [Authorize]
    public async Task<IActionResult> UserInfo()
    {
        var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        var discordUser = await _microbotContext.DiscordUsers.FirstOrDefaultAsync(x => x.DiscordId == userId);
        
        if (discordUser != null)
        {
            var userInfo = await _discordService.GetUserInfo(discordUser.Token);
        
            return Ok(userInfo);
        }

    
        return NotFound("User not found.");
    }
}