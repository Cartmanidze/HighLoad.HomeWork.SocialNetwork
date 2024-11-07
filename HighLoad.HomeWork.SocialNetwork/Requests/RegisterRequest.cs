namespace HighLoad.HomeWork.SocialNetwork.Requests;

public class RegisterRequest
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public DateTime DateOfBirth { get; init; }
    public string Gender { get; init; } = null!;
    public string Interests { get; init; } = null!;
    public string City { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
}