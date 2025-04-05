using System;
using System.Threading.Tasks;
using HighLoad.HomeWork.SocialNetwork.CounterService.Events;
using HighLoad.HomeWork.SocialNetwork.CounterService.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HighLoad.HomeWork.SocialNetwork.CounterService.Services;

public class MessageReadConsumer(
    ICounterService counterService,
    ILogger<MessageReadConsumer> logger)
    : IConsumer<MessageReadEvent>
{
    public async Task Consume(ConsumeContext<MessageReadEvent> context)
    {
        try
        {
            var message = context.Message;
                
            logger.LogInformation(
                "Получено событие о прочтении сообщения: UserId={UserId}, MessageId={MessageId}",
                message.UserId, message.MessageId);

            // Уменьшаем счетчик непрочитанных сообщений
            await counterService.DecrementCounterAsync(message.UserId, "UnreadMessages");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке события прочтения сообщения");
            throw;
        }
    }
}