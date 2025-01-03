namespace HighLoad.HomeWork.SocialNetwork.PostService.Responses;

public class PostResponse
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}