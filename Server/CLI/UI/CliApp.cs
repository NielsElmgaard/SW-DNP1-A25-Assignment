using CLI.UI.ManagePosts;
using CLI.UI.ManageUsers;
using RepositoryContracts;

namespace CLI.UI;

public class CliApp
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;

    public CliApp(IUserRepository userRepository,
        ICommentRepository commentRepository, IPostRepository postRepository)
    {
        _userRepository = userRepository;
        _commentRepository = commentRepository;
        _postRepository = postRepository;
    }

    public async Task StartAsync()
    {
        while (true)
        {
            Console.WriteLine("MAIN MENU");
            Console.WriteLine("1. Manage Users");
            Console.WriteLine("2. Manage Posts");
            Console.WriteLine("0. Exit");
            Console.Write("Choice: ");

            switch (Console.ReadLine())
            {
                case "1":
                    //To-DO
                    break;
                case "2":
                    var postView = new ManagePostsView(_postRepository,
                        _userRepository, _commentRepository);
                    postView.DisplayMenu();
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