using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Azure.Storage.Blobs;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace DatabaseBackupService.ConfigUI.NetFx
{
    public partial class MainForm : Form
    {
        private readonly RegistryConfigManager _configManager;
        private BackupConfig _currentConfig;

        public MainForm()
        {
            InitializeComponent();
            _configManager = new RegistryConfigManager();
            _currentConfig = new BackupConfig();
        }

        private void RepositionPanels()
        {
            int gap = 10;
            int y = gap;
            var panels = panelMain.Controls.OfType<CollapsiblePanel>().OrderBy(p => p.Top).ToList();
            foreach (var panel in panels)
            {
                panel.Location = new System.Drawing.Point(panel.Location.X, y);
                y += panel.Height + gap;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _currentConfig = _configManager.LoadConfig();
            LoadConfigToUI(_currentConfig);
        }

        private void LoadConfigToUI(BackupConfig config)
        {
            comboBoxDatabaseType.SelectedItem = config.DatabaseType;
            textBoxServerName.Text = config.ServerName;
            textBoxPort.Text = config.Port;
            textBoxDatabaseName.Text = config.DatabaseName;
            textBoxUserName.Text = config.UserName;
            textBoxPassword.Text = config.Password;
            textBoxSchedule.Text = config.BackupSchedule;
            textBoxBackupPath.Text = config.BackupFolderPath;
            checkBoxEnableAzure.Checked = config.EnableAzureBackup;

            checkBoxUseTimeWindow.Checked = config.UseTimeWindow;
            textBoxBackupStartTime.Text = config.BackupStartTime;
            textBoxBackupEndTime.Text = config.BackupEndTime;
            textBoxBackupInterval.Text = config.BackupInterval;

            if (config.UseAzureSasToken)
            {
                radioButtonSasToken.Checked = true;
                textBoxAzureStorageAccount.Text = config.AzureStorageAccountName;
                textBoxAzureSasToken.Text = config.AzureSasToken;
            }
            else
            {
                radioButtonConnectionString.Checked = true;
                textBoxAzureConnectionString.Text = config.AzureStorageConnectionString;
            }

            textBoxAzureContainerName.Text = config.AzureContainerName;
            textBoxAzureBlobPrefix.Text = config.AzureBlobPrefix;

            textBoxEmailSender.Text = config.EmailSenderAddress;
            textBoxEmailPassword.Text = config.EmailSenderPassword;
            textBoxEmailRecipient.Text = config.EmailRecipientAddress;

            numericUpDownRetentionDays.Value = config.BackupRetentionDays;
            numericUpDownMinBackupFiles.Value = config.MinimumBackupFiles;

            UpdatePortBasedOnDatabaseType();
            UpdateAzureControlsState();
            UpdateTimeWindowControlsState();
        }

        private void ComboBoxDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePortBasedOnDatabaseType();
        }

        private void UpdatePortBasedOnDatabaseType()
        {
            if (string.IsNullOrWhiteSpace(textBoxPort.Text) || 
                textBoxPort.Text == "1433" || 
                textBoxPort.Text == "3306")
            {
                if (comboBoxDatabaseType.SelectedItem?.ToString() == "mssql")
                {
                    textBoxPort.Text = "1433";
                }
                else if (comboBoxDatabaseType.SelectedItem?.ToString() == "mysql")
                {
                    textBoxPort.Text = "3306";
                }
            }
        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxBackupPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            _currentConfig.DatabaseType = comboBoxDatabaseType.SelectedItem?.ToString() ?? "mssql";
            _currentConfig.ServerName = textBoxServerName.Text.Trim();
            _currentConfig.Port = textBoxPort.Text.Trim();
            _currentConfig.DatabaseName = textBoxDatabaseName.Text.Trim();
            _currentConfig.UserName = textBoxUserName.Text.Trim();
            _currentConfig.Password = textBoxPassword.Text;
            _currentConfig.BackupSchedule = textBoxSchedule.Text.Trim();
            _currentConfig.BackupFolderPath = textBoxBackupPath.Text.Trim();
            _currentConfig.EnableAzureBackup = checkBoxEnableAzure.Checked;
            _currentConfig.UseAzureSasToken = radioButtonSasToken.Checked;

            _currentConfig.UseTimeWindow = checkBoxUseTimeWindow.Checked;
            _currentConfig.BackupStartTime = textBoxBackupStartTime.Text.Trim();
            _currentConfig.BackupEndTime = textBoxBackupEndTime.Text.Trim();
            _currentConfig.BackupInterval = textBoxBackupInterval.Text.Trim();

            if (radioButtonSasToken.Checked)
            {
                _currentConfig.AzureStorageAccountName = textBoxAzureStorageAccount.Text.Trim();
                _currentConfig.AzureSasToken = textBoxAzureSasToken.Text.Trim();
                _currentConfig.AzureStorageConnectionString = "";
            }
            else
            {
                _currentConfig.AzureStorageConnectionString = textBoxAzureConnectionString.Text.Trim();
                _currentConfig.AzureStorageAccountName = "";
                _currentConfig.AzureSasToken = "";
            }

            _currentConfig.AzureContainerName = textBoxAzureContainerName.Text.Trim();
            _currentConfig.AzureBlobPrefix = textBoxAzureBlobPrefix.Text.Trim();

            _currentConfig.EmailSenderAddress = textBoxEmailSender.Text.Trim();
            _currentConfig.EmailSenderPassword = textBoxEmailPassword.Text;
            _currentConfig.EmailRecipientAddress = textBoxEmailRecipient.Text.Trim();

            _currentConfig.BackupRetentionDays = (int)numericUpDownRetentionDays.Value;
            _currentConfig.MinimumBackupFiles = (int)numericUpDownMinBackupFiles.Value;

            _configManager.SaveConfig(_currentConfig);
        }

        private void CheckBoxEnableAzure_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAzureControlsState();
        }

        private void RadioButtonAuthMethod_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAzureControlsState();
        }

        private void CheckBoxUseTimeWindow_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTimeWindowControlsState();
        }

        private void UpdateTimeWindowControlsState()
        {
            bool enabled = checkBoxUseTimeWindow.Checked;

            textBoxSchedule.Enabled = !enabled;
            labelSchedule.Enabled = !enabled;

            textBoxBackupStartTime.Enabled = enabled;
            labelBackupStartTime.Enabled = enabled;
            textBoxBackupEndTime.Enabled = enabled;
            labelBackupEndTime.Enabled = enabled;
            textBoxBackupInterval.Enabled = enabled;
            labelBackupInterval.Enabled = enabled;
        }

        private void UpdateAzureControlsState()
        {
            bool enabled = checkBoxEnableAzure.Checked;
            bool useSasToken = radioButtonSasToken.Checked;

            radioButtonConnectionString.Enabled = enabled;
            radioButtonSasToken.Enabled = enabled;

            textBoxAzureConnectionString.Enabled = enabled && !useSasToken;
            textBoxAzureConnectionString.Visible = !useSasToken;
            labelAzureConnectionString.Visible = !useSasToken;

            textBoxAzureStorageAccount.Enabled = enabled && useSasToken;
            textBoxAzureStorageAccount.Visible = useSasToken;
            labelAzureStorageAccount.Visible = useSasToken;

            textBoxAzureSasToken.Enabled = enabled && useSasToken;
            textBoxAzureSasToken.Visible = useSasToken;
            labelAzureSasToken.Visible = useSasToken;

            textBoxAzureContainerName.Enabled = enabled;
            textBoxAzureBlobPrefix.Enabled = enabled;
            buttonTestAzure.Enabled = enabled;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async void ButtonTestAzure_Click(object sender, EventArgs e)
        {
            if (radioButtonSasToken.Checked)
            {
                if (string.IsNullOrWhiteSpace(textBoxAzureStorageAccount.Text))
                {
                    MessageBox.Show("Please enter Azure Storage account name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxAzureStorageAccount.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(textBoxAzureSasToken.Text))
                {
                    MessageBox.Show("Please enter SAS token.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxAzureSasToken.Focus();
                    return;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(textBoxAzureConnectionString.Text))
                {
                    MessageBox.Show("Please enter Azure Storage connection string.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxAzureConnectionString.Focus();
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(textBoxAzureContainerName.Text))
            {
                MessageBox.Show("Please enter Azure container name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxAzureContainerName.Focus();
                return;
            }

            Cursor = Cursors.WaitCursor;

            try
            {
                BlobContainerClient containerClient;

                if (radioButtonSasToken.Checked)
                {
                    string accountName = textBoxAzureStorageAccount.Text.Trim();
                    string sasToken = textBoxAzureSasToken.Text.Trim();
                    string containerName = textBoxAzureContainerName.Text.Trim();

                    if (!sasToken.StartsWith("?"))
                    {
                        sasToken = "?" + sasToken;
                    }

                    var blobUri = new Uri($"https://{accountName}.blob.core.windows.net/{containerName}{sasToken}");
                    containerClient = new BlobContainerClient(blobUri);

                    // For SAS tokens, directly test write permission without checking existence
                    // ExistsAsync() requires List permission which might not be in minimal SAS tokens
                    string blobPrefix = textBoxAzureBlobPrefix.Text.Trim();
                    string testBlobName = string.IsNullOrEmpty(blobPrefix)
                        ? $"_test_{Guid.NewGuid()}.tmp"
                        : $"{blobPrefix.TrimEnd('/')}/_test_{Guid.NewGuid()}.tmp";
                    var testBlobClient = containerClient.GetBlobClient(testBlobName);

                    try
                    {
                        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("test")))
                        {
                            await testBlobClient.UploadAsync(stream, overwrite: true);
                        }

                        // Try to delete test blob (optional - won't fail if no delete permission)
                        try
                        {
                            await testBlobClient.DeleteIfExistsAsync();
                        }
                        catch
                        {
                            // Ignore delete errors - SAS token might not have delete permission
                        }

                        MessageBox.Show(
                            "Azure connection successful!\n\n" +
                            "✓ Container is accessible\n" +
                            "✓ Write permission confirmed\n" +
                            (string.IsNullOrEmpty(blobPrefix) ? "" : $"✓ Blob prefix '{blobPrefix}' tested\n") +
                            "✓ Ready for backups",
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (Azure.RequestFailedException ex) when (ex.Status == 404)
                    {
                        MessageBox.Show(
                            "Container does not exist!\n\n" +
                            "Please create the container in Azure Portal first:\n\n" +
                            "1. Go to portal.azure.com\n" +
                            "2. Navigate to your Storage Account\n" +
                            $"3. Go to Containers\n" +
                            "4. Click '+ Container'\n" +
                            $"5. Name it exactly: {containerName}\n\n" +
                            "Then regenerate SAS token for this container.",
                            "Container Not Found",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                    catch (Azure.RequestFailedException ex) when (ex.Status == 403)
                    {
                        MessageBox.Show(
                            "Access Denied!\n\n" +
                            "Your SAS token doesn't have the required permissions.\n\n" +
                            "When generating the SAS token, ensure you select:\n" +
                            "☑ Write (w)\n" +
                            "☑ Create (c)\n\n" +
                            "Also verify:\n" +
                            $"• Storage Account: {accountName}\n" +
                            $"• Container: {containerName}\n" +
                            "• Token is not expired\n" +
                            "• Token is for THIS specific container",
                            "Insufficient Permissions",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
                    catch 
                    {
                        throw; // Re-throw to be caught by outer catch block
                    }
                }
                else
                {
                    var blobServiceClient = new BlobServiceClient(textBoxAzureConnectionString.Text.Trim());
                    containerClient = blobServiceClient.GetBlobContainerClient(textBoxAzureContainerName.Text.Trim());

                    // Connection strings have full permissions, can create container
                    await containerClient.CreateIfNotExistsAsync();

                    // Test write with blob prefix if specified
                    string blobPrefix = textBoxAzureBlobPrefix.Text.Trim();
                    string testBlobName = string.IsNullOrEmpty(blobPrefix)
                        ? $"_test_{Guid.NewGuid()}.tmp"
                        : $"{blobPrefix.TrimEnd('/')}/_test_{Guid.NewGuid()}.tmp";
                    var testBlobClient = containerClient.GetBlobClient(testBlobName);

                    using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("test")))
                    {
                        await testBlobClient.UploadAsync(stream, overwrite: true);
                    }

                    // Clean up test blob
                    await testBlobClient.DeleteIfExistsAsync();

                    MessageBox.Show(
                        "Azure connection successful!\n\n" +
                        "✓ Container is accessible\n" +
                        "✓ Write permission confirmed\n" +
                        (string.IsNullOrEmpty(blobPrefix) ? "" : $"✓ Blob prefix '{blobPrefix}' tested\n") +
                        "✓ Ready for backups",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Azure connection failed: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private bool ValidateInput()
        {
            if (comboBoxDatabaseType.SelectedItem == null)
            {
                MessageBox.Show("Please select a database type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBoxDatabaseType.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxServerName.Text))
            {
                MessageBox.Show("Please enter a server name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxServerName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxPort.Text))
            {
                MessageBox.Show("Please enter a port number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPort.Focus();
                return false;
            }

            if (!int.TryParse(textBoxPort.Text, out _))
            {
                MessageBox.Show("Port must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPort.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxDatabaseName.Text))
            {
                MessageBox.Show("Please enter a database name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxDatabaseName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxUserName.Text))
            {
                MessageBox.Show("Please enter a user name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxUserName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxPassword.Text))
            {
                MessageBox.Show("Please enter a password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPassword.Focus();
                return false;
            }

            if (checkBoxUseTimeWindow.Checked)
            {
                if (string.IsNullOrWhiteSpace(textBoxBackupStartTime.Text))
                {
                    MessageBox.Show("Please enter a backup start time (e.g., 08:00:00).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxBackupStartTime.Focus();
                    return false;
                }

                if (!TimeSpan.TryParse(textBoxBackupStartTime.Text, out _))
                {
                    MessageBox.Show("Backup start time must be in valid time format (HH:mm:ss).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxBackupStartTime.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(textBoxBackupEndTime.Text))
                {
                    MessageBox.Show("Please enter a backup end time (e.g., 18:00:00).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxBackupEndTime.Focus();
                    return false;
                }

                if (!TimeSpan.TryParse(textBoxBackupEndTime.Text, out _))
                {
                    MessageBox.Show("Backup end time must be in valid time format (HH:mm:ss).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxBackupEndTime.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(textBoxBackupInterval.Text))
                {
                    MessageBox.Show("Please enter a backup interval (e.g., 04:00:00).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxBackupInterval.Focus();
                    return false;
                }

                if (!TimeSpan.TryParse(textBoxBackupInterval.Text, out TimeSpan interval))
                {
                    MessageBox.Show("Backup interval must be in valid time format (HH:mm:ss).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxBackupInterval.Focus();
                    return false;
                }

                if (interval.TotalMinutes < 1)
                {
                    MessageBox.Show("Backup interval must be at least 1 minute.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxBackupInterval.Focus();
                    return false;
                }

                if (TimeSpan.TryParse(textBoxBackupStartTime.Text, out TimeSpan startTime) &&
                    TimeSpan.TryParse(textBoxBackupEndTime.Text, out TimeSpan endTime))
                {
                    if (endTime <= startTime)
                    {
                        MessageBox.Show("Backup end time must be after start time.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBoxBackupEndTime.Focus();
                        return false;
                    }
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(textBoxSchedule.Text))
                {
                    MessageBox.Show("Please enter a backup schedule (e.g., 02:00:00).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxSchedule.Focus();
                    return false;
                }

                if (!TimeSpan.TryParse(textBoxSchedule.Text, out _))
                {
                    MessageBox.Show("Backup schedule must be in valid time format (HH:mm:ss).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxSchedule.Focus();
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(textBoxBackupPath.Text) && !checkBoxEnableAzure.Checked)
            {
                MessageBox.Show("Please enter a backup folder path or enable Azure Blob Storage.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxBackupPath.Focus();
                return false;
            }

            if (checkBoxEnableAzure.Checked)
            {
                if (radioButtonSasToken.Checked)
                {
                    if (string.IsNullOrWhiteSpace(textBoxAzureStorageAccount.Text))
                    {
                        MessageBox.Show("Please enter Azure Storage account name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBoxAzureStorageAccount.Focus();
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(textBoxAzureSasToken.Text))
                    {
                        MessageBox.Show("Please enter SAS token.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBoxAzureSasToken.Focus();
                        return false;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(textBoxAzureConnectionString.Text))
                    {
                        MessageBox.Show("Please enter Azure Storage connection string.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBoxAzureConnectionString.Focus();
                        return false;
                    }
                }

                if (string.IsNullOrWhiteSpace(textBoxAzureContainerName.Text))
                {
                    MessageBox.Show("Please enter Azure container name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxAzureContainerName.Focus();
                    return false;
                }
            }

            bool hasAnyEmailField = !string.IsNullOrWhiteSpace(textBoxEmailSender.Text) ||
                                     !string.IsNullOrWhiteSpace(textBoxEmailPassword.Text) ||
                                     !string.IsNullOrWhiteSpace(textBoxEmailRecipient.Text);

            if (hasAnyEmailField)
            {
                if (string.IsNullOrWhiteSpace(textBoxEmailSender.Text))
                {
                    MessageBox.Show("Please enter sender email address (Gmail).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxEmailSender.Focus();
                    return false;
                }

                if (!textBoxEmailSender.Text.Trim().EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Sender email must be a Gmail address (@gmail.com).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxEmailSender.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(textBoxEmailPassword.Text))
                {
                    MessageBox.Show("Please enter Gmail App Password.\n\nClick 'How to get App Password' link for instructions.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxEmailPassword.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(textBoxEmailRecipient.Text))
                {
                    MessageBox.Show("Please enter recipient email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxEmailRecipient.Focus();
                    return false;
                }

                if (!IsValidEmail(textBoxEmailRecipient.Text.Trim()))
                {
                    MessageBox.Show("Please enter a valid recipient email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxEmailRecipient.Focus();
                    return false;
                }
            }

            return true;
        }

        private void ButtonTestConnection_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            string databaseType = comboBoxDatabaseType.SelectedItem?.ToString() ?? "mssql";
            string serverName = textBoxServerName.Text.Trim();
            string port = textBoxPort.Text.Trim();
            string databaseName = textBoxDatabaseName.Text.Trim();
            string userName = textBoxUserName.Text.Trim();
            string password = textBoxPassword.Text;

            Cursor = Cursors.WaitCursor;

            try
            {
                if (databaseType == "mssql")
                {
                    TestMsSqlConnection(serverName, port, databaseName, userName, password);
                }
                else if (databaseType == "mysql")
                {
                    TestMySqlConnection(serverName, port, databaseName, userName, password);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void TestMsSqlConnection(string serverName, string port, string databaseName, string userName, string password)
        {
            string serverAddress = string.IsNullOrWhiteSpace(port) || port == "1433" 
                ? serverName 
                : $"{serverName},{port}";

            string connectionString = $"Data Source={serverAddress};Initial Catalog={databaseName};Persist Security Info=True;Encrypt=True;TrustServerCertificate=True;User ID={userName};Password={password};Connection Timeout=10";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                MessageBox.Show("Connection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void TestMySqlConnection(string serverName, string port, string databaseName, string userName, string password)
        {
            string connectionString = $"Server={serverName};Port={port};Database={databaseName};User ID={userName};Password={password};Connection Timeout=10;";

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MessageBox.Show("Connection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LinkLabelGmailSetup_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://myaccount.google.com/apppasswords",
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show(
                    "Unable to open browser automatically.\n\n" +
                    "Please visit: https://myaccount.google.com/apppasswords\n\n" +
                    "Steps:\n" +
                    "1. Enable 2-Factor Authentication on your Google account\n" +
                    "2. Create an App Password for 'Mail' application\n" +
                    "3. Copy the 16-character password\n" +
                    "4. Paste it in the 'App Password' field above",
                    "Gmail App Password Setup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }
}
