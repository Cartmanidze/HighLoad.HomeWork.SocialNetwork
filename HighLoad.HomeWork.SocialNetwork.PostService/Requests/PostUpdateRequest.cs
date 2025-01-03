namespace HighLoad.HomeWork.SocialNetwork.PostService.Requests;

public class PostUpdateRequest
{
    public Guid Id { get; init; }
    
    public Guid AuthorId { get; init; }
    
    public string Content { get; init; } = null!;
}