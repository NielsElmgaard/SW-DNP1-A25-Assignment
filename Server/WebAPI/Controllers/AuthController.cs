using ApiContracts_DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;
using StudHub.SharedDTO;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AuthController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(
        [FromBody] LoginRequestDTO request)
    {
        User user;
        try
        {
            user = await _userRepository.GetByUsernameAsync(request.Username);
        }
        catch (InvalidOperationException)
        {
            throw new UnauthorizedAccessException("Incorrect username");
        }

        if (user.Password != request.Password)
        {
            throw new UnauthorizedAccessException("Incorrect password");
        }

        var dto = new UserDTO
        {
            Id = user.Id,
            Username = user.Username
        };

        return Ok(dto);
    }
}