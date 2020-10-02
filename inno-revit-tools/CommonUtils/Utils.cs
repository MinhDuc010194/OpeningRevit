using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using CommonUtils.Enums;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RSACryptography;

namespace CommonUtils
{
    public static class Utils
    {
        private static string patch = string.Empty;

        public static string GetExecutingDirectoryName ()
        {
            var location = new Uri ( Assembly.GetExecutingAssembly ().Location );
            return new FileInfo ( location.AbsolutePath ).Directory.FullName;
        }

        // check license status
        public static bool CheckLicenseStatus ( string revitVersion, string functionName, bool showMessage )
        {
            string localAppData = Environment.GetEnvironmentVariable ( DefineUtils.Local_App_Data );

            string clientPrivateKeyPath = string.Concat ( localAppData, DefineUtils.Client_Private_Key_Path );
            string clientPublicKeyPath = string.Concat ( localAppData, DefineUtils.Client_Public_Key_Path );

            string serverPublicKeyPath = string.Concat ( localAppData, DefineUtils.Server_Public_Key_Path );

            bool result = false;
            try
            {
                // get MAC address
                string macAddress = GetMacAddress ();

                // product code
                string productCode = ReadRegistryValue ( revitVersion, "ProductCode" );

                // client private key
                string clientPrivateKey = ReadFile ( clientPrivateKeyPath );

                // client public key
                string clientPublicKey = ReadFile ( clientPublicKeyPath );

                // server public key
                string serverPublicKey = ReadFile ( serverPublicKeyPath );

                macAddress = CryptographyHelper.Encrypt ( serverPublicKey, macAddress );
                productCode = CryptographyHelper.Encrypt ( serverPublicKey, productCode );
                functionName = CryptographyHelper.Encrypt ( serverPublicKey, functionName );
                clientPublicKey = CryptographyHelper.Encode ( clientPublicKey );

                Log log = CheckLicenseStatus ( macAddress, productCode, clientPublicKey, functionName );

                result = log.CheckedStatus == Log.Status.Failure ? false : true;

                // create a log
                CreateLog ( log );
            }
            catch ( Exception )
            {
                result = false;
            }

            if ( !result && showMessage )
                MessageBox.Show ( new Form { TopMost = true, StartPosition = FormStartPosition.CenterScreen },
                                "License check failed!", "License Failed", MessageBoxButtons.OK, MessageBoxIcon.Error );

            return result;
        }

        private static void CreateLog ( Log log )
        {
            string httpPostURL = string.Concat ( ConfigUtils.GetSetting ( DefineUtils.Lm_Url ), "/api/log" );

            JsonSerializer serializer = new JsonSerializer ();
            serializer.Converters.Add ( new JavaScriptDateTimeConverter () );
            serializer.NullValueHandling = NullValueHandling.Ignore;

            string jsonString = JsonConvert.SerializeObject ( log, Formatting.Indented );

            HttpWebRequest request = ( HttpWebRequest ) WebRequest.Create ( httpPostURL );
            request.Method = "POST";

            UTF8Encoding encoding = new UTF8Encoding ();
            byte [] byteArray = encoding.GetBytes ( jsonString );

            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json";

            using ( Stream dataStream = request.GetRequestStream () )
            {
                dataStream.Write ( byteArray, 0, byteArray.Length );
            }

            long length = 0;
            try
            {
                using ( HttpWebResponse response = ( HttpWebResponse ) request.GetResponse () )
                {
                    // got response
                    length = response.ContentLength;
                }
            }
            catch ( WebException ex )
            {
                WebResponse errorResponse = ex.Response;
                using ( Stream responseStream = errorResponse.GetResponseStream () )
                {
                    StreamReader reader = new StreamReader ( responseStream, Encoding.GetEncoding ( "utf-8" ) );
                    string errorText = reader.ReadToEnd ();
                }
            }
        }

        // check newer version
        public static bool CheckNewerVersion ( string revitVersion )
        {
            bool result = false;
            //string productVersionPath = DefineUtils.Product_Version_Path;
            try
            {
                // current product version
                //string currentVersion = ReadFile(productVersionPath);
                string currentVersion = ReadRegistryValue ( revitVersion, "ProductVersion" );
                currentVersion = GetProductVersion ( currentVersion );

                // get latest version from server
                string latestVersion = GetLatestVersion ();

                result = CompareVersion ( currentVersion, latestVersion );
            }
            catch ( Exception )
            {
                result = false;
            }

            return result;
        }

