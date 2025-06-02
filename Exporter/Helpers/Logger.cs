using log4net.Config;
using log4net;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ExporterFromRs.Helpers
{
    public class Logger
    {
        private static readonly Lazy<ILog> log = new Lazy<ILog>(() =>
        {
            //Cоздание папки для логера и лога с именем пользователя и сегодняшней датой
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Logger");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string userName = Environment.UserName;
            string dateTime = DateTime.Today.ToString("yyyy_MM_dd");
            string logName = dateTime + " " + userName + " " + "Exporter.log";

            string logFilePath = Path.Combine(folderPath, logName);

            var repositoryName = Assembly.GetExecutingAssembly().GetName().Name; // Уникальное имя репозитория для каждой сборки
            var repository = LogManager.CreateRepository(repositoryName);

            var configStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ExporterFromRs.log4net.config");
            if (configStream == null)
                throw new InvalidOperationException("Не удалось найти конфигурационный файл log4net.");

            var logger = LogManager.GetLogger(repositoryName, MethodBase.GetCurrentMethod()?.DeclaringType);

            // Конфигурируем конкретный репозиторий
            XmlConfigurator.Configure(repository, configStream);

            // Проверка наличия аппендеров
            var appenders = repository.GetAppenders();
            if (appenders.Length == 0)
                throw new InvalidOperationException("Appenders не сконфигурированы. Проверьте файл log4net.config.");

            // Установка пути к файлу лога для FileAppender
            var fileAppender = appenders.OfType<log4net.Appender.FileAppender>().FirstOrDefault(appender => appender.Name == "exportFromRs");
            if (fileAppender != null)
            {
                fileAppender.File = logFilePath;
                fileAppender.ActivateOptions(); // Применяем изменения
            }
            else
            {
                throw new InvalidOperationException("Appender с именем 'exportFromRs' не найден в конфигурационном файле.");
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
            string logName = dateTime + " " + userName + " " + "Exporter.log";


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
                Logger.Log.Info("Файл лога Exporter скопирован на внешний диск");
            }
            else
                LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType).Error($"Не удалось скопировать файл лога на внешний сервер");
        }
    }
}
