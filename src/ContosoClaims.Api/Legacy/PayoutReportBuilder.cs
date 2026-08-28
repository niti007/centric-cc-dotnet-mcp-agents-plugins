using ContosoClaims.Api.Data;
using ContosoClaims.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ContosoClaims.Api.Legacy;

/// <summary>
/// Builds the payout report for finance. Ported from the old reporting job;
/// kept here mostly as-is.
/// </summary>
public class PayoutReportBuilder
{
    private readonly ClaimsDbContext _db;

    public PayoutReportBuilder(ClaimsDbContext db)
    {
        _db = db;
    }

    public async Task<PayoutReportDto> BuildAsync(DateTime from, DateTime to)
    {
        var claims = await _db.Claims
            .Where(c => c.Status == "paid" && c.DecidedAt != null && c.DecidedAt >= from && c.DecidedAt <= to)
            .ToListAsync();

        var rows = new List<PayoutReportRowDto>();
        double total = 0;

        foreach (var claim in claims)
        {
            var policy = _db.Policies.First(p => p.Id == claim.PolicyId);
            var adjuster = claim.DecidedByAdjusterId.HasValue
                ? _db.Adjusters.First(a => a.Id == claim.DecidedByAdjusterId.Value)
                : null;

            var amount = claim.ApprovedAmount ?? 0m;
            total += (double)amount;

            rows.Add(new PayoutReportRowDto
            {
                ClaimId = claim.Id,
                ClaimNumber = claim.ClaimNumber,
                PolicyNumber = policy.PolicyNumber,
                AdjusterName = adjuster?.FullName ?? "Unassigned",
                ApprovedAmount = amount
            });
        }

        return new PayoutReportDto
        {
            From = from,
            To = to,
            Rows = rows,
            TotalPayout = Math.Round((decimal)total, 2)
        };
    }
}
