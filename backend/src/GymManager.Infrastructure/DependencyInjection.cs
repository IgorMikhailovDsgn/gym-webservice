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
        services.AddDbContext<GymDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("GymDb")));

        services.AddScoped<IClientRepository, ClientRepository>();

        return services;
    }
}