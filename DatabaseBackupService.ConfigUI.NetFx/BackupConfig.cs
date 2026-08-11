namespace DatabaseBackupService.ConfigUI.NetFx
{
    public class BackupConfig
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
        public bool UseAzureSasToken { get; set; }
        public string AzureStorageAccountName { get; set; }
        public string AzureSasToken { get; set; }
        public string AzureBlobPrefix { get; set; }

        public bool UseTimeWindow { get; set; }
        public string BackupStartTime { get; set; }
        public string BackupEndTime { get; set; }
        public string BackupInterval { get; set; }

        public string EmailSenderAddress { get; set; }
        public string EmailSenderPassword { get; set; }
        public string EmailRecipientAddress { get; set; }

        public int BackupRetentionDays { get; set; }
        public int MinimumBackupFiles { get; set; }

        public BackupConfig()
        {
            DatabaseType = "mssql";
            ServerName = "";
            Port = "";
            DatabaseName = "";
            UserName = "";
            Password = "";
            BackupSchedule = "";
            BackupFolderPath = "";
            EnableAzureBackup = false;
            AzureStorageConnectionString = "";
            AzureContainerName = "";
            UseAzureSasToken = false;
            AzureStorageAccountName = "";
            AzureSasToken = "";
            AzureBlobPrefix = "";
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
    }
}
