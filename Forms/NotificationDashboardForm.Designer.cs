namespace CompanyNotificationApp
{
    partial class NotificationDashboardForm
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
            // NotificationDashboardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Name = "NotificationDashboardForm";
            this.Text = "Notifikácie";
            this.Load += new System.EventHandler(this.NotificationDashboardForm_Load);
            this.ResumeLayout(false);
        }
    }
}
