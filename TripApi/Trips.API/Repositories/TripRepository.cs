using Microsoft.Data.SqlClient;
using Trips.API.Entities;
using Trips.API.Repositories.Abstractions;

namespace Trips.API.Repositories;

public class TripRepository : ITripRepository
{
    private readonly string _connectionString;

    public TripRepository(IConfiguration cfg)
    {
        _connectionString = cfg.GetConnectionString("Default") ??
                            throw new ArgumentNullException(nameof(cfg),
                                "Default connection string configuration is missing");
    }

    // To reduce loading unnecessary data, we may expand the method to optionally include destinations and participants
    public async Task<List<Trip>> GetAllTripsAsync(CancellationToken token = default)
    {
        Dictionary<int, Trip> trips = new();
        const string query = """
                             SELECT t.IdTrip,
                                    t.Name,
                                    t.Description,
                                    t.DateFrom,
                                    t.DateTo,
                                    t.MaxPeople,
                                    c.IdCountry,
                                    c.Name,
                                    cl.IdClient,
                                    cl.FirstName,
                                    cl.LastName,
                                    cl.Email,
                                    cl.Telephone,
                                    cl.Pesel,
                                    clt.RegisteredAt,
                                    clt.PaymentDate
                             FROM Country_Trip as ct 
                                 LEFT JOIN Country as c on ct.IdCountry = c.IdCountry 
                                 LEFT JOIN Trip as t on ct.IdTrip = t.IdTrip
                                 LEFT JOIN Client_Trip as clt on t.IdTrip = clt.IdTrip
                                 LEFT JOIN Client as cl on clt.IdClient = cl.IdClient
                             """;

        await using SqlConnection con = new(_connectionString);
        await using SqlCommand cmd = new(query, con);
        await con.OpenAsync(token);
        await using var reader = await cmd.ExecuteReaderAsync(token);

        while (await reader.ReadAsync(token))
        {
            var tripId = reader.GetInt32(0);

            if (!trips.TryGetValue(tripId, out var trip))
            {
                trip = new Trip
                {
                    Id = tripId,
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                    Destinations = []
                };
                trips.Add(tripId, trip);
            }

            if (!reader.IsDBNull(6))
            {
                var countryId = reader.GetInt32(6);
                var countryExists = trip.Destinations?.Any(c => c.Id == countryId) ?? false;
                if (!countryExists)
                {
                    Country country = new()
                    {
                        Id = countryId,
                        Name = reader.GetString(7)
                    };

                    trip.Destinations!.Add(country);
                }
            }

            if (!reader.IsDBNull(8))
            {
                var clientId = reader.GetInt32(8);
                var participantExists = trip.Participants?.Any(p => p.Client.Id == clientId) ?? false;

                if (!participantExists)
                {
                    Client client = new()
                    {
                        Id = clientId,
                        FirstName = reader.GetString(9),
                        LastName = reader.GetString(10),
                        Email = reader.GetString(11),
                        Telephone = reader.GetString(12),
                        Pesel = reader.GetString(13)
                    };

                    ClientTrip clientTrip = new()
                    {
                        Client = client,
                        Trip = trip,
                        RegisteredAt = reader.GetInt32(14),
                        PaymentDate = reader.IsDBNull(15) ? null : reader.GetInt32(15)
                    };

                    trip.Participants!.Add(clientTrip);
                }
            }
        }
        return trips.Values.ToList();
    }

    // To reduce loading unnecessary data, we may expand the method to optionally include destinations and participants
    public async Task<Trip?> GetTripByIdAsync(int tripId, CancellationToken token = default)
    {
        var tripExists = await TripExistsAsync(tripId, token);
        if (!tripExists)
            return null;

        Dictionary<int, Trip> trips = new();
        const string query = """
                             SELECT t.IdTrip,
                                    t.Name,
                                    t.Description,
                                    t.DateFrom,
                                    t.DateTo,
                                    t.MaxPeople,
                                    c.IdCountry,
                                    c.Name,
                                    cl.IdClient,
                                    cl.FirstName,
                                    cl.LastName,
                                    cl.Email,
                                    cl.Telephone,
                                    cl.Pesel,
                                    clt.RegisteredAt,
                                    clt.PaymentDate
                             FROM Country_Trip as ct 
                                 LEFT JOIN Country as c on ct.IdCountry = c.IdCountry 
                                 LEFT JOIN Trip as t on ct.IdTrip = t.IdTrip
                                 LEFT JOIN Client_Trip as clt on t.IdTrip = clt.IdTrip
                                 LEFT JOIN Client as cl on clt.IdClient = cl.IdClient
                             WHERE ct.IdTrip = @tripId;    
                             """;

        await using SqlConnection con = new(_connectionString);
        await using SqlCommand cmd = new(query, con);
        await con.OpenAsync(token);
        cmd.Parameters.AddWithValue("@tripId", tripId);
        await using var reader = await cmd.ExecuteReaderAsync(token);

        while (await reader.ReadAsync(token))
        {
            if (!trips.TryGetValue(tripId, out var trip))
            {
                trip = new Trip
                {
                    Id = tripId,
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                    Destinations = [],
                    Participants = []
                };
                trips.Add(tripId, trip);
            }

            // Add country if it exists
            if (!reader.IsDBNull(6))
            {
                var countryId = reader.GetInt32(6);
                var countryExists = trip.Destinations?.Any(c => c.Id == countryId) ?? false;
                if (!countryExists)
                {
                    Country country = new()
                    {
                        Id = countryId,
                        Name = reader.GetString(7)
                    };

                    trip.Destinations!.Add(country);
                }
            }

            // Add client if it exists
            if (!reader.IsDBNull(8))
            {
                var clientId = reader.GetInt32(8);
                var participantExists = trip.Participants?.Any(p => p.Client.Id == clientId) ?? false;

                if (!participantExists)
                {
                    Client client = new()
                    {
                        Id = clientId,
                        FirstName = reader.GetString(9),
                        LastName = reader.GetString(10),
                        Email = reader.GetString(11),
                        Telephone = reader.GetString(12),
                        Pesel = reader.GetString(13)
                    };

                    ClientTrip clientTrip = new()
                    {
                        Client = client,
                        Trip = trip,
                        RegisteredAt = reader.GetInt32(14),
                        PaymentDate = reader.IsDBNull(15) ? null : reader.GetInt32(15)
                    };

                    trip.Participants!.Add(clientTrip);
                }
            }
        }

        return trips.Values.FirstOrDefault();
    }

    public async Task<bool> TripExistsAsync(int tripId, CancellationToken token = default)
    {
        if (tripId <= 0)
            return false;

        const string query = """
                             SELECT 
                                 IIF(EXISTS (SELECT 1 FROM Trip 
                                         WHERE Trip.IdTrip = @tripId), 1, 0) AS TripExists;   
                             """;

        await using SqlConnection con = new(_connectionString);
        await using SqlCommand cmd = new(query, con);
        await con.OpenAsync(token);
        cmd.Parameters.AddWithValue("@tripId", tripId);

        var result = (int)await cmd.ExecuteScalarAsync(token);
        return result == 1;
    }
}