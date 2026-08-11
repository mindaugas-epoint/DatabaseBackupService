using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using MySql.Data.MySqlClient;

namespace DatabaseBackup
{
    public class MySqlDbBackup : IDbBackup
    {
        private static string _ConnectionString;

        public MySqlDbBackup(string connectionString)
        {
            _ConnectionString = connectionString;
        }

        public void BackupDatabase(string databaseName, string backupFolderPath)
        {
            string backupDateTime = DateTime.Now.ToString("yyyyMMddTHHmmss");
            string tempBackupFilePath = Path.Combine(backupFolderPath, "Backup", $"{databaseName}_{backupDateTime}.sql");
            string zipFilePath = Path.Combine(backupFolderPath, "Backup", $"{databaseName}_{backupDateTime}.zip");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_ConnectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            mb.ExportInfo.AddCreateDatabase = true;

                            mb.ExportToFile(tempBackupFilePath);
                            conn.Close();
                        }
                    }
                }

                using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(tempBackupFilePath, Path.GetFileName(tempBackupFilePath), CompressionLevel.Optimal);
                    }
                }
            }
            finally
            {
                if (File.Exists(tempBackupFilePath))
                {
                    File.Delete(tempBackupFilePath);
                }
            }
        }

        public async Task BackupDatabaseAsync(string databaseName, string backupFolderPath)
        {
            string backupDateTime = DateTime.Now.ToString("yyyyMMddTHHmmss");
            string tempBackupFilePath = Path.Combine(backupFolderPath, "Backup", $"{databaseName}_{backupDateTime}.sql");
            string zipFilePath = Path.Combine(backupFolderPath, "Backup", $"{databaseName}_{backupDateTime}.zip");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_ConnectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            mb.ExportInfo.AddCreateDatabase = true;

                            await Task.Run(() => mb.ExportToFile(tempBackupFilePath));
                            conn.Close();
                        }
                    }
                }

                using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(tempBackupFilePath, Path.GetFileName(tempBackupFilePath), CompressionLevel.Optimal);
                    }
                }
            }
            finally
            {
                if (File.Exists(tempBackupFilePath))
                {
                    File.Delete(tempBackupFilePath);
                }
            }
        }

        public List<(FileInfo file, string error)> VerifyBackup(string backupFilePath, string sqlQuery, string queryResult)
        {
            throw new NotImplementedException();
        }

        public async Task<List<(FileInfo file, string error)>> VerifyBackupAsync(string backupFilePath, string sqlQuery, string queryResult)
        {
            throw new NotImplementedException();
        }

        Task IDbBackup.BackupDatabaseToAzureBlobStorageAsync(string databaseName, string azureConnectionString)
        {
            return BackupDatabaseToAzureBlobStorageAsync(databaseName, azureConnectionString);
        }

        public async Task BackupDatabaseToAzureBlobStorageAsync(string databaseName, string azureConnectionString)
        {
            string backupDateTime = DateTime.Now.ToString("yyyyMMddTHHmmss");
            string tempBackupPath = Path.Combine(Path.GetTempPath(), $"{databaseName}_{backupDateTime}.sql");
            string tempZipPath = Path.Combine(Path.GetTempPath(), $"{databaseName}_{backupDateTime}.zip");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_ConnectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            mb.ExportInfo.AddCreateDatabase = true;

                            await Task.Run(() => mb.ExportToFile(tempBackupPath));
                            conn.Close();
                        }
                    }
                }

                using (FileStream zipToOpen = new FileStream(tempZipPath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(tempBackupPath, Path.GetFileName(tempBackupPath), CompressionLevel.Optimal);
                    }
                }

                var parts = azureConnectionString.Split('|');
                BlobContainerClient containerClient;
                string blobPrefix = "";

                if (parts[0] == "SAS")
                {
                    if (parts.Length < 4 || parts.Length > 5)
                    {
                        throw new ArgumentException("SAS format must be: SAS|accountName|containerName|sasToken[|blobPrefix]");
                    }

                    var accountName = parts[1];
                    var containerName = parts[2];
                    var sasToken = parts[3];
                    if (parts.Length == 5)
                    {
                        blobPrefix = parts[4];
                    }

                    if (!sasToken.StartsWith("?"))
                    {
                        sasToken = "?" + sasToken;
                    }

                    var blobUri = new Uri($"https://{accountName}.blob.core.windows.net/{containerName}{sasToken}");
                    containerClient = new BlobContainerClient(blobUri);

                    // For SAS tokens, don't check if container exists (requires List permission)
                    // Just try to upload - will fail with clear error if container doesn't exist
                }
                else if (parts[0] == "CS")
                {
                    if (parts.Length < 3 || parts.Length > 4)
                    {
                        throw new ArgumentException("Connection String format must be: CS|connectionString|containerName[|blobPrefix]");
                    }

                    var storageConnectionString = parts[1];
                    var containerName = parts[2];
                    if (parts.Length == 4)
                    {
                        blobPrefix = parts[3];
                    }

                    var blobServiceClient = new BlobServiceClient(storageConnectionString);
                    containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                    // Connection strings have full permissions, can create container
                    await containerClient.CreateIfNotExistsAsync();
                }
                else
                {
                    throw new ArgumentException("Invalid Azure connection format. Must start with 'SAS|' or 'CS|'");
                }

                // Construct blob name with prefix if provided
                string blobName = string.IsNullOrEmpty(blobPrefix) 
                    ? $"{databaseName}_{backupDateTime}.zip"
                    : $"{blobPrefix.TrimEnd('/')}/{databaseName}_{backupDateTime}.zip";

                var blobClient = containerClient.GetBlobClient(blobName);

                using (FileStream uploadFileStream = File.OpenRead(tempZipPath))
                {
                    await blobClient.UploadAsync(uploadFileStream, true);
                }
            }
            finally
            {
                if (File.Exists(tempBackupPath))
                {
                    File.Delete(tempBackupPath);
                }
                if (File.Exists(tempZipPath))
                {
                    File.Delete(tempZipPath);
                }
            }
        }
    }
}
