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
        int postId;
        Post post = null;
        
        Console.WriteLine("UPDATE POST");
        while (true)
        {

            Console.Write("Enter post id to edit: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out postId))
            {
                Console.WriteLine("Invalid post id.");
                continue;
            }

            try
            {
                post = await _postRepository.GetSingleAsync(postId);
                break;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        Console.Write("New title: ");
        string? newTitle = Console.ReadLine();

        post.Title = newTitle;

        Console.Write("New body: ");
        string? newBody = Console.ReadLine();

        post.Body = newBody;

        await _postRepository.UpdateAsync(post);
        Console.WriteLine($"Post {post.Id} updated successfully");
    }
}