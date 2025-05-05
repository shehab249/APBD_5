namespace Trips.API.Exceptions;

public class TripDoesNotExistException(int tripId) : Exception($"Trip with id: {tripId} does not exist")
{
    
}