using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Services;

internal sealed class FriendService(IFriendRepository friendRepository, IFeedCacheService feedCacheService)
    : IFriendService
{
    public async Task AddFriendAsync(Guid userId, Guid friendId)
    {
        await friendRepository.AddAsync(userId, friendId);

        // При добавлении друга логично также инвалидировать кэш userId,
        // чтобы при следующем запросе лента была пересчитана (новые друзья).
        feedCacheService.InvalidateFeed(userId);

        // Аналогично, можно инвалидировать feed для friendId (он теперь увидит post userId)
        feedCacheService.InvalidateFeed(friendId);
    }
    
    public async Task DeleteFriendAsync(Guid userId, Guid friendId)
    {
        await friendRepository.DeleteAsync(userId, friendId);

        // При удалении друга тоже стоит инвалидировать кэши
        feedCacheService.InvalidateFeed(userId);
        feedCacheService.InvalidateFeed(friendId);
    }
}