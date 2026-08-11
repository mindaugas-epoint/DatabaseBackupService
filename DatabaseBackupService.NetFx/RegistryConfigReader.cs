using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace DatabaseBackupService.NetFx
{
    public class RegistryConfigReader
    {
        private const string RegistryKeyPath = @"SOFTWARE\DatabaseBackupService";

        public BackupServiceConfig LoadConfig()
        {
            var config = new BackupServiceConfig();

            try
            {
                // Try LocalMachine first (for Windows Services), then fall back to CurrentUser (for console/debug mode)
                RegistryKey key = Registry.LocalMachine.OpenSubKey(RegistryKeyPath);

                if (key == null)
                {
                    // Fall back to CurrentUser for debug/console mode
                    key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
                }

                if (key != null)
                {
                    using (key)
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

                        config.BackupSchedule = key.GetValue("BackupSchedule")?.ToString() ?? "02:00:00";
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

                        // Load backup retention settings
                        if (int.TryParse(key.GetValue("BackupRetentionDays")?.ToString(), out int retentionDays))
                        {
                            config.BackupRetentionDays = retentionDays;
                        }

                        if (int.TryParse(key.GetValue("MinimumBackupFiles")?.ToString(), out int minimumFiles))
                        {
                            config.MinimumBackupFiles = minimumFiles;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration from registry: {ex.Message}");
            }

            return config;
        }

        private string DecryptPassword(byte[] encryptedPassword)
        {
            try
            {
                // Try LocalMachine first (new format for Windows Services)
                byte[] decryptedBytes = ProtectedData.Unprotect(encryptedPassword, null, DataProtectionScope.LocalMachine);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                try
                {
                    // Fall back to CurrentUser for backwards compatibility
                    byte[] decryptedBytes = ProtectedData.Unprotect(encryptedPassword, null, DataProtectionScope.CurrentUser);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
    }
}
