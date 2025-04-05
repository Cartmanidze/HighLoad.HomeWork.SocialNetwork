namespace HighLoad.HomeWork.SocialNetwork.DialogService.Events;

public class NewMessageEvent
{
    public Guid RecipientId { get; set; }
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public DateTime Timestamp { get; set; }
}