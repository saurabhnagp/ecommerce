using AmCart.ProductService.Application.Interfaces;
using AmCart.ProductService.Application.Services;
using AmCart.ProductService.Infrastructure.Data;
using AmCart.ProductService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AmCart.ProductService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ProductDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICheckoutRepository, CheckoutRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IProductService, AmCart.ProductService.Application.Services.ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IProductReviewService, ProductReviewService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
