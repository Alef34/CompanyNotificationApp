using System;
using System.Collections.Generic;
using System.Linq;
using CompanyNotificationApp.Data;
using CompanyNotificationApp.Models;

namespace CompanyNotificationApp.Services
{
    public class CompanyService
    {
        private readonly ApplicationDbContext _context;

        public CompanyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Company> GetAllCompanies()
        {
            return _context.Companies.ToList();
        }

        public Company GetCompanyById(int id)
        {
            return _context.Companies.FirstOrDefault(c => c.Id == id);
        }

        public void AddCompany(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            company.CreatedDate = DateTime.Now;
            _context.Companies.Add(company);
            _context.SaveChanges();

            CreateInitialNotifications(company);
        }

        private void CreateInitialNotifications(Company company)
        {
            var today = DateTime.Today;

            if (company.HasEmployees)
            {
                var dueDate = today.Day <= 10
                    ? new DateTime(today.Year, today.Month, 10)
                    : new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(9);

                _context.CompanyNotifications.Add(new CompanyNotification
                {
                    CompanyId = company.Id,
                    Description = "Mesačná povinnosť - Zamestnanci",
                    DueDate = dueDate,
                    NotificationType = "Monthly",
                    RelatedOption = "Employees",
                    IsCompleted = false,
                    CreatedDate = DateTime.Now
                });
            }

            if (company.HasVAT)
            {
                var dueDate = today.Day <= 20
                    ? new DateTime(today.Year, today.Month, 20)
                    : new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(19);

                _context.CompanyNotifications.Add(new CompanyNotification
                {
                    CompanyId = company.Id,
                    Description = "Mesačná povinnosť - DPH",
                    DueDate = dueDate,
                    NotificationType = "Monthly",
                    RelatedOption = "VAT",
                    IsCompleted = false,
                    CreatedDate = DateTime.Now
                });
            }

            if (company.IsFromSlovakia)
            {
                var anniversaryThisYear = new DateTime(today.Year, company.CreatedDate.Month, company.CreatedDate.Day);
                var dueDate = today < anniversaryThisYear
                    ? anniversaryThisYear
                    : anniversaryThisYear.AddYears(1);

                _context.CompanyNotifications.Add(new CompanyNotification
                {
                    CompanyId = company.Id,
                    Description = "Ročná povinnosť - Slovensko",
                    DueDate = dueDate,
                    NotificationType = "Annual",
                    RelatedOption = "Slovakia",
                    IsCompleted = false,
                    CreatedDate = DateTime.Now
                });
            }

            _context.SaveChanges();
        }

        public void UpdateCompany(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            var existingCompany = _context.Companies.FirstOrDefault(c => c.Id == company.Id);
            if (existingCompany == null)
                throw new InvalidOperationException($"Spoločnosť s ID {company.Id} neexistuje.");

            existingCompany.Name = company.Name;
            existingCompany.Email = company.Email;
            existingCompany.HasEmployees = company.HasEmployees;
            existingCompany.HasVAT = company.HasVAT;
            existingCompany.IsFromSlovakia = company.IsFromSlovakia;

            _context.SaveChanges();
        }

        public void DeleteCompany(int id)
        {
            var company = _context.Companies.FirstOrDefault(c => c.Id == id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                _context.SaveChanges();
            }
        }

        public List<Company> GetCompaniesByOption(CompanyOptionType optionType)
        {
            return optionType switch
            {
                CompanyOptionType.Employees => _context.Companies.Where(c => c.HasEmployees).ToList(),
                CompanyOptionType.VAT => _context.Companies.Where(c => c.HasVAT).ToList(),
                CompanyOptionType.Slovakia => _context.Companies.Where(c => c.IsFromSlovakia).ToList(),
                _ => new List<Company>()
            };
        }
    }
}
