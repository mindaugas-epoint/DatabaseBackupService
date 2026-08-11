using DatabaseBackup;
using DatabaseBackupService.EmailService;

namespace DatabaseBackupService
{
    public class DbBackupWorker : BackgroundService
    {
        private readonly IDbBackup _DbBackup;
        private readonly Logger.ILogger _Logger;
        private readonly BackupServiceConfig _BackupConfig;
        private readonly IEmailService _EmailService;
        private static TimeSpan _BackupSchedule;
        private static DateTime _LastBackupDate;
        private static DateTime _LastBackupTime;
        private static List<(string databaseName, string error)> _DbBackupErrors = new List<(string databaseName, string error)>();

        public DbBackupWorker(IDbBackup dbContext, Logger.ILogger logger, BackupServiceConfig backupConfig, IEmailService emailService)
        {
            _DbBackup = dbContext;
            _Logger = logger;
            _BackupConfig = backupConfig;
            _EmailService = emailService;
            _LastBackupDate = DateTime.MinValue;
            _LastBackupTime = DateTime.MinValue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _Logger.WriteLog(Logger.LogLevel.Information, $"Database backup service started");

            if (!TimeSpan.TryParse(_BackupConfig.BackupSchedule, out _BackupSchedule))
            {
                _Logger.WriteLog(Logger.LogLevel.Error, $"Invalid backup schedule format: {_BackupConfig.BackupSchedule}. Using default 02:00:00");
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
                _Logger.WriteLog(Logger.LogLevel.Error, $"Invalid BackupStartTime format: {_BackupConfig.BackupStartTime}");
                return false;
            }

            if (!TimeSpan.TryParse(_BackupConfig.BackupEndTime, out TimeSpan endTime))
            {
                _Logger.WriteLog(Logger.LogLevel.Error, $"Invalid BackupEndTime format: {_BackupConfig.BackupEndTime}");
                return false;
            }

            if (!TimeSpan.TryParse(_BackupConfig.BackupInterval, out TimeSpan interval))
            {
                _Logger.WriteLog(Logger.LogLevel.Error, $"Invalid BackupInterval format: {_BackupConfig.BackupInterval}");
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
            if (_BackupConfig.EnableAzureBackup)
            {
                string azureConnectionInfo;

                if (_BackupConfig.UseAzureSasToken && 
                    !string.IsNullOrEmpty(_BackupConfig.AzureStorageAccountName) && 
                    !string.IsNullOrEmpty(_BackupConfig.AzureSasToken) && 
                    !string.IsNullOrEmpty(_BackupConfig.AzureContainerName))
                {
                    azureConnectionInfo = $"SAS|{_BackupConfig.AzureStorageAccountName}|{_BackupConfig.AzureContainerName}|{_BackupConfig.AzureSasToken}|{_BackupConfig.AzureBlobPrefix}";
                }
                else if (!string.IsNullOrEmpty(_BackupConfig.AzureStorageConnectionString) && 
                         !string.IsNullOrEmpty(_BackupConfig.AzureContainerName))
                {
                    azureConnectionInfo = $"CS|{_BackupConfig.AzureStorageConnectionString}|{_BackupConfig.AzureContainerName}|{_BackupConfig.AzureBlobPrefix}";
                }
                else
                {
                    _Logger.WriteLog(Logger.LogLevel.Warning, "Azure backup enabled but credentials not configured properly");
                    azureConnectionInfo = null;
                }

                if (azureConnectionInfo != null)
                {
                    _Logger.WriteLog(Logger.LogLevel.Information, $"Starting Azure backup of {_BackupConfig.DatabaseName} database");

                    try
                    {
                        await _DbBackup.BackupDatabaseToAzureBlobStorageAsync(_BackupConfig.DatabaseName, azureConnectionInfo);
                        _Logger.WriteLog(Logger.LogLevel.Information, $"Completed Azure backup of {_BackupConfig.DatabaseName} database");
                        _LastBackupDate = DateTime.Now;
                        _LastBackupTime = DateTime.Now;

                        // Clean up old Azure backups
                        await CleanupOldAzureBackupsAsync(azureConnectionInfo);
                    }
                    catch (Exception azureException)
                    {
                        _Logger.WriteLog(Logger.LogLevel.Error, $"Failed Azure backup of {_BackupConfig.DatabaseName} database. Error: {azureException.Message}");
                        _DbBackupErrors.Add((_BackupConfig.DatabaseName, azureException.Message));
                    }
                }
            }

            if (!string.IsNullOrEmpty(_BackupConfig.BackupFolderPath))
            {
                _Logger.WriteLog(Logger.LogLevel.Information, $"Starting local backup of {_BackupConfig.DatabaseName} database");

                try
                {
                    await _DbBackup.BackupDatabaseAsync(_BackupConfig.DatabaseName, _BackupConfig.BackupFolderPath);
                    _Logger.WriteLog(Logger.LogLevel.Information, $"Completed local backup of {_BackupConfig.DatabaseName} database");
                    _LastBackupDate = DateTime.Now;
                    _LastBackupTime = DateTime.Now;

                    // Clean up old local backups
                    CleanupOldLocalBackups(_BackupConfig.BackupFolderPath);
                }
                catch (Exception fileException)
                {
                    _Logger.WriteLog(Logger.LogLevel.Error, $"Failed local backup of {_BackupConfig.DatabaseName} database. Error: {fileException.Message}");
                    _DbBackupErrors.Add((_BackupConfig.DatabaseName, fileException.Message));
                }
            }

            if (_DbBackupErrors.Count > 0)
            {
                try
                {
                    await _EmailService.SendBackupFailureNotificationAsync(_DbBackupErrors);
                    _DbBackupErrors.Clear();
                }
                catch (Exception emailException)
                {
                    _Logger.WriteLog(Logger.LogLevel.Error, $"Failed to send email notification on DB Backup service error: {emailException.Message}");
                }
            }
        }

        private void CleanupOldLocalBackups(string backupFolderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(backupFolderPath) || !Directory.Exists(backupFolderPath))
                {
                    return;
                }

                _Logger.WriteLog(Logger.LogLevel.Information, $"Starting cleanup of old backup files in {backupFolderPath}");

                // Get all .bak and .zip files in the backup folder
                var bakFiles = Directory.GetFiles(backupFolderPath, "*.bak");
                var zipFiles = Directory.GetFiles(backupFolderPath, "*.zip");
                var allBackupFiles = bakFiles.Concat(zipFiles);

                var backupFiles = allBackupFiles
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                int totalFiles = backupFiles.Count;
                _Logger.WriteLog(Logger.LogLevel.Information, $"Found {totalFiles} backup files (.bak and .zip)");

                // Ensure we always keep at least MinimumBackupFiles
                if (totalFiles <= _BackupConfig.MinimumBackupFiles)
                {
                    _Logger.WriteLog(Logger.LogLevel.Information, $"Number of backup files ({totalFiles}) is at or below minimum ({_BackupConfig.MinimumBackupFiles}). No cleanup needed.");
                    return;
                }

                // Calculate retention date
                DateTime retentionDate = DateTime.Now.AddDays(-_BackupConfig.BackupRetentionDays);
                _Logger.WriteLog(Logger.LogLevel.Information, $"Retention date: {retentionDate:yyyy-MM-dd HH:mm:ss} (keeping files newer than this)");

                int deletedCount = 0;
                int keptCount = 0;

                // Process files, keeping the newest MinimumBackupFiles regardless of age
                for (int i = 0; i < backupFiles.Count; i++)
                {
                    var file = backupFiles[i];

                    // Always keep the minimum number of newest files
                    if (i < _BackupConfig.MinimumBackupFiles)
                    {
                        _Logger.WriteLog(Logger.LogLevel.Information, $"Keeping file (within minimum count): {file.Name}, Created: {file.CreationTime:yyyy-MM-dd HH:mm:ss}");
                        keptCount++;
                        continue;
                    }

                    // Delete files older than retention period
                    if (file.CreationTime < retentionDate)
                    {
                        try
                        {
                            file.Delete();
                            _Logger.WriteLog(Logger.LogLevel.Information, $"Deleted old backup file: {file.Name}, Created: {file.CreationTime:yyyy-MM-dd HH:mm:ss}");
                            deletedCount++;
                        }
                        catch (Exception deleteEx)
                        {
                            _Logger.WriteLog(Logger.LogLevel.Error, $"Failed to delete backup file {file.Name}: {deleteEx.Message}");
                        }
                    }
                    else
                    {
                        _Logger.WriteLog(Logger.LogLevel.Information, $"Keeping file (within retention period): {file.Name}, Created: {file.CreationTime:yyyy-MM-dd HH:mm:ss}");
                        keptCount++;
                    }
                }

                _Logger.WriteLog(Logger.LogLevel.Information, $"Backup cleanup completed. Deleted: {deletedCount} files, Kept: {keptCount} files");
            }
            catch (Exception ex)
            {
                _Logger.WriteLog(Logger.LogLevel.Error, $"Error during backup cleanup: {ex.Message}");
            }
        }

