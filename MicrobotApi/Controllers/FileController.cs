using System.Web;
using Azure;
using Azure.Storage.Blobs.Models;
using MicrobotApi.Database;
using MicrobotApi.Models;
using MicrobotApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MicrobotApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : Controller
{
    private readonly AzureStorageService _azureStorageService;
    private readonly MicrobotContext _microbotContext;
    private readonly IMemoryCache _memoryCache;

    public FileController(AzureStorageService azureStorageService, MicrobotContext microbotContext, IMemoryCache memoryCache)
    {
        _azureStorageService = azureStorageService;
        _microbotContext = microbotContext;
        _memoryCache = memoryCache;
    }

    /*[HttpGet("download/{fileName}/{key}/{hwid}")]
    public async Task<IActionResult> Download(Guid fileName, string key, string hwid)
    {
        DateTime? dateTime = _memoryCache.Get<DateTime>(key);
        if (!dateTime.HasValue)
        {
            return Unauthorized();
        }
        
        var exists = await _microbotContext.Keys.AnyAsync(x => x.Key == key && (x.HWID == "" || x.HWID == hwid));
        
        if (!exists)
        {
            return Unauthorized();
        }

        var script = await _microbotContext.Scripts
            .FirstAsync(x => x.Id == fileName);

        var file = await _azureStorageService.DownloadFile(script.Name + "/" + script.Id + ".jar");
            
        return File(file.Value.Content, "application/octet-stream", script.Id.ToString());

    }*/
    
    [HttpGet("launcher")]
    public async Task<IActionResult> Launcher()
    {
        return Ok("1.0.1");
    }
    
    [HttpGet("client")]
    public async Task<IActionResult> Client()
    {
        return Ok("1.7.0");
    }
    
    [HttpGet("html")]
    public async Task<IActionResult> Html()
    {
        return Ok("1.0.0");
    }

    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string path)
    {
        var file = await _azureStorageService.DownloadFile(path);
            
        return File(file.Value.Content, "application/octet-stream", Path.GetFileName(HttpUtility.UrlDecode(path)));
    }
}
