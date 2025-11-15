using ApiContracts_DTOs;
using ApiContracts_DTOs.Users;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;

    public UsersController(IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IMemoryCache cache)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _cache = cache;
    }

    private async Task VerifyUserNameIsAvailableAsync(string username)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            // User already exists
            throw new InvalidOperationException(
                $"Username '{username}' is already taken.");
        }
        catch (InvalidOperationException e)
        {
            if (!e.Message.Contains("not found"))
            {
                throw; // some other error -> throw exception
            }
            // "not found" error from repository -> ignore catch since username is available then
        }
    }

    private void InvalidateUser(int userId)
    {
        _cache.Remove($"users:{userId}");
        _cache.Remove("users:all");
    }

    private void InvalidatePost(int postId)
    {
        _cache.Remove($"posts:{postId}");
        _cache.Remove($"posts:{postId}:comments");
        _cache.Remove("posts:all");
    }

    private void InvalidateComment(int commentId)
    {
        _cache.Remove($"comments:{commentId}");
        _cache.Remove("comments:all");
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

        InvalidateUser(created.Id);

        UserDTO dto = new()
        {
            Id = created.Id,
            Username = created.Username
        };
        return Created($"/Users/{dto.Id}", dto);
    }

    /*
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
        _cache.Remove("allComments");
        _cache.Remove("allPosts");

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
    */

    [HttpPut("{id:int}/update")]
    public async Task<ActionResult<UserDTO>> UpdateUser(int id,
        [FromBody] UpdateUserDTO request)
    {
        var user = await _userRepository.GetSingleAsync(id);

        string newPassword = string.IsNullOrWhiteSpace(request.Password)
            ? user.Password
            : request.Password;

        var updated = new User(user.Id, request.Username, newPassword);
        await _userRepository.UpdateAsync(updated);

        InvalidateUser(id);

        var userPosts = await _postRepository.GetMany()
            .Where(p => p.UserId == id).ToListAsync();
        foreach (var post in userPosts)
            InvalidatePost(post.Id);

        var dto = new UserDTO()
        {
            Id = user.Id,
            Username = user.Username,
        };

        return Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<UserDTO>> GetUsers(
        [FromQuery] string? startsWith,
        [FromQuery] string? sortBy)
    {
        string cacheKey = "users:all";
        if (!_cache.TryGetValue(cacheKey,
                out List<User>? cachedUsers))
        {
            cachedUsers = await _userRepository.GetMany().ToListAsync();
            _cache.Set(cacheKey, cachedUsers,
                new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });
        }

        var filteredUsers = cachedUsers.AsQueryable();


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

        var users = await filteredUsers.Select(u => new UserDTO
        {
            Id = u.Id, Username = u.Username
        }).ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSingleUserById(int id)
    {
        string cacheKey =  $"users:{id}";

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
        // Delete all comments of the user's posts
        var userPosts = await _postRepository.GetMany()
            .Where(p => p.UserId == id)
            .ToListAsync();
        foreach (var post in userPosts)
        {
            // Delete all comments for this post, by anyone
            var commentsOnPost = await _commentRepository.GetMany()
                .Where(c => c.PostId == post.Id).ToListAsync();

            foreach (var comment in commentsOnPost)
            {
                await _commentRepository.DeleteAsync(comment.Id);
                InvalidateComment(comment.Id);
            }

            await _postRepository.DeleteAsync(post.Id);
            InvalidatePost(post.Id);
        }

        // Delete comments of the user
        var userComments = await _commentRepository.GetMany()
            .Where(c => c.UserId == id)
            .ToListAsync();
        foreach (var comment in userComments)
        {
            await _commentRepository.DeleteAsync(comment.Id);
            InvalidateComment(comment.Id);
        }

        // Delete the user
        await _userRepository.DeleteAsync(id);

        InvalidateUser(id);

        return NoContent();
    }
}