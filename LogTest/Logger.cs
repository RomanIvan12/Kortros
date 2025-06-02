using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository;
using System.Reflection;
using System.Text;

namespace LogTest
{
    public class Logger
    {
        public static ILog InitLog(string loggerName)
        {

            string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Logger",
                    $"{loggerName}.log");

            ILoggerRepository repository = LoggerManager.GetRepository(Assembly.GetCallingAssembly());


            PatternLayout patternLayout = new PatternLayout("%date{yyyy-MM-dd HH:mm:ss} [%class] [%level] [%method] - %message%newline");

            var appender = new FileAppender()
            {
                AppendToFile = true,
                File = file,
                Threshold = Level.All,
                Encoding = Encoding.UTF8,
                Layout = patternLayout,
                ImmediateFlush = true,
                LockingModel = new FileAppender.MinimalLock(),
                Name = $"{loggerName}_appender"
            };

            LevelMatchFilter filter = new LevelMatchFilter
            {
                LevelToMatch = Level.All
            };
            filter.ActivateOptions();
            appender.AddFilter(filter);

            appender.ActivateOptions();

            BasicConfigurator.Configure(repository, appender);
            ILog logger = LogManager.GetLogger(typeof(Logger));
            return logger;
        }
    }
}
