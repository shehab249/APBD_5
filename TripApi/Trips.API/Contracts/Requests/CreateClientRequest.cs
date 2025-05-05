using System.ComponentModel.DataAnnotations;

namespace Trips.API.Contracts.Requests;

public class CreateClientRequest
{
    [Required]
    [StringLength(120)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(120)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    [StringLength(120)]
    public string Telephone { get; set; } = string.Empty;
    
    [Required]
    [StringLength(120)]
    public string Pesel { get; set; } = string.Empty;
}