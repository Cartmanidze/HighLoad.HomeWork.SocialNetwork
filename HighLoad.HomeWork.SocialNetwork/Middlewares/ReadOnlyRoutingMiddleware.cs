namespace HighLoad.HomeWork.SocialNetwork.Middlewares
{
    public class ReadOnlyRoutingMiddleware
    {
        private readonly RequestDelegate _next;

        public ReadOnlyRoutingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var isReadOnly = string.Equals(context.Request.Method, "GET", System.StringComparison.OrdinalIgnoreCase);

            context.Items["IsReadOnly"] = isReadOnly;

            await _next(context);
        }
    }
}