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

        Application.Configure(modelBuilder.Entity<Application>());
        Permission.Configure(modelBuilder.Entity<Permission>());
        UserPermission.Configure(modelBuilder.Entity<UserPermission>());
        PathoRole.Configure(modelBuilder.Entity<PathoRole>());
        PathoUserRole.Configure(modelBuilder.Entity<PathoUserRole>());
    }

    public DbSet<Application> Applications { get; set; }

    public DbSet<Permission> Permissions { get; set; }

    public DbSet<UserPermission> UserPermissions { get; set; }

    public DbSet<PathoRole> PathoRoles { get; set; }

    public DbSet<PathoUserRole> PathoUserRoles { get; set; }
}
