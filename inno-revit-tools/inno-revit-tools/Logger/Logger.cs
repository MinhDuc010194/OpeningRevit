using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository.Hierarchy;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace OpeningTools.Logger
{
    public static class Logger
    {
        private static readonly string fullPathFileConfig = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "RevitTools.xml");

        // create folder log
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static string folderPath = CreateFolder(@"Log\");

        // write text to log file
        private static void ExportLog(string fullPathFileConfig, ILog logger)
        {
            try {
                if (!fullPathFileConfig.EndsWith(".xml"))
                    fullPathFileConfig += ".xml";

                XmlConfigurator.ConfigureAndWatch(new FileInfo(fullPathFileConfig));

                var rootLogger = ((Hierarchy)logger.Logger.Repository).Root;
                var appender = rootLogger.GetAppender("RollingLogFileAppender") as FileAppender;

                appender.File = folderPath;
                appender.ActivateOptions();
            }
            catch (Exception) {
                MessageBox.Show("Could not create file log!", "Logger", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// logging info
        /// </summary>
        /// <param name="message"></param>
        /// <param name="callerMemberName"></param>
        public static void InfoLog(string message, ILog logger)
        {
            ExportLog(fullPathFileConfig, logger);
            logger.Info(logger.Logger.Name + " - " + message);
        }

        /// <summary>
        /// log error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        public static void ErrorLog(Exception exception, string message, ILog logger)
        {
            ExportLog(fullPathFileConfig, logger);
            logger.Error(logger.Logger.Name + " - " + message, exception);
        }

        /// <summary>
        /// logging lever debug
        /// </summary>
        /// <param name="message"></param>
        public static void DebugLog(string message, ILog logger)
        {
            ExportLog(fullPathFileConfig, logger);
            logger.Debug(logger.Logger.Name + " - " + message);
        }

        /// <summary>
        /// Create folder
        /// </summary>
        /// <param name="nameFolder"></param>
        /// <returns> Path of folder </returns>
        public static string CreateFolder(string nameFolder)
        {
            string pathFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), nameFolder);
            if (!Directory.Exists(pathFolder)) {
                try {
                    Directory.CreateDirectory(pathFolder);
                }
                catch (System.Exception) {
                    return pathFolder = string.Empty;
                }
            }
            return pathFolder;
        }
    }
}