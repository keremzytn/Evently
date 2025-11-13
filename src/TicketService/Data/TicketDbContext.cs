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
    }
}

