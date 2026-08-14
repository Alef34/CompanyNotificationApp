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
        private Button btnNotifications;
        private Company selectedCompany;
        private EventLogger _logger;
        private bool? _editedCheckboxOriginalValue;
        private int _editedCheckboxRowIndex = -1;
        private int _editedCheckboxColumnIndex = -1;
        private bool _isCheckboxUpdateInProgress;

        public CompanyManagementForm(CompanyService companyService, NotificationService notificationService)
        {
            InitializeComponent();
            _companyService = companyService;
            _notificationService = notificationService;
            _logger = EventLogger.Instance;
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

            btnNotifications = new Button
            {
                Text = "📋 Notifikácie",
                Location = new System.Drawing.Point(280, 110),
                Width = 120,
                Height = 25,
                BackColor = System.Drawing.Color.Purple,
                ForeColor = System.Drawing.Color.White
            };
            btnNotifications.Click += (s, e) => ManageNotifications();

            panelForm.Controls.AddRange(new Control[] { lblName, txtName, lblEmail, txtEmail, chkEmployees, chkVAT, chkSlovakia, btnAdd, btnUpdate, btnDelete, btnNotifications });

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
            dgvCompanies.CellBeginEdit += (s, e) => TrackCheckboxOriginalValue(e.RowIndex, e.ColumnIndex);
            dgvCompanies.CellEndEdit += (s, e) => HandleCheckboxChange(e);

            // Pridaj najprv panelForm, potom DataGridView (správne poradie)
            this.Controls.Add(dgvCompanies);
            this.Controls.Add(panelForm);
        }

        private void LoadCompanies()
        {
            try
            {
                var companies = _companyService.GetAllCompanies();
                if (companies != null && companies.Count > 0)
                {
                    dgvCompanies.DataSource = companies;
                }
                else
                {
                    dgvCompanies.DataSource = new System.Collections.Generic.List<Company>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Chyba pri načítavaní firiem: {0}", ex.Message), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                _logger.LogSuccess("CompanyManagement", string.Format("Firma pridaná: {0}", company.Name));
                MessageBox.Show("Firma bola pridaná!", "Úspech", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadCompanies();
            }
            catch (Exception ex)
            {
                _logger.LogError("CompanyManagement", string.Format("Chyba pri pridaní: {0}", ex.Message));
                MessageBox.Show(string.Format("Chyba: {0}", ex.Message), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                _logger.LogSuccess("CompanyManagement", string.Format("Firma upravená: {0}", selectedCompany.Name));
                MessageBox.Show("Firma bola upravená!", "Úspech", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadCompanies();
            }
            catch (Exception ex)
            {
                _logger.LogError("CompanyManagement", string.Format("Chyba pri úprave: {0}", ex.Message));
                MessageBox.Show(string.Format("Chyba: {0}", ex.Message), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteCompany()
        {
            if (selectedCompany == null)
            {
                MessageBox.Show("Vyber firmu na zmazanie.", "Upozornenie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(string.Format("Naozaj chceš zmazať firmu '{0}'?", selectedCompany.Name), "Potvrdenie", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _companyService.DeleteCompany(selectedCompany.Id);
                    _logger.LogSuccess("CompanyManagement", string.Format("Firma zmazaná: {0}", selectedCompany.Name));
                    MessageBox.Show("Firma bola zmazaná!", "Úspech", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadCompanies();
                }
                catch (Exception ex)
                {
                    _logger.LogError("CompanyManagement", string.Format("Chyba pri mazaní: {0}", ex.Message));
                    MessageBox.Show(string.Format("Chyba: {0}", ex.Message), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ManageNotifications()
        {
            if (selectedCompany == null)
            {
                MessageBox.Show("Vyber firmu.", "Upozornenie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _logger.LogInfo("CompanyManagement", string.Format("Otvorené spravovanie notifikácií pre: {0}", selectedCompany.Name));
            NotificationEditorForm form = new NotificationEditorForm(selectedCompany, _notificationService);
            form.ShowDialog();
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

        private void TrackCheckboxOriginalValue(int rowIndex, int columnIndex)
        {
            _editedCheckboxOriginalValue = null;
            _editedCheckboxRowIndex = -1;
            _editedCheckboxColumnIndex = -1;

            if (rowIndex < 0 || !IsCheckboxColumn(columnIndex))
            {
                return;
            }

            _editedCheckboxOriginalValue = ToBooleanValue(dgvCompanies.Rows[rowIndex].Cells[columnIndex].Value);
            _editedCheckboxRowIndex = rowIndex;
            _editedCheckboxColumnIndex = columnIndex;
        }

        private void HandleCheckboxChange(DataGridViewCellEventArgs e)
        {
            if (_isCheckboxUpdateInProgress || e.RowIndex < 0 || !IsCheckboxColumn(e.ColumnIndex))
            {
                return;
            }

            try
            {
                if (_editedCheckboxRowIndex != e.RowIndex || _editedCheckboxColumnIndex != e.ColumnIndex)
                {
                    return;
                }

                bool newValue = ToBooleanValue(dgvCompanies.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                if (!_editedCheckboxOriginalValue.HasValue || _editedCheckboxOriginalValue.Value == newValue)
                {
                    return;
                }

                Company companyFromRow = dgvCompanies.Rows[e.RowIndex].DataBoundItem as Company;
                if (companyFromRow == null)
                {
                    return;
                }

                selectedCompany = companyFromRow;

                if (e.ColumnIndex == 3)
                {
                    selectedCompany.HasEmployees = newValue;
                }
                else if (e.ColumnIndex == 4)
                {
                    selectedCompany.HasVAT = newValue;
                }
                else if (e.ColumnIndex == 5)
                {
                    selectedCompany.IsFromSlovakia = newValue;
                }

                chkEmployees.Checked = selectedCompany.HasEmployees;
                chkVAT.Checked = selectedCompany.HasVAT;
                chkSlovakia.Checked = selectedCompany.IsFromSlovakia;

                _isCheckboxUpdateInProgress = true;
                _companyService.UpdateCompany(selectedCompany);
                _logger.LogSuccess("CompanyManagement", string.Format("Checkbox zmenený pre firmu: {0}", selectedCompany.Name));
                LoadCompanies();
            }
            catch (Exception ex)
            {
                _logger.LogError("CompanyManagement", string.Format("Chyba pri zmene checkboxu: {0}", ex.Message));
                MessageBox.Show(string.Format("Chyba pri ukladaní zmeny: {0}", ex.Message), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isCheckboxUpdateInProgress = false;
                _editedCheckboxOriginalValue = null;
                _editedCheckboxRowIndex = -1;
                _editedCheckboxColumnIndex = -1;
            }
        }

        private bool IsCheckboxColumn(int columnIndex)
        {
            return columnIndex == 3 || columnIndex == 4 || columnIndex == 5;
        }

        private bool ToBooleanValue(object value)
        {
            return value != null && value != DBNull.Value && Convert.ToBoolean(value);
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
