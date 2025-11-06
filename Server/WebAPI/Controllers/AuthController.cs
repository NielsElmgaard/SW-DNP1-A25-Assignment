using System.Security.Claims;
using System.Text;
using ApiContracts_DTOs;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKeyThatIsAtMinimum32CharactersLong"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "your-issuer",
            audience: "your-audience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);

        var userDto = new UserDTO
        {
            Id = user.Id,
            Username = user.Username
        };

        return Ok(new { User = userDto, Token = jwt });
    }

    // For web api JWT testing (httpie)
    [HttpGet("isOG")]
    [Authorize(Policy = "OG")]
    public IActionResult GetOGStatus()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(new { UserId = userId });
    }

}