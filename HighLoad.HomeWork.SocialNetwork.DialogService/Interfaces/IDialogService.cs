using HighLoad.HomeWork.SocialNetwork.DialogService.Models;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;

public interface IDialogService
{
    Task SaveMessageAsync(Guid receiverId, Guid senderId, string text);
    
    Task<IReadOnlyCollection<Message>> GetDialogAsync(Guid userId, Guid otherUserId);
    
    Task MarkAsReadAsync(Guid userId, Guid messageId);
    
    Task MarkManyAsReadAsync(Guid userId, IEnumerable<Guid> messageIds);
}