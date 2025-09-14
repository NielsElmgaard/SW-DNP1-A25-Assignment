using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class DeletePostView
{
    private readonly IPostRepository _postRepository;

    public DeletePostView(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task DeletePostAsync()
    {
        Console.Write("Enter post id for post to delete: ");
        string? input = Console.ReadLine();
        if (!int.TryParse(input, out int postId))
        {
            Console.WriteLine("Invalid post id.");
            return;
        }

        await _postRepository.DeleteAsync(postId);
        Console.WriteLine("post deleted successfully");
    }
}