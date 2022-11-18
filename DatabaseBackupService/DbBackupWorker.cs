using DatabaseBackup;
using SendEmail;

namespace DatabaseBackupService
{
    public class DbBackupWorker : BackgroundService
    {
        private readonly IDbBackup _DbBackup;
        private readonly Logger.ILogger _Logger;
        private readonly ISendEmail _SendEmail;
        private readonly IConfiguration _Config;
        private static DateTime _StartTime;
        private static DateTime _StopTime;
        private static List<(string databaseName, string error)> _DbBackupErrors = new List<(string databaseName, string error)>();

        private class DatabasesToBackup
        {
            public string DbName { get; set; }
            public string BackupFolderPath { get; set; }
            public int BackupFilesRetentionInDays { get; set; }
        }

        public DbBackupWorker(IDbBackup dbContext, Logger.ILogger logger, ISendEmail sendEmail, IConfiguration config)
        {
            _DbBackup = dbContext;
            _Logger = logger;
            _SendEmail = sendEmail;
            _Config = config;   
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _Logger.WriteLog("Information", $"Database backup service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime currentTime = DateTime.Now;
                List<DatabasesToBackup> databasesToBackup = _Config.GetSection("DatabasesToBackup").Get<List<DatabasesToBackup>>();

                GetUpdateTime();

                if ((currentTime > _StartTime && currentTime < _StopTime))
                {
                    foreach (var db in databasesToBackup)
                    {

                        _Logger.WriteLog("Information", $"Starting backup of {db.DbName} database");

                        try
                        {
                            await _DbBackup.BackupDatabaseAsync(db.DbName, db.BackupFolderPath);
                            _Logger.WriteLog("Information", $"Completed backup of {db.DbName} database");
                        }
                        catch (Exception fileException)
                        {
                            _Logger.WriteLog("Error", $"Failed backup of {db.DbName} database. Error: {fileException.Message}");
                            _DbBackupErrors.Add((db.DbName, fileException.Message));
                        }
                       
                        if (_DbBackupErrors.Count > 0)
                        {
                            try
                            {
                                string emailBody = GetErrorEmailBody();
                                await _SendEmail.SendMessage($"Failed backup of {db.DbName} database.", emailBody);
                            }
                            catch (Exception emailException)
                            {
                                _Logger.WriteLog("Error", $"Failed to send email message on DB Backup service error: {emailException.Message}");
                            }
                        }
                    }
                }
                await Task.Delay(60000, stoppingToken);
            }
        }

        private string GetErrorEmailBody()
        {
            string emailBody = "";

            if (_DbBackupErrors.Count > 0)
            {

                emailBody += "<p>Database backup errors</p>";
                emailBody += "<table>";
                emailBody+= "<tr><th>Database</th><th>Error</th></tr>";

                foreach(var err in _DbBackupErrors)
                {
                    emailBody += $"<tr><td>{err.databaseName}</td><td>{err.error}</td></tr>";
                }
                emailBody += "</table>";
            }

            return emailBody;
        }

        private void GetUpdateTime()
        {
            _StartTime = DateTime.Parse(_Config.GetSection("ServiceSettings")["StartTime"], System.Globalization.CultureInfo.CurrentCulture);
            _StopTime = DateTime.Parse(_Config.GetSection("ServiceSettings")["StopTime"], System.Globalization.CultureInfo.CurrentCulture);

        }
    }
}