using Microsoft.Extensions.Configuration;
using Serilog;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Logger
{
    public class SeriLog : ILogger
    {
        IConfiguration Config;
        public SeriLog(IConfiguration config)
        {
            Config = config;
        }

        public void WriteLog(LogLevel logLevel, string logText)
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Config)
                .CreateLogger();

            switch (logLevel)
            {
                case LogLevel.Information:
                    logger.Information(logText);
                    break;
                case LogLevel.Error:
                    logger.Error(logText);
                    break;
                case LogLevel.Warning:
                    logger.Warning(logText);
                    break;
            }

            // Also log to system log (Windows Event Log or syslog) for important events
            WriteToSystemLog(logLevel, logText);
        }

        private void WriteToSystemLog(LogLevel logLevel, string logText)
        {
            try
            {
                EventLogEntryType entryType = logLevel switch
                {
                    LogLevel.Error => EventLogEntryType.Error,
                    LogLevel.Warning => EventLogEntryType.Warning,
                    _ => EventLogEntryType.Information
                };

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Application";
                        eventLog.WriteEntry($"DatabaseBackupService: {logText}", entryType);
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string priority = logLevel switch
                    {
                        LogLevel.Error => "err",
                        LogLevel.Warning => "warning",
                        _ => "info"
                    };
                    System.Diagnostics.Process.Start("logger", $"-p user.{priority} -t DatabaseBackupService \"{logText}\"");
                }
            }
            catch
            {
                // Silently fail if system log is not accessible
                // We don't want to break the application if system logging fails
            }
        }

    }
}