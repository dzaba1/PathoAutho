using Microsoft.AspNetCore.Identity;

namespace Dzaba.PathoAutho.Lib
{
    [Serializable]
    public class IdentityException : Exception
    {
        public IdentityError[] Errors { get; }

        public IdentityException(IEnumerable<IdentityError> errors)
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));

            Errors = errors.ToArray();
        }

        public IdentityException(string message, IEnumerable<IdentityError> errors)
        : base(message)
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));

            Errors = errors.ToArray();
        }

        public IdentityException(string message, IEnumerable<IdentityError> errors, Exception inner)
            : base(message, inner)
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));

            Errors = errors.ToArray();
        }
    }
}
