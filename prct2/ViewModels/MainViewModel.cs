using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using prct2.Commands;
using prct2.Infrastructure;
using prct2.Models;
using prct2.Services;

namespace prct2.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IScooterService _scooterService;
        private readonly AppDbContext _context;
        private Scooters? _selectedScooter;
        private Trips? _activeTrip;
        private string _statusMessage = string.Empty;
        private int _currentUserId = 1; 

        public MainViewModel()
        {
            _context = new AppDbContext();
            _scooterService = new ScooterService(_context);

            AvailableScooters = new ObservableCollection<Scooters>();
            TripHistory = new ObservableCollection<Trips>();
            Tariffs = new ObservableCollection<Tariff>();

            RefreshCommand = new RelayCommand(RefreshData);
            StartTripCommand = new RelayCommand(StartTrip, CanStartTrip);
            EndTripCommand = new RelayCommand(EndTrip, CanEndTrip);

            // Инициализация данных
            InitializeDatabase();
            RefreshData();
        }

        public ObservableCollection<Scooters> AvailableScooters { get; }
        public ObservableCollection<Trips> TripHistory { get; }
        public ObservableCollection<Tariff> Tariffs { get; }

        public Scooters? SelectedScooter
        {
            get => _selectedScooter;
            set { _selectedScooter = value; OnPropertyChanged(); }
        }

        public Trips? ActiveTrip
        {
            get => _activeTrip;
            set { _activeTrip = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand StartTripCommand { get; }
        public ICommand EndTripCommand { get; }

        /// <summary>
        /// Инициализация базы данных тестовыми данными
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                if (!_context.Users.Any())
                {
                    _context.Users.Add(new Users
                    {
                        Email = "user@example.com",
                        Name = "Test User",
                        Phone = "+1234567890",
                        CreatedAt = DateTime.UtcNow
                    });
                    _context.SaveChanges();
                }

                if (!_context.Tariffs.Any())
                {
                    _context.Tariffs.AddRange(
                        new Tariff
                        {
                            Name = "Стандарт",
                            PricePerMinute = 3.5m,
                            Description = "Обычный тариф"
                        },
                        new Tariff
                        {
                            Name = "Премиум",
                            PricePerMinute = 6.0m,
                            Description = "Премиум тариф"
                        }
                    );
                    _context.SaveChanges();
                }

                if (!_context.Scooters.Any())
                {
                    _context.Scooters.AddRange(
                        new Scooters
                        {
                            Name = "Xiaomi Mi Pro 2",
                            Status = "available",
                            Charge = 85,
                            Location = "Zone_A"
                        },
                        new Scooters
                        {
                            Name = "Ninebot Max G30",
                            Status = "available",
                            Charge = 92,
                            Location = "Zone_B"
                        },
                        new Scooters
                        {
                            Name = "Xiaomi Essential",
                            Status = "available",
                            Charge = 45,
                            Location = "Zone_A"
                        },
                        new Scooters
                        {
                            Name = "Ninebot F2 Pro",
                            Status = "in_use",
                            Charge = 78,
                            Location = "Zone_C"
                        }
                    );
                    _context.SaveChanges();
                }

                if (!_context.ParkingZones.Any())
                {
                    _context.ParkingZones.AddRange(
                        new ParkingZone
                        {
                            Name = "Zone_A",
                            IsActive = true
                        },
                        new ParkingZone
                        {
                            Name = "Zone_B",
                            IsActive = true
                        },
                        new ParkingZone
                        {
                            Name = "Zone_C",
                            IsActive = true
                        }
                    );
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка инициализации: {ex.Message}";
            }
        }

        private void RefreshData()
        {
            try
            {
                AvailableScooters.Clear();
                var scooters = _scooterService.GetAvailableScooters();
                foreach (var s in scooters)
                    AvailableScooters.Add(s);

                Tariffs.Clear();
                var tariffs = _context.Tariffs.ToList();
                foreach (var t in tariffs)
                    Tariffs.Add(t);

                TripHistory.Clear();
                var trips = _scooterService.GetUserTrips(_currentUserId);
                foreach (var t in trips)
                    TripHistory.Add(t);

                StatusMessage = $"Данные обновлены. Доступно самокатов: {AvailableScooters.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка обновления: {ex.Message}";
            }
        }

        private bool CanStartTrip() => SelectedScooter != null && ActiveTrip == null && Tariffs.Count > 0;

        private async void StartTrip()
        {
            if (SelectedScooter == null)
            {
                StatusMessage = "❌ Выберите самокат";
                return;
            }

            if (Tariffs.Count == 0)
            {
                StatusMessage = "❌ Нет доступных тарифов";
                return;
            }

            try
            {
                StatusMessage = "⏳ Начало поездки...";

                int tariffId = Tariffs.FirstOrDefault()?.Id ?? 1;

                var user = await _context.Users.FindAsync(_currentUserId);
                if (user == null)
                {
                    StatusMessage = "❌ Пользователь не найден";
                    return;
                }

                var tariff = await _context.Tariffs.FindAsync(tariffId);
                if (tariff == null)
                {
                    StatusMessage = "❌ Тариф не найден";
                    return;
                }

                var trip = await _scooterService.StartTripAsync(
                    _currentUserId,
                    SelectedScooter.Id,
                    tariffId,
                    SelectedScooter.Id 
                );

                ActiveTrip = trip;
                StatusMessage = $"✅ Поездка начата! Самокат: {SelectedScooter.Name}";

                RefreshData();
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Ошибка: {ex.Message}";
                if (ex.InnerException != null)
                {
                    StatusMessage += $"\nДетали: {ex.InnerException.Message}";
                }
            }
        }

        private bool CanEndTrip() => ActiveTrip != null;

        private async void EndTrip()
        {
            if (ActiveTrip == null)
            {
                StatusMessage = "❌ Нет активной поездки";
                return;
            }

            try
            {
                StatusMessage = "⏳ Завершение поездки...";

                var (trip, payment) = await _scooterService.EndTripAsync(
                    ActiveTrip.Id,
                    ActiveTrip.StartLocation ?? 1
                );

                StatusMessage = $"✅ Поездка завершена! Сумма: {payment.Amount:C}";
                ActiveTrip = null;

                RefreshData();
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Ошибка: {ex.Message}";
                if (ex.InnerException != null)
                {
                    StatusMessage += $"\nДетали: {ex.InnerException.Message}";
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}