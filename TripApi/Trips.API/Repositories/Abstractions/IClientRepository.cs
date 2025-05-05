using Trips.API.Entities;

namespace Trips.API.Repositories.Abstractions;

public interface IClientRepository
{
    public Task<bool> ClientExistsAsync(int clientId, CancellationToken token = default);
    public Task<Client?> GetClientByIdAsync(int clientId, CancellationToken token = default);
    public Task<List<ClientTrip>?> GetClientTripsAsync(int clientId, CancellationToken token = default);
    public Task<bool> CreateClientTripAsync(ClientTrip clientTrip, CancellationToken token = default);
    public Task<Client> CreateClientAsync(Client client, CancellationToken token = default);
    public ValueTask<bool> ClientExistsByPeselAsync(string pesel, CancellationToken token = default);
}