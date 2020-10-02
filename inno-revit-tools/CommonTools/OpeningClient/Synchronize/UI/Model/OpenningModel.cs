using Autodesk.Revit.DB;
using CommonTools.OpeningClient.JsonObject;
using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Support;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using RectangleGeometry = CommonTools.OpeningClient.JsonObject.RectangleGeometry;

namespace CommonTools.OpeningClient.Synchronize.UI.Model
{
    public class Obj : ObservableObject
    {
        public ComparisonCoupleElement _comparisonCoupleElement;
        private Document _doc;

        public ICommand ShowIdCommand { get; set; }
        public ICommand ShowCommentCommand { get; set; }

        public Obj(ref ComparisonCoupleElement comparisonCoupleElement, Document document)
        {
            _level = "None";

            _doc = document;
            IsSelected = false;
            ObjActions = new ObservableCollection<StatusItem>();
            StatusItem pull = new StatusItem("PUSH");
            StatusItem push = new StatusItem("PULL");
            StatusItem none = new StatusItem("NONE");
            StatusItem disconnect = new StatusItem("DISCONNECT");

            ObjActions.Add(pull);
            ObjActions.Add(push);
            ObjActions.Add(none);
            ObjActions.Add(disconnect);

            _comparisonCoupleElement = comparisonCoupleElement;
            Implement(_comparisonCoupleElement);

            SolidColorBrush greenBrush = new SolidColorBrush();
            greenBrush.Color = Colors.Green;

            SolidColorBrush redBrush = new SolidColorBrush();
            redBrush.Color = Colors.Red;

            SolidColorBrush purpleBrush = new SolidColorBrush();
            redBrush.Color = Colors.Purple;

            SolidColorBrush yelowBrush = new SolidColorBrush();
            yelowBrush.Color = Colors.Orange;

            SolidColorBrush blueBrush = new SolidColorBrush();
            blueBrush.Color = Colors.Blue;

            if (_comparisonCoupleElement.Action != OpeningClient.Model.Action.DISCONNECT) {
                if (_comparisonCoupleElement.LocalGeometry != null && _comparisonCoupleElement.ServerGeometry != null) {
                    if (!_comparisonCoupleElement.IsSameShapeAndLocation()) {
                        if (ElementVersionCurent == ElementVersionLastest) {
                            _comparisonCoupleElement.Action = OpeningClient.Model.Action.PUSH;
                            Status = "Edit by Curent User";
                        }
                        else {
                            _comparisonCoupleElement.Action = OpeningClient.Model.Action.PULL;
                            Status = "Edit by Another User";
                        }

                        _colorStatus = yelowBrush;
                    }
                    else {
                        _comparisonCoupleElement.Action = OpeningClient.Model.Action.NONE;
                        Status = "Nomal";
                        _colorStatus = greenBrush;
                    }
                }
                else {
                    if (_comparisonCoupleElement.ServerStatus != "Disconnect") {
                        if (_comparisonCoupleElement.LocalGeometry == null) {
                            _comparisonCoupleElement.Action = OpeningClient.Model.Action.NONE;
                            if (_comparisonCoupleElement.LocalStatus == "Deleted") {
                                Status = "Deleting";
                                _colorStatus = redBrush;
                            }
                            else {
                                Status = "New Opening From Server";
                                _colorStatus = blueBrush;
                            }
                        }
                        else if (_comparisonCoupleElement.ServerGeometry == null) {
                            _comparisonCoupleElement.Action = OpeningClient.Model.Action.NONE;
                            if (_comparisonCoupleElement.ServerStatus == "PendingDelete") {
                                Status = "Pending Delete";
                                _colorStatus = redBrush;
                            }
                            else {
                                Status = "New Opening From Drawing";
                                _colorStatus = blueBrush;
                            }
                        }
                    }
                    else {
                        if (_comparisonCoupleElement.LocalGeometry == null) {
                            _comparisonCoupleElement.Action = OpeningClient.Model.Action.DISCONNECT;
                            Status = "Deleting";
                            _colorStatus = redBrush;
                        }
                        else if (_comparisonCoupleElement.ServerGeometry == null) {
                            _comparisonCoupleElement.Action = OpeningClient.Model.Action.DISCONNECT;
                            Status = "Pending Delete";
                            _colorStatus = redBrush;
                        }
                        else if (_comparisonCoupleElement.LocalGeometry == null &&
                                 _comparisonCoupleElement.ServerGeometry == null) {
                            _comparisonCoupleElement.Action = OpeningClient.Model.Action.DISCONNECT;
                            Status = "Deleted";
                            _colorStatus = redBrush;
                        }
                    }
                }
            }
            else if (_comparisonCoupleElement.Action == OpeningClient.Model.Action.DISCONNECT) {
                _colorStatus = purpleBrush;
                Status = "Disconect";
            }

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(ColorStatus)));
            OpeningClient.Model.Action action = _comparisonCoupleElement.Action;
            switch (action) {
                case OpeningClient.Model.Action.PUSH:
                    CurrentAction = ObjActions[0];//.Where(x => x.Name == "PUSH") as StatusItem;
                    break;

                case OpeningClient.Model.Action.PULL:
                    CurrentAction = ObjActions.Where(x => x.Name == "PULL").First() as StatusItem;
                    break;

                case OpeningClient.Model.Action.NONE:
                    CurrentAction = ObjActions.Where(x => x.Name == "NONE").First() as StatusItem;
                    break;

                case OpeningClient.Model.Action.DISCONNECT:
                    CurrentAction = ObjActions.Where(x => x.Name == "DISCONNECT").First() as StatusItem;
                    break;
            }
            Level level = GetlevelFromOpening();
            if (_comparisonCoupleElement.LocalGeometry != null) {
                XYZ curentLocation = GetOriginalFromJson(_comparisonCoupleElement.LocalGeometry.Original);
                if (level != null)
                    _level = level.Name;
                else
                    _level = "None";
                Grid grid1 = GetNearestGridInLevel(level, curentLocation, out Grid seconLocalGr);
                if (grid1 != null)
                    _markLocationCurent = grid1.Name;
                if (seconLocalGr != null)
                    _markLocationCurent += ("-" + seconLocalGr.Name);
            }

