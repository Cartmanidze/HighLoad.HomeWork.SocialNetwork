using HighLoad.HomeWork.SocialNetwork.DialogService.Clients;
using HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Services;

public class UserValidationService(IUserClient userClient) : IUserValidationService
{
    public async Task<bool> UserExistsAsync(Guid userId)
    {
        var user = await userClient.GetUserByIdAsync(userId);
        
        return user != null;
    }
}