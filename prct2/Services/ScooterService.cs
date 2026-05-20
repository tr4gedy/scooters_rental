using Microsoft.EntityFrameworkCore;
using prct2.Infrastructure;
using prct2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace prct2.Services
{
    /// <summary>
    /// Реализация сервиса управления арендой электросамокатов.
    /// Содержит бизнес-логику: проверки, расчёты, управление поездками.
    /// </summary>
    public class ScooterService : IScooterService
    {
        private readonly AppDbContext _context;
        private const int MinimumChargePercent = 15;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ScooterService"/>.
        /// </summary>
        public ScooterService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

 
        public bool IsScooterAvailable(int scooterId)
        {
            var scooter = _context.Scooters.Find(scooterId);
            return scooter != null &&
                   string.Equals(scooter.Status, "available", StringComparison.OrdinalIgnoreCase) &&
                   scooter.Charge >= MinimumChargePercent;
        }

    
        public bool HasSufficientCharge(int scooterId, int requiredPercent = MinimumChargePercent)
        {
            var scooter = _context.Scooters.Find(scooterId);
            return scooter?.Charge >= requiredPercent;
        }

        public decimal CalculateTripCost(DateTime startTime, DateTime endTime, int tariffId)
        {
            var tariff = _context.Tariffs.Find(tariffId);
            if (tariff == null)
                throw new ArgumentException("Тариф не найден", nameof(tariffId));

            var minutes = (endTime - startTime).TotalMinutes;
            if (minutes < 0)
                throw new ArgumentException("Время окончания не может быть раньше времени начала", nameof(endTime));

            return Math.Round((decimal)minutes * tariff.PricePerMinute, 2);
        }

        public async Task<Trips> StartTripAsync(int userId, int scooterId, int tariffId, int? startLocation = null)
        {
            if (!IsScooterAvailable(scooterId))
                throw new InvalidOperationException("Самокат недоступен или заряд недостаточен");

            var scooter = await _context.Scooters.FindAsync(scooterId)
                ?? throw new ArgumentException("Самокат не найден", nameof(scooterId));

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                scooter.Status = "in_use";
                scooter.Charge = Math.Max(0, scooter.Charge - 2); 

                var trip = new Trips
                {
                    UserId = userId,
                    ScooterId = scooterId,
                    TariffId = tariffId,
                    StartTime = DateTime.UtcNow,
                    EndTime = null,
                    StartLocation = startLocation,
                    EndLocation = null,
                    TotalCost = null
                };

                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return trip;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(Trips trip, Payments payment)> EndTripAsync(int tripId, int endLocation)
        {
            var trip = await _context.Trips.FindAsync(tripId);
            if (trip == null || trip.EndTime != null)
                throw new InvalidOperationException("Поездка не найдена или уже завершена");

            var endTime = DateTime.UtcNow;
            var cost = CalculateTripCost(trip.StartTime, endTime, trip.TariffId);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                trip.EndTime = endTime;
                trip.EndLocation = endLocation;
                trip.TotalCost = cost;

                var scooter = await _context.Scooters.FindAsync(trip.ScooterId);
                if (scooter != null)
                {
                    scooter.Status = "available";
                    // Расход заряда: ~1% за 10 минут поездки
                    var minutes = (endTime - trip.StartTime).TotalMinutes;
                    scooter.Charge = Math.Max(0, scooter.Charge - (int)(minutes / 10));
                    scooter.Location = $"Zone_{endLocation}";
                }

                var payment = new Payments
                {
                    TripId = tripId,
                    Amount = cost,
                    PaymentStatus = "completed",
                    PaymentDate = endTime
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (trip, payment);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public IEnumerable<Trips> GetUserTrips(int userId, bool onlyCompleted = true)
        {
            var query = _context.Trips
                .Include(t => t.Scooter)
                .Include(t => t.Tariff)
                .Include(t => t.Payment)
                .Where(t => t.UserId == userId);

            return onlyCompleted
                ? query.Where(t => t.EndTime != null).ToList()
                : query.ToList();
        }

        public IEnumerable<Scooters> GetAvailableScooters()
        {
            return _context.Scooters
                .Where(s => s.Status == "available" && s.Charge >= MinimumChargePercent)
                .ToList();
        }
    }
}