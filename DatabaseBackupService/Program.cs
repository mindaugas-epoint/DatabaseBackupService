using DatabaseBackup;
using DatabaseBackupService.EmailService;
using Logger;
using System.Reflection;

namespace DatabaseBackupService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var registryConfigReader = new RegistryConfigReader();
            var backupConfig = registryConfigReader.LoadConfig();

            if (string.IsNullOrEmpty(backupConfig.ServerName))
            {
                Console.WriteLine("No configuration found in registry. Please run the Configuration UI to set up the backup service.");
                return;
            }

            string connectionString = backupConfig.GetConnectionString();
            bool windowsOS = Environment.OSVersion.Platform == PlatformID.Win32NT;

            IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton(backupConfig);

                    if (backupConfig.DatabaseType == "mysql")
                    {
                        services.AddSingleton<IDbBackup>(sp => new MySqlDbBackup(connectionString));
                    }
                    else 
                    {
                        services.AddSingleton<IDbBackup>(sp => new MsSqlDbBackup(connectionString, windowsOS));
                    }

                    var emailConfig = new EmailConfig
                    {
                        SenderEmail = backupConfig.EmailSenderAddress,
                        SenderPassword = backupConfig.EmailSenderPassword,
                        RecipientEmail = backupConfig.EmailRecipientAddress,
                        RecipientName = backupConfig.EmailRecipientAddress
                    };
                    services.AddSingleton(emailConfig);
                    services.AddSingleton<IEmailService, GmailEmailService>();

                    services.AddSingleton<Logger.ILogger, SeriLog>();
                    services.AddHostedService<DbBackupWorker>();
                })
                .Build();

            host.Run();
        }


    }
}