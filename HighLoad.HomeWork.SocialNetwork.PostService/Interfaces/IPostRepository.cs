using HighLoad.HomeWork.SocialNetwork.PostService.Models;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;

public interface IPostRepository
{
    Task<Post?> GetAsync(Guid postId);
    Task<Guid> CreateAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Guid postId);

    Task<IReadOnlyCollection<Post>> GetPostsByAuthorsAsync(IEnumerable<Guid> authorIds, int limit);
}