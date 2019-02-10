namespace RV.SubD.Core.Utils
{
    using System;
    using System.IO;

    public static class Logger
    {
        private const string LogFileName = "errors.log";

        public static void LogLine(string line)
        {
            File.AppendAllLines(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFileName),
                new[] { $"{DateTime.Now}: {line}" });
        }
    }
}
