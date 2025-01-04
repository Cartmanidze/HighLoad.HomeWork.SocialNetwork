namespace HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;

public interface IFriendService
{
    Task AddFriendAsync(Guid userId, Guid friendId);
    
    Task DeleteFriendAsync(Guid userId, Guid friendId);
}