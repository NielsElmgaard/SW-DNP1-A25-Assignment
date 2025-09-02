using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class PostInMemoryRepository : IPostRepository
{
    private List<Post> posts;


    public PostInMemoryRepository()
    {
        posts = new List<Post>();
        AddDummyData();
    }

    private void AddDummyData()
    {
        posts.AddRange(new List<Post>
        {
            new Post(1, "DK win", "DK win over Scotland", 1),
            new Post(2, "DK loose", "DK loose over Belarus", 2)
        });
    }

    public Task<Post> AddAsync(Post post)
    {
        post.Id = posts.Any()
            ? posts.Max(p => p.Id) +
              1 // Find the current maximum id and increment by 1 and generate the new id
            : 1; // If no posts on the list, use id=1
        posts.Add(post);
        return Task.FromResult(post); // Wrapping Post into a Task
    }

    public Task UpdateAsync(Post post)
    {
        Post? existingPost = // "?" indicates existingPost might be null
            posts.SingleOrDefault(p =>
                p.Id == post
                    .Id); // Loops through the list to match the predicate
        if (existingPost is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{post.Id}' not found");
        }

        posts.Remove(existingPost);
        posts.Add(post);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Post? postToRemove = posts.SingleOrDefault(p => p.Id == id);
        if (postToRemove is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{id}' not found");
        }

        posts.Remove(postToRemove);
        return Task.CompletedTask;
    }

    public Task<Post> GetSingleAsync(int id)
    {
        Post? post = posts.SingleOrDefault(p => p.Id == id);
        if (post is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{id}' not found");
        }

        return Task.FromResult(post);
    }

    public IQueryable<Post> GetMany()
    {
        return posts.AsQueryable();
    }
}