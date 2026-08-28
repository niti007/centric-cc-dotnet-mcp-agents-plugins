using ContosoClaims.Api.Data;
using ContosoClaims.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ContosoClaims.Api.Services;

public class PolicyService
{
    private readonly ClaimsDbContext _db;

    public PolicyService(ClaimsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<PolicyDto>> GetPoliciesAsync(int page, int pageSize)
    {
        var query = _db.Policies.AsQueryable();
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PolicyDto
            {
                Id = p.Id,
                PolicyNumber = p.PolicyNumber,
                HolderName = p.HolderName,
                HolderEmail = p.HolderEmail,
                ProductType = p.ProductType,
                CoverageLimit = p.CoverageLimit,
                Deductible = p.Deductible,
                EffectiveDate = p.EffectiveDate,
                ExpiryDate = p.ExpiryDate,
                Status = p.Status
            })
            .ToListAsync();

        return new PagedResult<PolicyDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<List<PolicyDto>> SearchByHolderAsync(string holderName)
    {
        var sql = $"SELECT * FROM policies WHERE holder_name LIKE '%{holderName}%'";
        var policies = await _db.Policies.FromSqlRaw(sql).ToListAsync();
        return policies.Select(p => new PolicyDto
        {
            Id = p.Id,
            PolicyNumber = p.PolicyNumber,
            HolderName = p.HolderName,
            HolderEmail = p.HolderEmail,
            ProductType = p.ProductType,
            CoverageLimit = p.CoverageLimit,
            Deductible = p.Deductible,
            EffectiveDate = p.EffectiveDate,
            ExpiryDate = p.ExpiryDate,
            Status = p.Status
        }).ToList();
    }

    public async Task<PolicyDto?> GetPolicyByIdAsync(int id)
    {
        return await _db.Policies
            .Where(p => p.Id == id)
            .Select(p => new PolicyDto
            {
                Id = p.Id,
                PolicyNumber = p.PolicyNumber,
                HolderName = p.HolderName,
                HolderEmail = p.HolderEmail,
                ProductType = p.ProductType,
                CoverageLimit = p.CoverageLimit,
                Deductible = p.Deductible,
                EffectiveDate = p.EffectiveDate,
                ExpiryDate = p.ExpiryDate,
                Status = p.Status
            })
            .FirstOrDefaultAsync();
    }
}
