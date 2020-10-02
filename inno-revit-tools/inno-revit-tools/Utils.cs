using log4net;
using System;
using System.IO;
using System.Reflection;

namespace OpeningTools
{
    internal static class Utils
    {
        private static ILog _logger { get; } = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetExecutingDirectoryName()
        {
            //var location = new Uri(Assembly.GetExecutingAssembly().Location);

            return @"C:\ProgramData\Autodesk\Revit\Addins\2019\OpeningCombiler";//new FileInfo(location.AbsolutePath).Directory.FullName;
        }
    }
}