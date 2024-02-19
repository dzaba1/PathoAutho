using System;
using System.ComponentModel.DataAnnotations;

namespace Dzaba.PathoAutho.Contracts;

public sealed class SetClaim
{
    public Guid ApplicationId { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string UserName { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Type { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Value { get; set; }
}
