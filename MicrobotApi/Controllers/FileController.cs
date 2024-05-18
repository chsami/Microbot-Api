using MicrobotApi.Database;
using MicrobotApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MicrobotApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : Controller
{
    private readonly AzureStorageService _azureStorageService;
    private readonly MicrobotContext _microbotContext;

    public FileController(AzureStorageService azureStorageService, MicrobotContext microbotContext)
    {
        _azureStorageService = azureStorageService;
        _microbotContext = microbotContext;
    }
    
    [Authorize]
    [HttpGet("download/{blobName}/{key}/{hwid}")]
    public async Task<IActionResult> Download(string blobName, string key, string hwid)
    {
        var exists = await _microbotContext.Keys.AnyAsync(x => x.Key == key && x.HWID == hwid);

        if (!exists)
            return Unauthorized();
        
        var file = await _azureStorageService.DownloadFile(blobName);

        return File(file.Value.Content, "application/octet-stream", blobName);
    }
}