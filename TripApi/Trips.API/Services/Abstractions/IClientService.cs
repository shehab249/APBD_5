using Trips.API.Contracts.Requests;
using Trips.API.Entities;

namespace Trips.API.Services.Abstractions;

public interface IClientService
{
    public Task<ICollection<ClientTrip>?> GetAllClientTripsAsync(int clientId, CancellationToken token = default);
    public Task<int> CreateClientAsync(CreateClientRequest client, CancellationToken token = default);
}