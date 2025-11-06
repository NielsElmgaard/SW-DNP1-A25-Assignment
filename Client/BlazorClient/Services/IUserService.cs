using ApiContracts_DTOs;
using ApiContracts_DTOs.Users;

namespace BlazorClient.Services;

public interface IUserService
{
    public Task<UserDTO> CreateUserAsync(CreateUserDTO request);
    public Task UpdateUsernameAsync(int id, UpdateUsernameDTO request); 
    public Task UpdatePasswordAsync(int id, UpdateUserPasswordDTO request);

    public Task UpdateUser(int id, UpdateUserDTO request);
    public Task<IEnumerable<UserDTO>> GetUsersAsync(string? startsWith, string? sortBy);
    public Task<UserDTO> GetSingleUserByIdAsync(int id);
    public Task DeleteUserAsync(int id);

}