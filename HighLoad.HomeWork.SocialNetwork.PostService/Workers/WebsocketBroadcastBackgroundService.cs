using System.Text;
using System.Text.Json;
using HighLoad.HomeWork.SocialNetwork.PostService.Events;
using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.PostService.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Workers;

public class WebsocketBroadcastBackgroundService : BackgroundService, IAsyncDisposable
{
    private readonly IWebsocketConnectionManager _wsManager;
    private readonly IFriendRepository _friendRepository;
    private readonly RabbitMqOptions _mqOptions;

    private IConnection? _connection;
    private IChannel? _channel;

    public WebsocketBroadcastBackgroundService(
        IWebsocketConnectionManager wsManager,
        IFriendRepository friendRepository,
        IOptions<RabbitMqOptions> mqOptionsAccessor)
    {
        _wsManager = wsManager;
        _friendRepository = friendRepository;
        _mqOptions = mqOptionsAccessor.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _mqOptions.HostName,
            Port = _mqOptions.Port,
            UserName = _mqOptions.UserName,
            Password = _mqOptions.Password
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _mqOptions.ExchangeName,
            type: _mqOptions.ExchangeType,
            durable: true,
            autoDelete: false, cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: _mqOptions.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false, cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: _mqOptions.QueueName,
            exchange: _mqOptions.ExchangeName,
            routingKey: _mqOptions.RoutingKey, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceived;
        
        await _channel.BasicConsumeAsync(_mqOptions.QueueName, autoAck: true, consumer: consumer, cancellationToken: stoppingToken);
    }

    private async Task OnMessageReceived(object? sender, BasicDeliverEventArgs e)
    {
        try
        {
            var body = e.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            var postEvt = JsonSerializer.Deserialize<PostCreatedEvent>(json);
            if (postEvt == null)
                return;

            // 1. Узнаём друзей автора
            var friendIds = await _friendRepository.GetFriendIdsAsync(postEvt.AuthorId);
            if (friendIds.Count == 0)
                return;

            // 2. Формируем JSON для отправки
            var payload = JsonSerializer.Serialize(new
            {
                type = "PostCreated",
                postId = postEvt.PostId,
                authorId = postEvt.AuthorId,
                content = postEvt.Content,
                createdAt = postEvt.CreatedAt
            });

            // 3. Шлём друзьям по вебсокету
            await _wsManager.SendToUsersAsync(friendIds, payload);
        }
        catch (Exception ex)
        {
            // Логируем/обрабатываем
            Console.WriteLine(ex);
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        base.Dispose();
        if (_connection != null) await _connection.DisposeAsync();
        if (_channel != null) await _channel.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
}