            if (_comparisonCoupleElement.LocalGeometry == null && _comparisonCoupleElement.ServerGeometry != null) {
                XYZ lastestLocation = GetOriginalFromJson(_comparisonCoupleElement.ServerGeometry.Original);
                _level = level.Name;
                Grid grid = GetNearestGridInLevel(level, lastestLocation, out Grid seconSeverGr);
                if (grid != null)
                    _markLocationLastest = grid.Name;
                if (seconSeverGr != null) {
                    _markLocationLastest += ("-" + seconSeverGr.Name);
                }
            }

            if (_level == "") {
                _level = "NULL";
            }

            if (_markLocationLastest == null || _markLocationLastest == "") {
                _markLocationLastest = "NULL";
            }

            if (_markLocationCurent == null || _markLocationCurent == "") {
                _markLocationCurent = "NULL";
            }

            if (/*_comparisonCoupleElement.LocalGeometry == null && */_comparisonCoupleElement.ServerGeometry != null) {
                if (DimentionLastest.Contains("Cylynder")) {
                    ElementName += "C";
                }
                else {
                    ElementName += "R";
                }
                if (DirectionLastest.Contains("0/0/1")) {
                    ElementName += "F/";
                }
                else {
                    ElementName += "W/";
                }
            }
            else if (_comparisonCoupleElement.LocalGeometry != null /*&& _comparisonCoupleElement.ServerGeometry == null*/) {
                if (DimentionCurent.Contains("Cylynder")) {
                    ElementName += "C";
                }
                else {
                    ElementName += "R";
                }
                if (DirectionCurent.Contains("0/0/1")) {
                    ElementName += "F_";
                }
                else {
                    ElementName += "W_";
                }
            }

