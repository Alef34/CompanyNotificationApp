using System;

namespace CompanyNotificationApp.Models
{
    public class NotificationTask
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? LastNotificationDate { get; set; }
        public string NotificationType { get; set; } // Email, Desktop, Both
        public CompanyOptionType RelatedOption { get; set; }
        public bool IsCompleted { get; set; }
        public Company Company { get; set; }
    }
}
