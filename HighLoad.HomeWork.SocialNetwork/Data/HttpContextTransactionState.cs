namespace HighLoad.HomeWork.SocialNetwork.Data;

public class HttpContextTransactionState : ITransactionState
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTransactionState(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsReadOnly
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return false;

            if (context.Items.TryGetValue("IsReadOnly", out var value) && value is bool b)
            {
                return b;
            }
            return false;
        }
    }
}