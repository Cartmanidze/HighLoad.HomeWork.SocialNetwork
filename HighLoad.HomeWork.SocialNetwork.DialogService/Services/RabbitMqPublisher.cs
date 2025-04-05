using HighLoad.HomeWork.SocialNetwork.DialogService.Events;
using HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;
using MassTransit;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Services;

public class MassTransitEventPublisher(
    IPublishEndpoint publishEndpoint,
    ILogger<MassTransitEventPublisher> logger)
    : IEventPublisher
{
    public async Task PublishNewMessageEventAsync(Guid messageId, Guid senderId, Guid receiverId)
    {
        try
        {
            var messageEvent = new NewMessageEvent
            {
                MessageId = messageId,
                SenderId = senderId,
                RecipientId = receiverId,
                Timestamp = DateTime.UtcNow
            };

            await publishEndpoint.Publish(messageEvent);

            logger.LogInformation(
                "Опубликовано событие о новом сообщении: MessageId={MessageId}, SenderId={SenderId}, RecipientId={RecipientId}",
                messageId, senderId, receiverId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, 
                "Ошибка при публикации события о новом сообщении: MessageId={MessageId}, SenderId={SenderId}, RecipientId={RecipientId}",
                messageId, senderId, receiverId);
            throw;
        }
    }

    public async Task PublishMessageReadEventAsync(Guid messageId, Guid userId)
    {
        try
        {
            var messageEvent = new MessageReadEvent
            {
                MessageId = messageId,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            await publishEndpoint.Publish(messageEvent);

            logger.LogInformation(
                "Опубликовано событие о прочтении сообщения: MessageId={MessageId}, UserId={UserId}",
                messageId, userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, 
                "Ошибка при публикации события о прочтении сообщения: MessageId={MessageId}, UserId={UserId}",
                messageId, userId);
            throw;
        }
    }
}