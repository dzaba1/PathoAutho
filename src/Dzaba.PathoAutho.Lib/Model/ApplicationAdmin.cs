using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dzaba.PathoAutho.Lib.Model;

[Table("ApplicationAdmins")]
public class ApplicationAdmin
{
    public Guid ApplicationId { get; set; }
    public string UserId { get; set; }

    public virtual Application Application { get; set; }

    public virtual PathoIdentityUser User { get; set; }

    public static void Configure(EntityTypeBuilder<ApplicationAdmin> builder)
    {
        builder.HasKey(p => new { p.ApplicationId, p.UserId });

        builder.HasOne(p => p.Application)
            .WithMany(p => p.Admins)
            .HasForeignKey(p => p.ApplicationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.User)
            .WithMany(p => p.AdminApplications)
            .HasForeignKey(p => p.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
