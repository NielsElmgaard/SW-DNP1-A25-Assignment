using ApiContracts_DTOs;

namespace BlazorClient.Services;

public interface IPostService
{
    public Task<PostDTO> CreatePostAsync(CreatePostDTO request);
    public Task UpdatePostAsync(int id, UpdatePostDTO request); 
    public Task<IEnumerable<PostDTO>> GetPostsAsync(string? title,int? userId, string? authorName);
    
    //public Task<PostWithCommentsDTO?> GetPostWithCommentsAsync(int postId);
    public Task<PostWithCommentsDTO> GetSinglePostByIdAsync(int id, string? include);
    public Task DeletePostAsync(int id);
}