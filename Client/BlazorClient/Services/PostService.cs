using System.Text.Json;
using ApiContracts_DTOs;

namespace BlazorClient.Services;

public class PostService : IPostService
{
    private readonly HttpClient _httpClient;

    public PostService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PostDTO> CreatePostAsync(CreatePostDTO request)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.PostAsJsonAsync("posts", request);
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return JsonSerializer.Deserialize<PostDTO>(response,
            new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true })!;
    }

    public async Task UpdatePostAsync(int id, UpdatePostDTO request)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.PutAsJsonAsync($"posts/{id}", request);
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }
    }

    public async Task<IEnumerable<PostDTO>> GetPostsAsync(string? title,
        int? userId, string? authorName)
    {
        var posts =
            await _httpClient.GetFromJsonAsync<List<PostDTO>>(
                $"posts?title={title}&userId={userId}&authorName={authorName}");

        return posts ?? Enumerable.Empty<PostDTO>();
    }

    /*
    public async Task<PostWithCommentsDTO?> GetPostWithCommentsAsync(int postId)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.GetAsync(
                $"posts?{postId}?include=comments");
        if (httpResponse.IsSuccessStatusCode)
        {
            return await httpResponse.Content.ReadFromJsonAsync<PostWithCommentsDTO>();;
        }

        return null;
    }
    */

    public async Task<PostWithCommentsDTO> GetSinglePostByIdAsync(int id,
        string? include)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.GetAsync($"posts/{id}?include={include}");
        string response = await httpResponse.Content.ReadAsStringAsync();

        if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null; // gracefully handle deleted posts
        }

        /*
         if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }
        */

        httpResponse.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PostWithCommentsDTO>(response,
            new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true })!;
    }

    public async Task DeletePostAsync(int id)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.DeleteAsync($"posts/{id}");
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }
    }
}