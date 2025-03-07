using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Services;

namespace InventoryPalletCoordinator;

public static class AddInventoryPalletCoordinatorExtensions
{
    public static IServiceCollection AddInventoryPalletCoordinator<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> action
    ) where T : DbContext, IInventoryPalletCoordinatorDbContext {
        services.AddDbContext<IInventoryPalletCoordinatorDbContext, T>(action, ServiceLifetime.Scoped);

        // 公開
        services.AddScoped<IInventoryStorageManagementService, InventoryStorageManagementService>();

        // ShippingOperatonCoordinator向け
        services.AddScoped<ShippingOperationCoordinator.Interfaces.IInventoryStorageLoader, InventoryStorageLoader>();
        services.AddScoped<ShippingOperationCoordinator.Interfaces.ITempStorageLoader, TemporaryStorageLoader>();
        services.AddScoped<ShippingOperationCoordinator.Interfaces.IPickupInventoryPalletItemService, PickupInventoryPalletItemService>();
        services.AddScoped<ShippingOperationCoordinator.Interfaces.IReturnInventoryPalletService, ReturnInventoryPalletService>();
        services.AddScoped<ShippingOperationCoordinator.Interfaces.ITakeInventoryPalletService, TakeInventoryPalletService>();

        // 内部向け
        services.AddScoped<IInventoryPalletLoader, InventoryPalletLoader>();
        services.AddScoped<ITemporaryStorageLoader, TemporaryStorageLoader>();
        services.AddScoped<IInboundInventoryPalletServices, InboundInventoryPalletServices>();

        return services;
    }
}