        public static void DownloadVersion ( string productVersion )
        {
            Uri uri = new Uri ( string.Concat ( ConfigUtils.GetSetting ( DefineUtils.Lm_Url ), "/api/upload/download/", productVersion ) );
            patch = string.Concat ( Path.GetTempPath (), "setup_", productVersion, ".exe" );

            if ( CheckInstallerExistence ( productVersion ) )
            {
                Download download = new Download ( uri, patch );
                download.ShowDialog ();
            }
        }

        // check newer patch
        public static bool CheckLatestPatch ( string revitVersion )
        {
            bool result = false;

            // product version
            //string productVersionPath = DefineUtils.Product_Version_Path;

            // patch
            //string patchPath = DefineUtils.Product_Patch_Path;

            // current product version
            //string currentVersion = ReadFile(productVersionPath);
            string currentVersion = ReadRegistryValue ( revitVersion, "ProductVersion" );
            currentVersion = GetProductVersion ( currentVersion );

            // current patch
            string currentPatch = ReadRegistryValue ( revitVersion, "ProductPatch" );
            if ( string.IsNullOrEmpty ( currentPatch ) )
            {
                currentPatch = "-1";
            }

            // get latest patch from server
            string latestPatch = GetLatestPatch ( currentVersion );

            int p1 = -1;
            int p2 = -1;

            int.TryParse ( currentPatch, out p1 );
            int.TryParse ( latestPatch, out p2 );

            if ( p1 != p2 )
            {
                result = true;
            }

            return result;
        }

        public static void UpdatePatch ( string revitVersion )
        {
            // product version
            //string productVersionPath = DefineUtils.Product_Version_Path;

            // current product version
            //string currentVersion = ReadFile(productVersionPath);

            string currentVersion = ReadRegistryValue ( revitVersion, "ProductVersion" );
            currentVersion = GetProductVersion ( currentVersion );

            // get latest patch from server
            string latestPatch = GetLatestPatch ( currentVersion );

            // download latest patch
            DownloadPatch ( currentVersion );

            //SaveFile(DefineUtils.Product_Patch_Path, latestPatch);
        }

        private static void DownloadPatch ( string productVersion )
        {
            Uri uri = new Uri ( string.Concat ( ConfigUtils.GetSetting ( DefineUtils.Lm_Url ), "/File/DownloadPatch?productversion=", productVersion ) );
            patch = string.Concat ( Path.GetTempPath (), "patch.exe" );

            if ( CheckPatchExistence () )
            {
                Download download = new Download ( uri, patch );
                download.ShowDialog ();
            }
        }

        public static string GetProductVersion ( string version4Digit )
        {
            int lastIndex = version4Digit.LastIndexOf ( '.' );
            return version4Digit.Substring ( 0, lastIndex );
        }

        private static bool CompareVersion ( string currentVersion, string latestVersion )
        {
            Version version1 = new Version ( currentVersion );
            Version version2 = new Version ( latestVersion );

            return version1.CompareTo ( version2 ) == 0 ? false : true;
        }

        public static string GetLatestVersion ()
        {
            string result = string.Empty;

            //string httpGetURL = ReadXMLConfig();
            string httpGetURL = ConfigUtils.GetSetting ( DefineUtils.Lm_Url );
            if ( !string.IsNullOrEmpty ( httpGetURL ) )
            {
                httpGetURL = string.Concat ( httpGetURL, "/api/productversion/getlatestversion" );

                HttpWebRequest webRequest = ( HttpWebRequest ) WebRequest.Create ( httpGetURL );
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";

                HttpWebResponse webResponse = ( HttpWebResponse ) webRequest.GetResponse ();
                Encoding enc = Encoding.GetEncoding ( "utf-8" );
                StreamReader loResponseStream = new StreamReader ( webResponse.GetResponseStream (), enc );
                result = loResponseStream.ReadToEnd ();

                loResponseStream.Close ();
                webResponse.Close ();
            }

            return result;
        }

