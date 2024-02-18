using System;
using System.ComponentModel.DataAnnotations;

namespace Dzaba.PathoAutho.Contracts;

public sealed class ChangeApplication
{
    public Guid Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string NewName { get; set; }
}
