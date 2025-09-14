using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class ManagePostsView
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;

    public ManagePostsView(IPostRepository postRepository,
        IUserRepository userRepository, ICommentRepository commentRepository)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _commentRepository = commentRepository;
    }

    public async Task DisplayMenu()
    {
        while (true)
        {
            Console.WriteLine("MANAGE POSTS MENU");
            Console.WriteLine("1. Create Posts");
            Console.WriteLine("2. Update Post");
            Console.WriteLine("3. Delete Post");
            Console.WriteLine("4. View Posts Overview");
            Console.WriteLine("5. View Single Post");
            Console.WriteLine("0. Back");
            Console.Write("Choice: ");

            switch (Console.ReadLine())
            {
                case "1":
                    var createPost = new CreatePostView(_postRepository);
                    await createPost.AddPostAsync();
                    break;
                case "2":
                    var updatePost = new UpdatePostView(_postRepository);
                    await updatePost.UpdatePostAsync();
                    break;
                case "3":
                    var deletePost = new DeletePostView(_postRepository);
                    await deletePost.DeletePostAsync();
                    break;
                case "4":
                    var listPosts = new ListPostsView(_postRepository);
                    await listPosts.GetManyPosts();
                    break;
                case "5":
                    var singlePost = new SinglePostView(_postRepository,
                        _userRepository, _commentRepository);
                    await singlePost.GetSinglePostAsync();
                    break;
                case "0" :
                    return;
                default:
                    Console.WriteLine("Invalid choice, try again.");
                    break;
            }
        }
    }
}