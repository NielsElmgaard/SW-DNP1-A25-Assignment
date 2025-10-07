using ApiContracts_DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;

    public CommentsController(ICommentRepository commentRepository,
        IUserRepository userRepository, IMemoryCache cache)
    {
        _commentRepository = commentRepository;
        _userRepository = userRepository;
        _cache = cache;
    }

    private void CacheInvalidate(int id)
    {
        _cache.Remove($"allComments-{id}");
        _cache.Remove("allComments");
    }

    [HttpPost]
    public async Task<ActionResult<CommentDTO>> CreateComment(
        [FromBody] CreateCommentDTO request)
    {
        Comment comment = new(0, request.Body, request.PostId, request.UserId);
        Comment created = await _commentRepository.AddAsync(comment);
        User author = await _userRepository.GetSingleAsync(created.UserId);

        // Cache invalidation
        CacheInvalidate(created.Id);

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
    public async Task<ActionResult<CommentDTO>> UpdatePost(int id,
        [FromBody] UpdateCommentDTO request)
    {
        var comment = await _commentRepository.GetSingleAsync(id);

        // Only Body updates
        comment = new(comment.Id, request.Body, comment.PostId, comment.UserId);
        await _commentRepository.UpdateAsync(comment);

        // Cache invalidation
        CacheInvalidate(id);

        var dto = new UpdateCommentDTO()
        {
            Id = comment.Id,
            Body = comment.Body
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
        string allCommentsCacheKey = "allComments";

        if (!_cache.TryGetValue(allCommentsCacheKey,
                out IEnumerable<Comment>? cachedComments))
        {
            cachedComments = _commentRepository.GetMany().ToList();
            _cache.Set(allCommentsCacheKey, cachedComments,
                new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });
        }
        
        var filteredComments = cachedComments;

        // Filters
        if (userid.HasValue)
        {
            filteredComments =
                filteredComments.Where(c => c.UserId == userid.Value);
        }

        if (postid.HasValue)
        {
            filteredComments =
                filteredComments.Where(c => c.PostId == postid.Value);
        }

        var userIds =
            filteredComments.Select(c => c.UserId).Distinct(); // no duplicates
        // Map to UserDTO
        var users = _userRepository.GetMany().Where(u => userIds.Contains(u.Id))
            .Select(u => new UserDTO()
                { Id = u.Id, Username = u.Username }).ToList();

        if (!string.IsNullOrWhiteSpace(authorName))
        {
            var author = users.FirstOrDefault(u => u.Username == authorName);
            if (author != null)
            {
                filteredComments =
                    filteredComments.Where(c => c.UserId == author.Id);
            }
            else
            {
                filteredComments = Enumerable.Empty<Comment>();
            }
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            switch (sortBy.ToLowerInvariant())
            {
                case "userid":
                    filteredComments = filteredComments.OrderBy(c => c.UserId);
                    break;
                case "postid":
                    filteredComments = filteredComments.OrderBy(u => u.PostId);
                    break;
            }
        }

        // Map to CommentDTO using LINQ
        var comments = filteredComments.Select(c => new CommentDTO()
        {
            Id = c.Id, Body = c.Body, PostId = c.PostId, UserId = c.UserId,
            Author = users.FirstOrDefault(u => u.Id == c.UserId)
        }).ToList();

        return Ok(comments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSingleCommentById(int id)
    {
        string cacheKey = $"comment-{id}";

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
        
        _cache.Set(cacheKey,commentDto,new MemoryCacheEntryOptions()
        {
            SlidingExpiration = TimeSpan.FromMinutes(2),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        return Ok(commentDto);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComment(int id)
    {
        await _commentRepository.DeleteAsync(id);

        CacheInvalidate(id);

        return NoContent();
    }
}