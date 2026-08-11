using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public enum LogLevel
    {
        Information,
        Warning,
        Error
    }

    public interface ILogger
    {
        void WriteLog(LogLevel logLevel, string logText);
    }
}
