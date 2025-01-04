using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Services;

internal sealed class FriendService : IFriendService
{
    private readonly IFriendRepository _friendRepository;
    
    private readonly IFeedCacheService _feedCacheService;

    public FriendService(IFriendRepository friendRepository, IFeedCacheService feedCacheService)
    {
        _friendRepository = friendRepository;
        _feedCacheService = feedCacheService;
    }
    
    public async Task AddFriendAsync(Guid userId, Guid friendId)
    {
        await _friendRepository.AddAsync(userId, friendId);

        // При добавлении друга логично также инвалидировать кэш userId,
        // чтобы при следующем запросе лента была пересчитана (новые друзья).
        _feedCacheService.InvalidateFeed(userId);

        // Аналогично, можно инвалидировать feed для friendId (он теперь увидит post userId)
        _feedCacheService.InvalidateFeed(friendId);
    }
    
    public async Task DeleteFriendAsync(Guid userId, Guid friendId)
    {
        await _friendRepository.DeleteAsync(userId, friendId);

        // При удалении друга тоже стоит инвалидировать кэши
        _feedCacheService.InvalidateFeed(userId);
        _feedCacheService.InvalidateFeed(friendId);
    }
}