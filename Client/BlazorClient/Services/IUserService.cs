using ApiContracts_DTOs;

namespace BlazorClient.Services;

public interface IUserService
{
    public Task<UserDTO> CreateUserAsync(CreateUserDTO request);
    public Task UpdateUsernameAsync(int id, UpdateUsernameDTO request); 
    public Task UpdatePasswordAsync(int id, UpdateUserPasswordDTO request);
    public Task<IEnumerable<UserDTO>> GetUsersAsync(string? startsWith, string? sortBy);
    public Task<UserDTO> GetSingleUserByIdAsync(int id);
    public Task DeleteUserAsync(int id);

}