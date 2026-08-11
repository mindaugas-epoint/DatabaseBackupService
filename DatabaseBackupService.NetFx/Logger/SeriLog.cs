using System;
using System.IO;
using Serilog;

namespace Logger
{
    public class SeriLog : ILogger
    {
        private readonly string _logPath;

        public SeriLog(string logPath)
        {
            _logPath = logPath;

            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
        }

        public void WriteLog(string logType, string logText)
        {
            var logFilePath = Path.Combine(_logPath, $"log-{DateTime.Now:yyyyMMdd}.txt");

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(logFilePath, 
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            switch (logType)
            {
                case "Information":
                    logger.Information(logText);
                    break;
                case "Error":
                    logger.Error(logText);
                    break;
                default:
                    logger.Information(logText);
                    break;
            }

            logger.Dispose();
        }
    }
}
