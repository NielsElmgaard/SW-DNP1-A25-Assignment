using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class DeletePostView
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;

    public DeletePostView(IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
    }

    public async Task DeletePostAsync()
    {
        int postId;
        while (true)
        {
            Console.Write("Enter post id for post to delete: ");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out postId))
            {
                Console.WriteLine("Invalid post id.");
                continue;
            }

            try
            {
                // Delete comments on post
                var commentsToDelete = _commentRepository.GetMany()
                    .Where(comments => comments.PostId == postId).ToList();
                foreach (var comment in commentsToDelete)
                {
                    await _commentRepository.DeleteAsync(comment.Id);
                }

                // Delete post
                await _postRepository.DeleteAsync(postId);
                Console.WriteLine("Post deleted successfully");
                break;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}