using ContosoClaims.Api.Auth;
using ContosoClaims.Api.Dtos;
using ContosoClaims.Api.Legacy;
using Microsoft.AspNetCore.Mvc;

namespace ContosoClaims.Api.Controllers;

[ApiController]
[Route("api/reports")]
[ServiceFilter(typeof(AdjusterAuthFilter))]
public class ReportsController : ControllerBase
{
    private readonly PayoutReportBuilder _payoutReportBuilder;

    public ReportsController(PayoutReportBuilder payoutReportBuilder)
    {
        _payoutReportBuilder = payoutReportBuilder;
    }

    [HttpGet("payouts")]
    public async Task<ActionResult<PayoutReportDto>> GetPayouts([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var report = await _payoutReportBuilder.BuildAsync(from, to);
        return Ok(report);
    }
}
