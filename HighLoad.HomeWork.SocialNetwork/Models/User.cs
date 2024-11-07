namespace HighLoad.HomeWork.SocialNetwork.Models;

public class User
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public DateTime DateOfBirth { get; init; }
    public string Gender { get; init; } = null!;
    public string? Interests { get; init; }
    public string? City { get; init; }
    public string Email { get; init; } = null!;
    public string PasswordHash { get; init; } = null!;
}