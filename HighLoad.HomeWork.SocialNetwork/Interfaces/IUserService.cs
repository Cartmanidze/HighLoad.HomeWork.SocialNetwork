using HighLoad.HomeWork.SocialNetwork.Models;

namespace HighLoad.HomeWork.SocialNetwork.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByEmailAsync(string email);
    
    Task<bool> UserExistsAsync(string email);
    
    Task<User?> GetUserByIdAsync(Guid id);
    
    Task SaveUserAsync(User user);
}