using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dzaba.PathoAutho.Lib.Model;

[Table("PathoUserRoles")]
public class PathoUserRole
{
    public int RoleId { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string UserId { get; set; } 

    public virtual PathoRole Role { get; set; }

    public virtual PathoIdentityUser User { get; set; }

    public static void Configure(EntityTypeBuilder<PathoUserRole> builder)
    {
        builder.HasKey(p => new {p.RoleId, p.UserId});

        builder.HasOne(p => p.Role)
            .WithMany(p => p.Users)
            .HasForeignKey(p => p.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.User)
            .WithMany(p => p.PathoRoles)
            .HasForeignKey(p => p.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
