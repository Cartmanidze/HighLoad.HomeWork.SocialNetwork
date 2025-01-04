namespace HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;

public interface IFriendRepository
{
    Task AddAsync(Guid userId, Guid friendId);
    
    Task DeleteAsync(Guid userId, Guid friendId);
    
    Task<IReadOnlyCollection<Guid>> GetFriendIdsAsync(Guid userId);
}