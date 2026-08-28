using ContosoClaims.Api.Auth;
using ContosoClaims.Api.Dtos;
using ContosoClaims.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContosoClaims.Api.Controllers;

[ApiController]
[Route("api/policies")]
[ServiceFilter(typeof(AdjusterAuthFilter))]
public class PoliciesController : ControllerBase
{
    private readonly PolicyService _policyService;

    public PoliciesController(PolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PolicyDto>>> GetPolicies([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 20;

        var result = await _policyService.GetPoliciesAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PolicyDto>> GetById(int id)
    {
        var policy = await _policyService.GetPolicyByIdAsync(id);
        if (policy is null)
        {
            return NotFound();
        }

        return Ok(policy);
    }
}
