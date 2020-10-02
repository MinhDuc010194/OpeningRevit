namespace CommonTools.OpeningClient.Support
{
    public static class Define
    {
        public const string SYNCHRONIZE_TITLE = "Opening Synchronize";
        public const string ROLL_BACK_TITLE = "Opening Roll Back";

        internal const double Epsilon = 1.0e-3;

        public const double UNIT_CONVERSION_RATIO = 304.8;
        public const string RECTANGLE_FAMILY = "Rectagle";
        public const string CYLYNDER_FAMILY = "Cylynder";

        public const string GUID_OPENNING_MEP_EXTENSIBLE_STORAGE_NEW = "18EE4EA1-A422-4735-92E6-23AFBD7D82E5";
        public const string GUID_OPENNING_STRUCTURE_EXTENSIBLE_STORAGE_NEW = "9A246984-5295-47D0-8779-636CCCA271A3";
        public const string GUID_OPENNING_ARCHITECHTURE_EXTENSIBLE_STORAGE_NEW = "3D271547-82D5-4850-BC04-478CA19C2005";
        public static string SCHEMA_NAME = "OpenningData";

        public const string CommonId = "CommonId";
        public static string ElementId = "ElementId";
        public static string DrawingId = "DrawingId";
        public static string CreateDate = "CreateDate";
        public static string UpdatedDate = "UpdatedDate";
        public static string ElementVersionStatus = "ElementVersionStatus";
        public static string UserID = "UserID";
        public static string VersionId = "VersionId";
        public static string RevitElementCommonId = "RevitElementCommonId";
        public static string Geometry = "Geometry";
        public static string Original = "Original";
        public static string Direction = "Direction";
        public static string Version = "Version";
        public static string Status = "Status";
        public static string Description = "Description";
        public static string CreateUserId = "CreateUserId";

        #region MEP

        public static string FLOOR_OPENNING_RECTANGLE_NAME_MEP = "INNO_CBO_Floor Openning";
        public static string FLOOR_OPENNING_CYLYNDER_NAME_MEP = "INNO_CBO_Floor Openning_Circle";
        public static string WALL_OPENNING_RECTANGLE_NAME_MEP = "INNO_CBO_Wall Openning";
        public static string WALL_OPENNING_CYLYNDER_NAME_MEP = "INNO_CBO_Wall Openning_Circle";
        public static string WIDTH_PARAMETER_NAME_MEP = "INNO_CBO_Width";
        public static string LENGHT_PARAMETER_NAME_MEP = "INNO_CBO_Length";
        public static string HEIGHT_PARAMETER_NAME_MEP = "INNO_CBO_Height";
        public static string RADIUS_PARAMETER_NAME_MEP = "INNO_CBO_R";
        public static string DEPTH_PARAMETER_NAME_MEP = "INNO_CBO_Depth";

        #endregion MEP

        #region Structure

        public static string OPENNING_RECTANGLE_NAME_STRUCTURE = "INNO_CBO_Wall Openning BF";
        public static string OPENNING_CYLYNDER_NAME_STRUCTURE = "INNO_CBO_Wall Openning BF_Circle";
        public static string WIDTH_PARAMETER_NAME_STRUCTURE = "Width";
        public static string LENGHT_PARAMETER_NAME_STRUCTURE = "Height";
        public static string HEIGHT_PARAMETER_NAME_STRUCTURE = "no hope";
        public static string RADIUS_PARAMETER_NAME_STRUCTURE = "INNO_CBO_R";

        #endregion Structure

        #region Architech

        public static string WINDOW_OPENNING_RECTANGLE_NAME = "INNO_CBO_Wall Openning BF";
        public static string WINDOW_OPENNING_CYLYNDER_NAME = "INNO_CBO_Wall Openning BF_Circle";
        public static string WIDTH_PARAMETER_NAME_ARCHITECHTURE = "Width";
        public static string LENGHT_PARAMETER_NAME_ARCHITECHTURE = "Height";
        public static string HEIGHT_PARAMETER_NAME_ARCHITECHTURE = "no hope";
        public static string RADIUS_PARAMETER_NAME_ARCHITECHTURE = "INNO_CBO_R";
        internal static double mmToFeet = 304.8;

        public static string MEP_SUBJECT = "MEP";
        public static string STRUCTURE_SUBJECT = "Structure";
        public static string ARCHITECHTURE_SUBJECT = "ARCHITECHTURE";

        #endregion Architech

        #region Commment

        public static string MissingFamilyInstance = "Can't find neccessary Family for this project.";
        public static string TimeOut = "Request time out.";
        public static string Ok = "Success";
        public static string DrawingNotFound = "Can't find this drawing in project";

        #endregion Commment

        //public static string BASE_URL = "http://192.168.1.138:9090/";
        //public static string BASE_URL = "http://192.168.1.6:9090/";

        public static string BASE_URL = "http://165.22.61.142:9090/";

        //public static string BASE_URL = "http://192.168.1.138:9090/";
        //public static string BASE_URL = "http://localhost:5000/";
    }
}