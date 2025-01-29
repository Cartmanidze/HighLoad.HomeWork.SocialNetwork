using HighLoad.HomeWork.SocialNetwork.DialogService.Extensions;
using HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.DialogService.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Controllers;

[Authorize]
[ApiController]
[Route("dialogs")]
public class DialogsController(IDialogService dialogService) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest? request)
    {
        var userId = User.GetUserId();
        
        if (userId == null)
        {
            return Unauthorized();
        }
        
        if (request == null || request.ReceiverId == Guid.Empty || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Invalid request body.");
        }

        await dialogService.SaveMessageAsync(request.ReceiverId, userId.Value, request.Text);
        
        return Ok(new { status = "Message sent" });
    }
    
    [HttpGet("list")]
    public async Task<IActionResult> GetDialog([FromQuery] Guid receiverId)
    {
        var userId = User.GetUserId();
        
        if (userId == null)
        {
            return Unauthorized();
        }
        
        if (receiverId == Guid.Empty)
        {
            return BadRequest("Missing or invalid 'receiverId' userId.");
        }
        
        var messages = await dialogService.GetDialogAsync(userId.Value, receiverId);
        
        return Ok(messages);
    }
}