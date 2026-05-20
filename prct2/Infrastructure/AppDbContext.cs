using Microsoft.EntityFrameworkCore;
using prct2.Models;

namespace prct2.Infrastructure
{
    /// <summary>
    /// Контекст базы данных для сервиса аренды электросамокатов.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public DbSet<Users> Users { get; set; } = null!;

        public DbSet<Scooters> Scooters { get; set; } = null!;

        public DbSet<Tariff> Tariffs { get; set; } = null!;

        public DbSet<Trips> Trips { get; set; } = null!;

        public DbSet<Payments> Payments { get; set; } = null!;

        public DbSet<ParkingZone> ParkingZones { get; set; } = null!;

    
        public AppDbContext() { }

       
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /// <summary>
        /// Настраивает подключение к базе данных.
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer(
                    "Server=localhost\\SQLEXPRESS;Database=prct2;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        /// <summary>
        /// Настраивает модель данных: имена таблиц, ограничения, связи.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Scooters>(entity =>
            {
                entity.ToTable("scooters");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Charge).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(255);
            });

            modelBuilder.Entity<Tariff>(entity =>
            {
                entity.ToTable("tariffs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PricePerMinute).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<Trips>(entity =>
            {
                entity.ToTable("trips");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StartTime).IsRequired();

              
                entity.Property(e => e.EndTime).IsRequired(false);  
                entity.Property(e => e.TotalCost).HasColumnType("decimal(10,2)").IsRequired(false); 

                entity.Property(e => e.EndLocation).IsRequired(false);
                entity.Property(e => e.StartLocation).IsRequired(false);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Trips)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Scooter)
                      .WithMany(s => s.Trips)
                      .HasForeignKey(e => e.ScooterId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Tariff)
                      .WithMany(t => t.Trips)
                      .HasForeignKey(e => e.TariffId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Payments>(entity =>
            {
                entity.ToTable("payments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(20).HasDefaultValue("pending");
                entity.Property(e => e.PaymentDate).HasDefaultValueSql("GETDATE()");

                entity.HasOne(p => p.Trip)
                      .WithOne(t => t.Payment)
                      .HasForeignKey<Payments>(p => p.TripId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ParkingZone>(entity =>
            {
                entity.ToTable("parking_zones");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });
        }
    }
}