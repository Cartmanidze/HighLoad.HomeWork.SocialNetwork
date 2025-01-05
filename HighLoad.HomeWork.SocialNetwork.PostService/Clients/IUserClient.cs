using Refit;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Clients;

[Headers("Content-Type: application/json")]
public interface IUserClient
{
    [Get("/users/ids")]
    Task<IReadOnlyCollection<Guid>> GetUserIdsAsync([Query] int limit = 100);
}