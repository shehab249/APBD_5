using Trips.API.Entities;

namespace Trips.API.Repositories.Abstractions;

public interface ITripRepository
{
    public Task<List<Trip>> GetAllTripsAsync(CancellationToken token = default);

    public Task<Trip?> GetTripByIdAsync(int tripId, CancellationToken token = default);
    public Task<bool> TripExistsAsync(int tripId, CancellationToken token = default);
}