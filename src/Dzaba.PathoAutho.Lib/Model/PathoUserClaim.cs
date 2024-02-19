using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dzaba.PathoAutho.Lib.Model;

[Table("PathoUserClaims")]
public class PathoUserClaim
{
    public int ClaimId { get; set; }
    public string UserId { get; set; }

    public virtual PathoClaim Claim { get; set; }

    public virtual PathoIdentityUser User { get; set; }

    public static void Configure(EntityTypeBuilder<PathoUserClaim> builder)
    {
        builder.HasKey(p => new { p.ClaimId, p.UserId });

        builder.HasOne(p => p.Claim)
            .WithMany(p => p.Users)
            .HasForeignKey(p => p.ClaimId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.User)
            .WithMany(p => p.PathoClaims)
            .HasForeignKey(p => p.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
