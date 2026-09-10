using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContosoDashboard.Services;

namespace ContosoDashboard.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentDownloadController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentDownloadController> _logger;

    public DocumentDownloadController(IDocumentService documentService, ILogger<DocumentDownloadController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out var id))
        {
            return id;
        }
        return 0;
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        try
        {
            var (stream, fileName, contentType) = await _documentService.DownloadDocumentAsync(id, userId);
            return File(stream, contentType, fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound("Document file could not be found.");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", id);
            return StatusCode(500, "Internal error processing document download.");
        }
    }

    [HttpGet("{id:int}/preview")]
    public async Task<IActionResult> Preview(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        try
        {
            var (stream, _, contentType) = await _documentService.DownloadDocumentAsync(id, userId);
            Response.Headers["Content-Disposition"] = "inline";
            return File(stream, contentType);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
