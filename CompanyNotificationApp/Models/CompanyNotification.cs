using System;

namespace CompanyNotificationApp.Models
{
    public class CompanyNotification
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string NotificationType { get; set; }
        public string RelatedOption { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
