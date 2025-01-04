using HighLoad.HomeWork.SocialNetwork.PostService.Requests;
using HighLoad.HomeWork.SocialNetwork.PostService.Responses;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;

public interface IPostService
{
    Task<PostResponse?> GetAsync(Guid postId);
    
    Task<Guid> CreateAsync(PostCreateRequest createRequest);
    
    Task UpdateAsync(PostUpdateRequest updateRequest);
    
    Task DeleteAsync(Guid postId);
    
    Task<IReadOnlyCollection<PostResponse>> GetFriendsFeedAsync(Guid userId, int limit);
}