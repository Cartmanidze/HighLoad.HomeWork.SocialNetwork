using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.PostService.Requests;
using HighLoad.HomeWork.SocialNetwork.PostService.Responses;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Services;

internal sealed class PostService : IPostService
{
    public Task<PostResponse> GetAsync(Guid postId)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> CreateAsync(PostCreateRequest createRequest)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(PostUpdateRequest updateRequest)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid postId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<PostResponse>> GetFriendsFeedAsync(Guid userId, int limit)
    {
        throw new NotImplementedException();
    }
}