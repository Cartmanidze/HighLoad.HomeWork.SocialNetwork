namespace HighLoad.HomeWork.SocialNetwork.PostService.Requests;

public class PostCreateRequest
{
    public Guid AuthorId { get; init; }
    
    public string Content { get; init; } = null!;
}