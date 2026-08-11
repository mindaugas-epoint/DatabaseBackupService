using DatabaseBackup;
using DatabaseBackupService;
using DatabaseBackupService.EmailService;
using DatabaseBackupService.Linux;
using Logger;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();

// Load configuration from environment variables
var environmentConfigReader = new EnvironmentConfigReader();
var backupConfig = environmentConfigReader.LoadConfig();

// Validate configuration
if (string.IsNullOrEmpty(backupConfig.ServerName))
{
    Console.WriteLine("ERROR: DB_SERVER environment variable is required. Please set up the required environment variables.");
    Environment.Exit(1);
}

if (string.IsNullOrEmpty(backupConfig.DatabaseName))
{
    Console.WriteLine("ERROR: DB_NAME environment variable is required. Please set up the required environment variables.");
    Environment.Exit(1);
}

// Build connection string
string connectionString = backupConfig.GetConnectionString();

// Register services
builder.Services.AddSingleton(backupConfig);

// Register database backup service based on database type
if (backupConfig.DatabaseType.Equals("mysql", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IDbBackup>(sp => new MySqlDbBackup(connectionString));
}
else // Default to MSSQL
{
    bool windowsOS = Environment.OSVersion.Platform == PlatformID.Win32NT;
    builder.Services.AddSingleton<IDbBackup>(sp => new MsSqlDbBackup(connectionString, windowsOS));
}

// Register email service
var emailConfig = new EmailConfig
{
    SenderEmail = backupConfig.EmailSenderAddress,
    SenderPassword = backupConfig.EmailSenderPassword,
    RecipientEmail = backupConfig.EmailRecipientAddress,
    RecipientName = backupConfig.EmailRecipientAddress
};
builder.Services.AddSingleton(emailConfig);
builder.Services.AddSingleton<IEmailService, GmailEmailService>();

// Register logger and worker service
builder.Services.AddSingleton<Logger.ILogger, SeriLog>();
builder.Services.AddHostedService<DbBackupWorker>();

var host = builder.Build();

Console.WriteLine("Database Backup Service (Linux Container) started");
Console.WriteLine($"Database Type: {backupConfig.DatabaseType}");
Console.WriteLine($"Server: {backupConfig.ServerName}");
Console.WriteLine($"Database: {backupConfig.DatabaseName}");
Console.WriteLine($"Backup Schedule: {backupConfig.BackupSchedule}");
Console.WriteLine($"Backup Folder: {backupConfig.BackupFolderPath}");
Console.WriteLine($"Azure Backup Enabled: {backupConfig.EnableAzureBackup}");
Console.WriteLine($"Use Time Window: {backupConfig.UseTimeWindow}");

await host.RunAsync();
