using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.PostService.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Repositories;

internal sealed class FriendRepository(IOptions<DbOptions> options) : IFriendRepository
{
    public async Task AddAsync(Guid userId, Guid friendId)
    {
        const string sql = @"
            INSERT INTO Friendships (Id, UserId, FriendId, CreatedAt)
            VALUES (@Id, @UserId, @FriendId, @CreatedAt)
            ON CONFLICT (UserId, FriendId) DO NOTHING
        ";

        var friendshipId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(options.Value.PostServiceDb);
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

        await using var conn = new NpgsqlConnection(options.Value.PostServiceDb);
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

        await using var conn = new NpgsqlConnection(options.Value.PostServiceDb);
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
    
    public async Task<IReadOnlyCollection<(Guid UserId, Guid FriendId)>> GetFriendsAsync(int limit)
    {
        if (limit <= 0)
            return Array.Empty<(Guid, Guid)>();

        const string sql = @"
        SELECT UserId, FriendId
        FROM Friendships
        ORDER BY UserId
        LIMIT @Limit
    ";

        var pairs = new List<(Guid UserId, Guid FriendId)>();

        await using var conn = new NpgsqlConnection(options.Value.PostServiceDb);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Limit", limit);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var uid = reader.GetGuid(reader.GetOrdinal("UserId"));
            var fid = reader.GetGuid(reader.GetOrdinal("FriendId"));
            pairs.Add((uid, fid));
        }

        return pairs;
    }
}