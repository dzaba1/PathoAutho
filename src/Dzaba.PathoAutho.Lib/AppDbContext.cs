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
        ApplicationAdmin.Configure(modelBuilder.Entity<ApplicationAdmin>());
        PathoClaim.Configure(modelBuilder.Entity<PathoClaim>());
        PathoUserClaim.Configure(modelBuilder.Entity<PathoUserClaim>());
        PathoRole.Configure(modelBuilder.Entity<PathoRole>());
        PathoUserRole.Configure(modelBuilder.Entity<PathoUserRole>());
    }

    public DbSet<Application> Applications { get; set; }

    public DbSet<PathoClaim> PathoClaims { get; set; }

    public DbSet<PathoUserClaim> PathoUserClaims { get; set; }

    public DbSet<PathoRole> PathoRoles { get; set; }

    public DbSet<PathoUserRole> PathoUserRoles { get; set; }

    public DbSet<ApplicationAdmin> ApplicationAdmins { get; set; }
}
