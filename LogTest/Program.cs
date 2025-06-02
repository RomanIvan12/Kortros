using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository.Hierarchy;

namespace LogTest
{
    class Program
    {
        private static ILog _log = LogManager.GetLogger("LogName1");
        private static ILog _log2 = LogManager.GetLogger("LogName2");
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            RenamePath("C:\\Users\\IvannikovRV\\Documents\\Logger\\Test111.log", "Test1_appender");
            RenamePath("C:\\Users\\IvannikovRV\\Documents\\Logger\\Test222.log", "Test2_appender");


            _log.Info("GHBDTN!222");
            _log.Info("GHBDTN!222");
            _log.Info("GHBDTN!222");
            _log.Info("GHBDTN!222");
            _log.Info("GHBDTN!222");
            _log.Info("111111111");
            _log2.Info("zzzzzz!3333");
            _log2.Info("222221");
        }

        public static void RenamePath(string newValue, string Name = "BaseValue")
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            IAppender appender = hierarchy.GetAppenders().First(a => a.Name == Name);
            FileAppender? fileAppender = appender as FileAppender;
            if (fileAppender != null)
            {
                fileAppender.File = newValue;
                fileAppender.ActivateOptions();
            }
        }
    }
}