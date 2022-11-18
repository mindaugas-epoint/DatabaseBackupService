using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string backupFilePath = Path.Combine(backupFolderPath, "Backup", $"{databaseName}_{backupDateTime}.sql");

            using (MySqlConnection conn = new MySqlConnection(_ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportInfo.AddCreateDatabase = true;

                        mb.ExportToFile(backupFilePath);
                        conn.Close();
                    }
                }
            }
        }

        public async Task BackupDatabaseAsync(string databaseName, string backupFolderPath)
        {
            string backupDateTime = DateTime.Now.ToString("yyyyMMddTHHmmss");
            string backupFilePath = Path.Combine(backupFolderPath, "Backup", $"{databaseName}_{backupDateTime}.sql");

            using (MySqlConnection conn = new MySqlConnection(_ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportInfo.AddCreateDatabase = true;

                        await Task.Run(() => mb.ExportToFile(backupFilePath));
                        conn.Close();
                    }
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
