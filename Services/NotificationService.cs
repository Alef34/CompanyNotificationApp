using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompanyNotificationApp.Data;
using CompanyNotificationApp.Models;

namespace CompanyNotificationApp.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public NotificationService(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task CheckAndNotifyAsync()
        {
            try
            {
                var allCompanies = _context.Companies.ToList();

                foreach (var company in allCompanies)
                {
                    // Kontorla NotificationTasks (automatické - Zamestnanci, DPH, Slovensko)
                    if (company.HasEmployees)
                        await CheckOptionAsync(company, CompanyOptionType.Employees);

                    if (company.HasVAT)
                        await CheckOptionAsync(company, CompanyOptionType.VAT);

                    if (company.IsFromSlovakia)
                        await CheckOptionAsync(company, CompanyOptionType.Slovakia);

                    // Kontrola CompanyNotifications (manuálne vytvorené)
                    await CheckCompanyNotificationsAsync(company);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri kontrole notifikácií: {ex.Message}");
            }
        }

        private async Task CheckOptionAsync(Company company, CompanyOptionType optionType)
        {
            var existingTask = _context.NotificationTasks
                .FirstOrDefault(nt => nt.CompanyId == company.Id && 
                                      nt.RelatedOption == optionType && 
                                      !nt.IsCompleted &&
                                      nt.DueDate > DateTime.Now);

            if (existingTask == null)
            {
                var newTask = new NotificationTask
                {
                    CompanyId = company.Id,
                    Description = GetTaskDescription(optionType),
                    DueDate = GetNextDueDate(optionType),
                    RelatedOption = optionType,
                    NotificationType = "Email",
                    IsCompleted = false,
                    LastNotificationDate = DateTime.Now
                };

                _context.NotificationTasks.Add(newTask);
                await _emailService.SendNotificationAsync(company, newTask);
                _context.SaveChanges();
            }
        }

        private async Task CheckCompanyNotificationsAsync(Company company)
        {
            var pendingNotifications = _context.CompanyNotifications
                .Where(cn => cn.CompanyId == company.Id && 
                            !cn.IsCompleted && 
                            cn.DueDate <= DateTime.Now)
                .ToList();

            foreach (var notification in pendingNotifications)
            {
                // Vytvor NotificationTask z CompanyNotification
                var task = new NotificationTask
                {
                    CompanyId = company.Id,
                    Description = notification.Description,
                    DueDate = notification.DueDate,
                    RelatedOption = CompanyOptionType.Slovakia, // Default, keďže CompanyNotification nemá konkrétny typ
                    NotificationType = notification.NotificationType,
                    IsCompleted = false,
                    LastNotificationDate = DateTime.Now
                };

                // Pošli email
                await _emailService.SendNotificationAsync(company, task);

                // Označ ako hotové
                notification.IsCompleted = true;
                _context.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"Notifikácia poslana: {company.Name} - {notification.Description}");
            }
        }

        private string GetTaskDescription(CompanyOptionType optionType)
        {
            switch (optionType)
            {
                case CompanyOptionType.Employees:
                    return "Povinnosť: Hlásenie zamestnancov";
                case CompanyOptionType.VAT:
                    return "Povinnosť: DPH vykazovanie";
                case CompanyOptionType.Slovakia:
                    return "Povinnosť: Hlásenie pre Slovensko";
                default:
                    return "Nová povinnosť";
            }
        }

        private DateTime GetNextDueDate(CompanyOptionType optionType)
        {
            switch (optionType)
            {
                case CompanyOptionType.Employees:
                    return DateTime.Now.AddMonths(1);
                case CompanyOptionType.VAT:
                    return DateTime.Now.AddMonths(1);
                case CompanyOptionType.Slovakia:
                    return DateTime.Now.AddMonths(3);
                default:
                    return DateTime.Now.AddMonths(1);
            }
        }

        public void MarkTaskAsCompleted(int taskId)
        {
            var task = _context.NotificationTasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.IsCompleted = true;
                _context.SaveChanges();
            }
        }

        public List<NotificationTask> GetPendingTasksForCompany(int companyId)
        {
            return _context.NotificationTasks
                .Where(nt => nt.CompanyId == companyId && !nt.IsCompleted && nt.DueDate <= DateTime.Now)
                .ToList();
        }

        public List<NotificationTask> GetAllPendingTasks()
        {
            return _context.NotificationTasks
                .Where(nt => !nt.IsCompleted && nt.DueDate <= DateTime.Now)
                .ToList();
        }

        // Nové metódy pre CompanyNotification
        public void AddNotification(CompanyNotification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            _context.CompanyNotifications.Add(notification);
            _context.SaveChanges();
        }

        public void DeleteNotification(int notificationId)
        {
            var notification = _context.CompanyNotifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                _context.CompanyNotifications.Remove(notification);
                _context.SaveChanges();
            }
        }

        public List<CompanyNotification> GetNotificationsByCompany(int companyId)
        {
            return _context.CompanyNotifications
                .Where(n => n.CompanyId == companyId)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
        }

        public CompanyNotification GetNotificationById(int notificationId)
        {
            return _context.CompanyNotifications.FirstOrDefault(n => n.Id == notificationId);
        }

        public void UpdateNotification(CompanyNotification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            var existing = _context.CompanyNotifications.FirstOrDefault(n => n.Id == notification.Id);
            if (existing != null)
            {
                existing.Description = notification.Description;
                existing.DueDate = notification.DueDate;
                existing.NotificationType = notification.NotificationType;
                existing.IsCompleted = notification.IsCompleted;
                _context.SaveChanges();
            }
        }
    }
}
