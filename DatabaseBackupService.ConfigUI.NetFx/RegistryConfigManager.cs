using System;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace DatabaseBackupService.ConfigUI.NetFx
{
    public class RegistryConfigManager
    {
        private const string RegistryKeyPath = @"SOFTWARE\DatabaseBackupService";

        public BackupConfig LoadConfig()
        {
            var config = new BackupConfig();

            try
            {
                // Try LocalMachine first (for Windows Services), then fall back to CurrentUser
                RegistryKey key = Registry.LocalMachine.OpenSubKey(RegistryKeyPath);

                if (key == null)
                {
                    // Fall back to CurrentUser for backwards compatibility
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
                // Save to LocalMachine so Windows Service can access it
                using (var key = Registry.LocalMachine.CreateSubKey(RegistryKeyPath))
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

                MessageBox.Show("Configuration saved successfully to HKEY_LOCAL_MACHINE!\n\nThe Windows Service can now access this configuration.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied. Please run this application as Administrator to save configuration to HKEY_LOCAL_MACHINE.", "Administrator Rights Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}\n\nTip: Make sure you're running this application as Administrator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private byte[] EncryptPassword(string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            // Use LocalMachine scope so the Windows Service (running as LocalSystem) can decrypt
            byte[] encryptedBytes = ProtectedData.Protect(passwordBytes, null, DataProtectionScope.LocalMachine);
            return encryptedBytes;
        }

        private string DecryptPassword(byte[] encryptedPassword)
        {
            try
            {
                // Try LocalMachine first (new format)
                byte[] decryptedBytes = ProtectedData.Unprotect(encryptedPassword, null, DataProtectionScope.LocalMachine);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                try
                {
                    // Fall back to CurrentUser for backwards compatibility with old configs
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
