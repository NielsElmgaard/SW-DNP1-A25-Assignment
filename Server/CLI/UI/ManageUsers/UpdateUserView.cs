using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class UpdateUserView
{
    private readonly IUserRepository _userRepository;

    public UpdateUserView(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task UpdateUserAsync()
    {
        Console.WriteLine("UPDATE USER");
        Console.Write("Enter user id to edit: ");
        string? input = Console.ReadLine();
        if (!int.TryParse(input, out int userId))
        {
            Console.WriteLine("Invalid user id.");
            return;
        }

        User user = await _userRepository.GetSingleAsync(userId);
        
        Console.Write("New Username: ");
        string? newUsername = Console.ReadLine();

        user.Username = newUsername;
        
        Console.Write("New password: ");
        string? newPassword = Console.ReadLine();

        user.Password = newPassword;

        await _userRepository.UpdateAsync(user);
        Console.WriteLine($"User {user.Id} updated successfully");
    }
}