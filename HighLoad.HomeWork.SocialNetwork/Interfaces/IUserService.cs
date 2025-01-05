using HighLoad.HomeWork.SocialNetwork.Models;

namespace HighLoad.HomeWork.SocialNetwork.Interfaces;

public interface IUserService
{
    Task<User?> GetByEmailAsync(string email);
    
    Task<bool> ExistsAsync(string email);
    
    Task<User?> GetByIdAsync(Guid id);
    
    Task SaveAsync(User user);
    
    Task<IReadOnlyCollection<User>> SearchAsync(string firstName, string lastName);

    Task BulkInsertAsync(IEnumerable<User> users);

    Task<IReadOnlyCollection<string>> GetAllEmailsAsync();

    Task<IReadOnlyCollection<Guid>> GetUserIdsAsync(int limit);
}