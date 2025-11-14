namespace Entities;

public class Comment
{
    public int Id { get; set; }
    public string Body { get; set; }
    public int PostId { get; set; }
    
    public Post Post { get; set; }
    public int UserId { get; set; }
    
    public User User { get; set; }

    public Comment(int id, string body, int postId, int userId)
    {
        Id = id;
        Body = body;
        PostId = postId;
        UserId = userId;
    }
    
    private Comment(){} // for EFC
}