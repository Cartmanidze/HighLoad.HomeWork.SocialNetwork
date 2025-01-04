namespace HighLoad.HomeWork.SocialNetwork.PostService.Models;

public class Post
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// AuthorId == UserId из основного сервиса пользователей
    /// </summary>
    public Guid AuthorId { get; init; }
    public string Content { get; init; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}