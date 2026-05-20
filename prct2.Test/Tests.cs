using Microsoft.EntityFrameworkCore;
using prct2.Infrastructure;
using prct2.Models;
using prct2.Services;
using Xunit;

namespace prct2.Tests
{
    /// <summary>
    /// Набор тестов для проверки бизнес-логики сервиса аренды самокатов.
    /// </summary>
    public class ScooterServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ScooterService _service;
        private readonly string _dbName;

        public ScooterServiceTests()
        {
            
            _dbName = $"TestDb_{Guid.NewGuid()}";

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: _dbName)
                .Options;

            _context = new AppDbContext(options);
            _service = new ScooterService(_context);

            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Tariffs.AddRange(
                new Tariff { Id = 1, Name = "Стандарт", PricePerMinute = 3.5m },
                new Tariff { Id = 2, Name = "Премиум", PricePerMinute = 6.0m }
            );

            _context.Scooters.AddRange(
                new Scooters { Id = 1, Name = "Scooter-1", Status = "available", Charge = 80, Location = "Zone_A" },
                new Scooters { Id = 2, Name = "Scooter-2", Status = "available", Charge = 10, Location = "Zone_B" },
                new Scooters { Id = 3, Name = "Scooter-3", Status = "in_use", Charge = 90, Location = "Zone_C" }
            );

            _context.Users.Add(new Users { Id = 1, Email = "test@example.com", Name = "Test User" });

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void CalculateTripCost_ValidInput_ReturnsExpectedCost()
        {
            var tariffId = 1;
            var start = new DateTime(2024, 1, 1, 10, 0, 0);
            var end = start.AddMinutes(30);

            var cost = _service.CalculateTripCost(start, end, tariffId);

            Assert.Equal(105.00m, cost); 
        }

        [Fact]
        public void IsScooterAvailable_ScooterWithSufficientCharge_ReturnsTrue()
        {
            Assert.True(_service.IsScooterAvailable(1));
        }

        [Fact]
        public void IsScooterAvailable_LowBattery_ReturnsFalse()
        {
            Assert.False(_service.IsScooterAvailable(2)); 
        }

        [Fact]
        public void IsScooterAvailable_InUseStatus_ReturnsFalse()
        {
            Assert.False(_service.IsScooterAvailable(3)); 
        }

      

        [Fact]
        public async Task EndTripAsync_NonExistentTrip_ThrowsException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.EndTripAsync(9999, 1)
            );
        }
    }
}