# Company Notification App

Windows Forms aplikácia pre správu firiem s automatickými notifikáciami na povinnosti.

## Funkcie

- **Správa firiem** - Evidencia firiem s možnosťou zaškrtnúť tri voľby:
  - Zamestnanci
  - DPH
  - Slovensko

- **Automatické notifikácie** - Posielanie emailov na základe zvolených volieb
- **Prehľad úloh** - Zobrazenie všetkých povinností a ich statusu
- **Plánovanie** - Automatická kontrola a notifikácia v určených intervaloch

## Požiadavky

- Visual Studio 2022
- .NET Framework 4.7.2+
- Entity Framework 6.0+
- DevExpress Controls
- SQL Server LocalDB alebo SQL Server Express

## Inštalácia

1. Klonuj repozitár
2. Otvor projekt v Visual Studio 2022
3. Nainštaluj NuGet balíčky:
   ```
   Install-Package EntityFramework
   Install-Package DevExpress.WindowsDesktop.Core (ak chceš DevExpress)
   ```

4. Vytvor databázu:
   ```
   Enable-Migrations
   Add-Migration Initial
   Update-Database
   ```

5. Nakonfiguruj SMTP nastavenia v EmailService.cs

## Použitie

1. Spusti aplikáciu
2. Pridaj nové firmy
3. Zaškrtnite relevantné voľby
4. Aplikácia automaticky pošle notifikácie na nakonfigurovaný email

## Štruktúra projektu

```
CompanyNotificationApp/
├── Models/
│   ├── Company.cs
│   ├── CompanyOptionType.cs
│   ├── NotificationTask.cs
│   └── NotificationTemplate.cs
├── Services/
│   ├── CompanyService.cs
│   ├── NotificationService.cs
│   ├── EmailService.cs
│   └── TaskSchedulerService.cs
├── Data/
│   └── ApplicationDbContext.cs
└── README.md
```

## Budúce vylepšenia

- GUI s DevExpress controls
- Viac typov notifikácií (SMS, Teams, Slack)
- Nastavenie eigentných dátumov pre každú firmu
- Export správ

## Autor

Alef34
