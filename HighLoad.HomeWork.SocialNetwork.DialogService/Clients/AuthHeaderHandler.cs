using System.Net.Http.Headers;

namespace HighLoad.HomeWork.SocialNetwork.DialogService.Clients;

public class AuthHeaderHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var bearerToken = httpContext.Request.Headers["Authorization"].ToString();
            
            if (!string.IsNullOrEmpty(bearerToken))
            {
                var parts = bearerToken.Split(' ', 2);
                
                if (parts.Length == 2 && parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                {
                    var token = parts[1];
                    
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}