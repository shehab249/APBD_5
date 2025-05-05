using Trips.API.Repositories.Abstractions;

namespace Trips.API.Repositories.Extensions;

public static class ServicesCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();

        return services;
    }
}