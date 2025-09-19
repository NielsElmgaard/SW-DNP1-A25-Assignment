using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

// You can view a single post and afterward add/edit/delete a comment on the post
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
        int id;
        Post? post = null;

        Console.WriteLine("SINGLE POST MENU");

        while (true)
        {
            Console.Write("Enter post id:");

            string? input = Console.ReadLine();

            if (!int.TryParse(input, out id))
            {
                Console.WriteLine("Invalid post id.");
                continue;
            }

            try
            {
                post = await _postRepository.GetSingleAsync(id);
                break;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        Console.WriteLine($"Title: {post.Title}\nBody: {post.Body}");

        await ShowCommentsAsync(id);

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
                    await AddCommentAsync(id);
                    await ShowCommentsAsync(id);
                    break;
                case "2":
                    await EditCommentAsync(id);
                    await ShowCommentsAsync(id);
                    break;
                case "3":
                    await DeleteCommentAsync(id);
                    await ShowCommentsAsync(id);
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
            User? user = null;
            try
            {
                user = await _userRepository.GetSingleAsync(comment.UserId);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine($"{comment.Body} (by {user.Username})");
        }
    }

    private async Task AddCommentAsync(int postId)
    {
        int userId;
        while (true)
        {
            Console.Write("Enter user id: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out userId))
            {
                Console.WriteLine("Invalid user id.");
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

        Console.Write("Comment body: ");
        string? body = Console.ReadLine();

        Comment comment = new Comment(0, body, postId, userId);
        Comment created = await _commentRepository.AddAsync(comment);
        Console.WriteLine(
            $"Comment with id {created.Id} successfully created");
    }

    private async Task EditCommentAsync(int postId)
    {
        int commentId;
        Comment? comment = null;
        while (true)
        {
            Console.Write("Enter comment id to edit: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out commentId))
            {
                Console.WriteLine("Invalid comment id.");
                continue;
            }

            try
            {
                comment =
                    await _commentRepository.GetSingleAsync(commentId);
                if (comment.PostId !=
                    postId) // If the comment isn't posted on this post
                {
                    Console.WriteLine("Comment not found");
                    continue;
                }

                break;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        Console.Write("New body: ");
        string? newBody = Console.ReadLine();

        comment.Body = newBody;

        await _commentRepository.UpdateAsync(comment);
        Console.WriteLine("Comment updated successfully");
    }

    private async Task DeleteCommentAsync(int postId)
    {
        int commentId;
        Comment? comment = null;
        while (true)
        {
            Console.Write("Enter comment id to delete: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out commentId))
            {
                Console.WriteLine("Invalid comment id.");
                continue;
            }

            try
            {
                comment =
                    await _commentRepository.GetSingleAsync(commentId);
                if (comment.PostId !=
                    postId) // If the comment isn't posted on this post
                {
                    Console.WriteLine("Comment not found");
                    continue;
                }

                await _commentRepository.DeleteAsync(commentId);
                Console.WriteLine("Comment deleted successfully");
                break;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}