using FluentEmail.Core;
using Microsoft.Data.SqlClient;
using System.Data;


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
            string backupFilePath = Path.Combine(backupFolderPath, "Backup", $"{databaseName}_{backupDateTime}.bak");

            var formatMediaName = $"DatabaseToolkitBackup_{databaseName}";
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

                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public async Task BackupDatabaseAsync(string databaseName, string backupFolderPath)
        {

            string backupDateTime = DateTime.Now.ToString("yyyyMMddTHHmmss");
            string tempBackupFilePath = Path.Combine(backupFolderPath, $"{databaseName}_{backupDateTime}.bak");
            string backupFilePath = _WindowsOS ? tempBackupFilePath : tempBackupFilePath.Replace(@"\", @"/");

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
        }

        public List<(FileInfo file, string error)> VerifyBackup(string backupFilePath, string sqlQuery, string queryResult)
        {
            throw new NotImplementedException();
        }

        public async Task<List<(FileInfo file, string error)>> VerifyBackupAsync(string backupFilePath, string sqlQuery, string queryResult)
        {
            throw new NotImplementedException();
        }
    }
}
