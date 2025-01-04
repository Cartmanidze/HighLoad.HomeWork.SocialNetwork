using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using Npgsql;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Repositories;

internal sealed class FriendRepository : IFriendRepository
{
    private readonly string _connectionString;

    public FriendRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostServiceDb")
                            ?? throw new InvalidOperationException("PostServiceDb connection string is missing.");
    }
    
    public async Task AddAsync(Guid userId, Guid friendId)
    {
        const string sql = @"
            INSERT INTO Friendships (Id, UserId, FriendId, CreatedAt)
            VALUES (@Id, @UserId, @FriendId, @CreatedAt)
            ON CONFLICT (UserId, FriendId) DO NOTHING
        ";

        var friendshipId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", friendshipId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@FriendId", friendId);
        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid userId, Guid friendId)
    {
        const string sql = @"
            DELETE FROM Friendships
            WHERE UserId = @UserId AND FriendId = @FriendId
        ";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@FriendId", friendId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyCollection<Guid>> GetFriendIdsAsync(Guid userId)
    {
        const string sql = @"
            SELECT FriendId
            FROM Friendships
            WHERE UserId = @UserId
        ";

        var friendIds = new List<Guid>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            friendIds.Add(reader.GetGuid(0));
        }

        return friendIds;
    }
}