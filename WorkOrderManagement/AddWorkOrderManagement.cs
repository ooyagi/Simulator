using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkOrderManagement.Interfaces;
using WorkOrderManagement.Services;

namespace WorkOrderManagement;

public static class AddWorkOrderManagementExtensions
{
    public static IServiceCollection AddWorkOrderManagement<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> action
    ) where T: DbContext, IWorkOrderDbContext {
        services.AddDbContext<IWorkOrderDbContext, T>(action, ServiceLifetime.Scoped);

        // 外部公開
        services.AddScoped<IWorkOrderRegister, WorkOrderRegister>();

        // ShippingOperationCoordinator向け
        services.AddScoped<ShippingOperationCoordinator.Interfaces.IWorkOrderLoader, WorkOrderLoader>();

        // ShippingPalletCoordinator向け
        services.AddScoped<ShippingPalletCoordinator.Interfaces.IWorkOrderLoader, WorkOrderLoader>();
        return services;
    }
}
