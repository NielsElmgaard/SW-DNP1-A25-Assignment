using System.ComponentModel.DataAnnotations;

namespace ApiContracts_DTOs.Users;

public class UpdateUserDTO
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Username is required and cannot be empty.")]
    public required string Username { get; set; }
    
    public string? Password { get; set; }
}