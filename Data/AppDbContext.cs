using Microsoft.EntityFrameworkCore;
using CreditCardRegistration.Models;

namespace CreditCardRegistration.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<CreditCard> CreditCards { get; set; } = null!;
        public DbSet<Document> Documents { get; set; } = null!;
        public DbSet<Option> Options { get; set; } = null!;
        public DbSet<FormControl> FormControls { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User-CreditCard relationship
            modelBuilder.Entity<User>()
                .HasMany(u => u.CreditCards)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure User-Document relationship
            modelBuilder.Entity<User>()
                .HasMany(u => u.Documents)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Document>()
                .HasIndex(d => d.UserId);

            // Configure string lengths
            modelBuilder.Entity<Document>()
                .Property(d => d.DocumentName)
                .HasMaxLength(255);

            modelBuilder.Entity<Document>()
                .Property(d => d.DocumentType)
                .HasMaxLength(50);

            // Configure Document entity
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.DocumentPath).IsRequired();
                entity.Property(d => d.FileType).IsRequired();
            });

            // Configure FormControl entity
            modelBuilder.Entity<FormControl>(entity =>
            {
                entity.HasKey(f => f.ControlId);
                entity.Property(f => f.ControlType).HasMaxLength(20).IsRequired();
                entity.Property(f => f.ControlName).HasMaxLength(50).IsRequired();
                entity.Property(f => f.OptionValue).HasMaxLength(100).IsRequired();
                entity.Property(f => f.IsSelected).HasDefaultValue(false);
                entity.Property(f => f.DisplayOrder).HasDefaultValue(0);
            });

            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Add Document seed data if needed
            modelBuilder.Entity<FormControl>().HasData(
                new FormControl { ControlId = 1, ControlType = "select", ControlName = "DocumentTypes", OptionValue = "Passport", DisplayOrder = 1 },
                new FormControl { ControlId = 2, ControlType = "select", ControlName = "DocumentTypes", OptionValue = "Driver's License", DisplayOrder = 2 },
                new FormControl { ControlId = 3, ControlType = "select", ControlName = "DocumentTypes", OptionValue = "National ID", DisplayOrder = 3 }
            );
        }
    }
}