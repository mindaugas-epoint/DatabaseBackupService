using Microsoft.Extensions.Configuration;
using Serilog;

namespace Logger
{
    public class SeriLog : ILogger
    {
        IConfiguration Config;
        public SeriLog(IConfiguration config)
        {
            Config = config;
        }

        public void WriteLog(string logType, string logText)
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Config)
                .CreateLogger();

            switch (logType)
            {
                case "Information":
                    logger.Information(logText);
                    break;
                case "Error":
                    logger.Error(logText);
                    break;
            }

        }

    }
}