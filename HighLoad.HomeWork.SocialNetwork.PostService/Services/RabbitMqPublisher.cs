using System.Text;
using System.Text.Json;
using HighLoad.HomeWork.SocialNetwork.PostService.Events;
using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.PostService.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Services;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly RabbitMqOptions _options;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task PublishPostCreatedAsync(PostCreatedEvent evt)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: _options.ExchangeType,
            durable: true,
            autoDelete: false
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
        
        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: _options.RoutingKey,
            body: body
        );
        
    }
}