using GymManager.Application.Clients;
using GymManager.Application.Tickets;
using GymManager.Application.Visits;
using GymManager.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace GymManager.Application;

/// Каждый слой сам объявляет, что регистрировать. Program.cs вызывает один
/// метод вместо того, чтобы знать про внутренние классы слоя.
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scoped — один экземпляр на HTTP-запрос, согласовано с DbContext.
        // Singleton, держащий Scoped-зависимость, обращался бы к уже
        // уничтоженному объекту (captive dependency).
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IVisitService, VisitService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
