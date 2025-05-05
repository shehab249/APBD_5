using Microsoft.Data.SqlClient;
using Trips.API.Entities;
using Trips.API.Repositories.Abstractions;

namespace Trips.API.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly string _connectionString;

    public ClientRepository(IConfiguration cfg)
    {
        _connectionString = cfg.GetConnectionString("Default") ??
                            throw new ArgumentNullException(nameof(cfg),
                                "Default connection string is missing in configuration");
    }

    public async Task<bool> ClientExistsAsync(int clientId, CancellationToken token = default)
    {
        const string query = """
                             SELECT 
                                 IIF(EXISTS (SELECT 1 FROM Client 
                                         WHERE Client.IdClient = @clientId), 1, 0);   
                             """;

        await using SqlConnection con = new(_connectionString);
        await using SqlCommand command = new SqlCommand(query, con);
        await con.OpenAsync(token);
        command.Parameters.AddWithValue("@clientId", clientId);
        var result = Convert.ToInt32(await command.ExecuteScalarAsync(token));

        return result == 1;
    }

    public async Task<Client?> GetClientByIdAsync(int clientId, CancellationToken token = default)
    {
        const string query = """
                             SELECT IdClient, FirstName, LastName, Email, Telephone, Pesel
                             FROM Client
                             WHERE IdClient = @clientId
                             """;
        Client? client = null;

        await using SqlConnection con = new(_connectionString);
        await using SqlCommand command = new SqlCommand(query, con);
        command.Parameters.AddWithValue("@clientId", clientId);
        await con.OpenAsync(token);

        var reader = await command.ExecuteReaderAsync(token);
        if (!reader.HasRows)
            return client;

        while (await reader.ReadAsync(token))
        {
            client = new Client
            {
                Id = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Email = reader.GetString(3),
                Pesel = reader.GetString(4),
                Telephone = reader.GetString(5),
                Trips = null
            };
        }

        return client;
    }

    public async Task<List<ClientTrip>?> GetClientTripsAsync(int clientId, CancellationToken token = default)
    {
        var client = await GetClientByIdAsync(clientId, token);
        if (client is null)
            return null;

        const string query = """
                             SELECT 
                                 T.IdTrip, 
                                 T.Name, 
                                 T.DateFrom, 
                                 T.DateTo, 
                                 T.Description, 
                                 T.MaxPeople, 
                                 CT.PaymentDate,
                                 CT.RegisteredAt,
                                 C.IdCountry,
                                 C.Name
                             FROM Client_Trip as CT 
                             JOIN Trip T on T.IdTrip = CT.IdTrip
                             JOIN Country_Trip as CT2 on CT2.IdTrip = T.IdTrip
                             JOIN Country as C on C.IdCountry = CT2.IdCountry
                             WHERE CT.IdClient = @clientId
                             """;

        await using SqlConnection con = new(_connectionString);
        await using SqlCommand command = new SqlCommand(query, con);
        await con.OpenAsync(token);
        
        command.Parameters.AddWithValue("@clientId", clientId);
        var reader = await command.ExecuteReaderAsync(token);
        if (!reader.HasRows)
            return [];

        var tripDictionary = new Dictionary<int, ClientTrip>();
        while (await reader.ReadAsync(token))
        {
            var tripId = reader.GetInt32(0);
            var countryId = reader.GetInt32(8);
            var countryName = reader.GetString(9);

            if (!tripDictionary.TryGetValue(tripId, out var clientTrip))
            {
                clientTrip = new ClientTrip
                {
                    Client = client,
                    Trip = new Trip
                    {
                        Id = tripId,
                        Name = reader.GetString(1),
                        DateFrom = reader.GetDateTime(2),
                        DateTo = reader.GetDateTime(3),
                        Description = reader.GetString(4),
                        MaxPeople = reader.GetInt32(5),
                        Destinations = []
                    },
                    PaymentDate = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    RegisteredAt = reader.GetInt32(7)
                };

                tripDictionary[tripId] = clientTrip;
            }

            var country = new Country { Id = countryId, Name = countryName };
            if (clientTrip.Trip?.Destinations.All(c => c.Name != countryName) == true)
            {
                clientTrip.Trip.Destinations.Add(country);
            }
        }

        return tripDictionary.Values.ToList();
    }

    public async Task<bool> CreateClientTripAsync(ClientTrip clientTrip, CancellationToken token = default)
    {
        const string query = """
                             INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
                             VALUES (@idClient, @idTrip, @registeredAt, @paymentDate)
                             """;

        await using SqlConnection con = new(_connectionString);
        await using SqlCommand command = new SqlCommand(query, con);
        await con.OpenAsync(token);

        await using var addClientTripCommand = new SqlCommand(query, con);
        addClientTripCommand.Parameters.AddWithValue("@idClient", clientTrip.Client?.Id);
        addClientTripCommand.Parameters.AddWithValue("@idTrip", clientTrip.Trip?.Id);
        addClientTripCommand.Parameters.AddWithValue("@registeredAt", clientTrip.RegisteredAt);
        addClientTripCommand.Parameters.AddWithValue("@paymentDate",
            clientTrip.PaymentDate.HasValue ? clientTrip.PaymentDate.Value : DBNull.Value);

        var rowsAffected = Convert.ToInt32(await addClientTripCommand.ExecuteScalarAsync(token));
        return rowsAffected > 0;
    }

    public async Task<Client> CreateClientAsync(Client client, CancellationToken token = default)
    {
        const string query = """
                             INSERT INTO Client(FirstName, LastName, Email, Telephone, Pesel)
                             VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)
                             SELECT SCOPE_IDENTITY();
                             """;

        await using SqlConnection con = new(_connectionString);
        await using SqlCommand cmd = new(query, con);
        await con.OpenAsync(token);
        cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
        cmd.Parameters.AddWithValue("@LastName", client.LastName);
        cmd.Parameters.AddWithValue("@Email", client.Email);
        cmd.Parameters.AddWithValue("@Telephone", client.Telephone);
        cmd.Parameters.AddWithValue("@Pesel", client.Pesel);

        var result = await cmd.ExecuteScalarAsync(token);
        client.Id = Convert.ToInt32(result);
        return client;
    }

    public async ValueTask<bool> ClientExistsByPeselAsync(string pesel, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(pesel))
            return false;

        const string query = """
                             SELECT 
                                 IIF(EXISTS (SELECT 1 FROM Client 
                                         WHERE Client.Pesel = @pesel), 1, 0) AS ClientExists;   
                             """;

        await using SqlConnection con = new(_connectionString);
        await using SqlCommand cmd = new(query, con);
        await con.OpenAsync(token);
        cmd.Parameters.AddWithValue("@pesel", pesel);

        var result = (int)await cmd.ExecuteScalarAsync(token);
        return result == 1;
    }
}