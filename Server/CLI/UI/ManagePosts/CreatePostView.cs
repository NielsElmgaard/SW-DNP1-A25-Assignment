using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class CreatePostView
{
    private readonly IPostRepository _postRepository;

    public CreatePostView(IPostRepository postRepository)
    {
        _postRepository = postRepository;
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

            if (int.TryParse(userIdInput, out userId))
            {
                break; // valid user id
            }

            Console.WriteLine("Invalid user id");
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