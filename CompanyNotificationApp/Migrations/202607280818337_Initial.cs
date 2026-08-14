namespace CompanyNotificationApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Email = c.String(),
                        HasEmployees = c.Boolean(nullable: false),
                        HasVAT = c.Boolean(nullable: false),
                        IsFromSlovakia = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NotificationTasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompanyId = c.Int(nullable: false),
                        Description = c.String(),
                        DueDate = c.DateTime(nullable: false),
                        LastNotificationDate = c.DateTime(),
                        NotificationType = c.String(),
                        RelatedOption = c.Int(nullable: false),
                        IsCompleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.CompanyId, cascadeDelete: true)
                .Index(t => t.CompanyId);
            
            CreateTable(
                "dbo.NotificationTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        EmailSubject = c.String(),
                        EmailBody = c.String(),
                        OptionType = c.Int(nullable: false),
                        DaysBeforeDue = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NotificationTasks", "CompanyId", "dbo.Companies");
            DropIndex("dbo.NotificationTasks", new[] { "CompanyId" });
            DropTable("dbo.NotificationTemplates");
            DropTable("dbo.NotificationTasks");
            DropTable("dbo.Companies");
        }
    }
}
