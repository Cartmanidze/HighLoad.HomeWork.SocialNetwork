namespace HighLoad.HomeWork.SocialNetwork.DialogService.Models;

public class Message
{
    public Guid Id { get; init; }
    
    public Guid SenderId { get; init; }
    
    public Guid ReceiverId { get; init; }
    
    public string Text { get; init; } = null!;
    
    public DateTime CreatedAt { get; init; }
}