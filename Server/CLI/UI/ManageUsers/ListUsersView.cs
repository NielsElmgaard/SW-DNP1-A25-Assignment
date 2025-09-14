using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class ListUsersView
{
    private readonly IUserRepository _userRepository;

    public ListUsersView(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task GetManyUsers()
    {
        Console.WriteLine("USERS");
        foreach (var user in _userRepository.GetMany())
        {
            Console.WriteLine($"({user.Id}) {user.Username}");
        }
    }
}