            ElementName += Level + "/";
            if (_markLocationCurent != "NULL")
                ElementName += _markLocationCurent + "/";
            if (_markLocationLastest != "NULL")
                ElementName += _markLocationLastest;
        }

        public void SetValueAction()
        {
            _comparisonCoupleElement.Action = CommonTools.OpeningClient.Model.Action.DISCONNECT;
        }

        private void Implement(ComparisonCoupleElement comparisonCoupleElement)
        {
            GeometryDetail openningModelSever = comparisonCoupleElement.ServerGeometry;
            GeometryDetail openningModelLocal = comparisonCoupleElement.LocalGeometry;
            if (openningModelLocal != null) {
                if (openningModelLocal.Original != null) {
                    string original = GetOriginalStrFromJson(openningModelLocal.Original);
                    CoordinateCurent = original;
                }

                if (openningModelLocal.Geometry != null) {
                    string geometry = GetGeometryStrFromJson(openningModelLocal.Geometry);
                    DimentionCurent = geometry;
                }

                if (openningModelLocal.Direction != null) {
                    string direction = GetDirectionStrFromJson(openningModelLocal.Direction);
                    DirectionCurent = direction;
                }

                if (comparisonCoupleElement.CurrentVersionGeometryOfLocal != null) {
                    ElementVersionCurent = comparisonCoupleElement.CurrentVersionGeometryOfLocal;
                }
            }
            if (openningModelSever != null) {
                if (openningModelSever.Original != null) {
                    string original = GetOriginalStrFromJson(openningModelSever.Original);
                    CoordinateLastest = original;
                }
                if (openningModelSever.Geometry != null) {
                    string geometry = GetGeometryStrFromJson(openningModelSever.Geometry);
                    DimentionLastest = geometry;
                }
                if (openningModelSever.Direction != null) {
                    string direction = GetDirectionStrFromJson(openningModelSever.Direction);
                    DirectionLastest = direction;
                }
                if (openningModelSever.Version != null) {
                    ElementVersionLastest = openningModelSever.Version;
                    ElementVersionLastestDate = String.Format("{0:f}", openningModelSever.UpdatedDate);
                }
            }
            Id = comparisonCoupleElement.Id;
            revitElementId = comparisonCoupleElement.IdRevitElement;
            _comment = comparisonCoupleElement.Comment;
        }

        private string GetDirectionStrFromJson(string direction)
        {
            DirectionAndAngle directionAndAngle = JsonConvert.DeserializeObject<DirectionAndAngle>(direction);
            if (directionAndAngle != null) {
                double.TryParse(directionAndAngle.Angle, out double angle);
                angle = (180 / Math.PI) * angle;
                angle = Math.Round(angle, 2);
                return directionAndAngle.Direction + "/" + angle.ToString();
            }
            else {
                return null;
            }
        }

        private string GetGeometryStrFromJson(string geometry)
        {
            RectangleGeometry rectangleGeometry = JsonConvert.DeserializeObject<RectangleGeometry>(geometry);
            if (rectangleGeometry.ShapeName == Define.RECTANGLE_FAMILY) {
                return rectangleGeometry.ShapeName + "/" + rectangleGeometry.Lenght + "/" + rectangleGeometry.Width + "/" + rectangleGeometry.Height;
            }
            else if (rectangleGeometry.ShapeName == Define.CYLYNDER_FAMILY) {
                CylynderGeometry cylynderGeometry = JsonConvert.DeserializeObject<CylynderGeometry>(geometry);
                return cylynderGeometry.ShapeName + "/" + cylynderGeometry.Radius + "/" + cylynderGeometry.Height;
            }
            else {
                return null;
            }
        }

        private string GetOriginalStrFromJson(string original)
        {
            Origin origin = JsonConvert.DeserializeObject<Origin>(original);
            if (origin != null) {
                XYZ originXYZ = ConvertOpenningStringToObjectReVit.GetOriginal(original) * 304.8;
                return Round2(originXYZ.X).ToString() + "/" + Round2(originXYZ.Y).ToString() + "/" + Round2(originXYZ.Z).ToString();
            }
            else {
                return null;
            }
        }

        private XYZ GetOriginalFromJson(string original)
        {
            Origin origin = JsonConvert.DeserializeObject<Origin>(original);
            if (origin != null) {
                XYZ originXYZ = ConvertOpenningStringToObjectReVit.GetOriginal(original) * 304.8;
                return originXYZ;
            }
            else {
                return null;
            }
        }

        private Grid GetNearestGridInLevel(Level level, XYZ point, out Grid seconGrid)
        {
            seconGrid = null;

            List<Grid> grids = GetGridsFromLevel(level);
            if (grids != null && grids.Count > 1) {
                SortedDictionary<double, Grid> sorted = new SortedDictionary<double, Grid>();
                foreach (var grid in grids) {
                    double distance = GetDistanceGridToPoint(grid, point);
                    if (!sorted.ContainsKey(distance))
                        sorted.Add(distance, grid);
                }

                List<Grid> firthAndseconGrid = new List<Grid>();
                int count = 0;
                foreach (var pair in sorted) {
                    Grid grid = pair.Value;
                    firthAndseconGrid.Add(grid);
                    count++;
                    if (count == 2)
                        break;
                }
                seconGrid = firthAndseconGrid[1];
                return firthAndseconGrid[0];
            }
            else {
                return null;
            }
        }

        private double GetDistanceGridToPoint(Grid grid, XYZ point)
        {
            XYZ projectpoin = new XYZ(point.X / 304.8, point.Y / 304.8, grid.Curve.GetEndPoint(0).Z);
            Line grline = grid.Curve as Line;
            XYZ firtPoint = grline.GetEndPoint(0);
            XYZ seconPoint = grline.GetEndPoint(1);
            XYZ nomal = (grline.Direction).CrossProduct(XYZ.BasisZ).Normalize();

            Plane plane = Plane.CreateByNormalAndOrigin(nomal, grline.Origin);
            plane.Project(projectpoin, out UV uV, out double distance);
            Transaction transaction = new Transaction(_doc);
            Line line = Line.CreateBound(projectpoin, projectpoin + XYZ.BasisY);

            return distance;
        }

        private List<Grid> GetGridsFromLevel(Level level)
        {
            if (level != null) {
                FilteredElementCollector viewCollector = new FilteredElementCollector(_doc);
                List<Element> viewCollection = viewCollector.OfClass(typeof(Autodesk.Revit.DB.View)).ToElements().ToList();
                Autodesk.Revit.DB.View levelView = null;
                foreach (Autodesk.Revit.DB.View view in viewCollection) {
                    Parameter viewLevel = view.LookupParameter("Associated Level");
                    if (viewLevel != null)
                        if (viewLevel.AsString() == level.Name) {
                            levelView = view;
                            break;
                        }
                }
                if (levelView != null) {
                    FilteredElementCollector GridCollector = new FilteredElementCollector(_doc, levelView.Id);
                    List<Element> elems = GridCollector.OfClass(typeof(Grid)).ToList();
                    List<Grid> grids = elems.ConvertAll(x => x as Grid);
                    return grids;
                }
            }
            return null;
        }

        private double Round2(double number)
        {
            number = Math.Round(number, 2);
            return number;
        }

        private Level GetLevelFromLocation(GeometryDetail geometryDetail)
        {
            if (geometryDetail != null) {
                XYZ curentLocation = GetOriginalFromJson(geometryDetail.Original) / 304.8;
                Autodesk.Revit.DB.Level level = Common.GetNearestLevel(_doc, curentLocation);
                return level;
            }
            else {
                return null;
            }
        }

        private Level GetlevelFromOpening()
        {
            if (Common.IsValidGuid(_comparisonCoupleElement.IdRevitElement)) {
                Element element = _doc.GetElement(_comparisonCoupleElement.IdRevitElement);
                if (element == null) {
                    return null;
                }
                if (element is Opening) {
                    Opening opening = element as Opening;
                    Parameter parameter = opening.LookupParameter("Base Constraint");
                    Level level = _doc.GetElement(parameter.AsElementId()) as Level;
                    return level;
                }
                else {
                    Level level = _doc.GetElement(element.LevelId) as Level;
                    if (level == null) {
                        XYZ locationPoint = (element.Location as LocationPoint).Point;
                        level = GetLevelFromLocation(_comparisonCoupleElement.LocalGeometry);
                    }
                    return level;
                }
            }
            else {
                return GetLevelFromLocation(_comparisonCoupleElement.ServerGeometry);
            }
        }

        public string Id { get; set; }
        public string revitElementId { get; set; }

        private string _comment;

        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(_comment)));
                _comparisonCoupleElement.Comment = _comment;
            }
        }

        public string ElementName { get; set; }
        public string ElementVersionCurentDate { get; set; }
        public string ElementVersionLastestDate { get; set; }
        public string ElementVersionCurent { get; set; }
        public string ElementVersionLastest { get; set; }
        public string DimentionCurent { get; set; }
        public string DimentionLastest { get; set; }
        public string CoordinateCurent { get; set; }
        public string CoordinateLastest { get; set; }
        public string DirectionCurent { get; set; }
        public string DirectionLastest { get; set; }
        public string Status { get; set; }
        private string _level;
        public string Level { get => _level; set { _level = value; } }

        private string _levelLocal;
        public string LevelLocal { get => _levelLocal; set { _levelLocal = value; } }
        public ObservableCollection<InfileObject> Infile { get; set; }
        private InfileObject _currentInfile;

        public InfileObject CurrentInfile
        {
            get => _currentInfile;
            set
            {
                _currentInfile = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(_currentAction)));
                //UpdateAction();
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(_isSelected)));
                //UpdateAction();
            }
        }

        private string _markLocationCurent;
        private SolidColorBrush _colorStatus;

        public SolidColorBrush ColorStatus
        {
            get
            {
                return _colorStatus;
            }

            set
            {
                _colorStatus = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(_colorStatus)));
            }
        }

        public string MarkLocationCurent
        {
            get => _markLocationCurent;
            set
            {
                _markLocationCurent = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(_currentAction)));
            }
        }

        private string _markLocationLastest;

        public string MarkLocationLastest
        {
            get => _markLocationLastest;
            set
            {
                _markLocationLastest = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(_markLocationLastest)));
            }
        }

        public ObservableCollection<StatusItem> ObjActions { get; set; }

        private StatusItem _currentAction;

        public StatusItem CurrentAction
        {
            get => _currentAction;
            set
            {
                _currentAction = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(_currentAction)));
                UpdateAction();
            }
        }

        private void UpdateAction()
        {
            switch (_currentAction.Name) {
                case "PULL":
                    _comparisonCoupleElement.Action = CommonTools.OpeningClient.Model.Action.PULL;
                    if (_comparisonCoupleElement.LocalStatus == "Disconnect") {
                        _comparisonCoupleElement.LocalStatus = "Normal";
                    }
                    //_comparisonCoupleElement.LocalStatus = "Normal";
                    break;

                case "PUSH":
                    _comparisonCoupleElement.Action = CommonTools.OpeningClient.Model.Action.PUSH;
                    if (_comparisonCoupleElement.LocalStatus == "Disconnect") {
                        if (_comparisonCoupleElement.LocalGeometry != null) {
                            _comparisonCoupleElement.LocalStatus = "Normal";
                        }
                        else {
                            _comparisonCoupleElement.LocalStatus = "Deleted";
                        }
                    }
                    //
                    break;

                case "NONE":
                    _comparisonCoupleElement.Action = CommonTools.OpeningClient.Model.Action.NONE;
                    //_comparisonCoupleElement.LocalStatus = "Normal";
                    break;

                case "DISCONNECT":
                    _comparisonCoupleElement.Action = CommonTools.OpeningClient.Model.Action.DISCONNECT;
                    _comparisonCoupleElement.LocalStatus = "Disconnect";
                    break;
            }
        }
    }

    abstract public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void Set<T>(ref T field, T value, string propertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class StatusItem : ObservableObject
    {
        public StatusItem(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class InfileObject : ObservableObject
    {
        public InfileObject(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}