namespace ContosoClaims.Api.Models;

public class Claim
{
    public int Id { get; set; }
    public string ClaimNumber { get; set; } = null!;
    public int PolicyId { get; set; }
    public int? AssignedAdjusterId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime IncidentDate { get; set; }
    public DateTime ReportedAt { get; set; }
    public string Description { get; set; } = null!;
    public decimal ClaimedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public int? DecidedByAdjusterId { get; set; }
    public DateTime? DecidedAt { get; set; }

    public Policy Policy { get; set; } = null!;
    public Adjuster? AssignedAdjuster { get; set; }
    public Adjuster? DecidedByAdjuster { get; set; }
    public ICollection<ClaimNote> Notes { get; set; } = new List<ClaimNote>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
