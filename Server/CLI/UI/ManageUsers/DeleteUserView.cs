using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class DeleteUserView
{
    private readonly IUserRepository _userRepository;

    public DeleteUserView(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task DeleteUserAsync()
    {
        while (true)
        {
            Console.Write("Enter user id for user to delete: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out int userId))
            {
                Console.WriteLine("Invalid user id.");
                continue;
            }

            try
            {
                await _userRepository.DeleteAsync(userId);
                Console.WriteLine("user deleted successfully");
                break;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}