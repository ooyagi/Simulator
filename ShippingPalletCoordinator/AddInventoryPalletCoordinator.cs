using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;
using ShippingPalletCoordinator.Services;

namespace ShippingPalletCoordinator;

public static class AddShippingPalletCoordinatorExtensions
{
    public static IServiceCollection AddShippingPalletCoordinator<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> action
    ) where T : DbContext, IShippingPalletCoordinatorDbContext {
        services.AddDbContext<IShippingPalletCoordinatorDbContext, T>(action, ServiceLifetime.Scoped);

        // Sigleton
        services.AddSingleton<ShippingStorageEventPublisher>();
        services.AddSingleton<IShippingStorageEventPublisher>(x => x.GetRequiredService<ShippingStorageEventPublisher>());
        services.AddSingleton<ShippingOperationCoordinator.Interfaces.IShippingStorageEventObserver>(x => x.GetRequiredService<ShippingStorageEventPublisher>());

        // 公開
        // services.AddScoped<IInventoryStorageManagementService, InventoryStorageManagementService>();

        // ShippingOperatonCoordinator向け
        services.AddScoped<ShippingOperationCoordinator.Interfaces.IShippingStorageLoader, ShippingStorageLoader>();
        services.AddScoped<ShippingOperationCoordinator.Interfaces.IShikakariStorageLoader, ShikakariStorageLoader>();
        services.AddScoped<ShippingOperationCoordinator.Interfaces.IPutonShippingPalletItemService, PutonShippingPalletItemService>();
        services.AddScoped<ShippingOperationCoordinator.Interfaces.IReturnShippingPalletService, ReturnShippingPalletService>();
        services.AddScoped<ShippingOperationCoordinator.Interfaces.ITakeShippingPalletService, TakeShippingPalletService>();

        // 内部向け
        services.AddScoped<IShippingPalletLoader, ShippingPalletLoader>();
        services.AddScoped<IShippingStorageLoader, ShippingStorageLoader>();
        services.AddScoped<IShikakariStorageLoader, ShikakariStorageLoader>();
        // services.AddScoped<IInboundShippingPalletServices, InboundShippingPalletServices>();
        services.AddScoped<ITransportRequestService, TransportRequestService>();

        return services;
    }
}
