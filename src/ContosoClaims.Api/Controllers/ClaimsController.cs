using ContosoClaims.Api.Auth;
using ContosoClaims.Api.Data;
using ContosoClaims.Api.Dtos;
using ContosoClaims.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContosoClaims.Api.Controllers;

[ApiController]
[Route("api/claims")]
[ServiceFilter(typeof(AdjusterAuthFilter))]
public class ClaimsController : ControllerBase
{
    private readonly ClaimService _claimService;
    private readonly ClaimsDbContext _db;

    public ClaimsController(ClaimService claimService, ClaimsDbContext db)
    {
        _claimService = claimService;
        _db = db;
    }

    private int CallingAdjusterId => (int)HttpContext.Items[AdjusterAuthFilter.ContextKey]!;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClaimListItemDto>>> GetClaims(
        [FromQuery] string? status,
        [FromQuery] int? assignedAdjusterId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 20;

        var result = await _claimService.GetClaimsAsync(status, assignedAdjusterId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<ClaimListItemDto>>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(new List<ClaimListItemDto>());
        }

        var results = await _claimService.SearchAsync(q);
        return Ok(results);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClaimDetailDto>> GetById(int id)
    {
        var claim = await _claimService.GetClaimEntityAsync(id);
        if (claim is null)
        {
            return NotFound();
        }

        if (claim.AssignedAdjusterId != CallingAdjusterId)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        return Ok(ClaimService.ToDetailDto(claim));
    }

    [HttpPost("{id:int}/notes")]
    public async Task<ActionResult<ClaimNoteDto>> AddNote(int id, [FromBody] AddClaimNoteRequest request)
    {
        var claim = await _claimService.GetClaimEntityAsync(id);
        if (claim is null)
        {
            return NotFound();
        }

        if (claim.AssignedAdjusterId != CallingAdjusterId)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Note body is required.");
        }

        var note = await _claimService.AddNoteAsync(id, CallingAdjusterId, request.Body);

        return Ok(new ClaimNoteDto
        {
            Id = note.Id,
            AuthorAdjusterId = note.AuthorAdjusterId,
            Body = note.Body,
            CreatedAt = note.CreatedAt
        });
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<ClaimDetailDto>> UpdateStatus(int id, [FromBody] UpdateClaimStatusRequest request)
    {
        var claim = await _claimService.GetClaimEntityAsync(id);
        if (claim is null)
        {
            return NotFound();
        }

        var validStatuses = new[] { "submitted", "under_review", "approved", "rejected", "paid" };
        if (!validStatuses.Contains(request.Status))
        {
            return BadRequest("Invalid status.");
        }

        await _claimService.UpdateStatusAsync(claim, request.Status, request.ApprovedAmount, CallingAdjusterId);

        return Ok(ClaimService.ToDetailDto(claim));
    }
}
