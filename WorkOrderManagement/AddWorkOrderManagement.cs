using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkOrderManagement.Services;

namespace WorkOrderManagement;

public static class AddWorkOrderManagementExtensions
{
    public static IServiceCollection AddWorkOrderManagement<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> action
    ) where T : DbContext {
        services.AddScoped<ShippingOperationCoordinator.Interfaces.IWorkOrderLoader, WorkOrderLoader>();
        return services;
    }
}
