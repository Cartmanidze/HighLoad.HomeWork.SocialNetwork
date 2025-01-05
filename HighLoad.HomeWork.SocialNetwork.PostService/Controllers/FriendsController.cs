using HighLoad.HomeWork.SocialNetwork.PostService.Clients;
using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Controllers;

[ApiController]
[Route("friends")]
public sealed class FriendsController(IFriendService friendService, IUserClient userClient) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddFriend([FromQuery] Guid userId, [FromQuery] Guid friendId)
    {
        if (userId == Guid.Empty || friendId == Guid.Empty)
            return BadRequest("UserId and FriendId must be valid GUIDs.");

        await friendService.AddFriendAsync(userId, friendId);
        return Ok($"User {userId} and {friendId} are now friends (one-way or two-way).");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFriend([FromQuery] Guid userId, [FromQuery] Guid friendId)
    {
        if (userId == Guid.Empty || friendId == Guid.Empty)
            return BadRequest("UserId and FriendId must be valid GUIDs.");

        await friendService.DeleteFriendAsync(userId, friendId);
        return Ok($"Friend relationship between {userId} and {friendId} deleted.");
    }
    
    /// <summary>
    /// Генерация случайных связей (дружбы) между пользователями.
    /// </summary>
    /// <param name="totalFriendships">Сколько всего связей надо создать</param>
    /// <param name="friendsPerUser">Примерное количество друзей на одного пользователя</param>
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateFriendships([FromQuery] int totalFriendships = 100, [FromQuery] int friendsPerUser = 5)
    {
        if (totalFriendships <= 0 || friendsPerUser <= 0)
        {
            return BadRequest("totalFriendships and friendsPerUser must be positive");
        }
        
        var allUserIds = await userClient.GetUserIdsAsync(50_000); 
        if (allUserIds.Count < 2)
        {
            return BadRequest("Not enough users in the system to create friendships.");
        }
        
        var userIds = allUserIds.ToList();
        
        var random = new Random();
        var createdFriendships = 0;

        // Храним пары (userId, friendId), чтобы не дублировать
        var usedPairs = new HashSet<(Guid, Guid)>();

        while (createdFriendships < totalFriendships)
        {
            // Случайно берём userId
            var userIndex = random.Next(userIds.Count);
            var userId = userIds[userIndex];

            // Примерно friendsPerUser раз на одного юзера
            for (var i = 0; i < friendsPerUser; i++)
            {
                // Выбираем friendId, не совпадающий с userId
                var friendIndex = random.Next(userIds.Count);
                var friendId = userIds[friendIndex];
                if (friendId == userId) 
                {
                    continue;
                }

                // Убедимся, что пары (userId, friendId) или (friendId, userId) нет
                var pair = (userId, friendId);
                var reversed = (friendId, userId);
                if (usedPairs.Contains(pair) || usedPairs.Contains(reversed))
                {
                    continue; // уже есть такая дружба
                }

                // Добавляем дружбу
                await friendService.AddFriendAsync(userId, friendId);

                // Запоминаем в usedPairs, что эти двое уже "друзья"
                usedPairs.Add(pair);
                usedPairs.Add(reversed);

                createdFriendships++;
                if (createdFriendships >= totalFriendships)
                {
                    break;
                }
            }
        }

        return Ok($"{createdFriendships} friendships generated successfully");
    }
}