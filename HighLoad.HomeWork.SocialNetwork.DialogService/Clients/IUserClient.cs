using Refit;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Clients;

[Headers("Content-Type: application/json")]
public interface IUserClient
{
    [Get("/users/{id}")]
    Task<UserResponse?> GetUserByIdAsync(Guid id);
}