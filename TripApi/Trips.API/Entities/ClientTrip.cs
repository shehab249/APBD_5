namespace Trips.API.Entities;

public class ClientTrip
{
    public Client Client { get; set; } = null!;
    public Trip Trip { get; set; } = null!;
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}