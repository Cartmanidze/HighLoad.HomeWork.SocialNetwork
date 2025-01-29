namespace HighLoad.HomeWork.SocialNetwork.PostService.Events;

public record PostCreatedEvent(Guid PostId, Guid AuthorId, string Content, DateTime CreatedAt);