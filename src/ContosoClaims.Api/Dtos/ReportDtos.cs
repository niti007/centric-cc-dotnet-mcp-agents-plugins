namespace ContosoClaims.Api.Dtos;

public class PayoutReportRowDto
{
    public int ClaimId { get; set; }
    public string ClaimNumber { get; set; } = null!;
    public string PolicyNumber { get; set; } = null!;
    public string AdjusterName { get; set; } = null!;
    public decimal ApprovedAmount { get; set; }
}

public class PayoutReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<PayoutReportRowDto> Rows { get; set; } = new();
    public decimal TotalPayout { get; set; }
}
