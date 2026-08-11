using System;
using System.Net;
using System.ServiceProcess;
using System.IO;
using DatabaseBackup;
using DatabaseBackupService.NetFx.EmailService;

namespace DatabaseBackupService.NetFx
{
    static class Program
    {
        static void Main()
        {
            // Azure Storage requires TLS 1.2. On Windows 7 / .NET Framework, TLS 1.2 is not
            // enabled by default. Force it here so Azure connections succeed instead of retrying.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls;

#if DEBUG
            // Run as console application in DEBUG mode
            var registryConfigReader = new RegistryConfigReader();
            var backupConfig = registryConfigReader.LoadConfig();

            if (string.IsNullOrEmpty(backupConfig.ServerName))
            {
                Console.WriteLine("No configuration found in registry. Please run the Configuration UI to set up the backup service.");
                return;
            }

            string connectionString = backupConfig.GetConnectionString();
            bool windowsOS = Environment.OSVersion.Platform == PlatformID.Win32NT;

            IDbBackup dbBackup;
            if (backupConfig.DatabaseType == "mysql")
            {
                dbBackup = new MySqlDbBackup(connectionString);
            }
            else
            {
                dbBackup = new MsSqlDbBackup(connectionString, windowsOS);
            }

            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            var logger = new Logger.SeriLog(logPath);

            var emailConfig = new EmailConfig
            {
                SenderEmail = backupConfig.EmailSenderAddress,
                SenderPassword = backupConfig.EmailSenderPassword,
                RecipientEmail = backupConfig.EmailRecipientAddress,
                RecipientName = backupConfig.EmailRecipientAddress
            };

            IEmailService emailService = null;
            if (!string.IsNullOrEmpty(emailConfig.SenderEmail))
            {
                emailService = new GmailEmailService(emailConfig, logger);
            }

            var service = new DbBackupService(dbBackup, logger, backupConfig, emailService);

            Console.WriteLine("Starting Database Backup Service in DEBUG mode...");
            Console.WriteLine("Press 'Q' to quit.\n");

            service.StartDebug();

            while (Console.ReadKey(true).Key != ConsoleKey.Q)
            {
                // Wait for Q key to exit
            }

            Console.WriteLine("\nStopping service...");
            service.StopDebug();
            Console.WriteLine("Service stopped. Press any key to exit.");
            Console.ReadKey();
#else
            // Run as Windows Service in RELEASE mode
            // Service initialization happens in OnStart to avoid timeout issues
            var service = new DbBackupService();
            ServiceBase[] ServicesToRun = new ServiceBase[]
            {
                service
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
