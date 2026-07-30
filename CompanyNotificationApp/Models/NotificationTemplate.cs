namespace CompanyNotificationApp.Models
{
    public class NotificationTemplate
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public CompanyOptionType OptionType { get; set; }
        public int DaysBeforeDue { get; set; } // Počet dní pred termínom na notifikáciu
    }
}
