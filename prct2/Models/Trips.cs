using prct2.Models;

public class Trips
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ScooterId { get; set; }
    public int TariffId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; } 
    public decimal? TotalCost { get; set; }
    public int? StartLocation { get; set; }
    public int? EndLocation { get; set; }

    // Навигация
    public virtual Users User { get; set; } = null!;
    public virtual Scooters Scooter { get; set; } = null!;
    public virtual Tariff Tariff { get; set; } = null!;
    public virtual Payments? Payment { get; set; }
}
