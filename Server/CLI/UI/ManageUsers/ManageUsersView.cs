using CLI.UI.ManagePosts;
using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class ManageUsersView
{
    private readonly IUserRepository _userRepository;

    public ManageUsersView(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task DisplayMenu()
    {
        while (true)
        {
            Console.WriteLine("MANAGE USERS MENU");
            Console.WriteLine("1. Create User");
            Console.WriteLine("2. Update User");
            Console.WriteLine("3. Delete User");
            Console.WriteLine("4. View Users Overview");
            Console.WriteLine("0. Back");
            Console.Write("Choice: ");

            switch (Console.ReadLine())
            {
                case "1":
                    var createUser = new CreateUserView(_userRepository);
                    await createUser.AddUserAsync();
                    break;
                case "2":
                    var updateUser = new UpdateUserView(_userRepository);
                    await updateUser.UpdateUserAsync();
                    break;
                case "3":
                    var deleteUser = new DeleteUserView(_userRepository);
                    await deleteUser.DeleteUserAsync();
                    break;
                case "4":
                    var listUsers = new ListUsersView(_userRepository);
                    await listUsers.GetManyUsers();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Invalid choice, try again.");
                    break;
            }
        }
    }
}