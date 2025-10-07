namespace ApiContracts_DTOs;

public class CommentDTO
{
    public int Id { get; set; }
    public string Body { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }

    public UserDTO? author { get; set; }
}