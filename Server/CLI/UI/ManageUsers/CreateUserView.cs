using RepositoryContracts;
using Entities;

namespace CLI.UI.ManageUsers;

public class CreateUserView
{
    private readonly IUserRepository _userRepository;

    public CreateUserView(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task AddUserAsync(string? username, string? password)
    {
        Console.WriteLine("CREATE USER MENU");
        Console.Write("Username: ");
        username = Console.ReadLine();
        
        Console.Write("Password: ");
        password = Console.ReadLine();
        
        User user = new User(0,username??"UnknownUser", password??"1234");
        User created = await _userRepository.AddAsync(user);
        Console.WriteLine($"User with id {created.Id} successfully created");
    }
}