using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using DatabaseBackup;
using DatabaseBackupService.NetFx.EmailService;

namespace DatabaseBackupService.NetFx
{
    public partial class DbBackupService : ServiceBase
    {
        private IDbBackup _DbBackup;
        private Logger.ILogger _Logger;
        private BackupServiceConfig _BackupConfig;
        private IEmailService _EmailService;
        private static TimeSpan _BackupSchedule;
        private static DateTime _LastBackupDate;
        private static DateTime _LastBackupTime;
        private static List<(string databaseName, string error)> _DbBackupErrors = new List<(string databaseName, string error)>();
        private CancellationTokenSource _cancellationTokenSource;
        private Task _workerTask;

        public DbBackupService()
        {
            _LastBackupDate = DateTime.MinValue;
            _LastBackupTime = DateTime.MinValue;
        }

        public DbBackupService(IDbBackup dbContext, Logger.ILogger logger, BackupServiceConfig backupConfig, IEmailService emailService)
        {
            _DbBackup = dbContext;
            _Logger = logger;
            _BackupConfig = backupConfig;
            _EmailService = emailService;
            _LastBackupDate = DateTime.MinValue;
            _LastBackupTime = DateTime.MinValue;
        }

        protected override void OnStart(string[] args)
        {
            // Initialize service components if not already initialized (when running as Windows Service)
            if (_DbBackup == null)
            {
                try
                {
                    InitializeService();
                }
                catch (Exception ex)
                {
                    // Log to Windows Event Log since our logger might not be initialized
                    using (var eventLog = new System.Diagnostics.EventLog("Application"))
                    {
                        eventLog.Source = "Application";
                        eventLog.WriteEntry(
                            $"Database Backup Service initialization failed: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                            System.Diagnostics.EventLogEntryType.Error);
                    }

                    // Stop the service gracefully
                    Stop();
                    return;
                }
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _workerTask = Task.Run(() => ExecuteAsync(_cancellationTokenSource.Token));
        }

        private void InitializeService()
        {
            // Initialize logger first so we can log any errors
            string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _Logger = new Logger.SeriLog(logPath);

            var registryConfigReader = new RegistryConfigReader();
            _BackupConfig = registryConfigReader.LoadConfig();

            if (string.IsNullOrEmpty(_BackupConfig.ServerName))
            {
                _Logger.WriteLog("Error", "No configuration found in registry. Please run the Configuration UI to set up the backup service.");
                throw new InvalidOperationException("No configuration found in registry. Please run the Configuration UI to set up the backup service.");
            }

            string connectionString = _BackupConfig.GetConnectionString();
            bool windowsOS = Environment.OSVersion.Platform == PlatformID.Win32NT;

            if (_BackupConfig.DatabaseType == "mysql")
            {
                _DbBackup = new MySqlDbBackup(connectionString);
            }
            else
            {
                _DbBackup = new MsSqlDbBackup(connectionString, windowsOS);
            }

            _Logger.WriteLog("Information", "Service initialized successfully");
            _Logger.WriteLog("Information", $"Configuration - Backup Retention Days: {_BackupConfig.BackupRetentionDays}, Minimum Backup Files: {_BackupConfig.MinimumBackupFiles}");

            if (!string.IsNullOrEmpty(_BackupConfig.EmailSenderAddress))
            {
                var emailConfig = new EmailConfig
                {
                    SenderEmail = _BackupConfig.EmailSenderAddress,
                    SenderPassword = _BackupConfig.EmailSenderPassword,
                    RecipientEmail = _BackupConfig.EmailRecipientAddress,
                    RecipientName = _BackupConfig.EmailRecipientAddress
                };
                _EmailService = new GmailEmailService(emailConfig, _Logger);
            }
        }

        protected override void OnStop()
        {
            _cancellationTokenSource?.Cancel();
            try
            {
                _workerTask?.Wait(TimeSpan.FromSeconds(30));
            }
            catch (AggregateException)
            {
                // Task was cancelled
            }
            _cancellationTokenSource?.Dispose();
        }

        /// <summary>
        /// Starts the service for debugging (console mode)
        /// </summary>
        public void StartDebug()
        {
            OnStart(null);
        }

        /// <summary>
        /// Stops the service for debugging (console mode)
        /// </summary>
        public void StopDebug()
        {
            OnStop();
        }

        private async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _Logger.WriteLog("Information", $"Database backup service started");

            if (!TimeSpan.TryParse(_BackupConfig.BackupSchedule, out _BackupSchedule))
            {
                _Logger.WriteLog("Error", $"Invalid backup schedule format: {_BackupConfig.BackupSchedule}. Using default 02:00:00");
                _BackupSchedule = new TimeSpan(2, 0, 0);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                bool shouldBackup = false;

                if (_BackupConfig.UseTimeWindow)
                {
                    shouldBackup = ShouldPerformTimeWindowBackup();
                }
                else
                {
                    shouldBackup = ShouldPerformDailyBackup();
                }

                if (shouldBackup)
                {
                    await PerformBackup();
                }

                await Task.Delay(60000, stoppingToken);
            }
        }

        private bool ShouldPerformDailyBackup()
        {
            DateTime currentTime = DateTime.Now;
            DateTime scheduledBackupTime = DateTime.Today.Add(_BackupSchedule);
            return currentTime >= scheduledBackupTime && _LastBackupDate.Date < DateTime.Today;
        }

        private bool ShouldPerformTimeWindowBackup()
        {
            if (!TimeSpan.TryParse(_BackupConfig.BackupStartTime, out TimeSpan startTime))
            {
                _Logger.WriteLog("Error", $"Invalid BackupStartTime format: {_BackupConfig.BackupStartTime}");
                return false;
            }

            if (!TimeSpan.TryParse(_BackupConfig.BackupEndTime, out TimeSpan endTime))
            {
                _Logger.WriteLog("Error", $"Invalid BackupEndTime format: {_BackupConfig.BackupEndTime}");
                return false;
            }

            if (!TimeSpan.TryParse(_BackupConfig.BackupInterval, out TimeSpan interval))
            {
                _Logger.WriteLog("Error", $"Invalid BackupInterval format: {_BackupConfig.BackupInterval}");
                return false;
            }

            DateTime currentTime = DateTime.Now;
            TimeSpan currentTimeOfDay = currentTime.TimeOfDay;

            if (currentTimeOfDay < startTime || currentTimeOfDay > endTime)
            {
                return false;
            }

            if (_LastBackupTime == DateTime.MinValue)
            {
                return true;
            }

            TimeSpan timeSinceLastBackup = currentTime - _LastBackupTime;
            return timeSinceLastBackup >= interval;
        }

        private async Task PerformBackup()
        {
            _Logger.WriteLog("Information", $"Backup started");
            _DbBackupErrors.Clear();

            try
            {
                if (_BackupConfig.EnableAzureBackup)
                {
                    // Build Azure connection string based on configuration
                    string azureConnectionString;

                    if (_BackupConfig.UseAzureSasToken)
                    {
                        // SAS Token format: SAS|accountName|containerName|sasToken[|blobPrefix]
                        azureConnectionString = $"SAS|{_BackupConfig.AzureStorageAccountName}|{_BackupConfig.AzureContainerName}|{_BackupConfig.AzureSasToken}";
                        if (!string.IsNullOrEmpty(_BackupConfig.AzureBlobPrefix))
                        {
                            azureConnectionString += $"|{_BackupConfig.AzureBlobPrefix}";
                        }
                    }
                    else
                    {
                        // Connection String format: CS|connectionString|containerName[|blobPrefix]
                        azureConnectionString = $"CS|{_BackupConfig.AzureStorageConnectionString}|{_BackupConfig.AzureContainerName}";
                        if (!string.IsNullOrEmpty(_BackupConfig.AzureBlobPrefix))
                        {
                            azureConnectionString += $"|{_BackupConfig.AzureBlobPrefix}";
                        }
                    }

                    await _DbBackup.BackupDatabaseToAzureBlobStorageAsync(_BackupConfig.DatabaseName, azureConnectionString);

                    _LastBackupDate = DateTime.Today;
                    _LastBackupTime = DateTime.Now;

                    // Clean up old Azure backups
                    await CleanupOldAzureBackupsAsync(azureConnectionString);
                }
                else
                {
                    await _DbBackup.BackupDatabaseAsync(_BackupConfig.DatabaseName, _BackupConfig.BackupFolderPath);

                    _LastBackupDate = DateTime.Today;
                    _LastBackupTime = DateTime.Now;
                }

                // Clean up old backups after successful backup
                if (!string.IsNullOrEmpty(_BackupConfig.BackupFolderPath))
                {
                    CleanupOldLocalBackups(_BackupConfig.BackupFolderPath);
                }

                _Logger.WriteLog("Information", $"Backup completed successfully");
                LogToWindowsEventLog($"Database backup completed successfully: {_BackupConfig.DatabaseName}", System.Diagnostics.EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                _Logger.WriteLog("Error", $"Backup failed: {ex.Message}");
                LogToWindowsEventLog($"Database backup failed: {_BackupConfig.DatabaseName}. Error: {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
                _DbBackupErrors.Add((_BackupConfig.DatabaseName, ex.Message));

                if (_EmailService != null && !string.IsNullOrEmpty(_BackupConfig.EmailRecipientAddress))
                {
                    try
                    {
                        await _EmailService.SendBackupFailureNotificationAsync(_DbBackupErrors);
                    }
                    catch (Exception emailEx)
                    {
                        _Logger.WriteLog("Error", $"Failed to send email notification: {emailEx.Message}");
                    }
                }
            }
        }

        private void LogToWindowsEventLog(string message, System.Diagnostics.EventLogEntryType entryType)
        {
            try
            {
                using (var eventLog = new System.Diagnostics.EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry($"DatabaseBackupService: {message}", entryType);
                }
            }
            catch (Exception ex)
            {
                _Logger.WriteLog("Error", $"Failed to write to Windows Event Log: {ex.Message}");
            }
        }

        private void CleanupOldLocalBackups(string backupFolderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(backupFolderPath) || !System.IO.Directory.Exists(backupFolderPath))
                {
                    return;
                }

                _Logger.WriteLog("Information", $"Starting cleanup of old backup files in {backupFolderPath}");

                // Get all .bak and .zip files in the backup folder
                var bakFiles = System.IO.Directory.GetFiles(backupFolderPath, "*.bak");
                var zipFiles = System.IO.Directory.GetFiles(backupFolderPath, "*.zip");
                var allBackupFiles = bakFiles.Concat(zipFiles);

                var backupFiles = allBackupFiles
                    .Select(f => new System.IO.FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                int totalFiles = backupFiles.Count;
                _Logger.WriteLog("Information", $"Found {totalFiles} backup files (.bak and .zip)");

                // Ensure we always keep at least MinimumBackupFiles
                if (totalFiles <= _BackupConfig.MinimumBackupFiles)
                {
                    _Logger.WriteLog("Information", $"Number of backup files ({totalFiles}) is at or below minimum ({_BackupConfig.MinimumBackupFiles}). No cleanup needed.");
                    return;
                }

                // Calculate retention date
                DateTime retentionDate = DateTime.Now.AddDays(-_BackupConfig.BackupRetentionDays);
                _Logger.WriteLog("Information", $"Retention date: {retentionDate:yyyy-MM-dd HH:mm:ss} (keeping files newer than this)");

                int deletedCount = 0;
                int keptCount = 0;

                // Process files, keeping the newest MinimumBackupFiles regardless of age
                for (int i = 0; i < backupFiles.Count; i++)
                {
                    var file = backupFiles[i];

                    // Always keep the minimum number of newest files
                    if (i < _BackupConfig.MinimumBackupFiles)
                    {
                        _Logger.WriteLog("Information", $"Keeping file (within minimum count): {file.Name}, Created: {file.CreationTime:yyyy-MM-dd HH:mm:ss}");
                        keptCount++;
                        continue;
                    }

                    // Delete files older than retention period
                    if (file.CreationTime < retentionDate)
                    {
                        try
                        {
                            file.Delete();
                            _Logger.WriteLog("Information", $"Deleted old backup file: {file.Name}, Created: {file.CreationTime:yyyy-MM-dd HH:mm:ss}");
                            deletedCount++;
                        }
                        catch (Exception deleteEx)
                        {
                            _Logger.WriteLog("Error", $"Failed to delete backup file {file.Name}: {deleteEx.Message}");
                        }
                    }
                    else
                    {
                        _Logger.WriteLog("Information", $"Keeping file (within retention period): {file.Name}, Created: {file.CreationTime:yyyy-MM-dd HH:mm:ss}");
                        keptCount++;
                    }
                }

                _Logger.WriteLog("Information", $"Backup cleanup completed. Deleted: {deletedCount} files, Kept: {keptCount} files");
            }
            catch (Exception ex)
            {
                _Logger.WriteLog("Error", $"Error during backup cleanup: {ex.Message}");
            }
        }

        private async Task CleanupOldAzureBackupsAsync(string azureConnectionInfo)
        {
            try
            {
                _Logger.WriteLog("Information", "Starting cleanup of old Azure backup blobs");

                // Parse Azure connection info
                var parts = azureConnectionInfo.Split('|');
                if (parts.Length < 3)
                {
                    _Logger.WriteLog("Warning", "Invalid Azure connection info format for cleanup");
                    return;
                }

                string connectionType = parts[0]; // "SAS" or "CS"
                string blobPrefix = "";

                // Extract blob prefix based on connection type
                if (connectionType == "SAS" && parts.Length > 4)
                {
                    blobPrefix = parts[4];
                }
                else if (connectionType == "CS" && parts.Length > 3)
                {
                    blobPrefix = parts[3];
                }

                Azure.Storage.Blobs.BlobContainerClient containerClient;

                if (connectionType == "SAS")
                {
                    if (parts.Length < 4)
                    {
                        _Logger.WriteLog("Warning", "Invalid SAS format for cleanup");
                        return;
                    }

                    var accountName = parts[1];
                    var containerName = parts[2];
                    var sasToken = parts[3];

                    if (!sasToken.StartsWith("?"))
                    {
                        sasToken = "?" + sasToken;
                    }

                    var blobUri = new Uri($"https://{accountName}.blob.core.windows.net/{containerName}{sasToken}");
                    containerClient = new Azure.Storage.Blobs.BlobContainerClient(blobUri);
                }
                else if (connectionType == "CS")
                {
                    var storageConnectionString = parts[1];
                    var containerName = parts[2];

                    var blobServiceClient = new Azure.Storage.Blobs.BlobServiceClient(storageConnectionString);
                    containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                }
                else
                {
                    _Logger.WriteLog("Warning", "Invalid Azure connection type for cleanup");
                    return;
                }

                // List all blobs with the specified prefix
                var blobs = new List<(Azure.Storage.Blobs.Models.BlobItem blob, DateTimeOffset? createdOn)>();

                // Prepare prefix for blob filtering
                string prefixFilter = null;
                if (!string.IsNullOrEmpty(blobPrefix))
                {
                    prefixFilter = blobPrefix.TrimEnd('/') + "/";
                }

                _Logger.WriteLog("Information", $"Listing blobs with prefix: '{prefixFilter ?? "(none)"}'");

                // Use ToListAsync instead of await foreach for C# 7.3 compatibility
                var blobPages = containerClient.GetBlobsAsync(Azure.Storage.Blobs.Models.BlobTraits.None, Azure.Storage.Blobs.Models.BlobStates.None, prefixFilter, default).AsPages();
                var enumerator = blobPages.GetAsyncEnumerator();
                try
                {
                    while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                    {
                        var page = enumerator.Current;
                        foreach (var blobItem in page.Values)
                        {
                            if (blobItem.Name.EndsWith(".zip") || blobItem.Name.EndsWith(".bak"))
                            {
                                blobs.Add((blobItem, blobItem.Properties.CreatedOn));
                            }
                        }
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync().ConfigureAwait(false);
                }

                // Order by creation date descending (newest first)
                var orderedBlobs = blobs
                    .OrderByDescending(b => b.createdOn ?? DateTimeOffset.MinValue)
                    .ToList();

                int totalBlobs = orderedBlobs.Count;
                _Logger.WriteLog("Information", $"Found {totalBlobs} backup blobs (.zip and .bak)");

                // Ensure we always keep at least MinimumBackupFiles
                if (totalBlobs <= _BackupConfig.MinimumBackupFiles)
                {
                    _Logger.WriteLog("Information", $"Number of backup blobs ({totalBlobs}) is at or below minimum ({_BackupConfig.MinimumBackupFiles}). No cleanup needed.");
                    return;
                }

                // Calculate retention date
                DateTime retentionDate = DateTime.Now.AddDays(-_BackupConfig.BackupRetentionDays);
                _Logger.WriteLog("Information", $"Retention date: {retentionDate:yyyy-MM-dd HH:mm:ss} (keeping blobs newer than this)");

                int deletedCount = 0;
                int keptCount = 0;

                // Process blobs, keeping the newest MinimumBackupFiles regardless of age
                for (int i = 0; i < orderedBlobs.Count; i++)
                {
                    var (blob, createdOn) = orderedBlobs[i];
                    var blobCreatedOn = createdOn ?? DateTimeOffset.MinValue;

                    // Always keep the minimum number of newest blobs
                    if (i < _BackupConfig.MinimumBackupFiles)
                    {
                        _Logger.WriteLog("Information", $"Keeping blob (within minimum count): {blob.Name}, Created: {blobCreatedOn:yyyy-MM-dd HH:mm:ss}");
                        keptCount++;
                        continue;
                    }

                    // Delete blobs older than retention period
                    if (blobCreatedOn.DateTime < retentionDate)
                    {
                        try
                        {
                            var blobClient = containerClient.GetBlobClient(blob.Name);
                            await blobClient.DeleteIfExistsAsync();
                            _Logger.WriteLog("Information", $"Deleted old backup blob: {blob.Name}, Created: {blobCreatedOn:yyyy-MM-dd HH:mm:ss}");
                            deletedCount++;
                        }
                        catch (Exception deleteEx)
                        {
                            _Logger.WriteLog("Error", $"Failed to delete backup blob {blob.Name}: {deleteEx.Message}");
                        }
                    }
                    else
                    {
                        _Logger.WriteLog("Information", $"Keeping blob (within retention period): {blob.Name}, Created: {blobCreatedOn:yyyy-MM-dd HH:mm:ss}");
                        keptCount++;
                    }
                }

                _Logger.WriteLog("Information", $"Azure backup cleanup completed. Deleted: {deletedCount} blobs, Kept: {keptCount} blobs");
            }
            catch (Exception ex)
            {
                _Logger.WriteLog("Error", $"Error during Azure backup cleanup: {ex.Message}");
            }
        }
    }
}

