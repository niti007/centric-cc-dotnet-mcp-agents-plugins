using System.Net.Http.Json;
using ContosoClaims.Api.Dtos;

namespace ContosoClaims.Tests;

public class ReportsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ReportsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient ClientWithAdjuster(int adjusterId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Adjuster-Id", adjusterId.ToString());
        return client;
    }

    [Fact]
    public async Task GetPayouts_ReturnsDataForWideDateRange()
    {
        var adjusterId = TestData.GetAnyAdjusterId(_factory);
        var client = ClientWithAdjuster(adjusterId);

        var response = await client.GetAsync("/api/reports/payouts?from=2000-01-01&to=2100-01-01");
        response.EnsureSuccessStatusCode();

        var report = await response.Content.ReadFromJsonAsync<PayoutReportDto>();
        Assert.NotNull(report);
        Assert.NotEmpty(report!.Rows);
        Assert.True(report.TotalPayout > 0);
    }
}
