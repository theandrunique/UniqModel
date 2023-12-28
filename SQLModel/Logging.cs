using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLModel
{
    public class Logger
    {
        static string currentDir = Directory.GetCurrentDirectory();
        public static string logfileName = "orm.log";
        public static bool isEnabled = false;
        public static void Error(string message)
        {
            WriteLog(LogLevel.Error, message);
        }
        public static void Info(string message)
        {
            WriteLog(LogLevel.Info, message);
        }
        public static void Warning(string message)
        {
            WriteLog(LogLevel.Warning, message);
        }
        public static void Debug(string message)
        {
            WriteLog(LogLevel.Debug, message);
        }
        public static void Critical(string message)
        {
            WriteLog(LogLevel.Critical, message);
        }
        private static void WriteLog(LogLevel level, string message)
        {
            if (isEnabled)
            {
                // string logMessage = $"{DateTime.Now} [{level}] {message}";
                using (var stream = new StreamWriter(Path.Combine(currentDir, logfileName), true))
                {
                    stream.WriteLine($"{DateTime.Now} [{level}] {message}");
                }
            }
        }
        public enum LogLevel
        {
            Error,
            Info,
            Warning,
            Debug,
            Critical,
        }
    }
}
