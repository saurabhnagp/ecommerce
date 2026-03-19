using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Application.Services;
using AmCart.UserService.Infrastructure.Data;
using AmCart.UserService.Infrastructure.Repositories;
using AmCart.UserService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AmCart.UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<UserDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, Application.Services.UserService>();
        services.AddScoped<IAddressService, AddressService>();

        return services;
    }
}
