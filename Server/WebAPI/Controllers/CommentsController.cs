using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RepositoryContracts;

namespace WebAPI.Controllers;


[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _commentRepository;
    private readonly IMemoryCache _cache;

    public CommentsController(ICommentRepository commentRepository, IMemoryCache cache)
    {
        _commentRepository = commentRepository;
        _cache = cache;
    }
    private void CacheInvalidate(int id)
    {
        _cache.Remove($"allComments-{id}");
        _cache.Remove("allComments");
    }
    
}