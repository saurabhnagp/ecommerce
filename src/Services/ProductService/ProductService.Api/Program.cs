using System.Security.Claims;
using System.Text;
using AmCart.ProductService.Infrastructure;
using AmCart.ProductService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProductDbContext>("database", failureStatus: HealthStatus.Unhealthy);

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "amcart";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "amcart-api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };

        // Optional bearer on anonymous endpoints (e.g. GET reviews): invalid/expired token must not 401 the whole request.
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var endpoint = context.HttpContext.GetEndpoint();
                if (endpoint is null)
                    return Task.CompletedTask;

                var requiresAuth = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>().Any();
                if (!requiresAuth)
                    context.NoResult();

                return Task.CompletedTask;
            }
        };
    });

// builder.Services.AddCors(options =>
// {
//     options.AddDefaultPolicy(policy =>
//     {
//         var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
//                       ?? new[] { "http://localhost:5173" };
//         policy.WithOrigins(origins)
//               .AllowAnyHeader()
//               .AllowAnyMethod()
//               .AllowCredentials();
//     });
// });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// app.UseCors();
app.UseAuthentication();
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
