namespace HighLoad.HomeWork.SocialNetwork.Data;

public static class TransactionContext
{
    private static readonly AsyncLocal<bool> _isReadOnly = new();

    public static bool IsReadOnly
    {
        get => _isReadOnly.Value;
        set => _isReadOnly.Value = value;
    }
}