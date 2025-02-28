using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ProductionPlanManagement.Interfaces;
using ProductionPlanManagement.Models;
using ProductionPlanManagement.Services;

namespace ProductionPlanManagement;

public static class AddProductionPlanManagementExtensions
{
    public static IServiceCollection AddProductionPlan<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> action
    ) where T : DbContext, IProductionPlanmanagementDbContext {
        services.Configure<ProductionPlanConfig>(configuration.GetSection("ProductionPlanSettings"));
        services.AddDbContext<IProductionPlanmanagementDbContext, T>(action, ServiceLifetime.Scoped);

        // 公開
        services.AddScoped<ILoadProductionPlanService, LoadProductionPlanService>();

        // 内部利用
        services.AddScoped<ProductionPlanFileReader>();

        return services;
    }
}
