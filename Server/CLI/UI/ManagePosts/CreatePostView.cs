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
        Console.WriteLine("CREATE POST MENU");
        Console.Write("Title: ");
        string? title = Console.ReadLine();
        
        Console.Write("Body: ");
        string? body = Console.ReadLine();
        
        Console.Write("User id: ");
        string? userIdInput =Console.ReadLine();
        if (!int.TryParse(userIdInput, out int userId))
        {
            Console.WriteLine("Invalid user id.");
            return;
        }
        
        Post post = new Post(0,title??"Unknown title", body??"No body",userId);
        Post created = await _postRepository.AddAsync(post);
        Console.WriteLine($"Post with id {created.Id} successfully created");
    }
}