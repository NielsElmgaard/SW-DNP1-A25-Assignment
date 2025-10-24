using System.ComponentModel.DataAnnotations;

namespace ApiContracts_DTOs;

public class CreateUserDTO
{
    [Required(ErrorMessage = "Username is required and cannot be empty.")]

    public required string Username { get; set; }
    
    [Required(ErrorMessage = "Password is required and cannot be empty.")]
    public required string Password { get; set; }
}