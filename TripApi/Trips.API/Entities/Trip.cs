namespace Trips.API.Entities;

public class Trip : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<Country> Destinations { get; set; } = [];
    public List<ClientTrip> Participants { get; set; } = [];
}