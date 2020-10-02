using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace CommonUtils
{
    public static class ConfigUtils
    {
        private static bool IsLoadedConfigFile { get; set; }

        private static Dictionary<string, string> SettingList { get; set; }

        private static Dictionary<string, string> MessageList { get; set; }

        public static bool CheckConfigFile ()
        {
            if ( !IsLoadedConfigFile )
                TaskDialog.Show ( "Error!", DefineUtils.Mesage_ErrorConfig );

            return IsLoadedConfigFile;
        }

        /// <summary>
        /// Load value from Config files
        /// </summary>
        public static void LoadFileConfig ()
        {
            string applicationPath = Path.Combine ( Path.GetDirectoryName ( Assembly.GetExecutingAssembly ().Location ) );
            SettingList = ReadXmlFile ( Path.Combine ( applicationPath, DefineUtils.File_Setting_Config ) );
            MessageList = ReadXmlFile ( Path.Combine ( applicationPath, DefineUtils.File_Message_Config ) );

            if ( SettingList != null && SettingList.Count > 0 && MessageList != null && MessageList.Count > 0 )
                IsLoadedConfigFile = true;
            else
                IsLoadedConfigFile = false;
        }

        /// <summary>
        /// retrieve message
        /// </summary>
        public static string GetMessage ( string messageId )
        {
            if ( IsLoadedConfigFile && MessageList.ContainsKey ( messageId ) )
            {
                return MessageList [messageId];
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// retrieve message
        /// </summary>
        public static string GetMessage ( string prefix, string caption, string noMessage )
        {
            return GetMessage ( prefix + caption + noMessage );
        }

        /// <summary>
        /// retrieve setting
        /// </summary>
        public static string GetSetting ( string settingId )
        {
            if ( IsLoadedConfigFile && SettingList.ContainsKey ( settingId ) )
            {
                return SettingList [settingId];
            }
            else
            {
                return string.Empty;
            }
        }

        // read config files
        private static Dictionary<string, string> ReadXmlFile ( string inputPath )
        {
            Dictionary<string, string> keyValues = new Dictionary<string, string> ();

            try
            {
                if ( !string.IsNullOrWhiteSpace ( inputPath ) && File.Exists ( inputPath ) )
                {
                    XmlDocument xmlDocument = new XmlDocument ();
                    xmlDocument.Load ( inputPath );
                    XmlElement element = xmlDocument.DocumentElement;
                    if ( element.HasChildNodes )
                    {
                        foreach ( XmlNode node in element.ChildNodes )
                        {
                            if ( node.HasChildNodes )
                            {
                                foreach ( XmlNode nodeChild in node.ChildNodes )
                                {
                                    string value = GetXmlText ( nodeChild );
                                    if ( !string.IsNullOrWhiteSpace ( nodeChild.Name ) &&
                                        !string.IsNullOrWhiteSpace ( value ) &&
                                        !keyValues.ContainsKey ( nodeChild.Name.Trim () ) )
                                    {
                                        value = value.Replace ( "\\n", "\n" );
                                        keyValues.Add ( nodeChild.Name.Trim (), value );
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch ( System.Exception )
            {
                return keyValues;
            }

            return keyValues;
        }

        /// <summary>
        /// Get the text value of an element node in xml input file
        /// </summary>
        private static string GetXmlText ( XmlNode node )
        {
            if ( node.NodeType != XmlNodeType.Element || !node.HasChildNodes )
                return string.Empty;

            return node.FirstChild.InnerText.Trim ();
        }
    }
}