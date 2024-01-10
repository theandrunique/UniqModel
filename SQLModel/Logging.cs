using NLog;
using NLog.Targets;

namespace SQLModel
{
    public class Logging
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static void INIT(bool loggingInFile, string logfileName)
        {
            var config = new NLog.Config.LoggingConfiguration();
            if (loggingInFile)
            {
                var logFile = new FileTarget("logFile") { FileName = $"{logfileName}" };
                config.AddRule(LogLevel.Info, LogLevel.Fatal, logFile);
            }
            LogManager.Configuration = config;
        }
        public static void Error(string message)
        {
            log.Error(message);
        }

        public static void Info(string message)
        {
            log.Info(message);
        }

        public static void Warning(string message)
        {
            log.Warn(message);
        }

        public static void Debug(string message)
        {
            log.Debug(message);
        }

        public static void Critical(string message)
        {
            log.Fatal(message);
        }
        //private static void WriteLog(LogLevel level, string message)
        //{

        //    string logMessage = $"{DateTime.Now} [{level}] {message}";
        //    if (IsEnabled)
        //    {
        //        using (var stream = new StreamWriter(Path.Combine(currentDir, LogfileName), true))
        //        {
        //            stream.WriteLine(logMessage);
        //        }
        //    }
        //    if (IsDebugConsoleOutputEnabled)
        //    {
        //        if (ConsoleOutput != null)
        //        {
        //            ConsoleOutput.WriteLine(logMessage);
        //        }
        //    }
        //}
        //public enum LogLevel
        //{
        //    Error,
        //    Info,
        //    Warning,
        //    Debug,
        //    Critical,
        //}
    }
}