        public static string GetLatestPatch ( string productVersion )
        {
            string result = string.Empty;

            string httpGetURL = ConfigUtils.GetSetting ( DefineUtils.Lm_Url );
            if ( !string.IsNullOrEmpty ( httpGetURL ) )
            {
                httpGetURL = string.Concat ( httpGetURL, "/api/productversion/getlatestpatch" );
                httpGetURL = string.Concat ( httpGetURL, "/", productVersion );

                HttpWebRequest webRequest = ( HttpWebRequest ) WebRequest.Create ( httpGetURL );
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";

                HttpWebResponse webResponse = ( HttpWebResponse ) webRequest.GetResponse ();
                Encoding enc = Encoding.GetEncoding ( "utf-8" );
                StreamReader loResponseStream = new StreamReader ( webResponse.GetResponseStream (), enc );
                result = loResponseStream.ReadToEnd ();

                loResponseStream.Close ();
                webResponse.Close ();
            }

            return result;
        }

        public static bool CheckPatchExistence ()
        {
            bool result = false;

            try
            {
                string httpGetURL = ConfigUtils.GetSetting ( DefineUtils.Lm_Url );
                httpGetURL = string.Concat ( httpGetURL, "/api/file/IsPatchExisted" );

                HttpWebRequest webRequest = ( HttpWebRequest ) WebRequest.Create ( httpGetURL );
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";

                HttpWebResponse webResponse = ( HttpWebResponse ) webRequest.GetResponse ();
                Encoding enc = Encoding.GetEncoding ( "utf-8" );
                StreamReader loResponseStream = new StreamReader ( webResponse.GetResponseStream (), enc );
                result = bool.Parse ( loResponseStream.ReadToEnd () );

                loResponseStream.Close ();
                webResponse.Close ();
            }
            catch ( Exception )
            {
                result = false;
            }

            return result;
        }

        public static bool CheckInstallerExistence ( string productVersion )
        {
            bool result = false;

            try
            {
                string httpGetURL = ConfigUtils.GetSetting ( DefineUtils.Lm_Url );
                httpGetURL = string.Concat ( httpGetURL, "/api/upload/isinstallerexisted" );
                httpGetURL = string.Concat ( httpGetURL, "/", productVersion );

                HttpWebRequest webRequest = ( HttpWebRequest ) WebRequest.Create ( httpGetURL );
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";

                HttpWebResponse webResponse = ( HttpWebResponse ) webRequest.GetResponse ();
                Encoding enc = Encoding.GetEncoding ( "utf-8" );
                StreamReader loResponseStream = new StreamReader ( webResponse.GetResponseStream (), enc );
                result = bool.Parse ( loResponseStream.ReadToEnd () );

                loResponseStream.Close ();
                webResponse.Close ();
            }
            catch ( Exception )
            {
                result = false;
            }

            return result;
        }

        public static bool HasKeywordSpecial ( string text )
        {
            return text.Contains ( "\\" )
                 || text.Contains ( ":" )
                 || text.Contains ( "{" )
                 || text.Contains ( "}" )
                 || text.Contains ( "[" )
                 || text.Contains ( "]" )
                 || text.Contains ( "|" )
                 || text.Contains ( ";" )
                 || text.Contains ( "<" )
                 || text.Contains ( ">" )
                 || text.Contains ( "?" )
                 || text.Contains ( "`" )
                 || text.Contains ( "~" );
        }

        private static Log CheckLicenseStatus ( string macAddress, string productCode, string clientPublicKey, string functionName )
        {
            string httpGetURL = ConfigUtils.GetSetting ( DefineUtils.Lm_Url );

            if ( !string.IsNullOrEmpty ( httpGetURL ) )
            {
                httpGetURL = string.Concat ( httpGetURL, "/api/user/checklicensestatus" );
                httpGetURL = string.Concat ( httpGetURL, "/", macAddress );
                httpGetURL = string.Concat ( httpGetURL, "/", productCode );
                httpGetURL = string.Concat ( httpGetURL, "/", functionName );
                httpGetURL = string.Concat ( httpGetURL, "/?clientPublicKey=", clientPublicKey );

                HttpWebRequest webRequest = ( HttpWebRequest ) WebRequest.Create ( httpGetURL );
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";

                HttpWebResponse webResponse = ( HttpWebResponse ) webRequest.GetResponse ();
                Encoding enc = Encoding.GetEncoding ( "utf-8" );
                StreamReader loResponseStream = new StreamReader ( webResponse.GetResponseStream (), enc );
                string result = loResponseStream.ReadToEnd ();
                loResponseStream.Close ();
                webResponse.Close ();

                return JsonConvert.DeserializeObject<Log> ( result );
            }

            return new Log ();
        }

