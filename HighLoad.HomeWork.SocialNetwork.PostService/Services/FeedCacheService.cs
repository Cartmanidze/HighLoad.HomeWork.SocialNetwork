using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.PostService.Responses;
using Microsoft.Extensions.Caching.Memory;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Services;

internal sealed class FeedCacheService : IFeedCacheService
{
    private const int FeedLimit = 1000;
    private const int CacheExpirationSeconds = 60;
    
    private readonly IPostService _postService;
    private readonly IMemoryCache _memoryCache;

    public FeedCacheService(IMemoryCache memoryCache, IPostService postService)
    {
        _memoryCache = memoryCache;
        _postService = postService;
    }
    
    public async Task<IReadOnlyCollection<PostResponse>> GetFeedAsync(Guid userId)
    {
        var cacheKey = $"feed_{userId}";
        
        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyCollection<PostResponse>? feed))
        {
            return feed!;
        }
        
        var freshFeed = await _postService.GetFriendsFeedAsync(userId, FeedLimit);
        
        _memoryCache.Set(cacheKey, freshFeed, TimeSpan.FromSeconds(CacheExpirationSeconds));
        
        return freshFeed;
    }

    public void InvalidateFeed(Guid userId)
    {
        var cacheKey = $"feed_{userId}";
        _memoryCache.Remove(cacheKey);
    }

    public async Task RebuildCacheAsync(Guid userId)
    {
        InvalidateFeed(userId);
        
        _ = await GetFeedAsync(userId);
    }
}