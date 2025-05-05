using Trips.API.Services.Abstractions;

namespace Trips.API.Services;

public static class ServicesCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}