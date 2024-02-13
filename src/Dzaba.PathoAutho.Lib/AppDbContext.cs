using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Dzaba.PathoAutho.Lib.Model;

namespace Dzaba.PathoAutho.Lib;

public class AppDbContext : IdentityDbContext<PathoIdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { }

    public DbSet<Application> Applications { get; set; }
}
