using HighLoad.HomeWork.SocialNetwork.DialogService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.DialogService.Requests;
using Microsoft.AspNetCore.Mvc;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Controllers;

[ApiController]
[Route("dialogs/{userId:guid}")]
public class DialogsController(IDialogService dialogService) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage(Guid userId, [FromBody] SendMessageRequest? request)
    {
        if (request == null || request.SenderId == Guid.Empty || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Invalid request body.");
        }

        await dialogService.SaveMessageAsync(request.SenderId, userId, request.Text);
        return Ok(new { status = "Message sent" });
    }
    
    [HttpGet("list")]
    public async Task<IActionResult> GetDialog(Guid userId, [FromQuery] Guid other)
    {
        if (other == Guid.Empty)
        {
            return BadRequest("Missing or invalid 'other' userId.");
        }

        var messages = await dialogService.GetDialogAsync(userId, other);
        return Ok(messages);
    }
}