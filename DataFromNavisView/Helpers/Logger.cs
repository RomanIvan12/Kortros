using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DataFromNavisView.Helpers
{
    public class Logger
    {
        private static readonly Lazy<ILog> log = new Lazy<ILog>(() =>
        {
            // Получаем текущий тип из вызывающего класса
            var logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

            //Cоздание папки для логера и лога с именем пользователя и сегодняшней датой
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Logger");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string userName = Environment.UserName;
            string dateTime = DateTime.Today.ToString("yyyy_MM_dd");
            string logName = dateTime + " " + userName + " " + "PostgreSql.log";

            string logFilePath = Path.Combine(folderPath, logName);


            var configStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DataFromNavisView.log4net.config");
            if (configStream == null)
                throw new InvalidOperationException("Не удалось найти конфигурационный файл log4net.");

            XmlConfigurator.Configure(configStream);

            // Установка пути к файлу лога для FileAppender
            var fileAppender = LogManager.GetRepository()
                .GetAppenders()
                .OfType<log4net.Appender.FileAppender>()
                .FirstOrDefault(appender => appender.Name == "postgre");
            if (fileAppender != null)
            {
                fileAppender.File = logFilePath;
                fileAppender.ActivateOptions(); // Применяем изменения
            }
            else
            {
                throw new InvalidOperationException("Appender с именем 'postgre' не найден в конфигурационном файле.");
            }

            return logger;
        });
        public static ILog Log => log.Value;

        public static void CreateLogFolderAndCopy()
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Logger");
            string userName = Environment.UserName;
            string dateTime = DateTime.Today.ToString("yyyy_MM_dd");
            string logName = dateTime + " " + userName + " " + "PostgreSql.log";


            string sourceFilePath = Path.Combine(folderPath, logName);

            string targetFolderPath = Path.Combine(@"F:\18. BIM\BIM_DATA\06_Logs",
                Environment.UserName);

            string targetFilePath = Path.Combine(targetFolderPath, Path.GetFileName(sourceFilePath));
            try
            {
                if (!Directory.Exists(targetFolderPath))
                    Directory.CreateDirectory(targetFolderPath);
            }
            catch { }


            if (File.Exists(sourceFilePath))
            {
                File.Copy(sourceFilePath, targetFilePath, true);
                Logger.Log.Info("Файл лога postgre скопирован на внешний диск");
            }
            else
                LogManager.GetLogger("postgre").Error($"Не удалось скопировать файл лога на внешний сервер");
        }

    }
}
