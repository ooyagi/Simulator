using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShippingOperationCoordinator.Interfaces;
using ShippingOperationCoordinator.Services;

namespace ShippingOperationCoordinator;

public static class AddShippingOperationCoordinatorExtensions
{
    public static IServiceCollection AddShippingOperationCoordinator<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> action
    ) where T : DbContext  {

        // 公開
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<IChangeInventoryPalletService, ReturnInventoryPalletService>();
        services.AddScoped<IChangeShippingPalletService, ReturnShippingPalletService>();

        // 内部利用
        services.AddSingleton<ShippingStorageEventSubscriber>();
        services.AddSingleton<TemporaryStorageEventSubscriber>();

        services.AddScoped<IDetermineTransferItemService, DetermineTransferItemService>();
        services.AddScoped<IReturnInventoryPalletSelector, ReturnInventoryPalletSelector>();
        services.AddScoped<IReturnShippingPalletSelector, ReturnShippingPalletSelector>();
        services.AddScoped<ITakeInventoryPalletSelector, TakeInventoryPalletSelector>();
        services.AddScoped<ITakeShippingPalletSelector, TakeShippingPalletSelector>();

        return services;
    }
}
