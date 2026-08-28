using System.Net;
using System.Net.Http.Json;
using ContosoClaims.Api.Dtos;

namespace ContosoClaims.Tests;

public class PoliciesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PoliciesControllerTests(CustomWebApplicationFactory factory)
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
    public async Task GetPolicies_ReturnsPagedResult()
    {
        var adjusterId = TestData.GetAnyAdjusterId(_factory);
        var client = ClientWithAdjuster(adjusterId);

        var response = await client.GetAsync("/api/policies");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<PolicyDto>>();
        Assert.NotNull(result);
        Assert.True(result!.TotalCount > 0);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetPolicyById_WhenExists_Returns200()
    {
        var adjusterId = TestData.GetAnyAdjusterId(_factory);
        var policyId = TestData.GetAnyPolicyId(_factory);
        var client = ClientWithAdjuster(adjusterId);

        var response = await client.GetAsync($"/api/policies/{policyId}");
        response.EnsureSuccessStatusCode();

        var policy = await response.Content.ReadFromJsonAsync<PolicyDto>();
        Assert.NotNull(policy);
        Assert.Equal(policyId, policy!.Id);
    }

    [Fact]
    public async Task GetPolicyById_WhenMissing_Returns404()
    {
        var adjusterId = TestData.GetAnyAdjusterId(_factory);
        var client = ClientWithAdjuster(adjusterId);

        var response = await client.GetAsync("/api/policies/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPolicies_WithoutHeader_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/policies");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
