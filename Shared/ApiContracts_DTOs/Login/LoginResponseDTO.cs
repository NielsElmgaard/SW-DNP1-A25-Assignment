using ApiContracts_DTOs;

namespace StudHub.SharedDTO;

public class LoginResponseDTO
{
    public UserDTO User { get; set; }
    
    // For web api JWT testing (httpie)
    public string Token { get; set; }
}
