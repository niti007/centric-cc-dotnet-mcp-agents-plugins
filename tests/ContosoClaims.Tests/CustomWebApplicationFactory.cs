using Microsoft.AspNetCore.Mvc.Testing;

namespace ContosoClaims.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Uses the live MySQL configured via appsettings.json / connection string default
    // (127.0.0.1:3307, contoso_claims). No overrides needed - tests run against the
    // real seeded database.
}
