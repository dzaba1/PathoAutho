namespace Dzaba.BasicAuthentication;

/// <summary>
/// Data returned while checking basic authenitcaion credentials.
/// </summary>
public sealed class CheckPasswordResult
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="success">If password check is correct.</param>
    public CheckPasswordResult(bool success)
        : this(success, null)
    {
        
    }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="success">If password check is correct.</param>
    /// <param name="context">Anything what you want. Later it can be used for adding claims. For example it can be some User record from your data base.</param>
    public CheckPasswordResult(bool success, object context)
    {
        Success = success;
        Context = context;
    }

    /// <summary>
    /// If password check is correct.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Anything what you want. Later it can be used for adding claims. For example it can be some User record from your data base.
    /// </summary>
    public object Context { get; }
}
