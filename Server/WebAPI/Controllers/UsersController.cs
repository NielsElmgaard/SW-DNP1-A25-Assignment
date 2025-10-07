using ApiContracts_DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    private async Task VerifyUserNameIsAvailableAsync(string username)
    {
        var users = _userRepository.GetMany();
        if (users.Any(u =>
                u.Username.Equals(username,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Username '{username}' is already taken.");
        }
    }

    private void CacheInvalidate(int id)
    {
        _cache.Remove($"user-{id}");
        _cache.Remove("allUsers");
    }


    [HttpPost]
    public async Task<ActionResult<UserDTO>> CreateUser(
        [FromBody] CreateUserDTO request)
    {
        await VerifyUserNameIsAvailableAsync(request.Username);

        User user = new(0, request.Username, request.Password);
        User created = await _userRepository.AddAsync(user);
        UserDTO dto = new()
        {
            Id = created.Id,
            Username = created.Username
        };
        return Created($"/Users/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDTO>> UpdateUsername(int id,
        [FromBody] UserDTO request)
    {
        
        if (id != request.Id)
        {
            return BadRequest(
                $"The User ID in the URL ({id}) does not match the ID in the request body ({request.Id}).");
        }
        await VerifyUserNameIsAvailableAsync(request.Username);


        var user = await _userRepository.GetSingleAsync(id);
        
        if (!user.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase))
        {
            await VerifyUserNameIsAvailableAsync(request.Username);
        }

        User userToUpdate = new(request.Id, request.Username, user.Password);
        await _userRepository.UpdateAsync(userToUpdate);
        
        CacheInvalidate(id);
        
        return Ok(request);
    }
}