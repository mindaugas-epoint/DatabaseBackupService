namespace DatabaseBackupService
{
    public class BackupServiceConfig
    {
        public string DatabaseType { get; set; }
        public string ServerName { get; set; }
        public string Port { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string BackupSchedule { get; set; }
        public string BackupFolderPath { get; set; }
        public bool EnableAzureBackup { get; set; }
        public string AzureStorageConnectionString { get; set; }
        public string AzureContainerName { get; set; }
        public string AzureBlobPrefix { get; set; }
        public bool UseAzureSasToken { get; set; }
        public string AzureStorageAccountName { get; set; }
        public string AzureSasToken { get; set; }

        public bool UseTimeWindow { get; set; }
        public string BackupStartTime { get; set; }
        public string BackupEndTime { get; set; }
        public string BackupInterval { get; set; }

        public string EmailSenderAddress { get; set; }
        public string EmailSenderPassword { get; set; }
        public string EmailRecipientAddress { get; set; }

        public int BackupRetentionDays { get; set; }
        public int MinimumBackupFiles { get; set; }

        public BackupServiceConfig()
        {
            DatabaseType = "mssql";
            ServerName = "";
            Port = "";
            DatabaseName = "";
            UserName = "";
            Password = "";
            BackupSchedule = "02:00:00";
            BackupFolderPath = "";
            EnableAzureBackup = false;
            AzureStorageConnectionString = "";
            AzureContainerName = "";
            AzureBlobPrefix = "";
            UseAzureSasToken = false;
            AzureStorageAccountName = "";
            AzureSasToken = "";
            UseTimeWindow = false;
            BackupStartTime = "08:00:00";
            BackupEndTime = "18:00:00";
            BackupInterval = "04:00:00";
            EmailSenderAddress = "";
            EmailSenderPassword = "";
            EmailRecipientAddress = "";
            BackupRetentionDays = 30;
            MinimumBackupFiles = 2;
        }

        public string GetConnectionString()
        {
            if (DatabaseType == "mysql")
            {
                return string.Format("Server={0};Port={1};Database={2};User ID={3};Password={4};",
                    ServerName, Port, DatabaseName, UserName, Password);
            }
            else // mssql
            {
                string serverAddress = string.IsNullOrWhiteSpace(Port) || Port == "1433"
                    ? ServerName
                    : string.Format("{0},{1}", ServerName, Port);
                return string.Format("Data Source={0};Initial Catalog={1};Persist Security Info=True;Encrypt=True;TrustServerCertificate=True;User ID={2};Password={3};",
                    serverAddress, DatabaseName, UserName, Password);
            }
        }
    }
}
