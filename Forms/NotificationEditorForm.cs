using System;
using System.Windows.Forms;
using CompanyNotificationApp.Models;
using CompanyNotificationApp.Services;

namespace CompanyNotificationApp
{
    public partial class NotificationEditorForm : Form
    {
        private NotificationService _notificationService;
        private Company _company;
        private EventLogger _logger;

        public NotificationEditorForm(Company company, NotificationService notificationService)
        {
            InitializeComponent();
            _company = company;
            _notificationService = notificationService;
            _logger = EventLogger.Instance;
            this.Text = string.Format("Notifikácie pre {0}", company.Name);
            this.Size = new System.Drawing.Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void NotificationEditorForm_Load(object sender, EventArgs e)
        {
            InitializeUI();
            LoadNotificationsForCompany();
        }

        private void InitializeUI()
        {
            // Panel pre formulár na pridávanie
            Panel panelForm = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = System.Drawing.Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Popis
            Label lblDescription = new Label { Text = "Popis:", Location = new System.Drawing.Point(10, 10), Width = 100 };
            TextBox txtDescription = new TextBox { Location = new System.Drawing.Point(120, 10), Width = 200, Name = "txtDescription" };

            // Termín
            Label lblDueDate = new Label { Text = "Termín:", Location = new System.Drawing.Point(10, 40), Width = 100 };
            DateTimePicker dtpDueDate = new DateTimePicker 
            { 
                Location = new System.Drawing.Point(120, 40), 
                Width = 200,
                Name = "dtpDueDate",
                Value = DateTime.Now
            };

            // Typ notifikácie
            Label lblType = new Label { Text = "Typ:", Location = new System.Drawing.Point(10, 70), Width = 100 };
            ComboBox cbxType = new ComboBox
            {
                Location = new System.Drawing.Point(120, 70),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "cbxType"
            };
            cbxType.Items.AddRange(new object[] { "Raz ročne", "Mesačne", "Štvrťročne", "Polročne", "Jednorázovo" });
            cbxType.SelectedIndex = 0;

            // Tlačidlo Pridať
            Button btnAdd = new Button
            {
                Text = "Pridať notifikáciu",
                Location = new System.Drawing.Point(10, 90),
                Width = 150,
                Height = 25,
                BackColor = System.Drawing.Color.Green,
                ForeColor = System.Drawing.Color.White
            };
            btnAdd.Click += (s, e) => AddNotification(txtDescription, dtpDueDate, cbxType);

            panelForm.Controls.AddRange(new Control[] { lblDescription, txtDescription, lblDueDate, dtpDueDate, lblType, cbxType, btnAdd });

            // DataGridView
            DataGridView dgvNotifications = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                Name = "dgvNotifications"
            };

            dgvNotifications.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
            dgvNotifications.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Popis", Width = 200 });
            dgvNotifications.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DueDate", HeaderText = "Termín", Width = 100 });
            dgvNotifications.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NotificationType", HeaderText = "Typ", Width = 120 });
            dgvNotifications.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "IsCompleted", HeaderText = "Hotovo", Width = 80 });

            // Tlačidlo Zmazať
            Button btnDelete = new Button
            {
                Text = "Zmazať vybranú",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = System.Drawing.Color.Red,
                ForeColor = System.Drawing.Color.White
            };
            btnDelete.Click += (s, e) => DeleteNotification(dgvNotifications);

            this.Controls.Add(btnDelete);
            this.Controls.Add(dgvNotifications);
            this.Controls.Add(panelForm);
        }

        private void LoadNotificationsForCompany()
        {
            try
            {
                var dgv = this.Controls["dgvNotifications"] as DataGridView;
                if (dgv != null)
                {
                    var notifications = _notificationService.GetNotificationsByCompany(_company.Id);
                    dgv.DataSource = notifications;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Chyba pri načítaní notifikácií: {0}", ex.Message), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.LogError("NotificationEditor", string.Format("Chyba pri načítaní: {0}", ex.Message));
            }
        }

        private void AddNotification(TextBox txtDescription, DateTimePicker dtpDueDate, ComboBox cbxType)
        {
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Prosím vyplň popis notifikácie.", "Validácia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var notification = new Models.CompanyNotification
                {
                    CompanyId = _company.Id,
                    Description = txtDescription.Text,
                    DueDate = dtpDueDate.Value,
                    NotificationType = cbxType.SelectedItem.ToString(),
                    RelatedOption = "Custom",
                    IsCompleted = false,
                    CreatedDate = DateTime.Now
                };

                _notificationService.AddNotification(notification);
                _logger.LogSuccess("NotificationEditor", string.Format("Notifikácia pridaná: {0}", txtDescription.Text));
                MessageBox.Show("Notifikácia bola pridaná!", "Úspech", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                txtDescription.Clear();
                dtpDueDate.Value = DateTime.Now;
                cbxType.SelectedIndex = 0;
                
                LoadNotificationsForCompany();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Chyba pri pridaní notifikácie: {0}", ex.Message), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.LogError("NotificationEditor", string.Format("Chyba pri pridaní: {0}", ex.Message));
            }
        }

        private void DeleteNotification(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vyber notifikáciu na zmazanie.", "Upozornenie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var row = dgv.SelectedRows[0];
                if (int.TryParse(row.Cells[0].Value.ToString(), out int notificationId))
                {
                    if (MessageBox.Show("Naozaj chceš zmazať túto notifikáciu?", "Potvrdenie", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _notificationService.DeleteNotification(notificationId);
                        _logger.LogSuccess("NotificationEditor", string.Format("Notifikácia zmazaná: ID {0}", notificationId));
                        MessageBox.Show("Notifikácia bola zmazaná!", "Úspech", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadNotificationsForCompany();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Chyba pri mazaní notifikácie: {0}", ex.Message), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.LogError("NotificationEditor", string.Format("Chyba pri mazaní: {0}", ex.Message));
            }
        }
    }
}
