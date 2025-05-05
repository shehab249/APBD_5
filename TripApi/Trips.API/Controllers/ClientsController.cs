using Microsoft.AspNetCore.Mvc;
using Trips.API.Contracts.Requests;
using Trips.API.Contracts.Responses;
using Trips.API.Exceptions;
using Trips.API.Mappers;
using Trips.API.Services.Abstractions;

namespace Trips.API.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly ITripService _tripService;

    public ClientsController(IClientService clientService, ITripService tripService)
    {
        _clientService = clientService;
        _tripService = tripService;
    }

    [HttpGet("{clientId:int}/trips")]
    [ProducesResponseType(typeof(ICollection<GetAllClientTripsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllClientTrips([FromRoute] int clientId, CancellationToken token = default)
    {
        if (clientId <= 0)
            return BadRequest($"{nameof(clientId)} should be greater than 0");

        var result = await _clientService.GetAllClientTripsAsync(clientId, token); 
        
        if (result is null)
            return NotFound("Client is not found");
        
        return Ok(result.MapToGetAllTripsResponse());
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateClientAsync([FromBody] CreateClientRequest request,
        CancellationToken token = default)
    {
        try
        {
            var clientId = await _clientService.CreateClientAsync(request, token);
            return CreatedAtAction(nameof(GetAllClientTrips), new { clientId }, clientId);
        }
        catch (ClientWithPeselNumberExistsException)
        {
            return BadRequest("Client with provided PESEL already exists");
        }
    }

    [HttpPut("{clientId:int}/trips/{tripId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateClientTripAsync(
        [FromRoute] int clientId,
        [FromRoute] int tripId,
        CancellationToken token = default)
    {
        if (clientId <= 0)
            return BadRequest($"{nameof(clientId)} should be greater than 0");

        if (tripId <= 0)
            return BadRequest($"{nameof(tripId)} should be greater than 0");

        try
        {
            var result = await _tripService.RegisterClientToTripAsync(tripId, clientId, token);
            if (!result)
            {
                return CreateProblemResult(StatusCodes.Status500InternalServerError, 
                    "Failed to update client trip");
            }

            return Ok();
        }
        catch (TripDoesNotExistException)
        {
            return NotFound($"Trip with id: {tripId} does not exist");
        }
        catch (ClientDoesNotExistException)
        {
            return NotFound($"Client with id: {clientId} does not exist");
        }
    }

    private ObjectResult CreateProblemResult(int statusCode, string detail)
    {
        return new ObjectResult(new ProblemDetails
        {
            Status = statusCode,
            Detail = detail
        })
        {
            StatusCode = statusCode
        };
    }
}