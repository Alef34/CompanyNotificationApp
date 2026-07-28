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
                    if (company.HasEmployees)
                        await CheckOptionAsync(company, CompanyOptionType.Employees);

                    if (company.HasVAT)
                        await CheckOptionAsync(company, CompanyOptionType.VAT);

                    if (company.IsFromSlovakia)
                        await CheckOptionAsync(company, CompanyOptionType.Slovakia);
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

        private string GetTaskDescription(CompanyOptionType optionType)
        {
            return optionType switch
            {
                CompanyOptionType.Employees => "Povinnosť: Hlásenie zamestnancov",
                CompanyOptionType.VAT => "Povinnosť: DPH vykazovanie",
                CompanyOptionType.Slovakia => "Povinnosť: Hlásenie pre Slovensko",
                _ => "Nová povinnosť"
            };
        }

        private DateTime GetNextDueDate(CompanyOptionType optionType)
        {
            return optionType switch
            {
                CompanyOptionType.Employees => DateTime.Now.AddMonths(1),
                CompanyOptionType.VAT => DateTime.Now.AddMonths(1),
                CompanyOptionType.Slovakia => DateTime.Now.AddMonths(3),
                _ => DateTime.Now.AddMonths(1)
            };
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
    }
}
