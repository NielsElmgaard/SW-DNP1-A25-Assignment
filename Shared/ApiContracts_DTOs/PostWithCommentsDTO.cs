namespace ApiContracts_DTOs;

public class PostWithCommentsDTO : PostDTO
{
    public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();
}