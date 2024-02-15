using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dzaba.PathoAutho.Lib.Model;

[Table("UserPermissions")]
public class UserPermission
{
    public int PermissionId { get; set; }
    public string UserId { get; set; } 

    public virtual Permission Permission { get; set; }

    public virtual PathoIdentityUser User { get; set; }

    public static void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.HasKey(p => new {p.PermissionId, p.UserId});

        builder.HasOne(p => p.Permission)
            .WithMany(p => p.Users)
            .HasForeignKey(p => p.PermissionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.User)
            .WithMany(p => p.Permissions)
            .HasForeignKey(p => p.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
