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

    public UsersController(IUserRepository userRepository, IMemoryCache cache)
    {
        _userRepository = userRepository;
        _cache = cache;
    }

    private async Task VerifyUserNameIsAvailableAsync(string username)
    {
        var users = _userRepository.GetMany();
        if (users.Any(u =>
                u.Username.Equals(username)))
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
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ArgumentException(
                $"Username is required and cannot be empty");
        }
        
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException(
                $"Password is required and cannot be empty");
        }

        await VerifyUserNameIsAvailableAsync(request.Username);

        User user = new(0, request.Username, request.Password);
        User created = await _userRepository.AddAsync(user);
        
        CacheInvalidate(created.Id);
        
        UserDTO dto = new()
        {
            Id = created.Id,
            Username = created.Username
        };
        return Created($"/Users/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UpdateUsernameDTO>> UpdateUsername(int id,
        [FromBody] UserDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ArgumentException(
                $"Username is required and cannot be empty");
        }
        
        var user = await _userRepository.GetSingleAsync(id);
        if (user.Username != request.Username)
        {
            await VerifyUserNameIsAvailableAsync(request.Username);
        }
        
        // Only update username
        user = new(user.Id, request.Username, user.Password);

        await _userRepository.UpdateAsync(user);

        CacheInvalidate(id);

        var dto = new UserDTO()
        {
            Id = user.Id,
            Username = user.Username
        };

        return Ok(dto);
    }

    [HttpPut("{id:int}/password")]
    public async Task<ActionResult<UpdateUserPasswordDTO>> UpdatePassword(
        int id,
        [FromBody] UpdateUserPasswordDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException(
                $"Password is required and cannot be empty");
        }
        
        var user = await _userRepository.GetSingleAsync(id);
        
        // Only update password
        user = new(user.Id, user.Username, request.Password);

        await _userRepository.UpdateAsync(user);

        CacheInvalidate(id);

        var dto = new UpdateUserPasswordDTO()
        {
            Id = user.Id,
            Password = user.Password
        };

        // maybe not ideal to return password
        return Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<UserDTO>> GetUsers(
        [FromQuery] string? startsWith,
        [FromQuery] string? sortBy)
    {
        string allUsersCacheKey = "allUsers";
        if (!_cache.TryGetValue(allUsersCacheKey,
                out IEnumerable<User>? cachedUsers))
        {
            cachedUsers = _userRepository.GetMany().ToList();
            _cache.Set(allUsersCacheKey, cachedUsers,
                new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });
        }

        var filteredUsers = cachedUsers;


        // Filter
        if (!string.IsNullOrWhiteSpace(startsWith))
        {
            filteredUsers = filteredUsers.Where(u =>
                u.Username.StartsWith(startsWith,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            switch (sortBy)
            {
                case "username":
                    filteredUsers = filteredUsers.OrderBy(u => u.Username);
                    break;
                case "id_desc":
                    filteredUsers = filteredUsers.OrderByDescending(u => u.Id);
                    break;
                case "id_asc":
                    filteredUsers = filteredUsers.OrderBy(u => u.Id);
                    break;
            }
        }

        var users = filteredUsers.Select(u => new UserDTO
        {
            Id = u.Id, Username = u.Username
        }).ToList();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSingleUserById(int id)
    {
        string cacheKey = $"post-{id}";

        if (_cache.TryGetValue(cacheKey, out UserDTO? cachedUser))
        {
            return Ok(cachedUser);
        }

        var user = await _userRepository.GetSingleAsync(id);

        var userDto = new UserDTO()
        {
            Id = user.Id, Username = user.Username
        };

        _cache.Set(cacheKey, userDto, new MemoryCacheEntryOptions()
        {
            SlidingExpiration = TimeSpan.FromMinutes(2),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });
        return Ok(userDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        await _userRepository.DeleteAsync(id);

        CacheInvalidate(id);

        return NoContent();
    }
}