namespace Dzaba.BasicAuthentication;

public sealed class CheckPasswordResult
{
    public CheckPasswordResult(bool success)
        : this(success, null)
    {
        
    }

    public CheckPasswordResult(bool success, object context)
    {
        Success = success;
        Context = context;
    }

    public bool Success { get; }

    public object Context { get; }
}
