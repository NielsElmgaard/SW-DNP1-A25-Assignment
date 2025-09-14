using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class UpdatePostView
{
    private readonly IPostRepository _postRepository;

    public UpdatePostView(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task UpdatePostAsync()
    {
        Console.WriteLine("UPDATE POST");
        Console.Write("Enter post id to edit: ");
        string? input = Console.ReadLine();
        if (!int.TryParse(input, out int postId))
        {
            Console.WriteLine("Invalid post id.");
            return;
        }

        Post post = await _postRepository.GetSingleAsync(postId);
        
        Console.Write("New title: ");
        string? newTitle = Console.ReadLine();

        post.Body = newTitle;
        
        Console.Write("New body: ");
        string? newBody = Console.ReadLine();

        post.Body = newBody;

        await _postRepository.UpdateAsync(post);
        Console.WriteLine($"Post {post.Id} updated successfully");
    }
}