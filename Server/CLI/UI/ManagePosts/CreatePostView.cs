using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class CreatePostView
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;

    public CreatePostView(IPostRepository postRepository, IUserRepository userRepository)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
    }

    public async Task AddPostAsync()
    {
        Console.WriteLine("CREATE POST");

        Console.Write("Title: ");
        string title = Console.ReadLine();

        Console.Write("Body: ");
        string body = Console.ReadLine();

        int userId;
        while (true)
        {
            Console.Write("User id: ");
            string? userIdInput = Console.ReadLine();

            if (!int.TryParse(userIdInput, out userId))
            {
                Console.WriteLine("Invalid user id");
                continue;
            }
            
            try
            {
                await _userRepository.GetSingleAsync(userId);
                break;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            title = "Unknown title";
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            body = "Unknown body";
        }

        Post post = new Post(0, title, body,
            userId);
        Post created = await _postRepository.AddAsync(post);
        Console.WriteLine($"Post with id {created.Id} successfully created");
    }
}