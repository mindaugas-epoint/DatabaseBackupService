using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace DatabaseBackupService.ConfigUI
{
    public class RegistryConfigManager
    {
        private const string RegistryKeyPath = @"SOFTWARE\DatabaseBackupService";

        public BackupConfig LoadConfig()
        {
            var config = new BackupConfig();

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    if (key != null)
                    {
                        config.DatabaseType = key.GetValue("DatabaseType")?.ToString() ?? "mssql";
                        config.ServerName = key.GetValue("ServerName")?.ToString() ?? "";
                        config.Port = key.GetValue("Port")?.ToString() ?? "";
                        config.DatabaseName = key.GetValue("DatabaseName")?.ToString() ?? "";
                        config.UserName = key.GetValue("UserName")?.ToString() ?? "";

                        var encryptedPassword = key.GetValue("Password") as byte[];
                        if (encryptedPassword != null && encryptedPassword.Length > 0)
                        {
                            config.Password = DecryptPassword(encryptedPassword);
                        }

                        config.BackupSchedule = key.GetValue("BackupSchedule")?.ToString() ?? "";
                        config.BackupFolderPath = key.GetValue("BackupFolderPath")?.ToString() ?? "";
                        config.EnableAzureBackup = key.GetValue("EnableAzureBackup")?.ToString() == "True";
                        config.UseAzureSasToken = key.GetValue("UseAzureSasToken")?.ToString() == "True";

                        config.UseTimeWindow = key.GetValue("UseTimeWindow")?.ToString() == "True";
                        config.BackupStartTime = key.GetValue("BackupStartTime")?.ToString() ?? "08:00:00";
                        config.BackupEndTime = key.GetValue("BackupEndTime")?.ToString() ?? "18:00:00";
                        config.BackupInterval = key.GetValue("BackupInterval")?.ToString() ?? "04:00:00";

                        var encryptedAzureConnectionString = key.GetValue("AzureStorageConnectionString") as byte[];
                        if (encryptedAzureConnectionString != null && encryptedAzureConnectionString.Length > 0)
                        {
                            config.AzureStorageConnectionString = DecryptPassword(encryptedAzureConnectionString);
                        }

                        config.AzureContainerName = key.GetValue("AzureContainerName")?.ToString() ?? "";
                        config.AzureBlobPrefix = key.GetValue("AzureBlobPrefix")?.ToString() ?? "";
                        config.AzureStorageAccountName = key.GetValue("AzureStorageAccountName")?.ToString() ?? "";

                        var encryptedSasToken = key.GetValue("AzureSasToken") as byte[];
                        if (encryptedSasToken != null && encryptedSasToken.Length > 0)
                        {
                            config.AzureSasToken = DecryptPassword(encryptedSasToken);
                        }

                        config.EmailSenderAddress = key.GetValue("EmailSenderAddress")?.ToString() ?? "";
                        var encryptedEmailPassword = key.GetValue("EmailSenderPassword") as byte[];
                        if (encryptedEmailPassword != null && encryptedEmailPassword.Length > 0)
                        {
                            config.EmailSenderPassword = DecryptPassword(encryptedEmailPassword);
                        }
                        config.EmailRecipientAddress = key.GetValue("EmailRecipientAddress")?.ToString() ?? "";

                        // Backup retention settings
                        if (int.TryParse(key.GetValue("BackupRetentionDays")?.ToString(), out int retentionDays))
                        {
                            config.BackupRetentionDays = retentionDays;
                        }
                        if (int.TryParse(key.GetValue("MinimumBackupFiles")?.ToString(), out int minFiles))
                        {
                            config.MinimumBackupFiles = minFiles;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return config;
        }

        public void SaveConfig(BackupConfig config)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
                {
                    if (key != null)
                    {
                        key.SetValue("DatabaseType", config.DatabaseType);
                        key.SetValue("ServerName", config.ServerName);
                        key.SetValue("Port", config.Port);
                        key.SetValue("DatabaseName", config.DatabaseName);
                        key.SetValue("UserName", config.UserName);

                        if (!string.IsNullOrEmpty(config.Password))
                        {
                            var encryptedPassword = EncryptPassword(config.Password);
                            key.SetValue("Password", encryptedPassword, RegistryValueKind.Binary);
                        }

                        key.SetValue("BackupSchedule", config.BackupSchedule);
                        key.SetValue("BackupFolderPath", config.BackupFolderPath);
                        key.SetValue("EnableAzureBackup", config.EnableAzureBackup.ToString());
                        key.SetValue("UseAzureSasToken", config.UseAzureSasToken.ToString());

                        key.SetValue("UseTimeWindow", config.UseTimeWindow.ToString());
                        key.SetValue("BackupStartTime", config.BackupStartTime);
                        key.SetValue("BackupEndTime", config.BackupEndTime);
                        key.SetValue("BackupInterval", config.BackupInterval);

                        if (!string.IsNullOrEmpty(config.AzureStorageConnectionString))
                        {
                            var encryptedAzureConnectionString = EncryptPassword(config.AzureStorageConnectionString);
                            key.SetValue("AzureStorageConnectionString", encryptedAzureConnectionString, RegistryValueKind.Binary);
                        }

                        key.SetValue("AzureContainerName", config.AzureContainerName);
                        key.SetValue("AzureStorageAccountName", config.AzureStorageAccountName ?? "");
                        key.SetValue("AzureBlobPrefix", config.AzureBlobPrefix ?? "");

                        if (!string.IsNullOrEmpty(config.AzureSasToken))
                        {
                            var encryptedSasToken = EncryptPassword(config.AzureSasToken);
                            key.SetValue("AzureSasToken", encryptedSasToken, RegistryValueKind.Binary);
                        }

                        key.SetValue("EmailSenderAddress", config.EmailSenderAddress ?? "");
                        if (!string.IsNullOrEmpty(config.EmailSenderPassword))
                        {
                            var encryptedEmailPassword = EncryptPassword(config.EmailSenderPassword);
                            key.SetValue("EmailSenderPassword", encryptedEmailPassword, RegistryValueKind.Binary);
                        }
                        key.SetValue("EmailRecipientAddress", config.EmailRecipientAddress ?? "");

                        // Backup retention settings
                        key.SetValue("BackupRetentionDays", config.BackupRetentionDays.ToString());
                        key.SetValue("MinimumBackupFiles", config.MinimumBackupFiles.ToString());
                    }
                }

                MessageBox.Show("Configuration saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private byte[] EncryptPassword(string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] encryptedBytes = ProtectedData.Protect(passwordBytes, null, DataProtectionScope.CurrentUser);
            return encryptedBytes;
        }

        private string DecryptPassword(byte[] encryptedPassword)
        {
            try
            {
                byte[] decryptedBytes = ProtectedData.Unprotect(encryptedPassword, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    public class BackupConfig
    {
        public string DatabaseType { get; set; } = "mssql";
        public string ServerName { get; set; } = "";
        public string Port { get; set; } = "";
        public string DatabaseName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string BackupSchedule { get; set; } = "";
        public string BackupFolderPath { get; set; } = "";
        public bool EnableAzureBackup { get; set; } = false;
        public string AzureStorageConnectionString { get; set; } = "";
        public string AzureContainerName { get; set; } = "";
        public bool UseAzureSasToken { get; set; } = false;
        public string AzureStorageAccountName { get; set; } = "";
        public string AzureSasToken { get; set; } = "";
        public string AzureBlobPrefix { get; set; } = "";

        public bool UseTimeWindow { get; set; } = false;
        public string BackupStartTime { get; set; } = "08:00:00";
        public string BackupEndTime { get; set; } = "18:00:00";
        public string BackupInterval { get; set; } = "04:00:00";

        public string EmailSenderAddress { get; set; } = "";
        public string EmailSenderPassword { get; set; } = "";
        public string EmailRecipientAddress { get; set; } = "";

        public int BackupRetentionDays { get; set; } = 30;
        public int MinimumBackupFiles { get; set; } = 2;
    }
}
