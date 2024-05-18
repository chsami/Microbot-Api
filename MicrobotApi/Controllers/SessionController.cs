using MicrobotApi.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MicrobotApi.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SessionController : Controller
{
    private readonly MicrobotContext _microbotContext;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
    
    public SessionController(MicrobotContext microbotContext, IMemoryCache memoryCache, IConfiguration configuration)
    {
        _microbotContext = microbotContext;
        _memoryCache = memoryCache;
        _configuration = configuration;
    }
    
    [HttpGet("")]
    public async Task<IActionResult> Session([FromQuery] Guid? sessionId, [FromQuery] bool? isLoggedIn, [FromQuery] string? version)
    {
        if (sessionId.HasValue && isLoggedIn.HasValue && version != null)
        {
            var result = await _microbotContext.Sessions.FirstOrDefaultAsync(x => x.Id == sessionId);
            if (result != null)
            {
                result.IsLoggedIn = isLoggedIn.Value;
                result.Version = version;
                result.LastPing = DateTime.UtcNow;
                await _microbotContext.SaveChangesAsync();
            }
            return Ok(sessionId);
        }

        var session = new Session()
        {
            IsLoggedIn = false,
            Version = "",
            LastPing = DateTime.UtcNow
        };

         _microbotContext.Sessions.Add(session);

        await _microbotContext.SaveChangesAsync();

        return Ok(session.Id);
    }
    
    [HttpDelete("")]
    public async Task<IActionResult> DeleteSession([FromQuery] Guid sessionId)
    {
        var session = await _microbotContext.Sessions.FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null) return BadRequest("Session id not found.");
        
        _microbotContext.Sessions.Remove(session);
        await _microbotContext.SaveChangesAsync();
        
        return Ok();
    }
    
    [HttpGet("count")]
    public IActionResult Count()
    {
        return Ok(GetCachedCount());
    }
    
    [HttpGet("count/loggedIn")]
    public IActionResult CountLoggedIn()
    {       
        return Ok(GetCachedLoggedInCount());
    }
    
    private int GetCachedCount()
    {
        if (!_memoryCache.TryGetValue("CachedCount", out int cachedData))
        {
            // Data not in cache, so get it from the source
            cachedData = GetCount();

            // Set cache options
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheDuration,
                SlidingExpiration = _cacheDuration
            };

            // Save data in cache
            _memoryCache.Set("CachedCount", cachedData, cacheEntryOptions);
        }

        return cachedData;
    }
    
    private int GetCachedLoggedInCount()
    {
        if (!_memoryCache.TryGetValue("CachedLoggedInCount", out int cachedData))
        {
            // Data not in cache, so get it from the source
            cachedData = GetCountLoggedIn();

            // Set cache options
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheDuration,
                SlidingExpiration = _cacheDuration
            };

            // Save data in cache
            _memoryCache.Set("CachedLoggedInCount", cachedData, cacheEntryOptions);
        }

        return cachedData;
    }
    
    private int GetCountLoggedIn()
    {
        var count = _microbotContext.Sessions.Count(x => x.IsLoggedIn && x.LastPing > DateTime.UtcNow.AddMinutes(-5));
        return count;
    }
    
    private int GetCount()
    {
        var count = _microbotContext.Sessions.Count(x => x.IsLoggedIn && x.LastPing > DateTime.UtcNow.AddMinutes(-5));
        return count;
    }
}