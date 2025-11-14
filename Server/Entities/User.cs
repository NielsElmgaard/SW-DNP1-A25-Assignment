namespace Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    
    public List<Post> Posts { get; set; } = new List<Post>();

    public List<Comment> Comments { get; set; } = new List<Comment>();
    
    public User(int id, string username, string password)
    {
        Id = id;
        Username = username;
        Password = password;
    }
    
    private User(){} // for EFC
}