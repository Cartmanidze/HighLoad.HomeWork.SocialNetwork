using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.PostService.Models;
using Npgsql;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Repositories;

internal sealed class PostRepository : IPostRepository
{
    private readonly string _connectionString;
    
    public PostRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostServiceDb")
                            ?? throw new InvalidOperationException("PostServiceDb connection string is missing.");
    }
    
    public async Task<Post?> GetAsync(Guid postId)
    {
        const string sql = @"
            SELECT Id, AuthorId, Content, CreatedAt, UpdatedAt
            FROM Posts
            WHERE Id = @Id
        ";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", postId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Post
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                AuthorId = reader.GetGuid(reader.GetOrdinal("AuthorId")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }
        return null;
    }

    public async Task<Guid> CreateAsync(Post post)
    {
        post.Id = Guid.NewGuid();
        post.CreatedAt = DateTime.UtcNow;
        post.UpdatedAt = post.CreatedAt;

        const string sql = @"
            INSERT INTO Posts (Id, AuthorId, Content, CreatedAt, UpdatedAt)
            VALUES (@Id, @AuthorId, @Content, @CreatedAt, @UpdatedAt)
        ";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", post.Id);
        cmd.Parameters.AddWithValue("@AuthorId", post.AuthorId);
        cmd.Parameters.AddWithValue("@Content", post.Content);
        cmd.Parameters.AddWithValue("@CreatedAt", post.CreatedAt);
        cmd.Parameters.AddWithValue("@UpdatedAt", post.UpdatedAt);

        await cmd.ExecuteNonQueryAsync();

        return post.Id;
    }

    public async Task UpdateAsync(Post post)
    {
        var now = DateTime.UtcNow;
        post.UpdatedAt = now;

        const string sql = @"
            UPDATE Posts
            SET Content   = @Content,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id
        ";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", post.Id);
        cmd.Parameters.AddWithValue("@Content", post.Content);
        cmd.Parameters.AddWithValue("@UpdatedAt", post.UpdatedAt);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid postId)
    {
        const string sql = "DELETE FROM Posts WHERE Id = @Id";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", postId);

        await cmd.ExecuteNonQueryAsync();
    }
}