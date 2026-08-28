namespace ContosoClaims.Api.Models;

public class Policy
{
    public int Id { get; set; }
    public string PolicyNumber { get; set; } = null!;
    public string HolderName { get; set; } = null!;
    public string HolderEmail { get; set; } = null!;
    public string ProductType { get; set; } = null!;
    public decimal CoverageLimit { get; set; }
    public decimal Deductible { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = null!;

    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
