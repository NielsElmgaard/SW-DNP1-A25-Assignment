using ApiContracts_DTOs;
using Entities;
using FileRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;

    public PostsController(IPostRepository postRepository,
        ICommentRepository commentRepository, IUserRepository userRepository,
        IMemoryCache cache)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _userRepository = userRepository;
        _cache = cache;
    }

    private void CacheInvalidate(int id)
    {
        _cache.Remove($"post-{id}Include");
        _cache.Remove($"post-{id}Includecomments");
        _cache.Remove("allPosts");
    }

    [HttpPost]
    public async Task<ActionResult<PostDTO>> CreatePost(
        [FromBody] CreatePostDTO request)
    {
        Post post = new(0, request.Title, request.Body, request.UserId);
        Post created = await _postRepository.AddAsync(post);
        User author = await _userRepository.GetSingleAsync(created.UserId);

        // Cache invalidation
        CacheInvalidate(created.Id);

        PostDTO dto = new()
        {
            Id = created.Id,
            Title = created.Title,
            Body = created.Body,
            UserId = created.UserId,
            Author = new UserDTO { Id = author.Id, Username = author.Username }
        };
        return Created($"/Posts/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostDTO>> UpdatePost(int id,
        [FromBody] PostDTO request)
    {
        if (id != request
                .Id) // id in URL is not the same as in the JSON string for the request
        {
            return BadRequest(
                $"The Post ID in the URL ({id}) does not match the ID in the request body ({request.Id}).");
        }

        Post postToUpdate = new(request.Id, request.Title, request.Body,
            request.UserId);
        await _postRepository.UpdateAsync(postToUpdate);

        // Cache invalidation
        CacheInvalidate(id);

        return Ok(request);
    }

    [HttpGet]
    public async Task<ActionResult<PostDTO>> GetPosts(
        [FromQuery] string? title,
        [FromQuery] int? userid,
        [FromQuery] string? authorName)
    {
        string allPostsCacheKey = "allPosts";

        if (!_cache.TryGetValue(allPostsCacheKey,
                out IEnumerable<Post>? cachedPosts))
        {
            cachedPosts = _postRepository.GetMany().ToList();
            _cache.Set(allPostsCacheKey, cachedPosts,
                new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });
        }

        // Filter
        if (!string.IsNullOrWhiteSpace(title))
        {
            cachedPosts = cachedPosts.Where(p =>
                p.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        if (userid.HasValue)
        {
            cachedPosts = cachedPosts.Where(p => p.UserId == userid.Value);
        }

        var userIds =
            cachedPosts.Select(p => p.UserId).Distinct(); // no duplicates
        // Map to UserDTO
        var users = _userRepository.GetMany().Where(u => userIds.Contains(u.Id))
            .Select(u => new UserDTO()
                { Id = u.Id, Username = u.Username }).ToList();

        if (!string.IsNullOrWhiteSpace(authorName))
        {
            var author = users.FirstOrDefault(u => u.Username == authorName);
            if (author != null)
            {
                cachedPosts = cachedPosts.Where(p => p.UserId == author.Id);
            }
            else
            {
                cachedPosts = Enumerable.Empty<Post>();
            }
        }

        // Map to PostDTO using LINQ
        var posts = cachedPosts.Select(p => new PostDTO
        {
            Id = p.Id, Title = p.Title, Body = p.Body, UserId = p.UserId,
            Author = users.FirstOrDefault(u => u.Id == p.UserId)
        }).ToList();

        return Ok(posts);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSingePostById(int id,
        [FromQuery] string? include)
    {
        string cacheKey = $"post-{id}Include{include?.ToLower()}";

        // cache check
        if (_cache.TryGetValue(cacheKey, out object? cachedResult))
        {
            return Ok(cachedResult);
        }


        var post = await _postRepository.GetSingleAsync(id);
        var author = await _userRepository.GetSingleAsync(post.UserId);


        object dtoToCache;

        if (include != null &&
            include.Contains("comments", StringComparison.OrdinalIgnoreCase))
        {
            var comments = _commentRepository.GetMany()
                .Where(c => c.PostId == id)
                .ToList();

            dtoToCache = new PostWithCommentsDTO()
            {
                Id = post.Id,
                Title = post.Title,
                Body = post.Body,
                UserId = post.UserId,
                Author = new UserDTO
                    { Id = author.Id, Username = author.Username },
                Comments = comments.Select(c => new CommentDTO
                {
                    Id = c.Id, Body = c.Body, PostId = c.PostId,
                    UserId = c.UserId
                }).ToList()
            };
        }
        else
        {
            dtoToCache = new PostDTO()
            {
                Id = post.Id,
                Title = post.Title,
                Body = post.Body,
                UserId = post.UserId,
                Author = new UserDTO
                    { Id = author.Id, Username = author.Username }
            };
        }

        _cache.Set(cacheKey, dtoToCache, new MemoryCacheEntryOptions()
        {
            SlidingExpiration = TimeSpan.FromMinutes(2),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        return Ok(dtoToCache);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePost(int id)
    {
        await _postRepository.DeleteAsync(id);

        CacheInvalidate(id);

        return NoContent();
    }
}