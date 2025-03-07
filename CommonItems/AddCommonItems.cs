using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommonItems.Interfaces;
using CommonItems.Services;

namespace CommonItems;

public static class AddCommonItemsExtensions
{
    public static IServiceCollection AddCommonItems<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> action
    ) where T : DbContext, ICommonItemsDbContext {
        services.AddDbContext<ICommonItemsDbContext, T>(action, ServiceLifetime.Scoped);

        // 公開
        services.AddScoped<ITransportRecordRegister, TransportRecordRegister>();

        return services;
    }
}
