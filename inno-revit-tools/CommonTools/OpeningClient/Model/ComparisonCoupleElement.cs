using Autodesk.Revit.DB;
using CommonTools.OpeningClient.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Model
{
    public class ComparisonCoupleElement : BaseModel
    {
        public string LocalStatus { get; set; }
        public string ServerStatus { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public GeometryDetail LocalGeometry { get; set; }
        public GeometryDetail ServerGeometry { get; set; }
        public string CurrentVersionGeometryOfLocal { get; set; }
        public Action Action { get; set; }

        public ComparisonCoupleElement()
        {
        }

        public ComparisonCoupleElement(OpeningModel elementOnServer, OpeningModel elementOnLocal)
        {
            Id = elementOnServer.IdServer;
            IdRevitElement = elementOnLocal.IdLocal;
            IdDrawing = elementOnServer.IdDrawing;
            IdManager = elementOnServer.IdManager;
            LocalStatus = DefineStatus.NORMAL;
            ServerStatus = elementOnServer.Status;
            LocalGeometry = elementOnLocal.Geometry;
            ServerGeometry = elementOnServer.Geometry;
            Comment = elementOnServer.Comment;
            if (elementOnServer.Status == "Disconnect") {
                Action = Action.DISCONNECT;
            }
        }

        /// <summary>
        /// use when opening in local is deleted
        /// </summary>
        /// <param name="element"></param>
        public ComparisonCoupleElement(OpeningModel element, bool isOpeningServer = true)
        {
            if (isOpeningServer) {
                Id = element.IdServer;
                IdDrawing = element.IdDrawing;
                IdManager = element.IdManager;
                ServerGeometry = element.Geometry;
                Comment = element.Comment;
                ServerStatus = element.Status;
            }
            else {
                IdRevitElement = element.IdLocal;
                LocalGeometry = element.Geometry;
                LocalStatus = DefineStatus.NORMAL;
                ServerStatus = DefineStatus.NONE;
                Comment = element.Comment;
                Id = Guid.Empty.ToString();
                IdManager = Guid.Empty.ToString();
            }
        }

        public bool IsSameGeometry()
        {
            if (ImplementCheckNullValue() != 0) {
                return false;
            }
            return ServerGeometry.Geometry.Equals(LocalGeometry.Geometry);
        }

        public bool IsSameOrigin()
        {
            if (ImplementCheckNullValue() != 0) {
                return false;
            }
            XYZ serverOrigin = ConvertOpenningStringToObjectReVit.GetOriginal(ServerGeometry.Original);
            XYZ localOriginal = ConvertOpenningStringToObjectReVit.GetOriginal(LocalGeometry.Original);
            return Common.IsEqual(serverOrigin, localOriginal);
            //return ServerGeometry.Original.Equals(LocalGeometry.Original);
        }

        public bool IsSameDirection()
        {
            if (ImplementCheckNullValue() != 0) {
                return false;
            }
            double localAngle = ConvertOpenningStringToObjectReVit.getAngle(LocalGeometry.Direction);
            double serverAngle = ConvertOpenningStringToObjectReVit.getAngle(ServerGeometry.Direction);
            XYZ localDirection = ConvertOpenningStringToObjectReVit.getDirection(LocalGeometry.Direction);
            XYZ serverDirection = ConvertOpenningStringToObjectReVit.getDirection(ServerGeometry.Direction);
            if (Common.IsEqual(localAngle, serverAngle) == true && (Common.IsEqual(localDirection, serverDirection) == true) || Common.IsEqual(localDirection.Negate(), serverDirection)) {
                return true;
            }
            else {
                return false;
            }
        }

        public bool IsSameShapeAndLocation()
        {
            return IsSameOrigin() && IsSameDirection() && IsSameGeometry();
        }

        public bool IsSameTypeGeometry()
        {
            JObject jObjectLocal = JObject.Parse(LocalGeometry.Geometry);
            string geometryNameLocal = (string)(JValue)jObjectLocal.SelectToken("ShapeName").ToString();
            JObject jObjectServe = JObject.Parse(ServerGeometry.Geometry);
            string geometryNameServe = (string)(JValue)jObjectServe.SelectToken("ShapeName").ToString();
            if (geometryNameLocal == geometryNameServe) {
                return true;
            }
            else {
                return false;
            }
        }

        private int ImplementCheckNullValue()
        {
            if (ServerGeometry == null) {
                return 1;
            }
            if (LocalGeometry == null) {
                return -1;
            }
            return 0;
        }
    }
}