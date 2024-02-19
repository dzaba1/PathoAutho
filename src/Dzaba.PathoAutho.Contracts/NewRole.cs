using System;
using System.ComponentModel.DataAnnotations;

namespace Dzaba.PathoAutho.Contracts;

public sealed class NewRole
{
    public Guid ApplicationId { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string RoleName { get; set; }
}
