namespace HighLoad.HomeWork.SocialNetwork.PostService.Middlewares;

public class RequestIdMiddleware(RequestDelegate next, ILogger<RequestIdMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        // Если заголовок x-request-id отсутствует – генерируем новый
        if (!context.Request.Headers.TryGetValue("x-request-id", out var requestId))
        {
            requestId = Guid.NewGuid().ToString();
            context.Request.Headers.Append("x-request-id", requestId);
        }

        logger.LogInformation("Запрос {RequestId} START. Путь: {Path}", requestId, context.Request.Path);
        
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["x-request-id"] = requestId;
            return Task.CompletedTask;
        });

        await next(context);

        logger.LogInformation("Запрос {RequestId} END.", requestId!);
    }
}