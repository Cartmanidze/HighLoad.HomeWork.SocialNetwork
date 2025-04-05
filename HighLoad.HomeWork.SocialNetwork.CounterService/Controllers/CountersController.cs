using Asp.Versioning;
using HighLoad.HomeWork.SocialNetwork.CounterService.Interfaces;
using HighLoad.HomeWork.SocialNetwork.CounterService.Requests;
using Microsoft.AspNetCore.Mvc;

namespace HighLoad.HomeWork.SocialNetwork.CounterService.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/counters")]
public class CountersController(
    ICounterService counterService,
    ILogger<CountersController> logger)
    : ControllerBase
{
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserCounters(Guid userId)
    {
        try
        {
            var counters = await counterService.GetUserCountersAsync(userId);
            return Ok(counters);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении счетчиков пользователя {UserId}", userId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpGet("user/{userId:guid}/{type}")]
    public async Task<IActionResult> GetCounter(Guid userId, string type)
    {
        try
        {
            var counter = await counterService.GetCounterAsync(userId, type);
            if (counter == null)
            {
                return NotFound($"Счетчик типа {type} для пользователя {userId} не найден");
            }
            return Ok(counter);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении счетчика {Type} пользователя {UserId}", type, userId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpPost("user/{userId:guid}/increment")]
    public async Task<IActionResult> IncrementCounter(Guid userId, [FromBody] CounterUpdateRequest request)
    {
        try
        {
            var counter = await counterService.IncrementCounterAsync(userId, request.Type, request.Value);
            return Ok(counter);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при инкрементировании счетчика {Type} пользователя {UserId}", request.Type, userId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpPost("user/{userId:guid}/decrement")]
    public async Task<IActionResult> DecrementCounter(Guid userId, [FromBody] CounterUpdateRequest request)
    {
        try
        {
            var counter = await counterService.DecrementCounterAsync(userId, request.Type, request.Value);
            return Ok(counter);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при декрементировании счетчика {Type} пользователя {UserId}", request.Type, userId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpPost("user/{userId:guid}/reset")]
    public async Task<IActionResult> ResetCounter(Guid userId, [FromBody] CounterUpdateRequest request)
    {
        try
        {
            var counter = await counterService.ResetCounterAsync(userId, request.Type);
            return Ok(counter);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при сбросе счетчика {Type} пользователя {UserId}", request.Type, userId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpPut("user/{userId:guid}")]
    public async Task<IActionResult> UpdateCounter(Guid userId, [FromBody] CounterUpdateRequest request)
    {
        try
        {
            var counter = await counterService.UpdateCounterAsync(userId, request.Value, request.Type);
            return Ok(counter);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении счетчика {Type} пользователя {UserId}", request.Type, userId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
}