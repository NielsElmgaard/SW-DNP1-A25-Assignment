namespace ApiContracts_DTOs;

public class CreateCommentDTO
{
    public required string Body { get; set; }
    public required int PostId { get; set; }
    public required int UserId { get; set; }
}