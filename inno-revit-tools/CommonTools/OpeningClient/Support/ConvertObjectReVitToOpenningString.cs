using Autodesk.Revit.DB;
using CommonTools.OpeningClient.JsonObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CommonTools.OpeningClient.Support
{
    public static class ConvertObjectReVitToOpenningString
    {
        public static string GetGeometry(Element openning, bool isRectangle, string subject)
        {
            string result = null;
            if (subject == Define.MEP_SUBJECT) {
                result = GetGeometryMEP(openning, isRectangle);
            }
            else if (subject == Define.STRUCTURE_SUBJECT) {
                if (openning is Opening) {
                    result = GetGeometrySharpOpening(openning, isRectangle);
                }
                else if (openning is FamilyInstance) {
                    result = GetGeometryWindowOpening(openning, isRectangle);
                }
            }
            else if (subject == Define.ARCHITECHTURE_SUBJECT) {
                if (openning is Opening) {
                    result = GetGeometrySharpOpening(openning, isRectangle);
                }
                else if (openning is FamilyInstance) {
                    result = GetGeometryWindowOpening(openning, isRectangle);
                }
            }
            return result;
        }

        private static string GetGeometryMEP(Element openning, bool isRectangle)
        {
            if (isRectangle == true) {
                string shapename = Define.RECTANGLE_FAMILY;
                string width = "";
                string lenght = "";
                string height = "";
                if (openning.Name == Define.FLOOR_OPENNING_RECTANGLE_NAME_MEP) {
                    Parameter widthParam = openning.LookupParameter(Define.WIDTH_PARAMETER_NAME_MEP);
                    width = routingStr(widthParam.AsValueString());
                    Parameter lenghtParam = openning.LookupParameter(Define.LENGHT_PARAMETER_NAME_MEP);
                    lenght = routingStr(lenghtParam.AsValueString());
                    Parameter heightParam = openning.LookupParameter(Define.HEIGHT_PARAMETER_NAME_MEP);
                    height = routingStr(heightParam.AsValueString());
                }
                else if (openning.Name == Define.WALL_OPENNING_RECTANGLE_NAME_MEP) {
                    Parameter widthParam = openning.LookupParameter(Define.WIDTH_PARAMETER_NAME_MEP);
                    width = routingStr(widthParam.AsValueString());
                    Parameter lenghtParam = openning.LookupParameter(Define.HEIGHT_PARAMETER_NAME_MEP);
                    lenght = routingStr(lenghtParam.AsValueString());
                    Parameter heightParam = openning.LookupParameter(Define.DEPTH_PARAMETER_NAME_MEP);
                    height = routingStr(heightParam.AsValueString());
                }
                RectangleGeometry rectangleGeometry = new RectangleGeometry();

                rectangleGeometry.ShapeName = shapename;
                rectangleGeometry.Lenght = lenght;
                rectangleGeometry.Width = width;
                rectangleGeometry.Height = height;
                return JsonConvert.SerializeObject(rectangleGeometry);
            }
            else {
                string shapename = Define.CYLYNDER_FAMILY;
                Parameter radiusParam = openning.LookupParameter(Define.RADIUS_PARAMETER_NAME_MEP);
                string radius = routingStr(radiusParam.AsValueString());
                string height = "";
                if (openning.Name == Define.FLOOR_OPENNING_CYLYNDER_NAME_MEP) {
                    Parameter heightParam = openning.LookupParameter(Define.HEIGHT_PARAMETER_NAME_MEP);
                    height = routingStr(heightParam.AsValueString());
                }
                else if (openning.Name == Define.WALL_OPENNING_CYLYNDER_NAME_MEP) {
                    Parameter heightParam = openning.LookupParameter(Define.DEPTH_PARAMETER_NAME_MEP);
                    height = routingStr(heightParam.AsValueString());
                }
                CylynderGeometry cylynderGeometry = new CylynderGeometry();
                cylynderGeometry.ShapeName = shapename;
                cylynderGeometry.Radius = radius;
                cylynderGeometry.Height = height;
                return JsonConvert.SerializeObject(cylynderGeometry);
            }
        }

        private static string GetGeometrySharpOpening(Element openning, bool isRectangle)
        {
            if (openning is Opening) {
                Opening openingObject = openning as Opening;
                string shapename = "";
                if (isRectangle == true) {
                    shapename = Define.RECTANGLE_FAMILY;
                    string widthstr = "";
                    string lenghtstr = "";
                    Parameter unconnectedHeight = openingObject.LookupParameter("Unconnected Height");
                    string heightstr = routingStr(unconnectedHeight.AsValueString());
                    CurveArray curveArray = openingObject.BoundaryCurves;
                    Curve firtLine = curveArray.get_Item(0);
                    double firtLength = Common.GetLengthFromCurve(firtLine);
                    for (int i = 1; i < curveArray.Size; i++) {
                        Curve line = curveArray.get_Item(i);
                        double lenght = Common.GetLengthFromCurve(line);
                        if (!Common.IsEqual(lenght, firtLength)) {
                            if (lenght < firtLength) {
                                widthstr = routingStr((lenght * Define.mmToFeet).ToString());
                                lenghtstr = routingStr((firtLength * Define.mmToFeet).ToString());
                            }
                            else if (lenght > firtLength) {
                                widthstr = routingStr((firtLength * Define.mmToFeet).ToString());
                                lenghtstr = routingStr((lenght * Define.mmToFeet).ToString());
                            }
                        }
                        else {
                            widthstr = routingStr((firtLength * Define.mmToFeet).ToString());
                            lenghtstr = routingStr((firtLength * Define.mmToFeet).ToString());
                        }
                        RectangleGeometry rectangleGeometry = new RectangleGeometry();
                        rectangleGeometry.ShapeName = shapename;
                        rectangleGeometry.Lenght = lenghtstr;
                        rectangleGeometry.Width = widthstr;
                        rectangleGeometry.Height = heightstr;
                        return JsonConvert.SerializeObject(rectangleGeometry);
                    }
                }
                else {
                    shapename = Define.CYLYNDER_FAMILY;
                    CurveArray curveArray = openingObject.BoundaryCurves;
                    Curve firtArc = curveArray.get_Item(0);
                    foreach (Curve curve in curveArray) {
                        if (curve is Arc) {
                            firtArc = curve;
                            break;
                        }
                    }
                    double radius = Common.GetRadiusFromCurve(firtArc);
                    Parameter heightParam = openning.LookupParameter(Define.HEIGHT_PARAMETER_NAME_MEP);
                    Parameter unconnectedHeight = openingObject.LookupParameter("Unconnected Height");
                    string height = routingStr(unconnectedHeight.AsValueString());
                    CylynderGeometry cylynderGeometry = new CylynderGeometry();
                    cylynderGeometry.ShapeName = shapename;
                    cylynderGeometry.Radius = routingStr((radius * Define.mmToFeet).ToString());
                    cylynderGeometry.Height = height;
                    return JsonConvert.SerializeObject(cylynderGeometry);
                }
            }
            return null;
        }

        private static string GetGeometryWindowOpening(Element openning, bool isRectangle)
        {
            List<string> stringToJson = new List<string>();
            string shapename = "";
            if (isRectangle == true) {
                shapename = Define.RECTANGLE_FAMILY;
                Parameter widthParam = openning.LookupParameter(Define.WIDTH_PARAMETER_NAME_STRUCTURE);
                string width = routingStr(widthParam.AsValueString());
                Parameter lenghtParam = openning.LookupParameter(Define.LENGHT_PARAMETER_NAME_STRUCTURE);
                string lenght = routingStr(lenghtParam.AsValueString());
                Parameter heightParam = openning.LookupParameter(Define.HEIGHT_PARAMETER_NAME_STRUCTURE);
                string height = "";
                if (heightParam != null)
                    height = routingStr(heightParam.AsValueString());
                else {
                    FamilyInstance familyInstance = openning as FamilyInstance;
                    Wall wall = familyInstance.Host as Wall;
                    height = routingStr((wall.Width * 304.8).ToString());
                }
                RectangleGeometry rectangleGeometry = new RectangleGeometry();
                rectangleGeometry.ShapeName = shapename;
                rectangleGeometry.Lenght = lenght;
                rectangleGeometry.Width = width;
                rectangleGeometry.Height = height;
                return JsonConvert.SerializeObject(rectangleGeometry);
            }
            else {
                shapename = Define.CYLYNDER_FAMILY;
                Parameter radiusParam = openning.LookupParameter(Define.RADIUS_PARAMETER_NAME_STRUCTURE);
                string radius = routingStr(radiusParam.AsValueString());
                Parameter heightParam = openning.LookupParameter(Define.HEIGHT_PARAMETER_NAME_STRUCTURE);
                string height = "";
                if (heightParam != null)
                    height = routingStr(heightParam.AsValueString());
                else {
                    FamilyInstance familyInstance = openning as FamilyInstance;
                    Wall wall = familyInstance.Host as Wall;
                    height = routingStr((wall.Width * 304.8).ToString());
                }
                CylynderGeometry cylynderGeometry = new CylynderGeometry();
                cylynderGeometry.ShapeName = shapename;
                cylynderGeometry.Radius = radius;
                cylynderGeometry.Height = height;
                return JsonConvert.SerializeObject(cylynderGeometry);
            }
        }

        public static string GetDirection(Element openning, bool isRectangle, string subject)
        {
            string result = null;
            if (subject == Define.MEP_SUBJECT) {
                OpenningInfomation openingInfor = new OpenningInfomation(openning, isRectangle);
                result = openingInfor.MEPOpenningDirection();
            }
            else if (subject == Define.STRUCTURE_SUBJECT) {
                OpenningInfomation openingInfor = new OpenningInfomation(openning, isRectangle);
                result = openingInfor.SructureOpenningDirection();
            }
            else if (subject == Define.ARCHITECHTURE_SUBJECT) {
                // Achitech co fam giong nhu struc
                OpenningInfomation openingInfor = new OpenningInfomation(openning, isRectangle);
                result = openingInfor.SructureOpenningDirection();
            }
            return result;
        }

        public static string GetOrigin(Element openning, bool isRectangle, string subject)
        {
            string result = null;
            if (subject == Define.MEP_SUBJECT) {
                OpenningInfomation openingInfor = new OpenningInfomation(openning, isRectangle);
                result = openingInfor.MEPOpeningLocation();
            }
            else if (subject == Define.STRUCTURE_SUBJECT) {
                OpenningInfomation openingInfor = new OpenningInfomation(openning, isRectangle);
                result = openingInfor.StructureOpeningLocation();
            }
            else if (subject == Define.ARCHITECHTURE_SUBJECT) {
                // Achitech co fam giong nhu struc
                OpenningInfomation openingInfor = new OpenningInfomation(openning, isRectangle);
                result = openingInfor.StructureOpeningLocation();
            }
            return result;
        }

        public static string routingStr(string numberStr)
        {
            double.TryParse(numberStr, out double number);
            number = Math.Round(number, 2);
            return number.ToString();
        }
    }
}