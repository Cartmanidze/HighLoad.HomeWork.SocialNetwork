namespace HighLoad.HomeWork.SocialNetwork.DialogService.Clients;

public class UserResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = null!;
    public string Interests { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Email { get; set; } = null!;
}