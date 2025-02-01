namespace HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;

public interface IUserValidationService
{
    Task<bool> UserExistsAsync(Guid userId);
}