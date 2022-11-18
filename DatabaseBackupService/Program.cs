using DatabaseBackup;
using Logger;
using SendEmail;
using System.Reflection;

namespace DatabaseBackupService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string connectionString;
            IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

            DBConfig dbConfig = config.GetSection("DbConfig").Get<DBConfig>();
            connectionString = dbConfig.GetConnectionString();

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    if (dbConfig.SqlServerType == "mysql")
                    {
                        services.AddSingleton<IDbBackup>(sp => new MySqlDbBackup(connectionString));
                    }
                    else 
                    {
                        services.AddSingleton<IDbBackup>(sp => new MsSqlDbBackup(connectionString, dbConfig.WindowsOS));
                    }
                    services.AddSingleton<ISendEmail, SendGridEmail>();
                    services.AddSingleton<Logger.ILogger, SeriLog>();
                    services.Configure<DbBackupWorker>(config);
                    services.AddHostedService<DbBackupWorker>();
                })
                .Build();

            host.Run();
        }


    }
}