        public static string ReadFile ( string filePath )
        {
            string result = string.Empty;
            using ( StreamReader sr = new StreamReader ( filePath ) )
            {
                result = sr.ReadLine ();
            }
            return result;
        }

        public static string ReadRegistryValue ( string revitVersion, string keyName )
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey ( "Software", true );

            key.CreateSubKey ( "InnoRevitTools" );
            key = key.OpenSubKey ( "InnoRevitTools", true );

            key.CreateSubKey ( revitVersion );
            key = key.OpenSubKey ( revitVersion, true );

            return ( string ) key.GetValue ( keyName );
        }

        public static string GetMacAddress ()
        {
            //const int MIN_MAC_ADDR_LENGTH = 12;
            //string macAddress = string.Empty;
            //long maxSpeed = -1;

            //foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces()) {
            //    string tempMac = nic.GetPhysicalAddress().ToString();
            //    if (nic.Speed > maxSpeed &&
            //        !string.IsNullOrEmpty(tempMac) &&
            //        tempMac.Length >= MIN_MAC_ADDR_LENGTH) {
            //        maxSpeed = nic.Speed;
            //        macAddress = tempMac;
            //    }
            //}
            var macAddress = NetworkInterface
                            .GetAllNetworkInterfaces ()
                            .Where ( nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback )
                            .Select ( nic => nic.GetPhysicalAddress ().ToString () )
                            .FirstOrDefault ();

            return macAddress;
        }

        public static DialogResult MessageInfor ( string content, MessageBoxButtons buttons = MessageBoxButtons.OK )
        {
            if ( string.IsNullOrWhiteSpace ( content ) )
                content = DefineUtils.Mesage_NoneContent;
            return MessageBox.Show ( content, DefineUtils.Mesage_Caption, buttons, MessageBoxIcon.Information );
        }

        public static DialogResult MessageWarning ( string content, MessageBoxButtons buttons = MessageBoxButtons.OK )
        {
            if ( string.IsNullOrWhiteSpace ( content ) )
                content = DefineUtils.Mesage_NoneContent;
            return MessageBox.Show ( content, DefineUtils.Mesage_Caption, buttons, MessageBoxIcon.Warning );
        }

        public static DialogResult MessageError ( string content, MessageBoxButtons buttons = MessageBoxButtons.OK )
        {
            if ( string.IsNullOrWhiteSpace ( content ) )
                content = DefineUtils.Mesage_NoneContent;
            return MessageBox.Show ( content, DefineUtils.Mesage_Caption, buttons, MessageBoxIcon.Error );
        }

        public static void OpenUserManualLink ()
        {
            System.Diagnostics.Process.Start ( DefineUtils.USER_MANUAL_LINK );
        }

        /// <summary>
        /// Init an image control with a given path to an actual image
        /// </summary>
        /// <returns></returns>
        public static BitmapImage CreateHelpImageWPF ()
        {
            string path = GetHelpImagePath ();
            if ( File.Exists ( path ) )
                return new BitmapImage ( new Uri ( path, UriKind.Absolute ) );
            return null;
        }

        /// <summary>
        /// Init an picture box control with a an actual image
        /// </summary>
        /// <returns></returns>
        public static Image CreateHelpImageWinform ()
        {
            string path = GetHelpImagePath ();
            if ( File.Exists ( path ) )
                return Image.FromFile ( path );
            return null;
        }

        /// <summary>
        /// Get the path to the Help Image for opening user manual link
        /// </summary>
        /// <returns></returns>
        public static string GetHelpImagePath ()
        {
            string resourceDir = GetResourceDir ();
            string otherDir = Path.Combine ( resourceDir, DefineUtils.OTHERS_DIR );
            string path = Path.Combine ( otherDir, DefineUtils.IMAGE_HELP );
            return path;
        }

