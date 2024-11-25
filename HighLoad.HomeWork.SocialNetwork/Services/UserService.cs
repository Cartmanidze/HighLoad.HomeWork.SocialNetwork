using HighLoad.HomeWork.SocialNetwork.Interfaces;
using HighLoad.HomeWork.SocialNetwork.Models;
using Npgsql;

namespace HighLoad.HomeWork.SocialNetwork.Services;

internal sealed class UserService(IConfiguration configuration) : IUserService
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        
        await connection.OpenAsync();

        const string query = "SELECT * FROM Users WHERE Email = @Email";

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapReaderToUser(reader);
        }

        return null;
    }

    public async Task<bool> ExistsAsync(string email)
    {
        var user = await GetByEmailAsync(email);
        
        return user != null;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = "SELECT * FROM Users WHERE Id = @Id";

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapReaderToUser(reader);
        }

        return null;
    }

    public async Task SaveAsync(User user)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"INSERT INTO Users 
                         (FirstName, LastName, DateOfBirth, Gender, Interests, City, Email, PasswordHash)
                         VALUES (@FirstName, @LastName, @DateOfBirth, @Gender, @Interests, @City, @Email, @PasswordHash)";

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@FirstName", user.FirstName);
        command.Parameters.AddWithValue("@LastName", user.LastName);
        command.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth);
        command.Parameters.AddWithValue("@Gender", user.Gender);

        if (user.Interests != null)
        {
            command.Parameters.AddWithValue("@Interests", user.Interests!);
        }

        if (user.City != null)
        {
            command.Parameters.AddWithValue("@City", user.City);
        }
        
        command.Parameters.AddWithValue("@Email", user.Email);
        
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyCollection<User>> SearchAsync(string firstName, string lastName)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
        SELECT * 
        FROM Users 
        WHERE FirstName ILIKE @FirstNamePattern AND LastName ILIKE @LastNamePattern";

        var users = new List<User>();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@FirstNamePattern", $"%{firstName}%");
        command.Parameters.AddWithValue("@LastNamePattern", $"%{lastName}%");

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(MapReaderToUser(reader));
        }

        return users;
    }

    public async Task BulkInsertAsync(IEnumerable<User> users)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var writer = await connection.BeginBinaryImportAsync(
            @"COPY Users (FirstName, LastName, DateOfBirth, Gender, Interests, City, Email, PasswordHash) 
          FROM STDIN (FORMAT BINARY)");

        foreach (var user in users)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(user.FirstName, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(user.LastName, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(user.DateOfBirth, NpgsqlTypes.NpgsqlDbType.Date);
            await writer.WriteAsync(user.Gender, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(user.Interests ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(user.City ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(user.Email, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(user.PasswordHash, NpgsqlTypes.NpgsqlDbType.Text);
        }

        await writer.CompleteAsync();
    }
    
    public async Task<IReadOnlyCollection<string>> GetAllEmailsAsync()
    {
        var emails = new List<string>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = "SELECT Email FROM Users";

        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            emails.Add(reader.GetString(0));
        }

        return emails;
    }

    private static User MapReaderToUser(NpgsqlDataReader reader) =>
        new()
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
            Gender = reader.GetString(reader.GetOrdinal("Gender")),
            Interests = reader.IsDBNull(reader.GetOrdinal("Interests")) ? null : reader.GetString(reader.GetOrdinal("Interests")),
            City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash"))
        };
}