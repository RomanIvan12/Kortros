using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using System.Reflection;
using System.Text;

namespace LogTest
{
    public class Log4NetHelper
    {
        //public Log4NetHelper(string name)
        //{
            
        //}
        #region Field
        private static ILog _logger;
        private static FileAppender _fileAppender;
        private static string _layout = "%date{yyyy-MM-dd HH:mm:ss} [%class] [%level] [%method] - %message%newline";
        #endregion

        #region Property

        public static string Layout { set { _layout = value; } }

        private static string _name = "testLog";

        public static string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        #endregion

        #region Private

        private static PatternLayout GetPatternLayout()
        {
            var patternLayout = new PatternLayout()
            {
                ConversionPattern = _layout
            };
            patternLayout.ActivateOptions();
            return patternLayout;
        }


        private static FileAppender GetFileAppender(string name)
        {
            string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Logger",
                    $"{name}.log");
            var fileAppender = new FileAppender()
            {
                AppendToFile = true,
                File = file,
                Encoding = Encoding.UTF8,
                Layout = GetPatternLayout(),
                ImmediateFlush = true,
                Name = $"{name}_appender"
            };
            fileAppender.ActivateOptions();
            return fileAppender;
        }
        
        #endregion


        #region Public
        
        public static ILog GetLogger(string name)
        {
            RootLogger root = new RootLogger(Level.All);


            _fileAppender = GetFileAppender(_name);

            root.AddAppender(_fileAppender);

            BasicConfigurator.Configure(_fileAppender);
            _logger = LogManager.GetLogger(name);
            return _logger;
        }
        
        #endregion
    }
}