        /// <summary>
        /// Get Resource directory
        /// </summary>
        /// <returns></returns>
        public static string GetResourceDir ()
        {
            var location = new Uri ( Assembly.GetExecutingAssembly ().Location );
            string exeDir = new FileInfo ( location.AbsolutePath ).Directory.FullName;
            string resourceDir = Path.Combine ( exeDir, DefineUtils.RESOURCE_DIR );
            return resourceDir;
        }

        /// <summary>
        /// Create UI icon (logo) for WPF
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static BitmapImage CreateIconWPF ( Tools code )
        {
            string path = GetIconPath ( code );
            if ( !string.IsNullOrEmpty ( path ) && File.Exists ( path ) )
                return new BitmapImage ( new Uri ( path, UriKind.Absolute ) );
            return null;
        }

        /// <summary>
        /// Create UI icon (logo) for winform
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static Icon CreateIconWinform ( Tools code )
        {
            string path = GetIconPath ( code );
            if ( !string.IsNullOrEmpty ( path ) && File.Exists ( path ) )
                return new Icon ( path );
            return null;
        }

        /// <summary>
        /// Get the path to the UI icon
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetIconPath ( Tools code )
        {
            string imageName = GetIconName ( code );
            if ( !string.IsNullOrEmpty ( imageName ) )
            {
                string resourceDir = GetResourceDir ();
                string logDir = Path.Combine ( resourceDir, DefineUtils.LOGOS_DIR );
                string path = Path.Combine ( logDir, imageName );
                return path;
            }
            return string.Empty;
        }

        /// <summary>
        /// Get name and extension for UI icon (logo)
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetIconName ( Tools code )
        {
            switch ( code )
            {
                case Tools.BeamReinforcement:
                    return "Beam32x32.ico";

                case Tools.WallReinforcement:
                    return "Wall32x32.ico";

                case Tools.ColumnReinforcement:
                    return "Column32x32.ico";

                case Tools.BeamDetail:
                    return "Sheet_Beam32x32.ico";

                case Tools.ColumnDetail:
                    return "SheetColumn32x32.ico";

                case Tools.DuplicateSheets:
                    return "Duplicate_Sheet32x32.ico";

                case Tools.DuplicateViews:
                    return "Duplicate_View32x32.ico";

                case Tools.ExcelImport:
                    return "import.ico";

                case Tools.ExcelExport:
                    return "export.ico";

                case Tools.DIVLocation:
                    return "DIV_Location32x32.ico";

                case Tools.RoomView:
                    return "Room_View32x32.ico";

                case Tools.RoomData:
                    return "Room_Data32x32.ico";

                case Tools.DimCutting:
                    return "Cutting32x32.ico";

                case Tools.DimRetention:
                    return "Retention32x32.ico";

                case Tools.DimReloading:
                    return "Reloading32x32.ico";

                case Tools.ProjectSetUpImport:
                    return "import.ico";

                case Tools.ProjectSetUpExport:
                    return "export.ico";

                case Tools.PrintManager:
                    return "Print_Manager32x32.ico";

                case Tools.PrintSelection:
                    return "Print_View_Sheet32x32.ico";

                case Tools.RoomNumber:
                    return "Room_Number32x32.ico";

                case Tools.FSC:
                    return "INNO_FSC32x32.ico";

                case Tools.ScheduleImport:
                    return "import.ico";

                case Tools.ScheduleExport:
                    return "export.ico";

                case Tools.DoorOpening:
                    return "Door_Opening32x32.ico";

                case Tools.BeamIntersection:
                    return "Beam_Intrersection32x32.ico";

                case Tools.RebarLayoutGrouping:
                    return "Rebar_Layout_Grouping32x32.ico";

                case Tools.RebarShapeCutting:
                    return "Rebar_Shape_Cutting32x32.ico";

                case Tools.RebarSetCutting:
                    return "Rebar_Set_Cutting32x32.ico";

                case Tools.DoubleBeamModification:
                    return "Double_Beam_Modification32x32.ico";

                case Tools.TripleBeamModification:
                    return "Triple_Beam_Modification32x32.ico";

                case Tools.OpeningRollBack:
                    return "Combine_Opening_RolBack32x32.ico";

                case Tools.OpeningSynchronous:
                    return "Combine_Opening_Synchronous32x32.ico";

                case Tools.TransferWipe:
                    return "Transfer_Wipe32x32.ico";

                case Tools.Information:
                    return "IBIM32x32.ico";
            }
            return string.Empty;
        }
    }
}