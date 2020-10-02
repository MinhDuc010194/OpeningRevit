using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Support;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonTools.OpeningClient.Synchronize.Process.Subject
{
    internal class StrAchOpenningMaker : StructureProcess
    {
        private bool _isVertical;
        private Document _document;
        private GeometryDetail _openningModel;
        private double _height;
        private double _width;
        private double _lenght;
        private double _radius;
        private double _angle;
        private XYZ _origin;
        private string _shapeProfile;
        private string _openningName;
        private XYZ _direction;
        private ComparisonCoupleElement _comparionCoupleElement;

        public StrAchOpenningMaker(Document document, ComparisonCoupleElement openningModel, string projectName = "") : base(document, projectName)
        {
            _document = document;
            _comparionCoupleElement = openningModel;
            _openningModel = openningModel.ServerGeometry;
            GetValueGeometry(openningModel.ServerGeometry);
            if (Common.IsEqual(_direction, XYZ.BasisZ) || Common.IsEqual(_direction, XYZ.BasisZ.Negate())) {
                _isVertical = true;
            }
            else {
                _isVertical = false;
                if (_shapeProfile == Define.RECTANGLE_FAMILY) {
                    _openningName = Define.WINDOW_OPENNING_RECTANGLE_NAME;
                }
                else if (_shapeProfile == Define.CYLYNDER_FAMILY) {
                    _openningName = Define.WINDOW_OPENNING_CYLYNDER_NAME;
                }
            }
        }

        #region Create

        public string CreateOpenning(Transaction transaction)
        {
            string uniqueId = "";
            if (_isVertical == true) {
                transaction.Start("Create Opening");
                uniqueId = LoadSharpOpenning(_openningModel, false);
                transaction.Commit();
            }
            else {
                uniqueId = LoadWindowOpenning(transaction);
            }
            return uniqueId;
        }

        public string CreateOpenningForPreview(Transaction transaction)
        {
            string uniqueId = "";
            if (_isVertical == true) {
                transaction.Start("Create Opening");
                uniqueId = LoadSharpOpenningForPreview(_openningModel, false);
                transaction.Commit();
            }
            else {
                uniqueId = LoadWindowOpenningPreview(transaction);
            }
            return uniqueId;
        }

        #region SharpOpening

        private string LoadSharpOpenning(GeometryDetail openingData, bool isEdit, double firthAngle = 0)
        {
            //transaction.Start("Create Str Opening");
            // get data for base Level
            XYZ basePoint = _origin;
            Level baseLevel = GetNearestLevel(basePoint, out XYZ baseDirectionToEndPoint, out double baseLenghtToEndPoint);
            // get data for top Level
            XYZ topPoint = _origin + XYZ.BasisZ * _height;
            Level topLevel = GetNearestLevel(topPoint, out XYZ topDirectionToEndPoint, out double topLenghtToEndPoint);
            // get CurveArray
            CurveArray profile = GetProfile(basePoint, openingData);
            Opening opening = _document.Create.NewOpening(baseLevel, topLevel, profile);
            TranformSharpOpenning(opening, baseDirectionToEndPoint, baseLenghtToEndPoint, topDirectionToEndPoint, topLenghtToEndPoint);
            //Get Guid and save to Comparetion
            _comparionCoupleElement.IdRevitElement = opening.UniqueId;
            if (isEdit == true)
                UpdateShaftAngle(opening.Id, firthAngle);
            UpdateShaftAngle(opening.Id, isEdit);
            return opening.UniqueId;
            //transaction.Commit();
        }

        private string LoadSharpOpenningForPreview(GeometryDetail openingData, bool isEdit, double firthAngle = 0)
        {
            //transaction.Start("Create Str Opening");
            // get data for base Level
            XYZ basePoint = _origin;
            Level baseLevel = GetNearestLevel(basePoint, out XYZ baseDirectionToEndPoint, out double baseLenghtToEndPoint);
            // get data for top Level
            XYZ topPoint = _origin + XYZ.BasisZ * _height;
            Level topLevel = GetNearestLevel(topPoint, out XYZ topDirectionToEndPoint, out double topLenghtToEndPoint);
            // get CurveArray
            CurveArray profile = GetProfile(basePoint, openingData);
            Opening opening = _document.Create.NewOpening(baseLevel, topLevel, profile);
            TranformSharpOpenning(opening, baseDirectionToEndPoint, baseLenghtToEndPoint, topDirectionToEndPoint, topLenghtToEndPoint);
            //Get Guid and save to Comparetion
            if (isEdit == true)
                UpdateShaftAngle(opening.Id, firthAngle);
            UpdateShaftAngle(opening.Id, isEdit);
            return opening.UniqueId;
            //transaction.Commit();
        }

        private void TranformSharpOpenning(Opening opening, XYZ baseDirectionToEndPoint, double baseLenghtToEndPoint, XYZ topDirectionToEndPoint, double topLenghtToEndPoint)
        {
            //Edit top
            if (Common.IsEqual(topDirectionToEndPoint, XYZ.BasisZ)) {
                Parameter topOfset = opening.LookupParameter("Top Offset");
                topOfset.Set(-topLenghtToEndPoint);
            }
            else if (Common.IsEqual(topDirectionToEndPoint, XYZ.BasisZ.Negate())) {
                Parameter topOfset = opening.LookupParameter("Top Offset");
                topOfset.Set(topLenghtToEndPoint);
            }
            //edit base
            if (Common.IsEqual(baseDirectionToEndPoint, XYZ.BasisZ)) {
                Parameter baseOfset = opening.LookupParameter("Base Offset");
                baseOfset.Set(-baseLenghtToEndPoint);
            }
            else if (Common.IsEqual(baseDirectionToEndPoint, XYZ.BasisZ.Negate())) {
                Parameter baseOfset = opening.LookupParameter("Base Offset");
                baseOfset.Set(baseLenghtToEndPoint);
            }
        }

        private Level GetNearestLevel(XYZ Point, out XYZ directionToEndPoint, out double lenghtToEndPoint)
        {
            Level result = null;
            FilteredElementCollector collector = new FilteredElementCollector(_document);
            List<Element> elements = collector.OfClass(typeof(Level)).ToList();
            List<Level> levels = elements.ConvertAll(x => x as Level);
            result = FindNearestLevelToPoint(levels, Point, out lenghtToEndPoint, out directionToEndPoint);
            return result;
        }

        private Level GetNearestLevelByDirection(XYZ Point)
        {
            Level result = null;
            FilteredElementCollector collector = new FilteredElementCollector(_document);
            List<Element> elements = collector.OfClass(typeof(Level)).ToList();
            List<Level> levels = elements.ConvertAll(x => x as Level);
            result = FindNearestLevelToPointByDirection(levels, Point, XYZ.BasisZ.Negate());
            return result;
        }

        private Level FindNearestLevelToPoint(List<Level> levels, XYZ Point, out double minDistance, out XYZ direction)
        {
            Level level = levels[0];
            minDistance = DistancePointToLevel(Point, level);
            direction = DirectionPointToLevel(Point, level);
            for (int i = 0; i < levels.Count; i++) {
                if (i != 0) {
                    Level childLevel = levels[i];
                    double distance = DistancePointToLevel(Point, childLevel);
                    if (distance <= minDistance) {
                        minDistance = distance;
                        level = childLevel;
                        direction = DirectionPointToLevel(Point, level);
                    }
                }
            }

            return level;
        }

        private Level FindNearestLevelToPointByDirection(List<Level> levels, XYZ Point, XYZ direction)
        {
            List<Level> levelsSameDirection = new List<Level>();
            foreach (Level level in levels) {
                XYZ curentdirection = DirectionPointToLevel(Point, level);
                if (Common.IsEqual(direction.Normalize(), curentdirection.Normalize())) {
                    levelsSameDirection.Add(level);
                }
            }
            Level minlevel = levelsSameDirection[0];
            double minDistance = DistancePointToLevel(Point, minlevel);
            for (int i = 1; i < levelsSameDirection.Count; i++) {
                Level childLevel = levelsSameDirection[i];
                double distance = DistancePointToLevel(Point, childLevel);
                if (distance <= minDistance) {
                    minDistance = distance;
                    minlevel = childLevel;
                }
            }
            return minlevel;
        }

        private XYZ DirectionPointToLevel(XYZ point, Level level)
        {
            double elevaton = level.Elevation;
            XYZ location = new XYZ(0, 0, elevaton);
            XYZ direcVec = location - point;
            double angle = direcVec.AngleOnPlaneTo(XYZ.BasisX, XYZ.BasisY);
            if (angle < Math.PI) {
                return XYZ.BasisZ;
            }
            else if (angle >= Math.PI) {
                return XYZ.BasisZ.Negate();
            }
            else {
                return new XYZ(0, 0, 0);
            }
        }

        private double DistancePointToLevel(XYZ point, Level level)
        {
            double elevation = level.Elevation;
            XYZ location = new XYZ(0, 0, elevation);
            XYZ prolectPoint = new XYZ(0, 0, point.Z);
            double distance = prolectPoint.DistanceTo(location);
            return distance;
        }

        #endregion SharpOpening

        #region WindowOpening

        private string LoadWindowOpenning(Transaction transaction)
        {
            FamilySymbol symbol = new FilteredElementCollector(_document).OfCategory(BuiltInCategory.OST_Windows)
                                                                        .OfClass(typeof(FamilySymbol))
                                                                        .FirstOrDefault(x => x.Name.Equals(_openningName)) as FamilySymbol;
            List<Wall> walls = GetWallContainOpening(_origin, _direction, _height, out Line cuttingLine);
            if (walls != null && walls.Count > 0) {
                //Loc wall
                Wall wall = walls[0];
                XYZ locationIntersecWithWall = GetPointIntersecWithWall(wall, cuttingLine, out bool isOvelapWithAnotherWindown);
                if (!isOvelapWithAnotherWindown) {
                    if (locationIntersecWithWall != null) {
                        ElementId id = null;
                        // create wall opening
                        transaction.Start("Create Opening");
                        symbol.Activate();
                        Parameter parameterBase = wall.LookupParameter("Base Constraint");
                        Level level = _document.GetElement(parameterBase.AsElementId()) as Level; /*GetNearestLevelByDirection(locationIntersecWithWall);*/
                                                                                                  //MessageBox.Show(level.Name);
                        FamilyInstance instance = _document.Create.NewFamilyInstance(locationIntersecWithWall, symbol, wall, level, StructuralType.UnknownFraming);
                        id = instance.Id;
                        transaction.Commit();

                        // update opening
                        if (Common.CanUpdateNewOpening(_document, transaction, id)) {
                            transaction.Start("UpDate");
                            UpdateGeometryAndDirection(instance);
                            transaction.Commit();
                            if (Common.CanUpdateNewOpening(_document, transaction, id))
                                _comparionCoupleElement.IdRevitElement = instance.UniqueId;
                        }
                        return instance.UniqueId;
                    }
                }
            }

            if (!Common.IsValidGuid(_comparionCoupleElement.IdRevitElement))
                _comparionCoupleElement.IdRevitElement = new Guid().ToString();

            return "";
        }

        private string LoadWindowOpenningPreview(Transaction transaction)
        {
            FamilySymbol symbol = new FilteredElementCollector(_document).OfCategory(BuiltInCategory.OST_Windows)
                                                                        .OfClass(typeof(FamilySymbol))
                                                                        .FirstOrDefault(x => x.Name.Equals(_openningName)) as FamilySymbol;
            List<Wall> walls = GetWallContainOpening(_origin, _direction, _height, out Line cuttingLine);
            if (walls != null && walls.Count > 0) {
                //Loc wall
                Wall wall = walls[0];
                XYZ locationIntersecWithWall = GetPointIntersecWithWall(wall, cuttingLine, out bool isOvelapWithAnotherWindown);
                if (!isOvelapWithAnotherWindown) {
                    if (locationIntersecWithWall != null) {
                        ElementId id = null;
                        // create wall opening
                        transaction.Start("Create Opening");
                        symbol.Activate();
                        Parameter parameterBase = wall.LookupParameter("Base Constraint");
                        Level level = _document.GetElement(parameterBase.AsElementId()) as Level; /*GetNearestLevelByDirection(locationIntersecWithWall);*/
                                                                                                  //MessageBox.Show(level.Name);
                        FamilyInstance instance = _document.Create.NewFamilyInstance(locationIntersecWithWall, symbol, wall, level, StructuralType.UnknownFraming);
                        id = instance.Id;
                        transaction.Commit();

                        // update opening
                        if (Common.CanUpdateNewOpening(_document, transaction, id)) {
                            transaction.Start("UpDate");
                            UpdateGeometryAndDirection(instance);
                            transaction.Commit();
                            //if (Common.CanUpdateNewOpening(_document, transaction, id))
                            //    _comparionCoupleElement.IdRevitElement = instance.UniqueId;
                        }
                        return instance.UniqueId;
                    }
                }
            }

            //if (!Common.IsValidGuid(_comparionCoupleElement.IdRevitElement))
            //    _comparionCoupleElement.IdRevitElement = new Guid().ToString();

            return "";
        }

        private XYZ GetPointIntersecWithWall(Wall wall, Line cuttingLine, out bool isOvelapWithAnotherWindown)
        {
            isOvelapWithAnotherWindown = false;
            List<Solid> solids = GetSolidFromWall(wall);
            foreach (Solid solid in solids) {
                if (solid.Faces != null && solid.Faces.Size > 0) {
                    foreach (Face face in solid.Faces) {
                        SetComparisonResult result = face.Intersect(cuttingLine, out IntersectionResultArray intersectionResultArray);
                        if (result == SetComparisonResult.Overlap || result == SetComparisonResult.Subset) {
                            if (intersectionResultArray != null) {
                                for (int i = 0; i < intersectionResultArray.Size; i++) {
                                    IntersectionResult intersectionResult = intersectionResultArray.get_Item(i);
                                    if (intersectionResult.XYZPoint != null)
                                        return intersectionResult.XYZPoint;
                                }
                            }
                            else
                                isOvelapWithAnotherWindown = true;
                        }
                    }
                }
            }
            return null;
        }

        private List<Wall> GetWallContainOpening(XYZ location, XYZ Direction, double height, out Line cuttingLine)
        {
            Line line = GetScaleLine(location, Direction, height);
            List<Wall> walls = new FilteredElementCollector(_document).OfCategory(BuiltInCategory.OST_Walls)
                                                                    .OfClass(typeof(Wall))
                                                                    .ToList()
                                                                    .ConvertAll(x => x as Wall);

            List<Wall> wallsResult = walls.Where(wall => IsWallIntersectLine(wall, line)).ToList();
            cuttingLine = walls.Count > 0 ? line : null;
            return wallsResult;
        }

        private bool IsWallIntersectLine(Wall wall, Line line)
        {
            List<Solid> solids = GetSolidFromWall(wall);
            foreach (Solid solid in solids) {
                if (solid.Faces != null && solid.Faces.Size > 0) {
                    foreach (Face face in solid.Faces) {
                        SetComparisonResult result = face.Intersect(line, out IntersectionResultArray array);
                        if (result == SetComparisonResult.Overlap)
                            return true;
                    }
                }
            }
            return false;
        }

        private Line GetScaleLine(XYZ origin, XYZ direction, double length)
        {
            direction = direction.Normalize();
            XYZ start = origin + direction.Negate() * length;
            XYZ end = origin + direction * 2 * length;
            Line scale = Line.CreateBound(start, end);
            return scale;
        }

        private List<Solid> GetSolidFromWall(Wall wall)
        {
            List<Solid> solids = new List<Solid>();
            GeometryElement geoElement = wall.get_Geometry(new Options());
            foreach (GeometryObject geometryObject in geoElement) {
                if (geometryObject is Solid) {
                    solids.Add(geometryObject as Solid);
                }
            }
            return solids;
        }

        private void UpdateGeometryAndDirection(Element opening)
        {
            if (_openningName == Define.WINDOW_OPENNING_RECTANGLE_NAME) {
                Common.SetDoubleValueToParam(opening, Define.WIDTH_PARAMETER_NAME_STRUCTURE, _width);
                Common.SetDoubleValueToParam(opening, Define.LENGHT_PARAMETER_NAME_STRUCTURE, _lenght);
                //Common.SetDoubleValueInParam(opening, Define.HEIGHT_PARAMETER_NAME_STRUCTURE, _height);
            }
            else if (_openningName == Define.WINDOW_OPENNING_CYLYNDER_NAME) {
                Common.SetDoubleValueToParam(opening, Define.RADIUS_PARAMETER_NAME_STRUCTURE, _radius);
            }

            //UpdateAngle(opening.Id);
        }

        #endregion WindowOpening

        #endregion Create

        #region Edit

        public void EditOpening(Transaction transaction, Element opening)
        {
            if (Common.IsValidGuid(_comparionCoupleElement.IdRevitElement)) {
                transaction.Start("Edit Opening");
                // Floor
                if (_isVertical == true) {
                    EditProfile(opening);
                    opening = _document.GetElement(_comparionCoupleElement.IdRevitElement);
                    EditVerticalLocation(opening);
                }
                else {
                    UpdateGeometryAndDirection(opening);
                    EditHorizontalLocation(transaction, opening);
                }
                transaction.Commit();
            }
        }

        public void UpdateShaftAngle(ElementId id, bool isEdit)
        {
            XYZ basicZ = XYZ.BasisZ;
            Line axis = Line.CreateBound((_origin), (_origin + basicZ));
            double localAngle = 0;
            if (_comparionCoupleElement.LocalGeometry != null)
                localAngle = ConvertOpenningStringToObjectReVit.getAngle(_comparionCoupleElement.LocalGeometry.Direction);
            if (localAngle >= Math.PI) {
                localAngle = localAngle - Math.PI;
            }
            double lastestAngle = _angle;
            if (lastestAngle >= Math.PI) {
                lastestAngle = lastestAngle - Math.PI;
            }
            double angle = lastestAngle - localAngle;
            if (isEdit == true)
                ElementTransformUtils.RotateElement(_document, id, axis, angle);
            else
                ElementTransformUtils.RotateElement(_document, id, axis, lastestAngle);
        }

        public void UpdateShaftAngle(ElementId id, double angle)
        {
            XYZ basicZ = XYZ.BasisZ;
            Line axis = Line.CreateBound((_origin), (_origin + basicZ));
            ElementTransformUtils.RotateElement(_document, id, axis, angle);
        }

        private void EditVerticalLocation(Element opening)
        {
            XYZ curentLocation = Common.GetSharfOpenningOrigin(opening);
            XYZ tranformVector = _origin - curentLocation;
            ElementTransformUtils.MoveElement(_document, opening.Id, tranformVector);
        }

        private void EditHorizontalLocation(Transaction transaction, Element opening)
        {
            XYZ curentLocation = Common.GetFamilyInstanceOrigin(opening);
            XYZ tranformVector = _origin - curentLocation;
            ElementTransformUtils.MoveElement(_document, opening.Id, tranformVector);
        }

        private void EditProfile(Element openingElement)
        {
            double AngleBeforeDelete = 0;
            if (_shapeProfile == Define.RECTANGLE_FAMILY) {
                AngleBeforeDelete = GetAngleFromBasicVec(openingElement, XYZ.BasisY);
            }
            _document.Delete(openingElement.Id);
            LoadSharpOpenning(_openningModel, true, AngleBeforeDelete);
        }

        #endregion Edit

        #region Common

        private double GetAngleFromBasicVec(Element element, XYZ basicVec)
        {
            if (element is Opening) {
                Opening opening = element as Opening;
                CurveArray curveArray = opening.BoundaryCurves;
                Curve lengthCurve = Common.getLongestCurve(curveArray, out Curve shorterCurve);
                XYZ lengthCurveDirection = GetDirectionCurve(lengthCurve);
                double angleFromBasicX = 0;
                if (lengthCurveDirection != null)
                    angleFromBasicX = lengthCurveDirection.AngleOnPlaneTo(basicVec, XYZ.BasisZ.Negate());

                if (angleFromBasicX >= Math.PI)
                    angleFromBasicX = angleFromBasicX - Math.PI;
                return angleFromBasicX;
            }
            else {
                return double.NaN;
            }
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

        private void GetValueGeometry(GeometryDetail openingData)
        {
            _shapeProfile = ConvertOpenningStringToObjectReVit.GetShapeFromGeometry(openingData.Geometry);
            _origin = ConvertOpenningStringToObjectReVit.GetOriginal(openingData.Original);
            _height = ConvertOpenningStringToObjectReVit.GetHeightFromGeometry(openingData.Geometry) / Define.mmToFeet;
            _direction = ConvertOpenningStringToObjectReVit.getDirection(openingData.Direction);
            _angle = ConvertOpenningStringToObjectReVit.getAngle(openingData.Direction);
            if (_shapeProfile == Define.RECTANGLE_FAMILY) {
                _lenght = ConvertOpenningStringToObjectReVit.GetLengthFromGeometry(openingData.Geometry) / Define.mmToFeet;
                _width = ConvertOpenningStringToObjectReVit.GetWidthFromGeometry(openingData.Geometry) / Define.mmToFeet;
            }
            else if (_shapeProfile == Define.CYLYNDER_FAMILY) {
                _radius = ConvertOpenningStringToObjectReVit.GetRadiusFromGeometry(openingData.Geometry) / Define.mmToFeet;
            }
        }

        private CurveArray GetProfile(XYZ origin, GeometryDetail openingData)
        {
            CurveArray curveArray = null;
            if (_shapeProfile == Define.RECTANGLE_FAMILY) {
                curveArray = SetRectangleProfile(origin, _lenght, _width);
            }
            else if (_shapeProfile == Define.CYLYNDER_FAMILY) {
                curveArray = SetCylynderProfile(origin, _radius);
            }
            return curveArray;
        }

        private CurveArray SetRectangleProfile(XYZ origin, double lenght, double width)
        {
            GetPlanVector(_direction, out XYZ right, out XYZ front);
            CurveArray curveArray = new CurveArray();
            XYZ leftTopPoint = origin + front * lenght / 2 + right.Negate() * width / 2;
            XYZ rightTopPoint = origin + front * lenght / 2 + right * width / 2;
            XYZ rightBottom = origin + front.Negate() * lenght / 2 + right * width / 2;
            XYZ leftBottom = origin + front.Negate() * lenght / 2 + right.Negate() * width / 2;

            Line topLine = Line.CreateBound(leftTopPoint, rightTopPoint);
            Line rightLine = Line.CreateBound(rightTopPoint, rightBottom);
            Line bottomLine = Line.CreateBound(rightBottom, leftBottom);
            Line leftLine = Line.CreateBound(leftBottom, leftTopPoint);

            curveArray.Append(topLine);
            curveArray.Append(rightLine);
            curveArray.Append(bottomLine);
            curveArray.Append(leftLine);

            return curveArray;
        }

        private void GetPlanVector(XYZ nomal, out XYZ right, out XYZ front)
        {
            right = null;
            front = null;
            if (Common.IsEqual(nomal, XYZ.BasisZ) || Common.IsEqual(nomal, XYZ.BasisZ.Negate())) {
                right = XYZ.BasisX;
                front = XYZ.BasisY;
            }
            else {
                if (Common.IsParallelVectorPlan(nomal, XYZ.BasisX, XYZ.BasisY)) {
                    front = XYZ.BasisZ;
                    right = nomal.CrossProduct(front);
                }
            }
        }

        private CurveArray SetCylynderProfile(XYZ origin, double radius)
        {
            CurveArray curveArray = new CurveArray();
            Arc topArc = Arc.Create(origin, radius, 0, Math.PI, XYZ.BasisX, XYZ.BasisY);
            Arc bottomArc = Arc.Create(origin, radius, Math.PI, Math.PI * 2, XYZ.BasisX, XYZ.BasisY);

            curveArray.Append(topArc);
            curveArray.Append(bottomArc);

            return curveArray;
        }

        #endregion Common
    }
}