using HighLoad.HomeWork.SocialNetwork.Interfaces;
using HighLoad.HomeWork.SocialNetwork.Models;
using HighLoad.HomeWork.SocialNetwork.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HighLoad.HomeWork.SocialNetwork.Controllers;

[ApiController]
[Route("users")]
public class UserController(IUserService userService) : ControllerBase
{
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await userService.GetByIdAsync(id);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var userProfile = MapUser(user);

        return Ok(userProfile);
    }
    
    [Authorize]
    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string firstName, [FromQuery] string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return BadRequest("Both firstName and lastName parameters are required.");
        }

        var users = await userService.SearchAsync(firstName, lastName);

        if (!users.Any())
        {
            return NotFound("No users found matching the criteria.");
        }

        var userProfiles = users.Select(MapUser);

        return Ok(userProfiles);
    }
    
    [Authorize]
    [HttpGet("ids")]
    public async Task<IActionResult> GetUserIds([FromQuery] int limit = 100)
    {
        if (limit <= 0)
            return BadRequest("Limit must be a positive number.");

        var ids = await userService.GetUserIdsAsync(limit);
        if (ids.Count == 0)
            return NotFound("No user ids found.");

        return Ok(ids);
    }

    private static UserResponse MapUser(User user)
    {
        var userProfile = new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Interests = user.Interests,
            City = user.City,
            Email = user.Email
        };
        return userProfile;
    }
}