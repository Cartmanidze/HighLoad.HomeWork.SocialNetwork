namespace HighLoad.HomeWork.SocialNetwork.PostService.Options;

public class RabbitMqOptions
{
    public string HostName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ExchangeName { get; set; } = "post-exchange";
    public string ExchangeType { get; set; } = "topic";
    public string RoutingKey { get; set; } = "post.created";
    public string QueueName { get; set; } = "websocket-broadcast";
}