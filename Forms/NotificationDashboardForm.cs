using System;
using System.Windows.Forms;
using CompanyNotificationApp.Services;

namespace CompanyNotificationApp
{
    public partial class NotificationDashboardForm : Form
    {
        private NotificationService _notificationService;
        private CompanyService _companyService;
        private DataGridView dgvNotifications;
        private Button btnMarkComplete;
        private Button btnRefresh;

        public NotificationDashboardForm(NotificationService notificationService, CompanyService companyService)
        {
            InitializeComponent();
            _notificationService = notificationService;
            _companyService = companyService;
            this.Text = "Notifikácie";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void NotificationDashboardForm_Load(object sender, EventArgs e)
        {
            InitializeUI();
            LoadNotifications();
        }

        private void InitializeUI()
        {
            // Panel pre tlačidlá
            Panel panelButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = System.Drawing.Color.LightGray
            };

            btnRefresh = new Button
            {
                Text = "Obnoviť",
                Location = new System.Drawing.Point(10, 10),
                Width = 100,
                BackColor = System.Drawing.Color.SteelBlue,
                ForeColor = System.Drawing.Color.White
            };
            btnRefresh.Click += (s, e) => LoadNotifications();

            btnMarkComplete = new Button
            {
                Text = "Označiť ako hotové",
                Location = new System.Drawing.Point(120, 10),
                Width = 150,
                BackColor = System.Drawing.Color.Green,
                ForeColor = System.Drawing.Color.White
            };
            btnMarkComplete.Click += (s, e) => MarkAsComplete();

            panelButtons.Controls.Add(btnRefresh);
            panelButtons.Controls.Add(btnMarkComplete);

            // DataGridView
            dgvNotifications = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            dgvNotifications.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
            dgvNotifications.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Popis", Width = 300 });
            dgvNotifications.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DueDate", HeaderText = "Termín", Width = 150, DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle { Format = "dd.MM.yyyy" } });
            dgvNotifications.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RelatedOption", HeaderText = "Typ", Width = 100 });
            dgvNotifications.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NotificationType", HeaderText = "Typ notifikácie", Width = 150 });
            dgvNotifications.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "IsCompleted", HeaderText = "Hotovo", Width = 80 });

            this.Controls.Add(dgvNotifications);
            this.Controls.Add(panelButtons);
        }

        private void LoadNotifications()
        {
            try
            {
                var allTasks = _notificationService.GetAllPendingTasks();
                dgvNotifications.DataSource = allTasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri načítaní notifikácií: {ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MarkAsComplete()
        {
            if (dgvNotifications.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vyber notifikáciu.", "Upozornenie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var row = dgvNotifications.SelectedRows[0];
                if (int.TryParse(row.Cells[0].Value.ToString(), out int taskId))
                {
                    _notificationService.MarkTaskAsCompleted(taskId);
                    MessageBox.Show("Notifikácia bola označená ako hotová!", "Úspech", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadNotifications();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba: {ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
