using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ContosoClaims.Api.Dtos;

namespace ContosoClaims.Tests;

public class ClaimsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ClaimsControllerTests(CustomWebApplicationFactory factory)
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
    public async Task GetClaims_WithoutHeader_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/claims");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetClaims_WithValidHeader_ReturnsPagedResult()
    {
        var adjusterId = TestData.GetAnyAdjusterId(_factory);
        var client = ClientWithAdjuster(adjusterId);

        var response = await client.GetAsync("/api/claims");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ClaimListItemDto>>();
        Assert.NotNull(result);
        Assert.True(result!.TotalCount > 0);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetClaims_FilteredByStatus_OnlyReturnsThatStatus()
    {
        var adjusterId = TestData.GetAnyAdjusterId(_factory);
        var client = ClientWithAdjuster(adjusterId);

        var response = await client.GetAsync("/api/claims?status=paid&pageSize=50");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ClaimListItemDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Items);
        Assert.All(result.Items, item => Assert.Equal("paid", item.Status));
    }

    [Fact]
    public async Task GetClaims_RespectsPageSize()
    {
        var adjusterId = TestData.GetAnyAdjusterId(_factory);
        var client = ClientWithAdjuster(adjusterId);

        var response = await client.GetAsync("/api/claims?page=1&pageSize=5");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ClaimListItemDto>>();
        Assert.NotNull(result);
        Assert.True(result!.Items.Count <= 5);
    }

    [Fact]
    public async Task GetById_WhenAssignedToCaller_Returns200()
    {
        var (claimId, adjusterId) = TestData.GetAssignedClaim(_factory);
        var client = ClientWithAdjuster(adjusterId);

        var response = await client.GetAsync($"/api/claims/{claimId}");
        response.EnsureSuccessStatusCode();

        var claim = await response.Content.ReadFromJsonAsync<ClaimDetailDto>();
        Assert.NotNull(claim);
        Assert.Equal(claimId, claim!.Id);
    }

    [Fact]
    public async Task GetById_WhenNotAssignedToCaller_Returns403()
    {
        var (claimId, assignedAdjusterId) = TestData.GetAssignedClaim(_factory);
        var otherAdjusterId = TestData.GetAdjusterIdOtherThan(_factory, assignedAdjusterId);

        var client = ClientWithAdjuster(otherAdjusterId);
        var response = await client.GetAsync($"/api/claims/{claimId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenClaimMissing_Returns404()
    {
        var adjusterId = TestData.GetAnyAdjusterId(_factory);
        var client = ClientWithAdjuster(adjusterId);

        var response = await client.GetAsync("/api/claims/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Search_ReturnsResults()
    {
        var adjusterId = TestData.GetAnyAdjusterId(_factory);
        var client = ClientWithAdjuster(adjusterId);

        var response = await client.GetAsync("/api/claims/search?q=a");
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<List<ClaimListItemDto>>();
        Assert.NotNull(results);
    }

    [Fact]
    public async Task AddNote_WhenAssignedToCaller_Returns200AndPersists()
    {
        var (claimId, adjusterId) = TestData.GetAssignedClaim(_factory);
        var client = ClientWithAdjuster(adjusterId);

        var response = await client.PostAsJsonAsync($"/api/claims/{claimId}/notes", new AddClaimNoteRequest
        {
            Body = "Follow-up call scheduled with the policyholder."
        });
        response.EnsureSuccessStatusCode();

        var note = await response.Content.ReadFromJsonAsync<ClaimNoteDto>();
        Assert.NotNull(note);
        Assert.Equal("Follow-up call scheduled with the policyholder.", note!.Body);

        var detailResponse = await client.GetAsync($"/api/claims/{claimId}");
        detailResponse.EnsureSuccessStatusCode();
        var detail = await detailResponse.Content.ReadFromJsonAsync<ClaimDetailDto>();
        Assert.Contains(detail!.Notes, n => n.Id == note.Id);
    }

    [Fact]
    public async Task AddNote_WhenNotAssignedToCaller_Returns403()
    {
        var (claimId, assignedAdjusterId) = TestData.GetAssignedClaim(_factory);
        var otherAdjusterId = TestData.GetAdjusterIdOtherThan(_factory, assignedAdjusterId);
        var client = ClientWithAdjuster(otherAdjusterId);

        var response = await client.PostAsJsonAsync($"/api/claims/{claimId}/notes", new AddClaimNoteRequest
        {
            Body = "Should not be allowed."
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
