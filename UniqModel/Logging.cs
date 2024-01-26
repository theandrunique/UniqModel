using NLog;

namespace UniqModel
{
    public class Logging
    {
        private static ILogger log;
        public static bool IsEnabled = false;
        public static void INIT(ILogger logger)
        {
            IsEnabled = true;
            log = logger;
        }
        public static void Error(string message)
        {
            if (IsEnabled)
                log.Error(message);
        }

        public static void Info(string message)
        {
            if (IsEnabled)
                log.Info(message);
        }

        public static void Warning(string message)
        {
            if (IsEnabled)
                log.Warn(message);
        }

        public static void Debug(string message)
        {
            if (IsEnabled)
                log.Debug(message);
        }

        public static void Critical(string message)
        {
            if (IsEnabled)
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
