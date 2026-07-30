using System;
using System.Timers;
using System.Threading.Tasks;
using CompanyNotificationApp.Data;

namespace CompanyNotificationApp.Services
{
    public class TaskSchedulerService
    {
        private Timer _timer;
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly EmailService _emailService;
        private bool _isRunning = false;

        public TaskSchedulerService(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = new NotificationService(context, emailService);
        }

        public void StartScheduler(int intervalMinutes = 60)
        {
            if (_isRunning)
                return;

            _timer = new Timer(intervalMinutes * 60 * 1000); // Konverzia na milisekundy
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();
            _isRunning = true;

            System.Diagnostics.Debug.WriteLine($"Scheduler spustený. Kontrola každých {intervalMinutes} minút.");
        }

        public void StopScheduler()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _isRunning = false;
                System.Diagnostics.Debug.WriteLine("Scheduler zastavený.");
            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Scheduler spustený o {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                _notificationService.CheckAndNotifyAsync().Wait();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri spustení schedulera: {ex.Message}");
            }
        }
    }
}
