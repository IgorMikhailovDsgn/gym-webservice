using GymManager.Application.Clients;
using GymManager.Application.Tickets;
using Microsoft.Extensions.DependencyInjection;

namespace GymManager.Application;

/// <summary>
/// Каждый слой сам объявляет, что регистрировать. Program.cs вызывает один
/// метод вместо того, чтобы знать про внутренние классы слоя.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scoped — один экземпляр на HTTP-запрос, согласовано с DbContext.
        // Singleton, держащий Scoped-зависимость, обращался бы к уже
        // уничтоженному объекту (captive dependency).
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ITicketService, TicketService>();

        return services;
    }
}
