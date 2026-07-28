using System.Data.Entity;
using CompanyNotificationApp.Models;

namespace CompanyNotificationApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<NotificationTask> NotificationTasks { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Company - NotificationTask vzťah
            modelBuilder.Entity<NotificationTask>()
                .HasRequired(nt => nt.Company)
                .WithMany(c => c.Tasks)
                .HasForeignKey(nt => nt.CompanyId)
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}
