using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Services;
using InventoryPalletCoordinator.Models;
using Microsoft.Win32.SafeHandles;

namespace InventoryPalletCoordinator;

public static class AddInventoryPalletCoordinatorExtensions
{
    public static IServiceCollection AddInventoryPalletCoordinator<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> action
    ) where T : DbContext, IInventoryPalletCoordinatorDbContext {
        services.AddDbContext<IInventoryPalletCoordinatorDbContext, T>(action, ServiceLifetime.Scoped);

        // Sigleton
        services.AddSingleton<TemporaryStorageEventPublisher>();
        services.AddSingleton<ITemporaryStorageEventPublisher>(x => x.GetRequiredService<TemporaryStorageEventPublisher>());
        services.AddSingleton<ShippingOperationCoordinator.Interfaces.ITemporaryStorageEventObserver>(x => x.GetRequiredService<TemporaryStorageEventPublisher>());

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
        services.AddScoped<IInventoryStorageLoader, InventoryStorageLoader>();
        services.AddScoped<IInboundInventoryPalletServices, InboundInventoryPalletServices>();
        services.AddScoped<IPickupTemporaryStorageService, PickupTemporaryStorageService>();
        services.AddScoped<IPickupInventoryStorageService, PickupInventoryStorageService>();
        services.AddScoped<IPlaceTemporaryStorageService, PlaceTemporaryStorageService>();
        services.AddScoped<IPlaceInventoryStorageService, PlaceInventoryStorageService>();
        services.AddScoped<ITransportRequestService, TransportRequestService>();

        return services;
    }
}
