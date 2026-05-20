using prct2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace prct2.Services
{
    /// <summary>
    /// Интерфейс сервиса для управления арендой электросамокатов.
    /// </summary>
    public interface IScooterService
    {
       
        bool IsScooterAvailable(int scooterId);

        /// <summary>
        /// Проверяет, достаточен ли заряд самоката для поездки.
        /// </summary>
        bool HasSufficientCharge(int scooterId, int requiredPercent = 15);

        /// <summary>
        /// Рассчитывает стоимость поездки по времени и тарифу.
        /// </summary>
        decimal CalculateTripCost(DateTime startTime, DateTime endTime, int tariffId);

        /// <summary>
        /// Начинает новую поездку.
        /// </summary>
        Task<Trips> StartTripAsync(int userId, int scooterId, int tariffId, int? startLocation = null);

        /// <summary>
        /// Завершает активную поездку и создаёт платёж.
        /// </summary>
        Task<(Trips trip, Payments payment)> EndTripAsync(int tripId, int endLocation);

        /// <summary>
        /// Возвращает историю поездок пользователя.
        /// </summary>
        IEnumerable<Trips> GetUserTrips(int userId, bool onlyCompleted = true);

        /// <summary>
        /// Возвращает список доступных для аренды самокатов.
        /// </summary>
        IEnumerable<Scooters> GetAvailableScooters();
    }
}