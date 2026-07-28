using System;
using System.Windows.Forms;
using CompanyNotificationApp.Services;
using CompanyNotificationApp.Data;

namespace CompanyNotificationApp
{
    public partial class MainForm : Form
    {
        private ApplicationDbContext _context;
        private CompanyService _companyService;
        private NotificationService _notificationService;
        private TaskSchedulerService _schedulerService;
        private EmailService _emailService;
        private EventLogger _eventLogger;

        public MainForm()
        {
            InitializeComponent();
            this.Text = "Company Notification App";
            this.Size = new System.Drawing.Size(700, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitializeDatabase();
            InitializeServices();
            InitializeUI();
            StartScheduler();
        }

        private void InitializeDatabase()
        {
            try
            {
                _context = new ApplicationDbContext();
                _context.Database.CreateIfNotExists();
                _eventLogger = EventLogger.Instance;
                _eventLogger.LogSuccess("Database", "Databáza inicializovaná");
            }
            catch (Exception ex)
            {
                _eventLogger?.LogError("Database", $"Chyba pri inicializácii: {ex.Message}");
                MessageBox.Show($"Chyba pri inicializácii databázy: {ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeServices()
        {
            _emailService = new EmailService(
                smtpServer: "smtp.gmail.com",
                smtpPort: 587,
                senderEmail: "tvoj-email@gmail.com",  // Vlož svoj email
                senderPassword: "tvoje-app-password"   // Vlož app password
            );

            _companyService = new CompanyService(_context);
            _notificationService = new NotificationService(_context, _emailService);
            _schedulerService = new TaskSchedulerService(_context, _emailService);
            
            _eventLogger.LogSuccess("Services", "Všetky služby inicializované");
        }

        private void InitializeUI()
        {
            // Panel pre tlačidlá
            Panel panelButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = System.Drawing.Color.LightGray
            };

            // Tlačidlo - Správa firiem
            Button btnCompanies = new Button
            {
                Text = "Správa firiem",
                Width = 140,
                Height = 40,
                Location = new System.Drawing.Point(10, 10),
                BackColor = System.Drawing.Color.SteelBlue,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
            };
            btnCompanies.Click += (s, e) => OpenCompanyManagement();

            // Tlačidlo - Notifikácie
            Button btnNotifications = new Button
            {
                Text = "Notifikácie",
                Width = 140,
                Height = 40,
                Location = new System.Drawing.Point(160, 10),
                BackColor = System.Drawing.Color.Green,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
            };
            btnNotifications.Click += (s, e) => OpenNotificationDashboard();

            // Tlačidlo - Spustiť kontrolu
            Button btnCheckNow = new Button
            {
                Text = "Kontrola teraz",
                Width = 140,
                Height = 40,
                Location = new System.Drawing.Point(310, 10),
                BackColor = System.Drawing.Color.Orange,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
            };
            btnCheckNow.Click += (s, e) => CheckNotificationsNow();

            // Tlačidlo - Viewer udalostí
            Button btnLogs = new Button
            {
                Text = "📋 Logy",
                Width = 140,
                Height = 40,
                Location = new System.Drawing.Point(460, 10),
                BackColor = System.Drawing.Color.Purple,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
            };
            btnLogs.Click += (s, e) => OpenLogViewer();

            panelButtons.Controls.Add(btnCompanies);
            panelButtons.Controls.Add(btnNotifications);
            panelButtons.Controls.Add(btnCheckNow);
            panelButtons.Controls.Add(btnLogs);

            // Label pre informácie
            Label lblInfo = new Label
            {
                Text = "Vitajte v Company Notification App!\n\nTáto aplikácia spravuje notifikácie na povinnosti firiem.\nKliknutím na 'Logy' si môžeš pozrieť všetky udalosti.",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Arial", 12)
            };

            this.Controls.Add(lblInfo);
            this.Controls.Add(panelButtons);
        }

        private void OpenCompanyManagement()
        {
            _eventLogger.LogInfo("UI", "Otvorená správa firiem");
            CompanyManagementForm form = new CompanyManagementForm(_companyService, _notificationService);
            form.ShowDialog();
        }

        private void OpenNotificationDashboard()
        {
            _eventLogger.LogInfo("UI", "Otvorený dashboard notifikácií");
            NotificationDashboardForm form = new NotificationDashboardForm(_notificationService, _companyService);
            form.ShowDialog();
        }

        private void OpenLogViewer()
        {
            _eventLogger.LogInfo("UI", "Otvorený Log Viewer");
            LogViewerForm form = new LogViewerForm();
            form.ShowDialog();
        }

        private void CheckNotificationsNow()
        {
            try
            {
                _eventLogger.LogInfo("Scheduler", "Spustená manuálna kontrola notifikácií");
                _notificationService.CheckAndNotifyAsync().Wait();
                _eventLogger.LogSuccess("Scheduler", "Kontrola notifikácií dokončená");
                MessageBox.Show("Kontrola notifikácií dokončená!", "Úspech", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _eventLogger.LogError("Scheduler", $"Chyba pri kontrole: {ex.Message}");
                MessageBox.Show($"Chyba pri kontrole: {ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartScheduler()
        {
            try
            {
                _eventLogger.LogInfo("Scheduler", "Spúšťa sa scheduler - kontrola každú hodinu");
                _schedulerService.StartScheduler(intervalMinutes: 60); // Kontrola každú hodinu
                _eventLogger.LogSuccess("Scheduler", "Scheduler spustený úspešne");
            }
            catch (Exception ex)
            {
                _eventLogger.LogError("Scheduler", $"Chyba pri spustení: {ex.Message}");
                MessageBox.Show($"Chyba pri spustení schedulera: {ex.Message}", "Upozornenie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _eventLogger.LogInfo("App", "Aplikácia sa zatvárajúca...");
            _schedulerService?.StopScheduler();
            _context?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
