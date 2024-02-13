using System.ComponentModel.DataAnnotations;

namespace Dzaba.PathoAutho.Contracts;

public sealed class RegisterUser
{
    [Required(AllowEmptyStrings = false)]
    public string Email { get; set; }

    [Required(AllowEmptyStrings = false)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(AllowEmptyStrings = false)]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
}
