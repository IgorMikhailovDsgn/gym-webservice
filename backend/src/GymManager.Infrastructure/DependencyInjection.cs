using GymManager.Application.Abstractions;
using GymManager.Infrastructure.Persistence;
using GymManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("GymDb")
            ?? throw new InvalidOperationException(
                "Строка подключения 'GymDb' не задана. " +
                "Проверьте секцию ConnectionStrings в appsettings.json.");

        // AddDbContext регистрирует контекст как Scoped: один экземпляр на
        // HTTP-запрос. DbContext не потокобезопасен и хранит состояние
        // отслеживания изменений, поэтому дольше жить не должен.
        services.AddDbContext<GymDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();

        return services;
    }
}
