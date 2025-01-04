using HighLoad.HomeWork.SocialNetwork.PostService.Responses;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;

public interface IFeedCacheService
{
    Task<IReadOnlyCollection<PostResponse>> GetFeedAsync(Guid userId);
    
    void InvalidateFeed(Guid userId);
    
    Task RebuildCacheAsync(Guid userId);
}