using System;
using System.Windows.Forms;
using CompanyNotificationApp.Services;

namespace CompanyNotificationApp
{
    public partial class LogViewerForm : Form
    {
        private EventLogger _logger;
        private ListBox lbLogs;
        private Button btnClear;
        private Button btnExport;
        private Label lblStatus;

        public LogViewerForm()
        {
            InitializeComponent();
            _logger = EventLogger.Instance;
            this.Text = "Viewer udalostí (Log Viewer)";
            this.Size = new System.Drawing.Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void LogViewerForm_Load(object sender, EventArgs e)
        {
            InitializeUI();
            LoadExistingLogs();
            
            // Subscribe na nové logy
            _logger.OnLogAdded += LogAdded;
        }

        private void InitializeUI()
        {
            // Panel pre tlačidlá
            Panel panelButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = System.Drawing.Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnClear = new Button
            {
                Text = "Vymazať logy",
                Location = new System.Drawing.Point(10, 10),
                Width = 100,
                Height = 30,
                BackColor = System.Drawing.Color.Red,
                ForeColor = System.Drawing.Color.White
            };
            btnClear.Click += (s, e) => ClearLogs();

            btnExport = new Button
            {
                Text = "Exportovať",
                Location = new System.Drawing.Point(120, 10),
                Width = 100,
                Height = 30,
                BackColor = System.Drawing.Color.Green,
                ForeColor = System.Drawing.Color.White
            };
            btnExport.Click += (s, e) => ExportLogs();

            lblStatus = new Label
            {
                Text = "Počet logov: 0",
                Location = new System.Drawing.Point(230, 15),
                Width = 200,
                AutoSize = false,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
            };

            panelButtons.Controls.Add(btnClear);
            panelButtons.Controls.Add(btnExport);
            panelButtons.Controls.Add(lblStatus);

            // ListBox pre logy
            lbLogs = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Courier New", 9),
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.Black
            };

            this.Controls.Add(lbLogs);
            this.Controls.Add(panelButtons);
        }

        private void LoadExistingLogs()
        {
            var logs = _logger.GetAllLogs();
            System.Diagnostics.Debug.WriteLine($"📋 Načítavám {logs.Count} logov");

            foreach (var log in logs)
            {
                lbLogs.Items.Add(log.ToString());
            }
            UpdateStatus();
        }

        private void LogAdded(LogEntry logEntry)
        {
            // Thread-safe operácia
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    lbLogs.Items.Add(logEntry.ToString());
                    lbLogs.TopIndex = lbLogs.Items.Count - 1; // Scroll na koniec
                    UpdateStatus();
                }));
            }
            else
            {
                lbLogs.Items.Add(logEntry.ToString());
                lbLogs.TopIndex = lbLogs.Items.Count - 1;
                UpdateStatus();
            }
        }

        private void UpdateStatus()
        {
            lblStatus.Text = string.Format("Počet logov: {0}", lbLogs.Items.Count);
        }

        private void ClearLogs()
        {
            if (MessageBox.Show("Naozaj chceš vymazať všetky logy?", "Potvrdenie", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                lbLogs.Items.Clear();
                _logger.ClearLogs();
                UpdateStatus();
                MessageBox.Show("Logy boli vymazané!", "Úspech", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ExportLogs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = string.Format("logs_{0:yyyy-MM-dd_HH-mm-ss}.txt", DateTime.Now)
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var logs = _logger.GetAllLogs();
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(saveFileDialog.FileName))
                    {
                        writer.WriteLine("=== LOG EXPORT ===");
                        writer.WriteLine(string.Format("Dátum exportu: {0:dd.MM.yyyy HH:mm:ss}", DateTime.Now));
                        writer.WriteLine(string.Format("Počet logov: {0}", logs.Count));
                        writer.WriteLine(new string('=', 50));
                        writer.WriteLine();

                        foreach (var log in logs)
                        {
                            writer.WriteLine(log.ToString());
                        }
                    }

                    MessageBox.Show(string.Format("Logy exportované do:\n{0}", saveFileDialog.FileName), "Úspech", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Chyba pri exporte: {0}", ex.Message), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Unsubscribe
            _logger.OnLogAdded -= LogAdded;
            base.OnFormClosing(e);
        }

       
    }
}
