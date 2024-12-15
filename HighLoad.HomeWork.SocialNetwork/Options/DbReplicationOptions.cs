namespace HighLoad.HomeWork.SocialNetwork.Options;

public class DbReplicationOptions
{
    public string Master { get; set; } = string.Empty;
    public string[] Slaves { get; set; } = [];
}