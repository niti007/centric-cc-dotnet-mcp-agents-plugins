namespace ContosoClaims.Api.Dtos;

public class ClaimListItemDto
{
    public int Id { get; set; }
    public string ClaimNumber { get; set; } = null!;
    public int PolicyId { get; set; }
    public string PolicyNumber { get; set; } = null!;
    public int? AssignedAdjusterId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime IncidentDate { get; set; }
    public decimal ClaimedAmount { get; set; }
}

public class ClaimDetailDto
{
    public int Id { get; set; }
    public string ClaimNumber { get; set; } = null!;
    public int PolicyId { get; set; }
    public string PolicyNumber { get; set; } = null!;
    public int? AssignedAdjusterId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime IncidentDate { get; set; }
    public DateTime ReportedAt { get; set; }
    public string Description { get; set; } = null!;
    public decimal ClaimedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public int? DecidedByAdjusterId { get; set; }
    public DateTime? DecidedAt { get; set; }
    public List<ClaimNoteDto> Notes { get; set; } = new();
}

public class ClaimNoteDto
{
    public int Id { get; set; }
    public int? AuthorAdjusterId { get; set; }
    public string Body { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class AddClaimNoteRequest
{
    public string Body { get; set; } = null!;
}

public class UpdateClaimStatusRequest
{
    public string Status { get; set; } = null!;
    public decimal? ApprovedAmount { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
