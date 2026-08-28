namespace ContosoClaims.Api.Models;

public class Payment
{
    public int Id { get; set; }
    public int ClaimId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public string Method { get; set; } = null!;
    public string Reference { get; set; } = null!;

    public Claim Claim { get; set; } = null!;
}
