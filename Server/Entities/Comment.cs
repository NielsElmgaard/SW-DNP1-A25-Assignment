namespace Entities;

public class Comment
{
    public int Id { get; set; }
    public string Body { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }

    public Comment(int id, string body, int postId, int userId)
    {
        Id = id;
        Body = body;
        PostId = postId;
        UserId = userId;
    }
}