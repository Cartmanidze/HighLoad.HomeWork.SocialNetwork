using HighLoad.HomeWork.SocialNetwork.DialogService.Models;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;

public interface IDialogService
{
    Task SaveMessageAsync(Guid senderId, Guid receiverId, string text);
    
    Task<IReadOnlyCollection<Message>> GetDialogAsync(Guid userId, Guid otherUserId);
}