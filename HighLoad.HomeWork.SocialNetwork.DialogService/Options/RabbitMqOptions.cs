namespace HighLoad.HomeWork.SocialNetwork.DialogService.Options;

public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public int Port { get; set; } = 5672;
} 