using Trips.API.Entities;
using Trips.API.Exceptions;
using Trips.API.Repositories.Abstractions;
using Trips.API.Services.Abstractions;

namespace Trips.API.Services;

public class TripService : ITripService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ITripRepository _tripRepository;
    private readonly IClientRepository _clientRepository;

    public TripService(IDateTimeProvider dateTimeProvider, ITripRepository tripRepository,
        IClientRepository clientRepository)
    {
        _dateTimeProvider = dateTimeProvider;
        _tripRepository = tripRepository;
        _clientRepository = clientRepository;
    }

    public async Task<ICollection<Trip>> GetAllTripsWithCountriesAsync(CancellationToken token = default)
        => await _tripRepository.GetAllTripsAsync(token);

    public async ValueTask<bool> RegisterClientToTripAsync(int clientId, int tripId, CancellationToken token = default)
    {
        var client = await _clientRepository.GetClientByIdAsync(clientId, token);
        if (client is null)
            throw new ClientDoesNotExistException(clientId);

        var trip = await _tripRepository.GetTripByIdAsync(tripId, token);
        if (trip is null)
            throw new TripDoesNotExistException(clientId);

        if (trip.Participants.Count + 1 > trip.MaxPeople)
            throw new ParticipantsWillBeExceededException();

        var clientTrip = new ClientTrip
        {
            Trip = trip,
            Client = client,
            PaymentDate = null,
            RegisteredAt = _dateTimeProvider.UtcNow.Year * 10000 + _dateTimeProvider.UtcNow.Month * 100 +
                           _dateTimeProvider.UtcNow.Day
        };

        var result = await _clientRepository.CreateClientTripAsync(clientTrip, token);
        return result;
    }
}