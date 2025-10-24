using System.Text.Json;
using ApiContracts_DTOs;

namespace BlazorClient.Services;

public class CommentService : ICommentService
{
    private readonly HttpClient _httpClient;

    public CommentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CommentDTO> CreateCommentAsync(CreateCommentDTO request)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.PostAsJsonAsync("comments", request);
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return JsonSerializer.Deserialize<CommentDTO>(response,
            new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true })!;
    }

    public async Task UpdateCommentAsync(int id, UpdateCommentDTO request)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.PutAsJsonAsync($"comments/{id}", request);
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }
    }

    public async Task<IEnumerable<CommentDTO>> GetCommentsAsync(int? userId,
        string? authorName, int? postId,
        string? sortBy)
    {
        var comments =
            await _httpClient.GetFromJsonAsync<List<CommentDTO>>(
                $"comments?userid={userId}&authorName={authorName}&postid={postId}&sortBy={sortBy}");

        return comments ?? Enumerable.Empty<CommentDTO>();
    }

    public async Task<CommentDTO> GetSingleCommentByIdAsync(int id)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.GetAsync($"comments/{id}");
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return JsonSerializer.Deserialize<CommentDTO>(response,
            new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true })!;
    }

    public async Task DeleteCommentAsync(int id)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.DeleteAsync($"comments/{id}");
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }
    }
}