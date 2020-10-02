using Autodesk.Revit.DB;
using CommonTools.OpeningClient.JsonObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Support
{
    internal class Common
    {
        /// <summary>
        /// Convert number from milimeter to foot
        /// </summary>
        /// <param name="mili"></param>
        /// <returns></returns>
        public static double MilimeterToFeet(double mili)
        {
            if (double.IsNaN(mili))
                return double.NaN;

            return mili / Define.UNIT_CONVERSION_RATIO;
        }

        /// <summary>
        /// Convert a point or a vector from milimeter to feet
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static XYZ MilimeterToFeet(XYZ point)
        {
            if (point == null)
                return null;

            double x = MilimeterToFeet(point.X);
            double y = MilimeterToFeet(point.Y);
            double z = MilimeterToFeet(point.Z);
            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Convert number from foot to milimeter
        /// </summary>
        /// <param name="feet"></param>
        /// <returns></returns>
        public static double FeetToMilimeter(double feet)
        {
            if (double.IsNaN(feet))
                return double.NaN;

            return feet * Define.UNIT_CONVERSION_RATIO;
        }

        /// <summary>
        /// Convert a point or a vector from feet to milimeter
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static XYZ FeetToMilimeter(XYZ point)
        {
            if (point == null)
                return null;

            double x = FeetToMilimeter(point.X);
            double y = FeetToMilimeter(point.Y);
            double z = FeetToMilimeter(point.Z);
            return new XYZ(x, y, z);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static Status StringToStatus(string status)
        {
            if (status == "NORMAL")
                return Status.NORMAL;
            else if (status == "DELETED")
                return Status.DELETED;
            else if (status == "PENDING_CREATE")
                return Status.PENDING_CREATE;
            else if (status == "PENDING_DELETE")
                return Status.PENDING_DELETE;
            else
                return Status.NONE;
        }

        /// <summary>
        /// Compare 2 Revit point/ vector with a pre-define double epsilon
        /// </summary>
        internal static bool IsEqual(XYZ first, XYZ second)
        {
            return first != null && second != null && IsEqual(first.X, second.X) && IsEqual(first.Y, second.Y) && IsEqual(first.Z, second.Z);
        }

        /// <summary>
        /// Compare 2 double values with a pre-define epsilon
        /// </summary>
        internal static bool IsEqual(double first, double second)
        {
            return Math.Abs(first - second) <= Define.Epsilon ? true : false;
        }

        /// <summary>
        /// Return true if vectorToCheck is on plan created by right and front vector
        /// </summary>
        /// <param name="vectorToCheck"></param>
        /// <param name="right"></param>
        /// <param name="front"></param>
        /// <returns></returns>
        internal static bool IsParallelVectorPlan(XYZ vectorToCheck, XYZ right, XYZ front)
        {
            double angle = vectorToCheck.AngleOnPlaneTo(right, front);
            if (IsEqual(angle, 0)) {
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Check two vector perpendicular
        /// </summary>
        internal static bool IsPerpendicular(XYZ vector1, XYZ vector2)
        {
            return vector1 != null && vector2 != null && Math.Abs(vector1.AngleTo(vector2) - Math.PI / 2) < Define.Epsilon;
        }

        /// <summary>
        /// Check two vector parallel
        /// </summary>
        internal static bool IsParallel(XYZ vector1, XYZ vector2)
        {
            return vector1 != null && vector2 != null && (Math.Abs(vector1.AngleTo(vector2) - Math.PI) < Define.Epsilon || Math.Abs(vector1.AngleTo(vector2)) < Define.Epsilon);
        }

        internal static void SetDoubleValueToParam(Element element, string paramName, double value)
        {
            Parameter widthParam = element.LookupParameter(paramName);
            if (widthParam != null) {
                widthParam.Set(value);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        internal static double GetLengthFromCurve(Curve curve)
        {
            if (curve is Line) {
                Line line = curve as Line;
                return line.Length;
            }
            else {
                return double.NaN;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        internal static double GetRadiusFromCurve(Curve curve)
        {
            if (curve is Arc) {
                Arc arc = curve as Arc;
                return arc.Radius;
            }
            else {
                return double.NaN;
            }
        }

        internal static Curve getLongestCurve(CurveArray curveArray, out Curve shorterCurve)
        {
            Curve firstCurveInCurveArray = curveArray.get_Item(0);
            double Maxlength = firstCurveInCurveArray.Length;
            Curve curve = null;
            for (int i = 1; i < curveArray.Size; i++) {
                curve = curveArray.get_Item(i);
                double length = curve.Length;
                if (length > Maxlength) {
                    shorterCurve = firstCurveInCurveArray;
                    return curve;
                }
            }
            shorterCurve = curve;
            return firstCurveInCurveArray;
        }

        internal static string directionToString(XYZ direction, double angle)
        {
            if (direction != null) {
                string x = Math.Round(direction.X, 6).ToString();
                string y = Math.Round(direction.Y, 6).ToString();
                string z = Math.Round(direction.Z, 6).ToString();
                DirectionAndAngle directionAndAngle = new DirectionAndAngle();
                directionAndAngle.Direction = x + "/" + y + "/" + z;
                directionAndAngle.Angle = angle.ToString();
                return JsonConvert.SerializeObject(directionAndAngle);
            }
            else {
                return null;
            }
        }

        internal static XYZ GetMidlePoint(Curve curve)
        {
            if (curve is Line) {
                Line line = curve as Line;
                return (line.GetEndPoint(1) + line.GetEndPoint(0)) / 2;
            }
            else {
                return null;
            }
        }

        internal static XYZ GetSharfOpenningOrigin(Element element)
        {
            XYZ origin = null;
            if (element is Opening) {
                Opening opening = element as Opening;
                if (IsRectangle(element)) {
                    CurveArray curveArray = opening.BoundaryCurves;
                    Curve firthCurve = curveArray.get_Item(0);
                    XYZ midleFirthCurve = Common.GetMidlePoint(firthCurve);
                    int indexCurveParalelFirthCurve = GetindexCurveParalelWithCurve(curveArray);
                    Curve curveParalelFirthCurve = curveArray.get_Item(indexCurveParalelFirthCurve);
                    XYZ MidlePointCurveParalelFirthCurve = Common.GetMidlePoint(curveParalelFirthCurve);
                    Line line1 = Line.CreateBound(midleFirthCurve, MidlePointCurveParalelFirthCurve);
                    Curve seconCurve = null;
                    //GetCoupleParalelCurve(curveArray, indexCurveParalelFirthCurve,out seconCurve, out curveParalelSeconCurve);
                    for (int i = 1; i < curveArray.Size; i++) {
                        if (i != indexCurveParalelFirthCurve) {
                            seconCurve = curveArray.get_Item(i);
                            break;
                        }
                    }
                    XYZ midlePointSeconCurve = Common.GetMidlePoint(seconCurve);
                    origin = line1.Project(midlePointSeconCurve).XYZPoint;
                }
                else {
                    CurveArray curveArray = opening.BoundaryCurves;
                    foreach (Curve curve in curveArray) {
                        if (curve is Arc) {
                            Arc arc = curve as Arc;
                            return arc.Center;
                        }
                    }
                }
            }
            return origin;
        }

        internal static XYZ GetFamilyInstanceOrigin(Element element)
        {
            XYZ origin = null;
            if (element is FamilyInstance) {
                FamilyInstance familyInstance = element as FamilyInstance;
                Location location = familyInstance.Location;
                origin = (location as LocationPoint).Point;
            }
            return origin;
        }

        internal static bool IsRectangle(Element element)
        {
            if (element is Opening) {
                Opening opening = element as Opening;
                CurveArray curveArray = opening.BoundaryCurves;
                if (curveArray.get_Item(0) is Line) {
                    return true;
                }
                else {
                    return false;
                }
            }
            return false;
        }

        internal static int GetindexCurveParalelWithCurve(CurveArray curveArray)
        {
            Curve firthCurve = curveArray.get_Item(0);
            XYZ firthCurveDirection = firthCurve.GetEndPoint(1) - firthCurve.GetEndPoint(0);
            for (int i = 1; i < curveArray.Size; i++) {
                Curve curve = curveArray.get_Item(i);
                XYZ curveDirection = curve.GetEndPoint(1) - curve.GetEndPoint(0);
                if (Common.IsParallel(firthCurveDirection, curveDirection)) {
                    return i;
                }
            }
            return 0;
        }

        internal static Level GetNearestLevel(Document document, XYZ Point)
        {
            Level result = null;
            FilteredElementCollector collector = new FilteredElementCollector(document);
            List<Element> elements = collector.OfClass(typeof(Level)).ToList();
            List<Level> levels = elements.ConvertAll(x => x as Level);
            result = FindNearestLevelToPoint(levels, Point, out double lenghtToEndPoint);
            return result;
        }

        internal static Level FindNearestLevelToPoint(List<Level> levels, XYZ Point, out double minDistance)
        {
            Level level = levels[0];
            minDistance = DistancePointToLevel(Point, level);
            for (int i = 0; i < levels.Count; i++) {
                if (i != 0) {
                    Level childLevel = levels[i];
                    double distance = DistancePointToLevel(Point, childLevel);
                    if (distance <= minDistance) {
                        minDistance = distance;
                        level = childLevel;
                    }
                }
            }

            return level;
        }

        internal static double DistancePointToLevel(XYZ point, Level level)
        {
            double elevation = level.Elevation;
            XYZ location = new XYZ(0, 0, elevation);
            double distance = point.DistanceTo(location);
            return distance;
        }

        /// <summary>
        /// determine if a text can be used to created a valid guid.
        /// Handle the case when use cancel pull operation/ delete
        /// opening during revit popup warning and try to synch to server.
        /// </summary>
        /// <param name="textId"></param>
        /// <returns></returns>
        public static bool IsValidGuid(string textId)
        {
            return !string.IsNullOrEmpty(textId);
        }

        /// <summary>
        /// Determine if the newly created opening is valid to udpate its parameter.
        /// Handle the case when revit pops up warning ( opening is completely inside a larger one,
        /// opening does not intersect host...) that allow user to cancel operation or
        /// delete newly created opening.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="openingCreatingTransaction"></param>
        /// <param name="openingId"></param>
        /// <returns></returns>
        public static bool CanUpdateNewOpening(Document doc, Transaction openingCreatingTransaction, ElementId openingId)
        {
            // when user cancel operation
            if (openingCreatingTransaction.GetStatus() != TransactionStatus.Committed)
                return false;

            // when user delete new opening
            if (openingId == null
                || openingId.Equals(ElementId.InvalidElementId))
                return false;

            Element opening = doc.GetElement(openingId);
            return opening != null;
        }
    }
}