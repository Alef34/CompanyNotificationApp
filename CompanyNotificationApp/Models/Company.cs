using System;
using System.Collections.Generic;

namespace CompanyNotificationApp.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool HasEmployees { get; set; }      // Zamestnanci
        public bool HasVAT { get; set; }             // DPH
        public bool IsFromSlovakia { get; set; }     // Slovensko
        public DateTime CreatedDate { get; set; }
        public ICollection<NotificationTask> Tasks { get; set; } = new List<NotificationTask>();
    }
}
