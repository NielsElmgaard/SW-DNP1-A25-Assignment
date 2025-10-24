using ApiContracts_DTOs;

namespace BlazorClient.Services;

public interface ICommentService
{
    public Task<CommentDTO> CreateCommentAsync(CreateCommentDTO request);
    public Task UpdateCommentAsync(int id, UpdateCommentDTO request); 
    public Task<IEnumerable<CommentDTO>> GetCommentsAsync(int? userId, string? authorName,int? postId,string? sortBy);
    public Task<CommentDTO> GetSingleCommentByIdAsync(int id);
    public Task DeleteCommentAsync(int id);
}