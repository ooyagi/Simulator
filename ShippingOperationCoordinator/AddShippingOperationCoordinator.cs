using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShippingOperationCoordinator.Interfaces;
using ShippingOperationCoordinator.Models;
using ShippingOperationCoordinator.Services;

namespace ShippingOperationCoordinator;

public static class AddShippingOperationCoordinatorExtensions
{
    public static IServiceCollection AddShippingOperationCoordinator<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> action
    ) where T : DbContext, IShippingOperationCoordinatorDbContext {
        services.Configure<ShippingOperationSettings>(configuration.GetSection("ShippingOperationSettings"));
        services.AddDbContext<IShippingOperationCoordinatorDbContext, T>(action, ServiceLifetime.Scoped);

        // 公開
        services.AddScoped<IInitializationService, InitializationService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<IShippingStationLoader, ShippingStationLoader>();
        services.AddScoped<IChangeInventoryPalletService, ReturnInventoryPalletService>();
        services.AddScoped<IChangeShippingPalletService, ReturnShippingPalletService>();

        // 本来は外部サービスだが搬送回数の試算にあたっては該当ドメインを作成しないのでここに定義
        // services.AddScoped<ITransferService, TransferService>();
        // 複数実装を登録する場合は IEnumerable<ITransferService> での注入を検討

        // 内部利用
        services.AddSingleton<ShippingStorageEventSubscriber>();
        services.AddSingleton<TemporaryStorageEventSubscriber>();

        services.AddScoped<Services.ITakeShippingPalletService, TakeShippingPalletService>();
        services.AddScoped<Services.ITakeInventoryPalletService, TakeInventoryPalletService>();
        services.AddScoped<IShippingStationManagementService, ShippingStationManagementService>();
        services.AddScoped<IDetermineTransferItemService, DetermineTransferItemService>();
        services.AddScoped<IReturnInventoryPalletSelector, ReturnInventoryPalletSelector>();
        services.AddScoped<IReturnShippingPalletSelector, ReturnShippingPalletSelector>();
        services.AddScoped<ITakeInventoryPalletSelector, TakeInventoryPalletSelector>();
        services.AddScoped<ITakeShippingPalletSelector, TakeShippingPalletSelector>();

        services.AddHostedService<SubscriveWorker>();
        return services;
    }
}
