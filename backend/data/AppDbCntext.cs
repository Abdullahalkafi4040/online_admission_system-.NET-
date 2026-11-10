using Microsoft.EntityFrameworkCore;
using SchoolBackend.Models;

namespace SchoolBackend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    
    public DbSet<SchoolClass> SchoolClasses { get; set; }
    public DbSet<SeatHold> SeatHolds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        
        modelBuilder.Entity<SeatHold>()
            .HasIndex(h => new { h.SchoolClassId, h.StudentNid });
    }
}