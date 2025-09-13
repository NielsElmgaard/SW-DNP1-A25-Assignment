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
    
    public async Task AddPostAsync(string? title, string? body)
    {
        Console.WriteLine("CREATE POST MENU");
        Console.Write("Title: ");
        title = Console.ReadLine();
        
        Console.Write("Password: ");
        body = Console.ReadLine();
        
        Post post = new Post(0,title??"UnknownTitle", body??"1234",0);
        Post created = await _postRepository.AddAsync(post);
        Console.WriteLine($"Post with id {created.Id} successfully created");
    }
}