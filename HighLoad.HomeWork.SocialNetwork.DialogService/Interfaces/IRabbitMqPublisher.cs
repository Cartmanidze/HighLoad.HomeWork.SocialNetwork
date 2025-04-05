using System;
using System.Threading.Tasks;
using HighLoad.HomeWork.SocialNetwork.DialogService.Models;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishNewMessageEventAsync(Guid messageId, Guid senderId, Guid receiverId);
        Task PublishMessageReadEventAsync(Guid messageId, Guid userId);
    }
} 