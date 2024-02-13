using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dzaba.PathoAutho.Lib.Model;

[Table("Applications")]
public class Application
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [MaxLength(256)]
    public string Name { get; set; }

    public virtual ICollection<Permission> Permissions { get; set; }
}
