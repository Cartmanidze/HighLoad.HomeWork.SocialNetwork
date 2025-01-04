namespace HighLoad.HomeWork.SocialNetwork.PostService.Responses;

public class PostResponse
{
    public Guid Id { get; init; }
    public Guid AuthorId { get; init; }
    public string Content { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}