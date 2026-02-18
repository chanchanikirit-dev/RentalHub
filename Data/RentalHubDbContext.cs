using Microsoft.EntityFrameworkCore;
using RentalHub.Model;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RentalHub.Data
{
    public class RentalHubDbContext : DbContext
    {
        public RentalHubDbContext(DbContextOptions<RentalHubDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Order> Orders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.Username)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(e => e.Username)
                      .IsUnique();
            });

            // ITEM
            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("items");
                entity.HasKey(e => e.ItemId);

                entity.Property(e => e.ItemCode)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasIndex(e => e.ItemCode)
                      .IsUnique();
            });

            // ORDER
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.OrderId);

                entity.Property(e => e.ClientName)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Rent)
                      .HasColumnType("numeric(10,2)");

                entity.Property(e => e.Advance)
                      .HasColumnType("numeric(10,2)");

                entity.Property(e => e.Remaining)
                      .HasColumnType("numeric(10,2)");

                entity.HasIndex(e => new { e.ItemId, e.FromDate, e.ToDate });

                entity.HasOne(e => e.Item)
                      .WithMany()
                      .HasForeignKey(e => e.ItemId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
