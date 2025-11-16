using ApiContracts_DTOs;
using Entities;
using FileRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<ActionResult<PostDTO>> CreatePost(
        [FromBody] CreatePostDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException(
                $"Title is required and cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            throw new ArgumentException(
                $"Body is required and cannot be empty");
        }

        Post post = new(0, request.Title, request.Body, request.UserId!.Value);
        Post created = await _postRepository.AddAsync(post);
        User author = await _userRepository.GetSingleAsync(created.UserId);

        // Cache invalidation
        InvalidatePost(created.Id);

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
        [FromBody] UpdatePostDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException(
                $"Title is required and cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            throw new ArgumentException(
                $"Body is required and cannot be empty");
        }

        var post = await _postRepository.GetSingleAsync(id);

        // Only Title and Body updates
        post.Title = request.Title;
        post.Body = request.Body;
        
        await _postRepository.UpdateAsync(post);

        // Cache invalidation
        InvalidatePost(id);

        var author = await _userRepository.GetSingleAsync(post.UserId);

        var dto = new PostDTO
        {
            Id = post.Id,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId,
            Author = new UserDTO { Id = author.Id, Username = author.Username }
        };

        return Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<PostDTO>> GetPosts(
        [FromQuery] string? title,
        [FromQuery] int? userid,
        [FromQuery] string? authorName)
    {
        string cacheKey = "posts:all";

        if (!_cache.TryGetValue(cacheKey,
                out List<Post>? cachedPosts))
        {
            cachedPosts = await _postRepository.GetMany().ToListAsync();
            _cache.Set(cacheKey, cachedPosts,
                new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });
        }

        IEnumerable<Post> filteredPosts = cachedPosts;

        // Filters
        if (!string.IsNullOrWhiteSpace(title))
        {
            filteredPosts = filteredPosts.Where(p =>
                p.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        if (userid.HasValue)
        {
            filteredPosts = filteredPosts.Where(p => p.UserId == userid.Value);
        }

        var userIds =
            filteredPosts.Select(p => p.UserId).Distinct()
                .ToList(); // no duplicates
        // Map to UserDTO
        var users = await _userRepository.GetMany()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new UserDTO()
                { Id = u.Id, Username = u.Username }).ToListAsync();

        if (!string.IsNullOrWhiteSpace(authorName))
        {
            var author =
                users.FirstOrDefault(u =>
                    u.Username
                        .Contains(
                            authorName)); // Partial matching. Switch to == for exact match
            if (author != null)
            {
                filteredPosts = filteredPosts.Where(p => p.UserId == author.Id);
            }
            else
            {
                filteredPosts = Enumerable.Empty<Post>();
            }
        }

        // Map to PostDTO using LINQ
        var posts = filteredPosts.Select(p => new PostDTO
        {
            Id = p.Id, Title = p.Title, Body = p.Body, UserId = p.UserId,
            Author = users.FirstOrDefault(u => u.Id == p.UserId)
        }).ToList();

        return Ok(posts);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSinglePostById(int id,
        [FromQuery] string? include)
    {
        string cacheKey = $"posts:{id}";
        if (!string.IsNullOrWhiteSpace(include) && include.Contains("comments",
                StringComparison.OrdinalIgnoreCase))
        {
            cacheKey = $"posts:{id}:comments";
        }

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
            var comments = await _commentRepository.GetMany()
                .Where(c => c.PostId == id)
                .ToListAsync();
            var userIds = comments.Select(c => c.UserId).Distinct().ToList();
            var users = await _userRepository.GetMany()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new UserDTO()
                    { Id = u.Id, Username = u.Username }).ToListAsync();

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
                    UserId = c.UserId,
                    Author = users.FirstOrDefault(u => u.Id == c.UserId)
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
        var comments = await _commentRepository.GetMany()
            .Where(c => c.PostId == id)
            .ToListAsync();
        foreach (var comment in comments)
        {
            await _commentRepository.DeleteAsync(comment.Id);
            InvalidateComment(comment.Id);
        }

        await _postRepository.DeleteAsync(id);

        InvalidatePost(id);

        return NoContent();
    }
}