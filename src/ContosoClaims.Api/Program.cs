using ContosoClaims.Api.Auth;
using ContosoClaims.Api.Data;
using ContosoClaims.Api.Legacy;
using ContosoClaims.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("ClaimsDb")
    ?? "Server=127.0.0.1;Port=3307;User ID=root;Password=ContosoDemo!23;Database=contoso_claims";

builder.Services.AddDbContext<ClaimsDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<AdjusterAuthFilter>();
builder.Services.AddScoped<ClaimService>();
builder.Services.AddScoped<PolicyService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<PayoutReportBuilder>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
