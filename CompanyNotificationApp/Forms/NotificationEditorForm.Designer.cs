namespace CompanyNotificationApp
{
    partial class NotificationEditorForm
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 500);
            this.Name = "NotificationEditorForm";
            this.Text = "Notifikácie";
            this.Load += new System.EventHandler(this.NotificationEditorForm_Load);
            this.ResumeLayout(false);
        }
    }
}
