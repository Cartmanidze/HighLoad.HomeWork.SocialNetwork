namespace HighLoad.HomeWork.SocialNetwork.CounterService.Events;

public interface CounterUpdatedEvent
{
    Guid UserId { get; }
    string Type { get; }
    int Count { get; }
    DateTime Timestamp { get; }
}