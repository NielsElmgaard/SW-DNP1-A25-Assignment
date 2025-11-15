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

    private void CacheInvalidate(int postId, int commentId)
    {
        _cache.Remove($"comment-{commentId}");
        _cache.Remove("allComments");
        _cache.Remove($"post-{postId}Includecomments");
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
        
        User author = await _userRepository.GetSingleAsync(request.UserId.Value);
        await _postRepository.GetSingleAsync(request.PostId);
        Comment comment = new(0, request.Body, request.PostId, request.UserId.Value);
        Comment created = await _commentRepository.AddAsync(comment);


        // Cache invalidation
        CacheInvalidate(created.PostId,created.Id);

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
        var updatedComment = new Comment(comment.Id, request.Body,
            comment.PostId, comment.UserId);
        await _commentRepository.UpdateAsync(updatedComment);

        // Cache invalidation
        CacheInvalidate(comment.PostId,comment.Id);

        var author =
            await _userRepository.GetSingleAsync(updatedComment.UserId);

        var dto = new CommentDTO()
        {
            Id = updatedComment.Id,
            Body = updatedComment.Body,
            PostId = updatedComment.PostId,
            UserId = updatedComment.UserId,
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
        string allCommentsCacheKey = "allComments";

        if (!_cache.TryGetValue(allCommentsCacheKey,
                out IEnumerable<Comment>? cachedComments))
        {
            cachedComments = await _commentRepository.GetMany().ToListAsync();
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
            filteredComments.Select(c => c.UserId).Distinct().ToList(); // no duplicates
        // Map to UserDTO
        var users = await _userRepository.GetMany().Where(u => userIds.Contains(u.Id))
            .Select(u => new UserDTO()
                { Id = u.Id, Username = u.Username }).ToListAsync();

        if (!string.IsNullOrWhiteSpace(authorName))
        {
            var author = users.FirstOrDefault(u => u.Username.Contains(authorName,StringComparison.OrdinalIgnoreCase)); // Partial matching. Switch to == for exact match
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
                case "userid_asc":
                    filteredComments = filteredComments.OrderBy(c => c.UserId);
                    break;
                case "userid_desc":
                    filteredComments = filteredComments.OrderByDescending(c => c.UserId);
                    break;
                case "postid_asc":
                    filteredComments = filteredComments.OrderBy(u => u.PostId);
                    break;
                case "postid_desc":
                    filteredComments = filteredComments.OrderByDescending(u => u.PostId);
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
        var commentToDelete = await _commentRepository.GetSingleAsync(id);
        var postId = commentToDelete.PostId;
        
        await _commentRepository.DeleteAsync(id);

        CacheInvalidate(postId,id);

        return NoContent();
    }
}