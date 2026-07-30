namespace CompanyNotificationApp
{
    partial class CompanyManagementForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CompanyManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Name = "CompanyManagementForm";
            this.Text = "Správa firiem";
            this.Load += new System.EventHandler(this.CompanyManagementForm_Load);
            this.ResumeLayout(false);
        }
    }
}
