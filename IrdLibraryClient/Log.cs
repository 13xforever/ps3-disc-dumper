using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace IrdLibraryClient;

public static class Log
{
    private static readonly StreamWriter FileLog;
    private static readonly Stopwatch Timer = Stopwatch.StartNew();
    public static readonly string LogPath;

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
            FileLog = new(filestream, new UTF8Encoding(false));
            LogPath = path;
        }
        catch (Exception e)
        {
            Warn(e.Message);
            FileLog = new(new MemoryStream());
            LogPath = "<n/a>";
        }
    }

    public static void Trace(Exception e) => LogInternal(e, e.Message, LogLevel.TRACE, ConsoleColor.DarkGray);
    public static void Debug(Exception e) => LogInternal(e, e.Message, LogLevel.DEBUG, ConsoleColor.Gray);
    public static void Info(Exception e) => LogInternal(e, e.Message, LogLevel.INFO, ConsoleColor.White);
    public static void Warn(Exception e) => LogInternal(e, e.Message, LogLevel.WARN, ConsoleColor.Yellow);
    public static void Error(Exception e) => LogInternal(e, e.Message, LogLevel.ERROR, ConsoleColor.Red);
    public static void Fatal(Exception e) => LogInternal(e, e.Message, LogLevel.FATAL, ConsoleColor.White, ConsoleColor.Red);

    public static void Trace(string message) => LogInternal(null, message, LogLevel.TRACE, ConsoleColor.DarkGray);
    public static void Debug(string message) => LogInternal(null, message, LogLevel.DEBUG, ConsoleColor.Gray);
    public static void Info(string message) => LogInternal(null, message, LogLevel.INFO, ConsoleColor.White);
    public static void Warn(string message) => LogInternal(null, message, LogLevel.WARN, ConsoleColor.Yellow);
    public static void Error(string message) => LogInternal(null, message, LogLevel.ERROR, ConsoleColor.Red);
    public static void Fatal(string message) => LogInternal(null, message, LogLevel.FATAL, ConsoleColor.White, ConsoleColor.Red);

    public static void Trace(Exception? e, string message) => LogInternal(e, message, LogLevel.TRACE, ConsoleColor.DarkGray);
    public static void Debug(Exception? e, string message) => LogInternal(e, message, LogLevel.DEBUG, ConsoleColor.Gray);
    public static void Info(Exception? e, string message) => LogInternal(e, message, LogLevel.INFO, ConsoleColor.White);
    public static void Warn(Exception? e, string message) => LogInternal(e, message, LogLevel.WARN, ConsoleColor.Yellow);
    public static void Error(Exception? e, string message) => LogInternal(e, message, LogLevel.ERROR, ConsoleColor.Red);
    public static void Fatal(Exception? e, string message) => LogInternal(e, message, LogLevel.FATAL, ConsoleColor.White, ConsoleColor.Red);

    private static void LogInternal(Exception? e, string message, LogLevel level, ConsoleColor foregroundColor, ConsoleColor? backgroundColor = null)
    {
        try
        {
#if DEBUG
                const LogLevel minLevel = LogLevel.TRACE;
#else
            const LogLevel minLevel = LogLevel.DEBUG;
#endif
            if (level >= minLevel)
            {
                Console.ForegroundColor = foregroundColor;
                if (backgroundColor is ConsoleColor bg)
                    Console.BackgroundColor = bg;
                Console.WriteLine(DateTime.Now.ToString("hh:mm:ss ") + message);
                Console.ResetColor();
            }
            FileLog.WriteLine($"{DateTime.Now:yyyy-MM-dd hh:mm:ss}\t{(long)Timer.Elapsed.TotalMilliseconds}\t{level}\t{message}");
            if (e != null)
                FileLog.WriteLine(e.ToString());
            FileLog.Flush();
        }
        catch { }
    }

    private enum LogLevel
    {
        DISABLED,
        TRACE,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL,
    }
}