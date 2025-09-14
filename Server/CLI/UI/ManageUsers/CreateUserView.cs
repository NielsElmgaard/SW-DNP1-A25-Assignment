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

    public async Task AddUserAsync()
    {
        Console.WriteLine("CREATE USER MENU");
        string username;

        while (true)
        {
            Console.Write("Username: ");
            username = Console.ReadLine();

            // Check for username already taken
            var usersAlreadyInList = _userRepository.GetMany();

            var userWithSameUsername =
                usersAlreadyInList.FirstOrDefault(userInList =>
                    userInList.Username == username);

            if (userWithSameUsername != null)
            {
                Console.WriteLine("Username already taken");
            }
            else if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("Username cannot be empty");
            }
            else
            {
                break; // valid username
            }
        }

        string password;

        while (true)
        {
            Console.Write("Password: ");
            password = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Password cannot be empty");
            }
            else
            {
                break; // valid password
            }
        }

        User user = new User(0, username, password);
        User created = await _userRepository.AddAsync(user);
        Console.WriteLine(
            $"User ({created.Username}) with id {created.Id} successfully created");
    }
}