using Autodesk.Revit.DB;
using CommonTools.OpeningClient.JsonObject;
using Newtonsoft.Json;
using System;
using System.Windows;

namespace CommonTools.OpeningClient.Support
{
    internal class OpenningInfomation
    {
        private bool _isVertical;
        private bool _isRectangle;
        private Element _element;

        public OpenningInfomation(Element element, bool isRectangle)
        {
            _isRectangle = isRectangle;
            _element = element;
            _isVertical = CheckIsVertical();
        }

        private bool CheckIsVertical()
        {
            if (_element is Opening)
                return true;
            else if (_element.Category.Name == "Windows")
                return false;
            else if (_element.Category.Name == "Generic Models") {
                if (_element.Name == "INNO_CBO_Floor Openning" || _element.Name == "INNO_CBO_Floor Openning_Circle") {
                    return true;
                }
                else if (_element.Name == "INNO_CBO_Wall Openning" || _element.Name == "INNO_CBO_Wall Openning_Circle") {
                    return false;
                }
            }
            return false;
        }

        #region direction

        public string MEPOpenningDirection()
        {
            if (_isVertical) {
                XYZ direction = XYZ.BasisZ;
                double angle = MEPGetAngleFromBasicVec();
                string directionInfor = Common.directionToString(direction, angle);
                return directionInfor;
            }
            else {
                FamilyInstance familyInstance = _element as FamilyInstance;
                XYZ direction = familyInstance.FacingOrientation;
                double angle = MEPGetAngleFromBasicVec();
                string directionInfor = Common.directionToString(direction, angle);
                return directionInfor;
            }
        }

        private double MEPGetAngleFromBasicVec()
        {
            if (_element is FamilyInstance) {
                Location location = _element.Location;
                LocationPoint locationPoint = location as LocationPoint;
                double rorate = locationPoint.Rotation;
                if (rorate >= Math.PI)
                    rorate = rorate - Math.PI;
                return rorate;
            }
            else {
                return double.NaN;
            }
        }

        public string SructureOpenningDirection()
        {
            if (_isVertical == true) {
                if (_isRectangle) {
                    XYZ direction = XYZ.BasisZ;
                    double angle = GetAngleFromBasicVec(XYZ.BasisY);
                    string directionInfor = Common.directionToString(direction, angle);
                    return directionInfor;
                }
                else {
                    XYZ direction = XYZ.BasisZ;
                    double angle = 0;
                    string directionInfor = Common.directionToString(direction, angle);
                    return directionInfor;
                }
            }
            else {
                XYZ direction = GetWindowDirection();
                double angle = 0;
                if (_isRectangle) {
                    FamilyInstance familyInstance = _element as FamilyInstance;
                    LocationPoint lp = familyInstance.Location as LocationPoint;
                    angle = lp.Rotation;
                    if (angle >= Math.PI)
                        angle = angle - Math.PI;
                    if (angle >= 0.99 * Math.PI)
                        angle = angle - Math.PI;
                }
                string directionInfor = Common.directionToString(direction, angle);
                return directionInfor;
            }
        }

        private double GetAngleFromBasicVec(XYZ basicVec)
        {
            if (_element is Opening) {
                Opening opening = _element as Opening;
                CurveArray curveArray = opening.BoundaryCurves;
                Curve lengthCurve = Common.getLongestCurve(curveArray, out Curve shorterCurve);
                XYZ lengthCurveDirection = GetDirectionCurve(lengthCurve);
                double angleFromBasicX = lengthCurveDirection.AngleOnPlaneTo(basicVec, XYZ.BasisZ.Negate());
                if (angleFromBasicX >= Math.PI)
                    angleFromBasicX = angleFromBasicX - Math.PI;
                if (angleFromBasicX >= 0.99 * Math.PI)
                    angleFromBasicX = angleFromBasicX - Math.PI;
                return angleFromBasicX;
            }
            else {
                return double.NaN;
            }
        }

        private XYZ GetWindowDirection()
        {
            if (_element is FamilyInstance) {
                FamilyInstance window = _element as FamilyInstance;
                return window.FacingOrientation;
            }
            return null;
        }

        private XYZ GetDirectionCurve(Curve curve)
        {
            if (curve is Line) {
                Line line = curve as Line;
                XYZ firstPoint = line.GetEndPoint(0);
                XYZ seconPoint = line.GetEndPoint(1);
                return seconPoint - firstPoint;
            }
            else {
                return null;
            }
        }

        private string AchitechtureOpenningDirection()
        {
            return "";
        }

        #endregion direction

        #region origin

        public string MEPOpeningLocation()
        {
            string result;
            XYZ location = GetFamilyInstanceOrigin();
            result = location.X.ToString() + "/" + location.Y.ToString() + "/" + location.Z.ToString();
            Origin origin = new Origin();
            origin.Location = result;
            return JsonConvert.SerializeObject(origin);
        }

        public string StructureOpeningLocation()
        {
            string result;
            XYZ location = null;
            if (_isVertical) {
                location = GetSharfOpenningOrigin();
            }
            else {
                location = GetFamilyInstanceOrigin();
            }
            result = location.X.ToString() + "/" + location.Y.ToString() + "/" + location.Z.ToString();
            Origin origin = new Origin();
            origin.Location = result;
            return JsonConvert.SerializeObject(origin);
        }

        private XYZ GetFamilyInstanceOrigin()
        {
            XYZ origin = null;
            if (_element is FamilyInstance) {
                FamilyInstance familyInstance = _element as FamilyInstance;
                Location location = familyInstance.Location;
                origin = (location as LocationPoint).Point;
            }
            return origin;
        }

        private XYZ GetSharfOpenningOrigin()
        {
            XYZ origin = null;
            if (_element is Opening) {
                Opening opening = _element as Opening;
                Document document = opening.Document;
                Level level = document.GetElement(opening.LevelId) as Level;
                double elevation = 0;
                if (level != null) {
                    elevation = level.Elevation;
                }
                if (_isRectangle) {
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
                    double elevationOfset = 0;
                    if (!Common.IsEqual(elevation, origin.Z)) {
                        elevationOfset = elevation;
                    }
                    Parameter paramBaseOfset = opening.LookupParameter("Base Offset");
                    double baseOfset = paramBaseOfset.AsDouble();
                    origin = origin + new XYZ(0, 0, baseOfset + elevationOfset);
                    //origin = origin + new XYZ(0, 0, elevationOfset - baseOfset);
                }
                else {
                    CurveArray curveArray = opening.BoundaryCurves;
                    foreach (Curve curve in curveArray) {
                        if (curve is Arc) {
                            Arc arc = curve as Arc;
                            origin = arc.Center;
                            double elevationOfset = 0;
                            if (!Common.IsEqual(elevation, origin.Z)) {
                                elevationOfset = elevation;
                            }
                            Parameter paramBaseOfset = opening.LookupParameter("Base Offset");
                            double baseOfset = paramBaseOfset.AsDouble();
                            origin = origin + new XYZ(0, 0, baseOfset + elevationOfset);
                            return origin;
                        }
                    }
                }
            }
            return origin;
        }

        private int GetindexCurveParalelWithCurve(CurveArray curveArray)
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

        #endregion origin
    }
}