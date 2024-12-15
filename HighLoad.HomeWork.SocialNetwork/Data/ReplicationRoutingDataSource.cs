using System.Data;
using HighLoad.HomeWork.SocialNetwork.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HighLoad.HomeWork.SocialNetwork.Data;

public class ReplicationRoutingDataSource : IDbConnectionFactory
{
    private readonly string _masterConnectionString;
    private readonly List<string> _slaveConnectionStrings;
    private readonly ITransactionState _transactionState;
    
    private int _lastSlaveIndex = -1;

    public ReplicationRoutingDataSource(IOptions<DbReplicationOptions> options, ITransactionState transactionState)
    {
        _masterConnectionString = options.Value.Master;
        _slaveConnectionStrings = new List<string>(options.Value.Slaves);
        _transactionState = transactionState;
    }

    public IDbConnection CreateConnection()
    {
        var isReadOnly = _transactionState.IsReadOnly;
        if (isReadOnly && _slaveConnectionStrings.Count > 0)
        {
            var nextIndex = Interlocked.Increment(ref _lastSlaveIndex);
            var slave = _slaveConnectionStrings[nextIndex % _slaveConnectionStrings.Count];
            return new NpgsqlConnection(slave);
        }
        
        return new NpgsqlConnection(_masterConnectionString);
    }
}