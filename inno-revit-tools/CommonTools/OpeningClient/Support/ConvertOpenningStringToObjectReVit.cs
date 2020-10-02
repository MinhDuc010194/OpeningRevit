using Autodesk.Revit.DB;
using CommonTools.OpeningClient.JsonObject;
using MoreLinq;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace CommonTools.OpeningClient.Support
{
    internal static class ConvertOpenningStringToObjectReVit
    {
        static public double GetHeightFromGeometry(string geometry)
        {
            CommonGeometry commonGeometry = JsonConvert.DeserializeObject<CommonGeometry>(geometry);
            string heightStr = commonGeometry.Height;
            double.TryParse(heightStr, out double height);
            return height;
        }

        static public string GetShapeFromGeometry(string geometry)
        {
            CommonGeometry commonGeometry = JsonConvert.DeserializeObject<CommonGeometry>(geometry);
            string shape = commonGeometry.ShapeName;
            return shape;
        }

        static public double GetWidthFromGeometry(string geometry)
        {
            RectangleGeometry rectangleGeometry = JsonConvert.DeserializeObject<RectangleGeometry>(geometry);
            string widthStr = rectangleGeometry.Width;
            double.TryParse(widthStr, out double width);
            return width;
        }

        static public double GetLengthFromGeometry(string geometry)
        {
            RectangleGeometry rectangleGeometry = JsonConvert.DeserializeObject<RectangleGeometry>(geometry);
            string lenghtStr = rectangleGeometry.Lenght;
            double.TryParse(lenghtStr, out double lenght);
            return lenght;
        }

        static public double GetRadiusFromGeometry(string geometry)
        {
            CylynderGeometry cylynderGeometry = JsonConvert.DeserializeObject<CylynderGeometry>(geometry);
            string radiusStr = cylynderGeometry.Radius;
            double.TryParse(radiusStr, out double radius);
            return radius;
        }

        static public XYZ GetOriginal(string original)
        {
            Origin origin = JsonConvert.DeserializeObject<Origin>(original);
            string originStr = origin.Location;
            //string[] numbers = Regex.Split(originStr, @"\D+");
            string[] numbers = originStr.Split(new char[] { '/' });
            if (numbers.Length == 3) {
                double.TryParse(numbers[0], out double x);
                double.TryParse(numbers[1], out double y);
                double.TryParse(numbers[2], out double z);
                XYZ location = new XYZ(x, y, z);
                return location;
            }
            else {
                return null;
            }
        }

        static public XYZ getDirection(string directionJson)
        {
            DirectionAndAngle directionAndAngle = JsonConvert.DeserializeObject<DirectionAndAngle>(directionJson);
            string directionStr = directionAndAngle.Direction;
            //string[] numbers = Regex.Split(directionStr, @"\D+");
            string[] numbers = directionStr.Split(new char[] { '/' });
            if (numbers.Length == 3) {
                double.TryParse(numbers[0], out double x);
                double.TryParse(numbers[1], out double y);
                double.TryParse(numbers[2], out double z);
                XYZ direction = new XYZ(x, y, z);
                return direction;
            }
            else {
                return null;
            }
        }

        static public double getAngle(string directionJson)
        {
            DirectionAndAngle directionAndAngle = JsonConvert.DeserializeObject<DirectionAndAngle>(directionJson);
            string angleStr = directionAndAngle.Angle;
            double.TryParse(angleStr, out double angle);
            return angle;
        }
    }
}