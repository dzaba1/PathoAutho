namespace Dzaba.BasicAuthentication;

/// <summary>
/// Data returned while checking basic authenitcaion credentials.
/// </summary>
public sealed class CheckPasswordResult
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="failReason">Reason of the check failure. Pass null for success.</param>
    public CheckPasswordResult(string failReason)
        : this(failReason, null)
    {
        
    }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="failReason">Reason of the check failure. Pass null for success.</param>
    /// <param name="context">Anything what you want. Later it can be used for adding claims. For example it can be some User record from your data base.</param>
    public CheckPasswordResult(string failReason, object context)
    {
        FailReason = failReason;
        Context = context;
    }

    /// <summary>
    /// Reason of the check failure.
    /// </summary>
    public string FailReason { get; }

    /// <summary>
    /// Anything what you want. Later it can be used for adding claims. For example it can be some User record from your data base.
    /// </summary>
    public object Context { get; }

    /// <summary>
    /// True if everything is OK.
    /// </summary>
    public bool IsSuccess => string.IsNullOrWhiteSpace(FailReason);

    /// <summary>
    /// Creates result for success.
    /// </summary>
    /// <returns><see cref="CheckPasswordResult"/></returns>
    public static CheckPasswordResult Success()
    {
        return Success(null);
    }

    /// <summary>
    /// Creates result for success.
    /// </summary>
    /// <param name="context">Anything what you want. Later it can be used for adding claims. For example it can be some User record from your data base.</param>
    /// <returns><see cref="CheckPasswordResult"/></returns>
    public static CheckPasswordResult Success(object context)
    {
        return new CheckPasswordResult(null, context);
    }
}
