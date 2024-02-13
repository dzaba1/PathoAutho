namespace Dzaba.PathoAutho.Lib;

[Serializable]
public class ModelStateException : Exception
{
    public KeyValuePair<string, string>[] Errors { get; }

    public ModelStateException(IEnumerable<KeyValuePair<string, string>> errors)
    {
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));

        Errors = errors.ToArray();
    }

    public ModelStateException(string message, IEnumerable<KeyValuePair<string, string>> errors)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));

        Errors = errors.ToArray();
    }

    public ModelStateException(string message, IEnumerable<KeyValuePair<string, string>> errors, Exception inner)
        : base(message, inner)
    {
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));

        Errors = errors.ToArray();
    }
}
