using ContosoClaims.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContosoClaims.Tests;

/// <summary>
/// Looks up real rows from the live seeded database so tests never depend on
/// hardcoded ids that could shift when the seed data is reloaded.
/// </summary>
public static class TestData
{
    public static (int claimId, int assignedAdjusterId) GetAssignedClaim(CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        var claim = db.Claims.First(c => c.AssignedAdjusterId != null);
        return (claim.Id, claim.AssignedAdjusterId!.Value);
    }

    public static int GetAnyAdjusterId(CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        return db.Adjusters.First().Id;
    }

    public static int GetAnyPolicyId(CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        return db.Policies.First().Id;
    }

    public static int GetAdjusterIdOtherThan(CustomWebApplicationFactory factory, int excludedAdjusterId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        return db.Adjusters.First(a => a.Id != excludedAdjusterId).Id;
    }
}
