using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzaba.PathoAutho.Lib.Model;

[Table("PathoRoles")]
public class PathoRole
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [MaxLength(256)]
    public string Name { get; set; }

    public Guid ApplicationId { get; set; }
    public virtual Application Application { get; set; }

    public virtual ICollection<UserRole> Users { get; set; }

    public static void Configure(EntityTypeBuilder<PathoRole> builder)
    {
        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.HasOne(p => p.Application)
            .WithMany(p => p.Roles)
            .HasForeignKey(p => p.ApplicationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
