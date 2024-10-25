using AnyoneForTennis.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AnyoneForTennis.Areas.Identity.Data;

namespace AnyoneForTennis.Areas.Identity.Data;

public class AnyoneForTennisContext(DbContextOptions<AnyoneForTennisContext> options) : IdentityDbContext<AnyoneForTennisUser>(options)
{
    public DbSet<Coach> Coaches { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<MemberSchedule> MemberSchedules { get; set; }
    public DbSet<Schedule> Schedules { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure the relationships
        builder.Entity<Member>()
            .HasOne(static m => m.User)
            .WithOne(static u => u.Member)
            .HasForeignKey<Member>(static m => m.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Coach>()
            .HasOne(static c => c.User)
            .WithOne(static u => u.Coach)
            .HasForeignKey<Coach>(static c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
