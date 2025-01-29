using HighLoad.HomeWork.SocialNetwork.PostService.Events;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;

public interface IRabbitMqPublisher
{
    Task PublishPostCreatedAsync(PostCreatedEvent evt);
}