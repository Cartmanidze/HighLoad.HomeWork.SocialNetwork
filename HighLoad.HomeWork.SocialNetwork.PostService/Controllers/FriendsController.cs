using HighLoad.HomeWork.SocialNetwork.PostService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HighLoad.HomeWork.SocialNetwork.PostService.Controllers;

[ApiController]
[Route("friends")]
public sealed class FriendsController : ControllerBase
{
    private readonly IFriendService _friendService;

    public FriendsController(IFriendService friendService)
    {
        _friendService = friendService;
    }
    
    [HttpPost]
    public async Task<IActionResult> AddFriend([FromQuery] Guid userId, [FromQuery] Guid friendId)
    {
        if (userId == Guid.Empty || friendId == Guid.Empty)
            return BadRequest("UserId and FriendId must be valid GUIDs.");

        await _friendService.AddFriendAsync(userId, friendId);
        return Ok($"User {userId} and {friendId} are now friends (one-way or two-way).");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFriend([FromQuery] Guid userId, [FromQuery] Guid friendId)
    {
        if (userId == Guid.Empty || friendId == Guid.Empty)
            return BadRequest("UserId and FriendId must be valid GUIDs.");

        await _friendService.DeleteFriendAsync(userId, friendId);
        return Ok($"Friend relationship between {userId} and {friendId} deleted.");
    }
}