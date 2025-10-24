namespace ApiContracts_DTOs;

public class CreateCommentDTO
{
    public string Body { get; set; }
    public int PostId { get; set; }
    public int? UserId { get; set; }
}