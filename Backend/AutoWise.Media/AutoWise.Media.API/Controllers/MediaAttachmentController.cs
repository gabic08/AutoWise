using AutoWise.Media.API.Requests;
using AutoWise.Media.Application.Dtos;
using AutoWise.Media.Application.Features.MediaAttachments;
using Microsoft.AspNetCore.Mvc;

namespace AutoWise.Media.API.Controllers;

[Route("api")]
[ApiController]
public class MediaAttachmentController(IMediaAttachmentService mediaAttachmentService) : ControllerBase
{
    private readonly IMediaAttachmentService _mediaAttachmentService = mediaAttachmentService;

    [HttpPost("media")]
    public async Task<IActionResult> UploadAsync([FromForm] UploadMediaFormRequest request, CancellationToken cancellationToken)
    {
        await using var stream = request.File.OpenReadStream();
        var uploadRequest = new UploadMediaRequest(stream, request.File.ContentType, request.File.FileName, request.ParentType, request.ParentEntityId);

        return Ok(await _mediaAttachmentService.UploadAsync(uploadRequest, cancellationToken));
    }

    [HttpGet("media/{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediaAttachmentService.GetByIdAsync(id, cancellationToken));
    }

    [HttpDelete("media/{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _mediaAttachmentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
