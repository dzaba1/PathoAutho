using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Dzaba.PathoAutho.Lib.Model;

[Table("Permissions")]
public class Permission
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [MaxLength(256)]
    public string Name { get; set; }

    public int ApplicationId { get; set; }
    public virtual Application Application { get; set; }

    public virtual ICollection<UserPermission> Users { get; set; }

    public static void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasOne(p => p.Application)
            .WithMany(p => p.Permissions)
            .HasForeignKey(p => p.ApplicationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
