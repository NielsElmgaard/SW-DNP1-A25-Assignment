using System.ComponentModel.DataAnnotations;

namespace ApiContracts_DTOs;

public class CreatePostDTO
{
    [Required(ErrorMessage = "Title is required and cannot be empty.")]
    public required string Title { get; set; }

    [Required(ErrorMessage = "Body is required and cannot be empty.")]
    public required string Body { get; set; }
    public int? UserId { get; set; }
}