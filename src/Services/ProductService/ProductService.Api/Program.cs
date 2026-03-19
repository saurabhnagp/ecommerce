using AmCart.ProductService.Infrastructure;
using AmCart.ProductService.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProductDbContext>("database", failureStatus: HealthStatus.Unhealthy);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("api/v1/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var status = report.Status == HealthStatus.Healthy ? "healthy" : "unhealthy";
        var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";
        var result = System.Text.Json.JsonSerializer.Serialize(new { status, service = "ProductService", version, checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() }) });
        await context.Response.WriteAsync(result);
    }
});

app.Run();
