using Trips.API.Entities;

namespace Trips.API.Services.Abstractions;

public interface ITripService
{
    public Task<ICollection<Trip>> GetAllTripsWithCountriesAsync(CancellationToken token = default);
    public ValueTask<bool> RegisterClientToTripAsync(int clientId, int tripId, CancellationToken token = default);
}