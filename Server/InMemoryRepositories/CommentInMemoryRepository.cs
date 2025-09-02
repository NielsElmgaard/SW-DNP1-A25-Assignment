using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class CommentInMemoryRepository : ICommentRepository
{
    private List<Comment> comments;
    
    public CommentInMemoryRepository()
    {
        comments = new List<Comment>();
        AddDummyData();
    }
    
    private void AddDummyData()
    {
        comments.AddRange(new List<Comment>
        {
            new Comment(1, "Nice!", 1,2),
            new Comment(2, "Buuh", 2,1),
            new Comment(3, "Good.", 2,3),
        });
    }

    public Task<Comment> AddAsync(Comment comment)
    {
        comment.Id = comments.Any()
            ? comments.Max(p => p.Id) +
              1 
            : 1; 
        comments.Add(comment);
        return Task.FromResult(comment);
    }

    public Task UpdateAsync(Comment comment)
    {
        Comment? existingComment =
            comments.SingleOrDefault(p =>
                p.Id == comment
                    .Id);
        if (existingComment is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{comment.Id}' not found");
        }

        comments.Remove(existingComment);
        comments.Add(comment);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Comment? commentToRemove = comments.SingleOrDefault(p => p.Id == id);
        if (commentToRemove is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{id}' not found");
        }

        comments.Remove(commentToRemove);
        return Task.CompletedTask;
    }

    public Task<Comment> GetSingleAsync(int id)
    {
        Comment? comment = comments.SingleOrDefault(p => p.Id == id);
        if (comment is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{id}' not found");
        }

        return Task.FromResult(comment);
    }
    
    public IQueryable<Comment> GetMany()
    {
        return comments.AsQueryable();
    }
}