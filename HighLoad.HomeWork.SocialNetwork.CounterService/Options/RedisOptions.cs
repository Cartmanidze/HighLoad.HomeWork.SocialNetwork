namespace HighLoad.HomeWork.SocialNetwork.CounterService.Options
{
    public class RedisOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string InstanceName { get; set; } = "Counters_";
        public int DefaultExpirationMinutes { get; set; } = 60;
    }
} 