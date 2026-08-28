using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// CRITICAL: stdout is the MCP JSON-RPC transport for a stdio server. Any log line written to
// stdout (e.g. from the default console logger) corrupts the protocol stream. Clear all logging
// providers here; if diagnostics are ever needed, add a provider that writes to stderr instead.
builder.Logging.ClearProviders();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
