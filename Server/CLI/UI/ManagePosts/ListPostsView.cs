using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class ListPostsView
{
    private readonly IPostRepository _postRepository;

    public ListPostsView(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task GetManyPosts()
    {
        Console.WriteLine("POSTS");

        foreach (var post in _postRepository.GetMany())
        {
            Console.WriteLine($"[{post.Title}, {post.Id}]");
        }
    }
}