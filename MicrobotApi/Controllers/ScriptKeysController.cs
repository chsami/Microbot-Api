using MicrobotApi.Database;
using MicrobotApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MicrobotApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScriptKeysController(MicrobotContext microbotContext) : Controller
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] HmacRequest request)
    {
        var key = await microbotContext.Keys.FirstOrDefaultAsync(x => x.Key == request.Key);

        if (key == null)
            return NotFound("Key not found!");

        key.Active = true;
        key.HWID = request.HWID;

        await microbotContext.SaveChangesAsync();

        return Ok();
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get()
    {
        var keys = await microbotContext.DiscordUsers
            .Include(x => x.Keys)
            .Where(x => x.DiscordId == User.GetUserId())
            .Select(x => x.Keys)
            .FirstOrDefaultAsync();

        return Ok(keys);
    }
    
    public class HmacRequest
    {
        public string Key { get; set; }
        public string HWID { get; set; }
    }
}