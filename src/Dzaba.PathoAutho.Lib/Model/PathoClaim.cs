using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Dzaba.PathoAutho.Lib.Model;

[Table("PathoClaims")]
public class PathoClaim
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [MaxLength(256)]
    public string Type { get; set; }

    [Required(AllowEmptyStrings = false)]
    [MaxLength(256)]
    public string Value { get; set; }

    public Guid ApplicationId { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string UserId { get; set; }

    public virtual Application Application { get; set; }

    public virtual PathoIdentityUser User { get; set; }

    public static void Configure(EntityTypeBuilder<PathoClaim> builder)
    {
        builder.HasIndex(p => new { p.Type, p.Value, p.ApplicationId, p.UserId })
            .IsUnique();

        builder.HasOne(p => p.Application)
            .WithMany(p => p.Claims)
            .HasForeignKey(p => p.ApplicationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.User)
            .WithMany(p => p.PathoClaims)
            .HasForeignKey(p => p.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
