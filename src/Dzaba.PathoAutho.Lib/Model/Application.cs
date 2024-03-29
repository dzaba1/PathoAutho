﻿using Dzaba.PathoAutho.Contracts;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dzaba.PathoAutho.Lib.Model;

[Table("Applications")]
public class Application
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [MaxLength(256)]
    public string Name { get; set; }

    public virtual ICollection<PathoRole> Roles { get; set; }

    public virtual ICollection<ApplicationAdmin> Admins { get; set; }

    public virtual ICollection<PathoClaim> Claims { get; set; }

    public static void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.HasIndex(p => p.Name)
            .IsUnique();
    }

    public NamedEntity<Guid> ToModel()
    {
        return new NamedEntity<Guid>
        {
            Id = Id,
            Name = Name
        };
    }
}
