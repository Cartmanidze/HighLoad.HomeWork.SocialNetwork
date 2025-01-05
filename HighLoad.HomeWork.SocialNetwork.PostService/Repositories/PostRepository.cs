using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.PostService.Models;
using HighLoad.HomeWork.SocialNetwork.PostService.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Repositories;

internal sealed class PostRepository(IOptions<DbOptions> options) : IPostRepository
{
    public async Task<Post?> GetAsync(Guid postId)
    {
        const string sql = @"
            SELECT Id, AuthorId, Content, CreatedAt, UpdatedAt
            FROM Posts
            WHERE Id = @Id
        ";

        await using var conn = new NpgsqlConnection(options.Value.PostServiceDb);
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

        await using var conn = new NpgsqlConnection(options.Value.PostServiceDb);
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

        await using var conn = new NpgsqlConnection(options.Value.PostServiceDb);
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

        await using var conn = new NpgsqlConnection(options.Value.PostServiceDb);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", postId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyCollection<Post>> GetPostsByAuthorsAsync(IEnumerable<Guid> authorIds, int limit)
    {
        var authorsArray = authorIds.ToArray();
        if (authorsArray.Length == 0)
        {
            return Array.Empty<Post>();
        }

        const string sql = @"
            SELECT Id, AuthorId, Content, CreatedAt, UpdatedAt
            FROM Posts
            WHERE AuthorId = ANY(@Authors)
            ORDER BY CreatedAt DESC
            LIMIT @Limit
        ";

        await using var conn = new NpgsqlConnection(options.Value.PostServiceDb);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Authors", authorsArray);
        cmd.Parameters.AddWithValue("@Limit", limit);

        var posts = new List<Post>();

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var post = new Post
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                AuthorId = reader.GetGuid(reader.GetOrdinal("AuthorId")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
            posts.Add(post);
        }
        return posts;
    }
}