namespace ApiContracts_DTOs;

public class PostWithCommentsDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public int UserId { get; set; }
    public UserDTO Author { get; set; }
    public List<CommentDTO> Comments { get; set; } = new();
}