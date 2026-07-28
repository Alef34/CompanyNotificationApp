using System;
using System.Windows.Forms;
using CompanyNotificationApp.Models;
using CompanyNotificationApp.Services;

namespace CompanyNotificationApp
{
    public partial class CompanyManagementForm : Form
    {
        private CompanyService _companyService;
        private NotificationService _notificationService;
        private DataGridView dgvCompanies;
        private TextBox txtName;
        private TextBox txtEmail;
        private CheckBox chkEmployees;
        private CheckBox chkVAT;
        private CheckBox chkSlovakia;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Company selectedCompany;

        public CompanyManagementForm(CompanyService companyService, NotificationService notificationService)
        {
            InitializeComponent();
            _companyService = companyService;
            _notificationService = notificationService;
            this.Text = "Správa firiem";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void CompanyManagementForm_Load(object sender, EventArgs e)
        {
            InitializeUI();
            LoadCompanies();
        }

        private void InitializeUI()
        {
            // Panel pre formulár
            Panel panelForm = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = System.Drawing.Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Názov
            Label lblName = new Label { Text = "Názov firmy:", Location = new System.Drawing.Point(10, 10), Width = 100 };
            txtName = new TextBox { Location = new System.Drawing.Point(120, 10), Width = 200 };

            // Email
            Label lblEmail = new Label { Text = "Email:", Location = new System.Drawing.Point(10, 40), Width = 100 };
            txtEmail = new TextBox { Location = new System.Drawing.Point(120, 40), Width = 200 };

            // Checkboxy
            chkEmployees = new CheckBox { Text = "Zamestnanci", Location = new System.Drawing.Point(10, 70), Width = 120 };
            chkVAT = new CheckBox { Text = "DPH", Location = new System.Drawing.Point(140, 70), Width = 100 };
            chkSlovakia = new CheckBox { Text = "Slovensko", Location = new System.Drawing.Point(250, 70), Width = 120 };

            // Tlačidlá
            btnAdd = new Button
            {
                Text = "Pridať",
                Location = new System.Drawing.Point(10, 110),
                Width = 80,
                BackColor = System.Drawing.Color.Green,
                ForeColor = System.Drawing.Color.White
            };
            btnAdd.Click += (s, e) => AddCompany();

            btnUpdate = new Button
            {
                Text = "Upraviť",
                Location = new System.Drawing.Point(100, 110),
                Width = 80,
                BackColor = System.Drawing.Color.Blue,
                ForeColor = System.Drawing.Color.White
            };
            btnUpdate.Click += (s, e) => UpdateCompany();

            btnDelete = new Button
            {
                Text = "Zmazať",
                Location = new System.Drawing.Point(190, 110),
                Width = 80,
                BackColor = System.Drawing.Color.Red,
                ForeColor = System.Drawing.Color.White
            };
            btnDelete.Click += (s, e) => DeleteCompany();

            panelForm.Controls.AddRange(new Control[] { lblName, txtName, lblEmail, txtEmail, chkEmployees, chkVAT, chkSlovakia, btnAdd, btnUpdate, btnDelete });

            // DataGridView
            dgvCompanies = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
            dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "Názov", Width = 250 });
            dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Email", Width = 200 });
            dgvCompanies.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "HasEmployees", HeaderText = "Zamestnanci", Width = 100 });
            dgvCompanies.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "HasVAT", HeaderText = "DPH", Width = 80 });
            dgvCompanies.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "IsFromSlovakia", HeaderText = "Slovensko", Width = 100 });

            dgvCompanies.CellClick += (s, e) => SelectCompany();

            this.Controls.Add(dgvCompanies);
            this.Controls.Add(panelForm);
        }

        private void LoadCompanies()
        {
            dgvCompanies.DataSource = _companyService.GetAllCompanies();
        }

        private void AddCompany()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Prosím vyplň názov a email.", "Validácia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var company = new Company
            {
                Name = txtName.Text,
                Email = txtEmail.Text,
                HasEmployees = chkEmployees.Checked,
                HasVAT = chkVAT.Checked,
                IsFromSlovakia = chkSlovakia.Checked,
                CreatedDate = DateTime.Now
            };

            try
            {
                _companyService.AddCompany(company);
                MessageBox.Show("Firma bola pridaná!", "Úspech", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadCompanies();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba: {ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateCompany()
        {
            if (selectedCompany == null)
            {
                MessageBox.Show("Vyber firmu na úpravu.", "Upozornenie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            selectedCompany.Name = txtName.Text;
            selectedCompany.Email = txtEmail.Text;
            selectedCompany.HasEmployees = chkEmployees.Checked;
            selectedCompany.HasVAT = chkVAT.Checked;
            selectedCompany.IsFromSlovakia = chkSlovakia.Checked;

            try
            {
                _companyService.UpdateCompany(selectedCompany);
                MessageBox.Show("Firma bola upravená!", "Úspech", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadCompanies();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba: {ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteCompany()
        {
            if (selectedCompany == null)
            {
                MessageBox.Show("Vyber firmu na zmazanie.", "Upozornenie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"Naozaj chceš zmazať firmu '{selectedCompany.Name}'?", "Potvrdenie", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _companyService.DeleteCompany(selectedCompany.Id);
                    MessageBox.Show("Firma bola zmazaná!", "Úspech", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadCompanies();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Chyba: {ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SelectCompany()
        {
            if (dgvCompanies.SelectedRows.Count > 0)
            {
                var row = dgvCompanies.SelectedRows[0];
                selectedCompany = row.DataBoundItem as Company;

                if (selectedCompany != null)
                {
                    txtName.Text = selectedCompany.Name;
                    txtEmail.Text = selectedCompany.Email;
                    chkEmployees.Checked = selectedCompany.HasEmployees;
                    chkVAT.Checked = selectedCompany.HasVAT;
                    chkSlovakia.Checked = selectedCompany.IsFromSlovakia;
                }
            }
        }

        private void ClearForm()
        {
            txtName.Clear();
            txtEmail.Clear();
            chkEmployees.Checked = false;
            chkVAT.Checked = false;
            chkSlovakia.Checked = false;
            selectedCompany = null;
        }
    }
}
