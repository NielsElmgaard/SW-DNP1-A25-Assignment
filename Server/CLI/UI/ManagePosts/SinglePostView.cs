using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class SinglePostView
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICommentRepository _commentRepository;

    public SinglePostView(IPostRepository postRepository,
        IUserRepository userRepository, ICommentRepository commentRepository)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _commentRepository = commentRepository;
    }

    public async Task GetSinglePostAsync()
    {
        Console.WriteLine("SINGLE POST MENU");
        Console.Write("Enter post id:");

        string? input = Console.ReadLine();

        if (!int.TryParse(input, out int id))
        {
            Console.WriteLine("Invalid post id.");
            return;
        }

        Post post = await _postRepository.GetSingleAsync(id);
        Console.WriteLine($"Title: {post.Title}\nBody: {post.Body}");

        var comments = _commentRepository.GetMany()
            .Where(comment => comment.PostId == id);

        Console.WriteLine("Comments:");
        foreach (var comment in comments)
        {
            User user = await _userRepository.GetSingleAsync(comment.UserId);
            Console.WriteLine($"{comment.Body} (by {user.Username} )");
        }
    }
}