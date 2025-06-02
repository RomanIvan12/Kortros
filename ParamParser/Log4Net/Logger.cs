using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository;
using System;
using System.IO;
using System.Reflection;

namespace ParamParser.Log4Net
{
    public sealed class Logger
    {
        private const string LOGGER_NAME = "RevitAddin";
        private const string LOGGER_NAME2 = "TemporaryFile";
        private static readonly Lazy<ILog> InstanceHolder =
            new Lazy<ILog>(InitLog);
        private static readonly Lazy<ILog> InstanceHolder2 =
            new Lazy<ILog>(InitLog2);

        public static ILog Instance => InstanceHolder.Value;
        public static ILog Instance2 => InstanceHolder2.Value;

        public static ILog InitLog()
        {
            var appender = new RollingFileAppender
            {
                AppendToFile = true,
                File = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    $@"KortrosPluginData\{LOGGER_NAME}.log")
            };

            PatternLayout patternLayout = new PatternLayout("%date{yyyy - MM - dd HH:mm:ss} [%class] [%level] [%method] - %message%newline");
            LevelMatchFilter filter = new LevelMatchFilter
            {
                LevelToMatch = Level.All
            };
            filter.ActivateOptions();

            appender.ImmediateFlush = true;
            appender.RollingStyle = RollingFileAppender.RollingMode.Size;
            appender.MaxSizeRollBackups = 10;
            appender.LockingModel = new FileAppender.MinimalLock();
            appender.ActivateOptions();
            appender.Layout = patternLayout;
            appender.AddFilter(filter);
            appender.Name = "LogRollAppender";

            ILoggerRepository repository = LoggerManager.GetRepository(Assembly.GetCallingAssembly());
            BasicConfigurator.Configure(repository, appender);

            return LogManager.GetLogger(LOGGER_NAME);
        }

        public static ILog InitLog2()
        {
            var appender = new FileAppender
            {
                AppendToFile = false,
                File = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    $"{LOGGER_NAME2}.log")
            };

            PatternLayout patternLayout = new PatternLayout("[%class] [%level] [%method] - %message%newline");
            LevelMatchFilter filter = new LevelMatchFilter
            {
                LevelToMatch = Level.All
            };
            filter.ActivateOptions();

            appender.ImmediateFlush = true;;
            appender.LockingModel = new FileAppender.MinimalLock();
            appender.ActivateOptions();
            appender.Layout = patternLayout;
            appender.AddFilter(filter);
            appender.Name = "LogFileAppender";

            ILoggerRepository repository = LoggerManager.GetRepository(Assembly.GetCallingAssembly());
            BasicConfigurator.Configure(repository, appender);

            return LogManager.GetLogger(LOGGER_NAME2);
        }
    }
}
