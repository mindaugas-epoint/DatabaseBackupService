namespace DatabaseBackupService.Linux
{
    public class EnvironmentConfigReader
    {
        public BackupServiceConfig LoadConfig()
        {
            var config = new BackupServiceConfig
            {
                DatabaseType = GetEnvironmentVariable("DB_TYPE", "mssql"),
                ServerName = GetEnvironmentVariable("DB_SERVER", ""),
                Port = GetEnvironmentVariable("DB_PORT", ""),
                DatabaseName = GetEnvironmentVariable("DB_NAME", ""),
                UserName = GetEnvironmentVariable("DB_USER", ""),
                Password = GetEnvironmentVariable("DB_PASSWORD", ""),
                BackupSchedule = GetEnvironmentVariable("BACKUP_SCHEDULE", "02:00:00"),
                BackupFolderPath = GetEnvironmentVariable("BACKUP_FOLDER_PATH", "/backups"),
                EnableAzureBackup = GetBoolEnvironmentVariable("ENABLE_AZURE_BACKUP", false),
                UseAzureSasToken = GetBoolEnvironmentVariable("USE_AZURE_SAS_TOKEN", false),
                UseTimeWindow = GetBoolEnvironmentVariable("USE_TIME_WINDOW", false),
                BackupStartTime = GetEnvironmentVariable("BACKUP_START_TIME", "08:00:00"),
                BackupEndTime = GetEnvironmentVariable("BACKUP_END_TIME", "18:00:00"),
                BackupInterval = GetEnvironmentVariable("BACKUP_INTERVAL", "04:00:00"),
                AzureStorageConnectionString = GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING", ""),
                AzureContainerName = GetEnvironmentVariable("AZURE_CONTAINER_NAME", ""),
                AzureBlobPrefix = GetEnvironmentVariable("AZURE_BLOB_PREFIX", ""),
                AzureStorageAccountName = GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_NAME", ""),
                AzureSasToken = GetEnvironmentVariable("AZURE_SAS_TOKEN", ""),
                EmailSenderAddress = GetEnvironmentVariable("EMAIL_SENDER_ADDRESS", ""),
                EmailSenderPassword = GetEnvironmentVariable("EMAIL_SENDER_PASSWORD", ""),
                EmailRecipientAddress = GetEnvironmentVariable("EMAIL_RECIPIENT_ADDRESS", ""),
                BackupRetentionDays = GetIntEnvironmentVariable("BACKUP_RETENTION_DAYS", 30),
                MinimumBackupFiles = GetIntEnvironmentVariable("MINIMUM_BACKUP_FILES", 2)
            };

            return config;
        }

        private string GetEnvironmentVariable(string key, string defaultValue)
        {
            return Environment.GetEnvironmentVariable(key) ?? defaultValue;
        }

        private bool GetBoolEnvironmentVariable(string key, bool defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            return value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1";
        }

        private int GetIntEnvironmentVariable(string key, int defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out int result))
            {
                return defaultValue;
            }
            return result;
        }
    }
}
