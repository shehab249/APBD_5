using Trips.API.Contracts.Responses;
using Trips.API.Entities;

namespace Trips.API.Mappers;

public static class ClientTripMappers
{
    public static GetAllClientTripsResponse? MapToGetAllClientTripsResponse(this ClientTrip clientTrip)
    {
        if (clientTrip.Trip is null)
            return null;
        
        return new GetAllClientTripsResponse
        {
            Id = clientTrip.Trip.Id,
            Name = clientTrip.Trip.Name,
            Description = clientTrip.Trip.Description,
            DateFrom = clientTrip.Trip.DateFrom,
            DateTo = clientTrip.Trip.DateFrom,
            MaxPeople = clientTrip.Trip.MaxPeople,
            RegisteredAt = clientTrip.RegisteredAt,
            PaymentDate = clientTrip.PaymentDate,
            Countries = clientTrip.Trip.Destinations.Select(country => new CountryResponse(country.Id, country.Name)).ToList()
        };
    }

    public static ICollection<GetAllClientTripsResponse?> MapToGetAllTripsResponse(
        this ICollection<ClientTrip> clientTrips)
    {
        return clientTrips.Select(x => x.MapToGetAllClientTripsResponse()).ToList();
    }
}