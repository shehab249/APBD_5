using Trips.API.Contracts.Responses;
using Trips.API.Entities;

namespace Trips.API.Mappers;

public static class TripMappers
{
    public static GetAllTripsResponse MapToGetAllTripsResponse(this Trip trip)
    {
        return new GetAllTripsResponse
        {
            Id = trip.Id,
            Name = trip.Name,
            Description = trip.Description,
            DateFrom = trip.DateFrom,
            DateTo = trip.DateTo,
            MaxPeople = trip.MaxPeople,
            Countries = trip.Destinations.Select(country => new CountryResponse(country.Id, country.Name)).ToList()
        };
    }

    public static ICollection<GetAllTripsResponse> MapToGetAllTripsResponse(this ICollection<Trip> trips)
    {
        return trips.Select(x => x.MapToGetAllTripsResponse()).ToList();
    }
}