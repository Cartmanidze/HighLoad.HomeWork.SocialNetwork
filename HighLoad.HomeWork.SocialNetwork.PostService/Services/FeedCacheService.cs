using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.PostService.Models;
using HighLoad.HomeWork.SocialNetwork.PostService.Responses;
using Microsoft.Extensions.Caching.Memory;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Services;

internal sealed class FeedCacheService(
    IMemoryCache memoryCache,
    IFriendRepository friendRepository,
    IPostRepository postRepository)
    : IFeedCacheService
{
    private const int FeedLimit = 1000;
    private const int CacheExpirationSeconds = 60;

    public async Task<IReadOnlyCollection<PostResponse>> GetFeedAsync(Guid userId)
    {
        var cacheKey = $"feed_{userId}";
        
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyCollection<PostResponse>? feed))
        {
            return feed!;
        }
        
        var friendIds = await friendRepository.GetFriendIdsAsync(userId);

        var posts = await postRepository.GetPostsByAuthorsAsync(friendIds, FeedLimit);

        var freshFeed = posts.Select(Map).ToArray();
        
        memoryCache.Set(cacheKey, freshFeed, TimeSpan.FromSeconds(CacheExpirationSeconds));
        
        return freshFeed;
    }

    public void InvalidateFeed(Guid userId)
    {
        var cacheKey = $"feed_{userId}";
        memoryCache.Remove(cacheKey);
    }

    public async Task RebuildCacheAsync(Guid userId)
    {
        InvalidateFeed(userId);
        
        _ = await GetFeedAsync(userId);
    }
    
    private PostResponse Map(Post post) =>
        new()
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
}