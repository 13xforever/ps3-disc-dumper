using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace IrdLibraryClient
{
    public static class Log
    {
        private static readonly StreamWriter FileLog;
        private static readonly Stopwatch Timer = Stopwatch.StartNew();

        static Log()
        {
            try
            {
                var path = Path.Combine("logs", DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ps3-iso-dumper", path);
                var folder = Path.GetDirectoryName(path) ?? ".";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                var filestream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                filestream.Seek(0, SeekOrigin.End);
                FileLog = new StreamWriter(filestream, new UTF8Encoding(false));
            }
            catch (Exception e)
            {
                Warn(e.Message);
            }
        }

        public static void Trace(Exception e) => LogInternal(e, e?.Message, "TRACE", ConsoleColor.DarkGray);
        public static void Debug(Exception e) => LogInternal(e, e?.Message, "DEBUG", ConsoleColor.Gray);
        public static void Info(Exception e) => LogInternal(e, e?.Message, "INFO", ConsoleColor.White);
        public static void Warn(Exception e) => LogInternal(e, e?.Message, "WARN", ConsoleColor.Yellow);
        public static void Error(Exception e) => LogInternal(e, e?.Message, "ERROR", ConsoleColor.Red);
        public static void Fatal(Exception e) => LogInternal(e, e?.Message, "FATAL", ConsoleColor.White, ConsoleColor.Red);

        public static void Trace(string message) => LogInternal(null, message, "TRACE", ConsoleColor.DarkGray);
        public static void Debug(string message) => LogInternal(null, message, "DEBUG", ConsoleColor.Gray);
        public static void Info(string message) => LogInternal(null, message, "INFO", ConsoleColor.White);
        public static void Warn(string message) => LogInternal(null, message, "WARN", ConsoleColor.Yellow);
        public static void Error(string message) => LogInternal(null, message, "ERROR", ConsoleColor.Red);
        public static void Fatal(string message) => LogInternal(null, message, "FATAL", ConsoleColor.White, ConsoleColor.Red);

        public static void Trace(Exception e, string message) => LogInternal(e, message, "TRACE", ConsoleColor.DarkGray);
        public static void Debug(Exception e, string message) => LogInternal(e, message, "DEBUG", ConsoleColor.Gray);
        public static void Info(Exception e, string message) => LogInternal(e, message, "INFO", ConsoleColor.White);
        public static void Warn(Exception e, string message) => LogInternal(e, message, "WARN", ConsoleColor.Yellow);
        public static void Error(Exception e, string message) => LogInternal(e, message, "ERROR", ConsoleColor.Red);
        public static void Fatal(Exception e, string message) => LogInternal(e, message, "FATAL", ConsoleColor.White, ConsoleColor.Red);

        private static void LogInternal(Exception e, string message, string level, ConsoleColor foregroundColor, ConsoleColor? backgroundColor = null)
        {
            try
            {
                if (message != null)
                {
                    Console.ForegroundColor = foregroundColor;
                    if (backgroundColor is ConsoleColor bg)
                        Console.BackgroundColor = bg;
                    Console.WriteLine(DateTime.Now.ToString("hh:mm:ss ") + message);
                    Console.ResetColor();
                }
                if (FileLog != null)
                {
                    if (message != null)
                        FileLog.WriteLine($"{DateTime.Now:yyyy-MM-dd hh:mm:ss}\t{(long)Timer.Elapsed.TotalMilliseconds}\t{level}\t{message}");
                    if (e != null)
                        FileLog.WriteLine(e.ToString());
                    FileLog.Flush();
                }
            }
            catch { }
        }
    }
}