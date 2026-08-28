using ContosoClaims.Api.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ContosoClaims.Api.Auth;

/// <summary>
/// Resolves the calling adjuster from the X-Adjuster-Id header. 401s when the header
/// is missing or does not match a known adjuster. Downstream actions can read the
/// resolved id from HttpContext.Items["AdjusterId"].
/// </summary>
public class AdjusterAuthFilter : IAsyncActionFilter
{
    public const string HeaderName = "X-Adjuster-Id";
    public const string ContextKey = "AdjusterId";

    private readonly ClaimsDbContext _db;

    public AdjusterAuthFilter(ClaimsDbContext db)
    {
        _db = db;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var headerValue) ||
            !int.TryParse(headerValue, out var adjusterId))
        {
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
            return;
        }

        var exists = await _db.Adjusters.AnyAsync(a => a.Id == adjusterId);
        if (!exists)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
            return;
        }

        context.HttpContext.Items[ContextKey] = adjusterId;
        await next();
    }
}
