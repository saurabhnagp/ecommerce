using System.Text;
using AmCart.UserService.Api.Services;
using AmCart.UserService.Infrastructure;
using AmCart.UserService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpClient<ProductServiceProxy>((sp, client) =>
{
    var url = builder.Configuration["ProductService:BaseUrl"]?.Trim().TrimEnd('/');
    if (string.IsNullOrEmpty(url))
        url = "http://localhost:5002";
    client.BaseAddress = new Uri(url + "/");
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddHealthChecks()
    .AddDbContextCheck<UserDbContext>("database", failureStatus: HealthStatus.Unhealthy);

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
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

// Apply pending EF Core migrations on startup (code-first: DB stays in sync with the app).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
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

app.Run();
