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
    public virtual Application Application { get; set; }

    public virtual ICollection<PathoUserClaim> Users { get; set; }

    public static void Configure(EntityTypeBuilder<PathoClaim> builder)
    {
        builder.HasOne(p => p.Application)
            .WithMany(p => p.Claims)
            .HasForeignKey(p => p.ApplicationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
