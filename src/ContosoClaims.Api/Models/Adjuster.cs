namespace ContosoClaims.Api.Models;

public class Adjuster
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Region { get; set; } = null!;
    public bool IsActive { get; set; }

    public ICollection<Claim> AssignedClaims { get; set; } = new List<Claim>();
    public ICollection<Claim> DecidedClaims { get; set; } = new List<Claim>();
}
