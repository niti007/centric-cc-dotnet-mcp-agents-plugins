using ContosoClaims.Api.Data;
using ContosoClaims.Api.Dtos;
using ContosoClaims.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoClaims.Api.Services;

public class ClaimService
{
    private readonly ClaimsDbContext _db;

    public ClaimService(ClaimsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ClaimListItemDto>> GetClaimsAsync(string? status, int? assignedAdjusterId, int page, int pageSize)
    {
        var query = _db.Claims.Include(c => c.Policy).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.Status == status);
        }

        if (assignedAdjusterId.HasValue)
        {
            query = query.Where(c => c.AssignedAdjusterId == assignedAdjusterId.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClaimListItemDto
            {
                Id = c.Id,
                ClaimNumber = c.ClaimNumber,
                PolicyId = c.PolicyId,
                PolicyNumber = c.Policy.PolicyNumber,
                AssignedAdjusterId = c.AssignedAdjusterId,
                Status = c.Status,
                IncidentDate = c.IncidentDate,
                ClaimedAmount = c.ClaimedAmount
            })
            .ToListAsync();

        return new PagedResult<ClaimListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Claim?> GetClaimEntityAsync(int id)
    {
        return await _db.Claims
            .Include(c => c.Policy)
            .Include(c => c.Notes)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public static ClaimDetailDto ToDetailDto(Claim claim)
    {
        return new ClaimDetailDto
        {
            Id = claim.Id,
            ClaimNumber = claim.ClaimNumber,
            PolicyId = claim.PolicyId,
            PolicyNumber = claim.Policy.PolicyNumber,
            AssignedAdjusterId = claim.AssignedAdjusterId,
            Status = claim.Status,
            IncidentDate = claim.IncidentDate,
            ReportedAt = claim.ReportedAt,
            Description = claim.Description,
            ClaimedAmount = claim.ClaimedAmount,
            ApprovedAmount = claim.ApprovedAmount,
            DecidedByAdjusterId = claim.DecidedByAdjusterId,
            DecidedAt = claim.DecidedAt,
            Notes = claim.Notes
                .OrderBy(n => n.CreatedAt)
                .Select(n => new ClaimNoteDto
                {
                    Id = n.Id,
                    AuthorAdjusterId = n.AuthorAdjusterId,
                    Body = n.Body,
                    CreatedAt = n.CreatedAt
                })
                .ToList()
        };
    }

    public async Task<List<ClaimListItemDto>> SearchAsync(string q)
    {
        var sql = $"SELECT * FROM claims WHERE description LIKE '%{q}%' OR claim_number LIKE '%{q}%'";
        var claims = await _db.Claims.FromSqlRaw(sql).ToListAsync();

        var policyIds = claims.Select(c => c.PolicyId).Distinct().ToList();
        var policies = await _db.Policies.Where(p => policyIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        return claims.Select(c => new ClaimListItemDto
        {
            Id = c.Id,
            ClaimNumber = c.ClaimNumber,
            PolicyId = c.PolicyId,
            PolicyNumber = policies.TryGetValue(c.PolicyId, out var p) ? p.PolicyNumber : string.Empty,
            AssignedAdjusterId = c.AssignedAdjusterId,
            Status = c.Status,
            IncidentDate = c.IncidentDate,
            ClaimedAmount = c.ClaimedAmount
        }).ToList();
    }

    public async Task<ClaimNote> AddNoteAsync(int claimId, int authorAdjusterId, string body)
    {
        var note = new ClaimNote
        {
            ClaimId = claimId,
            AuthorAdjusterId = authorAdjusterId,
            Body = body,
            CreatedAt = DateTime.UtcNow
        };
        _db.ClaimNotes.Add(note);
        await _db.SaveChangesAsync();
        return note;
    }

    public async Task<bool> UpdateStatusAsync(Claim claim, string newStatus, decimal? approvedAmount, int callingAdjusterId)
    {
        claim.Status = newStatus;

        if (newStatus is "approved" or "rejected")
        {
            claim.DecidedByAdjusterId = callingAdjusterId;
            claim.DecidedAt = DateTime.UtcNow;
        }

        if (newStatus == "approved")
        {
            claim.ApprovedAmount = approvedAmount;
        }

        await _db.SaveChangesAsync();
        return true;
    }
}
