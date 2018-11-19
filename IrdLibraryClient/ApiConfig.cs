using System.Collections.Generic;
using System.Threading;
using NLog;
using NLog.Conditions;
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
                FileName = "../../../logs/disc_dump.log",
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
            var consoleTarget = new ColoredConsoleTarget("logconsole");
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression($"level == LogLevel.{nameof(Log.Trace)}"), ConsoleOutputColor.DarkGray, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression($"level == LogLevel.{nameof(Log.Debug)}"), ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression($"level == LogLevel.{nameof(Log.Info)}"), ConsoleOutputColor.White, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression($"level == LogLevel.{nameof(Log.Warn)}"), ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression($"level == LogLevel.{nameof(Log.Error)}"), ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression($"level == LogLevel.{nameof(Log.Fatal)}"), ConsoleOutputColor.White, ConsoleOutputColor.Red));
#if DEBUG
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget, "default"); // only echo messages from default logger to the console
#else
            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget, "default");
#endif
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, asyncFileTarget);
            LogManager.Configuration = config;
            return LogManager.GetLogger("default");
        }
    }
}