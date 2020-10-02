using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Support;
using System.Linq;
using System.Windows.Forms;
using CommonUtils;

namespace CommonTools.OpeningClient.Synchronize.Process.Subject
{
    internal class MEPOpeningMaker
    {
        private bool _isVertical;
        private Document _doc;
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

        public MEPOpeningMaker(Document document, ComparisonCoupleElement openningModel, string projectName = "")
        {
            _doc = document;
            _comparionCoupleElement = openningModel;
            _openningModel = openningModel.ServerGeometry;
            GetValueGeometry(openningModel.ServerGeometry);
            if (Common.IsEqual(_direction, XYZ.BasisZ) || Common.IsEqual(_direction, XYZ.BasisZ.Negate())) {
                _isVertical = true;
                if (_shapeProfile == Define.RECTANGLE_FAMILY) {
                    _openningName = Define.FLOOR_OPENNING_RECTANGLE_NAME_MEP;
                }
                else if (_shapeProfile == Define.CYLYNDER_FAMILY) {
                    _openningName = Define.FLOOR_OPENNING_CYLYNDER_NAME_MEP;
                }
            }
            else {
                _isVertical = false;
                if (_shapeProfile == Define.RECTANGLE_FAMILY) {
                    _openningName = Define.WALL_OPENNING_RECTANGLE_NAME_MEP;
                }
                else if (_shapeProfile == Define.CYLYNDER_FAMILY) {
                    _openningName = Define.WALL_OPENNING_CYLYNDER_NAME_MEP;
                }
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

        public string LoadFammilyInstance(Transaction transaction)
        {
            transaction.Start("Generate Openning");
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            FamilySymbol symbol = collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel).FirstOrDefault(x => x.Name.Equals(_openningName)) as FamilySymbol;
            symbol.Activate();
            FamilyInstance instance = _doc.Create.NewFamilyInstance(_origin, symbol, StructuralType.UnknownFraming);
            _comparionCoupleElement.IdRevitElement = instance.UniqueId;
            transaction.Commit();
            UpdateGeometryAndDirection(transaction, instance.Id);
            return instance.UniqueId;
        }

        public string LoadFammilyInstanceForPreview(Transaction transaction)
        {
            transaction.Start("Generate Openning");
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            FamilySymbol symbol = collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel).FirstOrDefault(x => x.Name.Equals(_openningName)) as FamilySymbol;
            symbol.Activate();
            FamilyInstance instance = _doc.Create.NewFamilyInstance(_origin, symbol, StructuralType.UnknownFraming);
            //_comparionCoupleElement.IdRevitElement = instance.UniqueId;
            transaction.Commit();
            UpdateGeometryAndDirection(transaction, instance.Id);
            return instance.UniqueId;
        }

        public void UpdateGeometryAndDirection(Transaction transaction, ElementId id)
        {
            transaction.Start("Generate Openning");
            Element element = _doc.GetElement(id);
            if (_isVertical == true) {
                if (_shapeProfile == Define.RECTANGLE_FAMILY) {
                    double width = _width;
                    double length = _lenght;
                    if (_lenght < _width) {
                        width = _lenght;
                        length = _width;
                    }
                    UpdateParam(element, width, Define.WIDTH_PARAMETER_NAME_MEP);
                    UpdateParam(element, length, Define.LENGHT_PARAMETER_NAME_MEP);
                    UpdateParam(element, _height, Define.HEIGHT_PARAMETER_NAME_MEP);
                }
                else if (_shapeProfile == Define.CYLYNDER_FAMILY) {
                    UpdateParam(element, _radius, Define.RADIUS_PARAMETER_NAME_MEP);
                    UpdateParam(element, _height, Define.HEIGHT_PARAMETER_NAME_MEP);
                }
            }
            else {
                if (_shapeProfile == Define.RECTANGLE_FAMILY) {
                    UpdateParam(element, _width, Define.WIDTH_PARAMETER_NAME_MEP);
                    UpdateParam(element, _lenght, Define.HEIGHT_PARAMETER_NAME_MEP);
                    UpdateParam(element, _height, Define.DEPTH_PARAMETER_NAME_MEP);
                }
                else if (_shapeProfile == Define.CYLYNDER_FAMILY) {
                    UpdateParam(element, _radius, Define.RADIUS_PARAMETER_NAME_MEP);
                    UpdateParam(element, _height, Define.DEPTH_PARAMETER_NAME_MEP);
                }
            }
            EditLocation(element);
            if (_isVertical == true) {
                UpdateAngle(id);
            }
            else {
                UpdateDirection(id);
            }

            transaction.Commit();
        }

        private void EditLocation(Element opening)
        {
            XYZ curentLocation = Common.GetFamilyInstanceOrigin(opening);
            XYZ tranformVector = _origin - curentLocation;
            ElementTransformUtils.MoveElement(_doc, opening.Id, tranformVector);
        }

        private void UpdateParam(Element element, double value, string name)
        {
            Parameter parameter = element.LookupParameter(name);
            if (parameter != null && parameter.IsReadOnly != true) {
                parameter.Set(value);
            }
            else {
                Utils.MessageWarning(name + " Parameter is null");
            }
        }

        private void UpdateDirection(ElementId id)
        {
            Element element = _doc.GetElement(id);
            FamilyInstance familyInstance = element as FamilyInstance;
            XYZ basicZ = XYZ.BasisZ;
            Line axis = Line.CreateBound((_origin), (_origin + basicZ));
            double angle = familyInstance.FacingOrientation.AngleOnPlaneTo(_direction, basicZ);
            ElementTransformUtils.RotateElement(_doc, id, axis, angle);
        }

        private void UpdateAngle(ElementId id)
        {
            XYZ basicZ = XYZ.BasisZ;
            Line axis = Line.CreateBound((_origin), (_origin + basicZ));
            double localAngle = 0;
            if (_comparionCoupleElement.LocalGeometry != null)
                localAngle = ConvertOpenningStringToObjectReVit.getAngle(_comparionCoupleElement.LocalGeometry.Direction);
            double angle = _angle - localAngle;
            ElementTransformUtils.RotateElement(_doc, id, axis, angle);
        }
    }
}