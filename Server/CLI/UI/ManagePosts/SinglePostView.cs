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
        if (post == null)
        {
            Console.WriteLine("Post not found");
            return;
        }

        Console.WriteLine($"Title: {post.Title}\nBody: {post.Body}");

        ShowCommentsAsync(id);

        while (true)
        {
            Console.WriteLine("COMMENTS MENU");
            Console.WriteLine("1. Add comment");
            Console.WriteLine("2. Edit comment");
            Console.WriteLine("3. Delete comment");
            Console.WriteLine("0. Back");
            Console.Write("Choice: ");

            switch (Console.ReadLine())
            {
                case "1":
                    AddCommentAsync(id);
                    break;
                case "2":
                    EditCommentAsync(id);
                    break;
                case "3":
                    DeleteCommentAsync(id);
                    break;
                case "0": return;
                default:
                    Console.WriteLine("Invalid choice, try again.");
                    break;
            }
        }
    }

    private async Task ShowCommentsAsync(int postId)
    {
        var comments = _commentRepository.GetMany()
            .Where(comment => comment.PostId == postId);

        Console.WriteLine("Comments:");
        foreach (var comment in comments)
        {
            User user = await _userRepository.GetSingleAsync(comment.UserId);
            Console.WriteLine($"{comment.Body} (by {user.Username})");
        }
    }

    private async Task AddCommentAsync(int postId)
    {
        Console.Write("Enter user id: ");
        string? input = Console.ReadLine();
        if (!int.TryParse(input, out int userId))
        {
            Console.WriteLine("Invalid user id.");
            return;
        }

        Console.Write("Comment body: ");
        string? body = Console.ReadLine();

        Comment comment = new Comment(0, body, postId, userId);
        Comment created = await _commentRepository.AddAsync(comment);
        Console.WriteLine($"Comment with id {created.Id} successfully created");
    }

    private async Task EditCommentAsync(int postId)
    {
        Console.Write("Enter comment id to edit: ");
        string? input = Console.ReadLine();
        if (!int.TryParse(input, out int commentId))
        {
            Console.WriteLine("Invalid comment id.");
            return;
        }

        Comment comment = await _commentRepository.GetSingleAsync(commentId);
        if (comment == null ||
            comment.PostId !=
            postId) // If the comment isn't posted on this post
        {
            Console.WriteLine("Comment not found");
            return;
        }

        Console.Write("New body: ");
        string? newBody = Console.ReadLine();

        comment.Body = newBody;

        await _commentRepository.UpdateAsync(comment);
        Console.WriteLine("Comment updated successfully");
    }

    private async Task DeleteCommentAsync(int postId)
    {
        Console.Write("Enter comment id to delete: ");
        string? input = Console.ReadLine();
        if (!int.TryParse(input, out int commentId))
        {
            Console.WriteLine("Invalid comment id.");
            return;
        }

        Comment comment = await _commentRepository.GetSingleAsync(commentId);
        if (comment == null ||
            comment.PostId !=
            postId) // If the comment isn't posted on this post
        {
            Console.WriteLine("Comment not found");
            return;
        }

        await _commentRepository.DeleteAsync(commentId);
        Console.WriteLine("Comment deleted successfully");
    }
}