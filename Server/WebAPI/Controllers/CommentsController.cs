using ApiContracts_DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly IMemoryCache _cache;

    public CommentsController(ICommentRepository commentRepository,
        IUserRepository userRepository, IPostRepository postRepository,
        IMemoryCache cache)
    {
        _commentRepository = commentRepository;
        _userRepository = userRepository;
        _postRepository = postRepository;
        _cache = cache;
    }

    private void InvalidateComment(int commentId)
    {
        _cache.Remove($"comments:{commentId}");
        _cache.Remove("comments:all");
    }

    private void InvalidatePost(int postId)
    {
        _cache.Remove("posts:all");
        _cache.Remove($"posts:{postId}");
        _cache.Remove($"posts:{postId}:comments");
    }

    [HttpPost]
    public async Task<ActionResult<CommentDTO>> CreateComment(
        [FromBody] CreateCommentDTO request)
    {
        if (!request.UserId.HasValue)
        {
            throw new ArgumentException(
                $"User ID is required and cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            throw new ArgumentException(
                $"Body is required and cannot be empty");
        }

        User author =
            await _userRepository.GetSingleAsync(request.UserId.Value);
        await _postRepository.GetSingleAsync(request.PostId);
        Comment comment = new(0, request.Body, request.PostId,
            request.UserId.Value);
        Comment created = await _commentRepository.AddAsync(comment);


        // Cache invalidation
        InvalidateComment(created.Id);
        InvalidatePost(created.PostId);

        CommentDTO dto = new()
        {
            Id = created.Id,
            Body = created.Body,
            PostId = created.PostId,
            UserId = created.UserId,
            Author = new UserDTO()
                { Id = author.Id, Username = author.Username }
        };
        return Created($"/Comments/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CommentDTO>> UpdateComment(int id,
        [FromBody] UpdateCommentDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
        {
            throw new ArgumentException(
                $"Body is required and cannot be empty");
        }

        var comment = await _commentRepository.GetSingleAsync(id);
        
        // Only Body updates
        comment.Body = request.Body;

        await _commentRepository.UpdateAsync(comment);

        // Cache invalidation
        InvalidateComment(id);
        InvalidatePost(comment.PostId);

        var author =
            await _userRepository.GetSingleAsync(comment.UserId);

        var dto = new CommentDTO()
        {
            Id = comment.Id,
            Body = comment.Body,
            PostId = comment.PostId,
            UserId = comment.UserId,
            Author = new UserDTO
                { Id = author.Id, Username = author.Username }
        };

        return Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<CommentDTO>> GetComments(
        [FromQuery] int? userid,
        [FromQuery] string? authorName,
        [FromQuery] int? postid,
        [FromQuery] string? sortBy)
    {
        string cacheKey = "comments:all";

        if (!_cache.TryGetValue(cacheKey,
                out List<Comment>? cached))
        {
            cached = await _commentRepository.GetMany().ToListAsync();
            _cache.Set(cacheKey, cached,
                new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });
        }

        IEnumerable<Comment> filtered = cached;

        // Filters
        if (userid.HasValue)
        {
            filtered =
                filtered.Where(c => c.UserId == userid.Value);
        }

        if (postid.HasValue)
        {
            filtered =
                filtered.Where(c => c.PostId == postid.Value);
        }

        var userIds =
            filtered.Select(c => c.UserId).Distinct()
                .ToList(); // no duplicates
        // Map to UserDTO
        var users = await _userRepository.GetMany()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new UserDTO()
                { Id = u.Id, Username = u.Username }).ToListAsync();

        if (!string.IsNullOrWhiteSpace(authorName))
        {
            var author = users.FirstOrDefault(u =>
                u.Username.Contains(authorName,
                    StringComparison
                        .OrdinalIgnoreCase)); // Partial matching. Switch to == for exact match
            if (author != null)
            {
                filtered =
                    filtered.Where(c => c.UserId == author.Id);
            }
            else
            {
                filtered = Enumerable.Empty<Comment>();
            }
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            switch (sortBy.ToLowerInvariant())
            {
                case "userid_asc":
                    filtered = filtered.OrderBy(c => c.UserId);
                    break;
                case "userid_desc":
                    filtered =
                        filtered.OrderByDescending(c => c.UserId);
                    break;
                case "postid_asc":
                    filtered = filtered.OrderBy(u => u.PostId);
                    break;
                case "postid_desc":
                    filtered =
                        filtered.OrderByDescending(u => u.PostId);
                    break;
            }
        }

        // Map to CommentDTO using LINQ
        var comments = filtered.Select(c => new CommentDTO()
        {
            Id = c.Id, Body = c.Body, PostId = c.PostId, UserId = c.UserId,
            Author = users.FirstOrDefault(u => u.Id == c.UserId)
        }).ToList();

        return Ok(comments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSingleCommentById(int id)
    {
        string cacheKey = $"comments:{id}";

        // cache check
        if (_cache.TryGetValue(cacheKey, out CommentDTO? cachedResult))
        {
            return Ok(cachedResult);
        }

        var comment = await _commentRepository.GetSingleAsync(id);
        var author = await _userRepository.GetSingleAsync(comment.UserId);

        var commentDto = new CommentDTO
        {
            Id = comment.Id, Body = comment.Body, PostId = comment.PostId,
            UserId = comment.UserId, Author = new UserDTO
                { Id = author.Id, Username = author.Username }
        };

        _cache.Set(cacheKey, commentDto, new MemoryCacheEntryOptions()
        {
            SlidingExpiration = TimeSpan.FromMinutes(2),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        return Ok(commentDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComment(int id)
    {
        var comment = await _commentRepository.GetSingleAsync(id);

        await _commentRepository.DeleteAsync(id);

        InvalidateComment(comment.Id);
        InvalidatePost(comment.PostId);
        
        return NoContent();
    }
}