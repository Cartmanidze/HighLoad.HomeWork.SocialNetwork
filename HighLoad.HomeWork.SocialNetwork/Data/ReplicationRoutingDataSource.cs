using System.Data;
using HighLoad.HomeWork.SocialNetwork.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HighLoad.HomeWork.SocialNetwork.Data;

public class ReplicationRoutingDataSource : IDbConnectionFactory
{
    private readonly NpgsqlDataSource _masterDataSource;
    private readonly List<NpgsqlDataSource> _slaveDataSources;
    private readonly ITransactionState _transactionState;

    private int _lastSlaveIndex = -1;

    public ReplicationRoutingDataSource(IOptions<DbReplicationOptions> options, ITransactionState transactionState)
    {
        _transactionState = transactionState;
            
        _masterDataSource = NpgsqlDataSource.Create(options.Value.Master);
            
        _slaveDataSources = new List<NpgsqlDataSource>(options.Value.Slaves.Length);
        foreach (var slaveConnString in options.Value.Slaves)
        {
            var slaveDataSource = NpgsqlDataSource.Create(slaveConnString);
            _slaveDataSources.Add(slaveDataSource);
        }
    }

    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var isReadOnly = _transactionState.IsReadOnly;
        if (isReadOnly && _slaveDataSources.Count > 0)
        {
            var nextIndex = Interlocked.Increment(ref _lastSlaveIndex);
            var dataSource = _slaveDataSources[nextIndex % _slaveDataSources.Count];

            var conn = dataSource.CreateConnection();
            await conn.OpenAsync();
            return conn;
        }

        var masterConn = _masterDataSource.CreateConnection();
        await masterConn.OpenAsync();
        return masterConn;
    }
}