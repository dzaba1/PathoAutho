using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Dzaba.PathoAutho.Lib.Model;

namespace Dzaba.PathoAutho.Lib;

public class AppDbContext : IdentityDbContext<PathoIdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Permission.Configure(modelBuilder.Entity<Permission>());
    }

    public DbSet<Application> Applications { get; set; }

    public DbSet<Permission> Permissions { get; set; }
}