        private async Task CleanupOldAzureBackupsAsync(string azureConnectionInfo)
        {
            try
            {
                _Logger.WriteLog(Logger.LogLevel.Information, "Starting cleanup of old Azure backup blobs");

                // Parse Azure connection info
                var parts = azureConnectionInfo.Split('|');
                if (parts.Length < 3)
                {
                    _Logger.WriteLog(Logger.LogLevel.Warning, "Invalid Azure connection info format for cleanup");
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
                        _Logger.WriteLog(Logger.LogLevel.Warning, "Invalid SAS format for cleanup");
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
                    _Logger.WriteLog(Logger.LogLevel.Warning, "Invalid Azure connection type for cleanup");
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

                _Logger.WriteLog(Logger.LogLevel.Information, $"Listing blobs with prefix: '{prefixFilter ?? "(none)"}'");

                await foreach (var blobItem in containerClient.GetBlobsAsync(Azure.Storage.Blobs.Models.BlobTraits.None, Azure.Storage.Blobs.Models.BlobStates.None, prefixFilter, default))
                {
                    if (blobItem.Name.EndsWith(".zip") || blobItem.Name.EndsWith(".bak"))
                    {
                        blobs.Add((blobItem, blobItem.Properties.CreatedOn));
                    }
                }

                // Order by creation date descending (newest first)
                var orderedBlobs = blobs
                    .OrderByDescending(b => b.createdOn ?? DateTimeOffset.MinValue)
                    .ToList();

                int totalBlobs = orderedBlobs.Count;
                _Logger.WriteLog(Logger.LogLevel.Information, $"Found {totalBlobs} backup blobs (.zip and .bak)");

                // Ensure we always keep at least MinimumBackupFiles
                if (totalBlobs <= _BackupConfig.MinimumBackupFiles)
                {
                    _Logger.WriteLog(Logger.LogLevel.Information, $"Number of backup blobs ({totalBlobs}) is at or below minimum ({_BackupConfig.MinimumBackupFiles}). No cleanup needed.");
                    return;
                }

                // Calculate retention date
                DateTime retentionDate = DateTime.Now.AddDays(-_BackupConfig.BackupRetentionDays);
                _Logger.WriteLog(Logger.LogLevel.Information, $"Retention date: {retentionDate:yyyy-MM-dd HH:mm:ss} (keeping blobs newer than this)");

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
                        _Logger.WriteLog(Logger.LogLevel.Information, $"Keeping blob (within minimum count): {blob.Name}, Created: {blobCreatedOn:yyyy-MM-dd HH:mm:ss}");
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
                            _Logger.WriteLog(Logger.LogLevel.Information, $"Deleted old backup blob: {blob.Name}, Created: {blobCreatedOn:yyyy-MM-dd HH:mm:ss}");
                            deletedCount++;
                        }
                        catch (Exception deleteEx)
                        {
                            _Logger.WriteLog(Logger.LogLevel.Error, $"Failed to delete backup blob {blob.Name}: {deleteEx.Message}");
                        }
                    }
                    else
                    {
                        _Logger.WriteLog(Logger.LogLevel.Information, $"Keeping blob (within retention period): {blob.Name}, Created: {blobCreatedOn:yyyy-MM-dd HH:mm:ss}");
                        keptCount++;
                    }
                }

                _Logger.WriteLog(Logger.LogLevel.Information, $"Azure backup cleanup completed. Deleted: {deletedCount} blobs, Kept: {keptCount} blobs");
            }
            catch (Exception ex)
            {
                _Logger.WriteLog(Logger.LogLevel.Error, $"Error during Azure backup cleanup: {ex.Message}");
            }
        }
    }
}
