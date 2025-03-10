using System.Data;

namespace HighLoad.HomeWork.SocialNetwork.Data;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}