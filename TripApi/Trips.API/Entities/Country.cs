namespace Trips.API.Entities;

public class Country : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public List<Trip> Trips { get; set; } = [];
}