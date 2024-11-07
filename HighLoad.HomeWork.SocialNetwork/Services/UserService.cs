using HighLoad.HomeWork.SocialNetwork.Interfaces;
using HighLoad.HomeWork.SocialNetwork.Models;
using Npgsql;

namespace HighLoad.HomeWork.SocialNetwork.Services;

internal sealed class UserService(IConfiguration configuration) : IUserService
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;


    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        
        await connection.OpenAsync();

        var query = "SELECT * FROM Users WHERE Email = @Email";

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapReaderToUser(reader);
        }

        return null;
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        var user = await GetUserByEmailAsync(email);
        
        return user != null;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
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

    public async Task SaveUserAsync(User user)
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

    private static User MapReaderToUser(NpgsqlDataReader reader)
    {
        return new User
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
}