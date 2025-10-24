using System.Text.Json;
using ApiContracts_DTOs;

namespace BlazorClient.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserDTO> CreateUserAsync(CreateUserDTO request)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.PostAsJsonAsync("users", request);
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return JsonSerializer.Deserialize<UserDTO>(response,
            new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true })!;
    }

    public async Task UpdateUsernameAsync(int id, UpdateUsernameDTO request)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.PutAsJsonAsync($"users/{id}", request);
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }
    }

    public async Task UpdatePasswordAsync(int id, UpdateUserPasswordDTO request)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.PutAsJsonAsync($"users/{id}/password", request);
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }
    }

    public async Task<IEnumerable<UserDTO>> GetUsersAsync(string? startsWith,
        string? sortBy)
    {
        var users =
            await _httpClient.GetFromJsonAsync<List<UserDTO>>(
                $"users?startsWith={startsWith}&sortBy={sortBy}");

        return users ?? Enumerable.Empty<UserDTO>();
    }

    public async Task<UserDTO> GetSingleUserByIdAsync(int id)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.GetAsync($"users/{id}");
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return JsonSerializer.Deserialize<UserDTO>(response,
            new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true })!;
    }

    public async Task DeleteUserAsync(int id)
    {
        HttpResponseMessage httpResponse =
            await _httpClient.DeleteAsync($"users/{id}");
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }
    }
}