namespace DatabaseBackup
{
    public interface IDbBackup
    {
        void BackupDatabase(string databaseName, string backupFolderPath);
        Task BackupDatabaseAsync(string databaseName, string backupFolderPath);
        List<(FileInfo file, string error)> VerifyBackup(string backupFilePath, string sqlQuery, string queryResult);
        Task<List<(FileInfo file, string error)>> VerifyBackupAsync(string backupFilePath, string sqlQuery, string queryResult);
    }
}
