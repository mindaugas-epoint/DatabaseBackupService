using Azure.Storage.Blobs;
using Microsoft.Data.SqlClient;
using System.Data;
using System.IO.Compression;


namespace DatabaseBackup
{
    internal class MsSqlDbBackup : IDbBackup
    {
        private readonly string _ConnectionString;
        private readonly bool _WindowsOS;

        public MsSqlDbBackup(string connectionString, bool windowsOS)
        {
            _ConnectionString = connectionString;   
            _WindowsOS = windowsOS; 
        }

        public void BackupDatabase(string databaseName, string backupFolderPath)
        {

            string backupDateTime = DateTime.Now.ToString("yyyyMMddTHHmmss");
            string tempBackupFilePath = Path.Combine(backupFolderPath, "Backup", $"{databaseName}_{backupDateTime}.bak");
            string zipFilePath = Path.Combine(backupFolderPath, "Backup", $"{databaseName}_{backupDateTime}.zip");

            var formatMediaName = $"DatabaseToolkitBackup_{databaseName}";
            var formatName = $"Full Backup of {databaseName}";

            try
            {
                using (var connection = new SqlConnection(_ConnectionString))
                {
                    var sql = @"BACKUP DATABASE @databaseName
                        TO DISK = @localDatabasePath
                        WITH FORMAT,
                          MEDIANAME = @formatMediaName,
                            NAME = @formatName";

                    connection.Open();

                    using (var sqlCommand = new SqlCommand(sql, connection))
                    {
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandTimeout = 7200;
                        sqlCommand.Parameters.AddWithValue("@databaseName", databaseName);
                        sqlCommand.Parameters.AddWithValue("@localDatabasePath", tempBackupFilePath);
                        sqlCommand.Parameters.AddWithValue("@formatMediaName", formatMediaName);
                        sqlCommand.Parameters.AddWithValue("@formatName", formatName);

                        sqlCommand.ExecuteNonQuery();
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
            string tempBackupFilePath = Path.Combine(backupFolderPath, $"{databaseName}_{backupDateTime}.bak");
            string backupFilePath = _WindowsOS ? tempBackupFilePath : tempBackupFilePath.Replace(@"\", @"/");
            string zipFilePath = Path.Combine(backupFolderPath, $"{databaseName}_{backupDateTime}.zip");

            var formatMediaName = $"DatabaseBackup_{databaseName}";
            var formatName = $"Full Backup of {databaseName}";

            try
            {
                using (var connection = new SqlConnection(_ConnectionString))
                {
                    var sql = @"BACKUP DATABASE @databaseName
                        TO DISK = @localDatabasePath
                        WITH FORMAT,
                          MEDIANAME = @formatMediaName,
                            NAME = @formatName";

                    connection.Open();

                    using (var sqlCommand = new SqlCommand(sql, connection))
                    {
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandTimeout = 7200;
                        sqlCommand.Parameters.AddWithValue("@databaseName", databaseName);
                        sqlCommand.Parameters.AddWithValue("@localDatabasePath", backupFilePath);
                        sqlCommand.Parameters.AddWithValue("@formatMediaName", formatMediaName);
                        sqlCommand.Parameters.AddWithValue("@formatName", formatName);

                        await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(backupFilePath, Path.GetFileName(backupFilePath), CompressionLevel.Optimal);
                    }
                }
            }
            finally
            {
                if (File.Exists(backupFilePath))
                {
                    File.Delete(backupFilePath);
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
            string tempBackupPath = Path.Combine(Path.GetTempPath(), $"{databaseName}_{backupDateTime}.bak");
            string tempZipPath = Path.Combine(Path.GetTempPath(), $"{databaseName}_{backupDateTime}.zip");

            try
            {
                string backupFilePath = _WindowsOS ? tempBackupPath : tempBackupPath.Replace(@"\", @"/");

                var formatMediaName = $"DatabaseBackup_{databaseName}";
                var formatName = $"Full Backup of {databaseName}";

                using (var connection = new SqlConnection(_ConnectionString))
                {
                    var sql = @"BACKUP DATABASE @databaseName
                        TO DISK = @localDatabasePath
                        WITH FORMAT,
                          MEDIANAME = @formatMediaName,
                            NAME = @formatName";

                    connection.Open();

                    using (var sqlCommand = new SqlCommand(sql, connection))
                    {
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandTimeout = 7200;
                        sqlCommand.Parameters.AddWithValue("@databaseName", databaseName);
                        sqlCommand.Parameters.AddWithValue("@localDatabasePath", backupFilePath);
                        sqlCommand.Parameters.AddWithValue("@formatMediaName", formatMediaName);
                        sqlCommand.Parameters.AddWithValue("@formatName", formatName);

                        await sqlCommand.ExecuteNonQueryAsync();
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
