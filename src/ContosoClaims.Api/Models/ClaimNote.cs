namespace ContosoClaims.Api.Models;

public class ClaimNote
{
    public int Id { get; set; }
    public int ClaimId { get; set; }
    public int? AuthorAdjusterId { get; set; }
    public string Body { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public Claim Claim { get; set; } = null!;
    public Adjuster? AuthorAdjuster { get; set; }
}
