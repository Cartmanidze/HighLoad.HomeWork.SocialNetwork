namespace HighLoad.HomeWork.SocialNetwork.DialogService.Requests;

public class SendMessageRequest
{
    public Guid SenderId { get; init; }
    
    public string Text { get; init; } = null!;
}