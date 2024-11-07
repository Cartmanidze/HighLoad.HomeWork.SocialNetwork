namespace HighLoad.HomeWork.SocialNetwork.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);
    
    bool VerifyPassword(string password, string storedHash);
}