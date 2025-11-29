using Microsoft.EntityFrameworkCore;
using TicketService.Models;

namespace TicketService.Data;

public class TicketDbContext : DbContext
{
    public TicketDbContext(DbContextOptions<TicketDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<SeatLock> SeatLocks { get; set; } = null!;
    public DbSet<SeatingPlan> SeatingPlans { get; set; } = null!;
    public DbSet<TicketCancellationRequest> CancellationRequests { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.TicketCode).IsRequired().HasMaxLength(50);
            entity.Property(t => t.EventId).IsRequired().HasMaxLength(100);
            entity.Property(t => t.UserId).IsRequired();
            entity.Property(t => t.Price).HasColumnType("decimal(18,2)");
            entity.HasIndex(t => t.TicketCode).IsUnique();
        });

        modelBuilder.Entity<SeatLock>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.EventId).IsRequired().HasMaxLength(100);
            entity.Property(s => s.SeatCode).IsRequired().HasMaxLength(50);
            entity.Property(s => s.LockToken).IsRequired().HasMaxLength(64);
            entity.HasIndex(s => s.LockToken).IsUnique();
            entity.HasIndex(s => new { s.EventId, s.SeatCode, s.Status });
        });

        modelBuilder.Entity<SeatingPlan>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.EventId).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Version).IsRequired().HasMaxLength(20);
            entity.Property(p => p.LayoutJson).IsRequired();
            entity.HasIndex(p => p.EventId).IsUnique();
        });

        modelBuilder.Entity<TicketCancellationRequest>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Reason).HasMaxLength(1000);
            entity.Property(c => c.UserId).IsRequired().HasMaxLength(100);
            entity.Property(c => c.RefundCurrency).HasMaxLength(10);
            entity.HasOne(c => c.Ticket)
                  .WithMany()
                  .HasForeignKey(c => c.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

