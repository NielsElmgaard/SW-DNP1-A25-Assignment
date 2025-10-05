using ApiContracts_DTOs;
using Entities;
using FileRepositories;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;

    public PostsController(IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
    }

    [HttpPost]
    public async Task<ActionResult<PostDTO>> CreatePost(
        [FromBody] CreatePostDTO request)
    {
        Post post = new(0, request.Title, request.Body, request.UserId);
        Post created = await _postRepository.AddAsync(post);
        PostDTO dto = new()
        {
            Id = created.Id,
            Title = created.Title,
            Body = created.Body,
            UserId = created.UserId
        };
        return Created($"/Posts/{dto.Id}", created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostDTO>> UpdatePost(int id,
        [FromBody] PostDTO request)
    {
        if (id != request.Id) // id in URL is not the same as in the JSON string for the request
        {
            return BadRequest(
                $"The Post ID in the URL ({id}) does not match the ID in the request body ({request.Id}).");
        }

        Post postToUpdate = new(request.Id, request.Title, request.Body,
            request.UserId);
        _postRepository.UpdateAsync(postToUpdate);
        return Ok(request);
    }

    [HttpGet]
    
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePost(int id)
    {
        _postRepository.DeleteAsync(id);
        return NoContent();
    }
}