using MicrobotApi.Database;
using MicrobotApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MicrobotApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScriptController(XataService xataService)
    : Controller
{
    private readonly XataService _xataService = xataService;

    /*[HttpGet("{key}/{hwid}")]
    public async Task<IActionResult> List(string key, string hwid)
    {
        var exists = await microbotContext.Keys.AnyAsync(x => x.Key == key && (x.HWID == "" || x.HWID == hwid));

        if (!exists) return Unauthorized();
        
        var scripts = await microbotContext.Scripts
            .Where(x => x.ScriptKeys.Any(s => s.Key == key && (s.HWID == "" || s.HWID == hwid)))
            .ToListAsync();
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(30));
        
        memoryCache.Set(key, DateTime.Now, cacheEntryOptions);

        return Ok(scripts.Select(x => x.Id));
    }*/


    [HttpPost("runtime")]
    public async Task<IActionResult> UpdateRunTime([FromBody] ScriptStats scriptStats)
    {
        var a = await xataService.GetRecordsAsync("script_runtime");
        
        foreach (var scriptStat in scriptStats.scriptRunTimes)
        {
            // Check if the script name exists in the records
            var script = a.Records
                .FirstOrDefault(record => string.Equals(record.Name, scriptStat.Key, StringComparison.CurrentCultureIgnoreCase));

            if (script == null)
            {
                // If no matching script is found, add a new record
                await xataService.AddRecordAsync("script_runtime", new { name = scriptStat.Key, runtime_minutes = scriptStat.Value });
            }
            else
            {
                // If a matching script is found, update its runtime_minutes
                script.RuntimeMinutes += scriptStat.Value;
                await xataService.PatchRecordAsync("script_runtime", script.Id, new { name = script.Name, runtime_minutes = script.RuntimeMinutes });
            }
        }


        return Ok();
    }
    
    
}

public class ScriptStats {
    public Dictionary<String, int>  scriptRunTimes { get; set; }
}