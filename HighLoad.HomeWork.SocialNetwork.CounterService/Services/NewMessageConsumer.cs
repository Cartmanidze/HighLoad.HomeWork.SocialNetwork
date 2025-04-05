using System;
using System.Threading.Tasks;
using HighLoad.HomeWork.SocialNetwork.CounterService.Events;
using HighLoad.HomeWork.SocialNetwork.CounterService.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HighLoad.HomeWork.SocialNetwork.CounterService.Services;

public class NewMessageConsumer(
    ICounterService counterService,
    ILogger<NewMessageConsumer> logger)
    : IConsumer<NewMessageEvent>
{
    public async Task Consume(ConsumeContext<NewMessageEvent> context)
    {
        try
        {
            var message = context.Message;
                
            logger.LogInformation(
                "Получено событие о новом сообщении: RecipientId={RecipientId}, MessageId={MessageId}, SenderId={SenderId}",
                message.RecipientId, message.MessageId, message.SenderId);

            // Увеличиваем счетчик непрочитанных сообщений
            await counterService.IncrementCounterAsync(message.RecipientId, "UnreadMessages");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке события нового сообщения");
            throw;
        }
    }
}