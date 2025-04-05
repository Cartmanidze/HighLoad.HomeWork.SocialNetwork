using HighLoad.HomeWork.SocialNetwork.DialogService.Extensions;
using HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.DialogService.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Controllers;

[Authorize]
[ApiController]
[Route("dialogs")]
public class DialogsController(IDialogService dialogService, IUserValidationService userValidationService) : ControllerBase
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

        var userExists = await userValidationService.UserExistsAsync(request.ReceiverId);

        if (!userExists)
        {
            return BadRequest("The user does not exist.");
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
    
    [HttpPost("read/{messageId:guid}")]
    public async Task<IActionResult> MarkAsRead(Guid messageId)
    {
        var userId = User.GetUserId();
        
        if (userId == null)
        {
            return Unauthorized();
        }
        
        if (messageId == Guid.Empty)
        {
            return BadRequest("Invalid message ID.");
        }
        
        await dialogService.MarkAsReadAsync(userId.Value, messageId);
        
        return Ok(new { status = "Message marked as read" });
    }
    
    [HttpPost("read-batch")]
    public async Task<IActionResult> MarkMultipleAsRead([FromBody] MarkAsReadRequest request)
    {
        var userId = User.GetUserId();
        
        if (userId == null)
        {
            return Unauthorized();
        }
        
        if (request == null || request.MessageIds == null || request.MessageIds.Count == 0)
        {
            return BadRequest("No message IDs provided.");
        }
        
        await dialogService.MarkManyAsReadAsync(userId.Value, request.MessageIds);
        
        return Ok(new { status = $"{request.MessageIds.Count} messages marked as read" });
    }
}