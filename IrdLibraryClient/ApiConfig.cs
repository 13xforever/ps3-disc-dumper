using System.Threading;
using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace IrdLibraryClient
{
    public static class ApiConfig
    {
        public static readonly CancellationTokenSource Cts = new CancellationTokenSource();
        public static readonly string IrdCachePath = "./ird/";

        public static readonly ILogger Log;

        static ApiConfig()
        {
            Log = GetLog();
        }

        private static ILogger GetLog()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var fileTarget = new FileTarget("logfile")
            {
                FileName = "../../../logs/bot.log",
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                KeepFileOpen = true,
                ConcurrentWrites = false,
                AutoFlush = false,
                OpenFileFlushTimeout = 1,
            };
            var asyncFileTarget = new AsyncTargetWrapper(fileTarget)
            {
                TimeToSleepBetweenBatches = 0,
                OverflowAction = AsyncTargetWrapperOverflowAction.Block,
                BatchSize = 500,
            };
            var logTarget = new ColoredConsoleTarget("logconsole");
#if DEBUG
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logTarget, "default"); // only echo messages from default logger to the console
#else
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logTarget, "default");
#endif
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, asyncFileTarget);
            LogManager.Configuration = config;
            return LogManager.GetLogger("default");
        }
    }
}