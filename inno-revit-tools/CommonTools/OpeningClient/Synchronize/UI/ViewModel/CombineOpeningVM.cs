using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommonTools.OpeningClient.Service;
using CommonTools.OpeningClient.Support;
using CommonTools.OpeningClient.Synchronize.UI.Model;
using CommonTools.OpeningClient.Synchronize.UI.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CommonTools.OpeningClient.Synchronize.UI.ViewModel
{
    internal class CombineOpeningVM : BindableBase
    {
        private Document _document;
        private SolidColorBrush _lightBrush;
        private SolidColorBrush _darkBrush;
        private string _catergoryName;
        private string _stateUniquId;

        public CombineOpeningVM(Document document, string catergoryName)
        {
            ListObjectCurent = new ObservableCollection<Obj>();
            _catergoryName = catergoryName;
            InitData(document);
            InitCommand();
            FilteredElementCollector collecotr = new FilteredElementCollector(document);
            //IEnumerable<Autodesk.Revit.DB.View> secs = from Element f in collecotr where (f as Autodesk.Revit.DB.View).CanBePrinted == true select f as Autodesk.Revit.DB.View;
            collecotr.OfClass(typeof(Autodesk.Revit.DB.View));
            List<Element> elements = collecotr.ToList();
            ListView = elements.ConvertAll(x => x as Autodesk.Revit.DB.View);
            ListView = ListView.Where(x => x.CanBePrinted == true).ToList();
            IComparer<Element> comparer = new MyOrderingClass();
            ListView.Sort(comparer);

            //UserView = new PreviewControl(document, CurentView.Id);
        }

        private void InitData(Document document)
        {
            _document = document;
            ElementVersionCurentDate = "";
            ElementVersionLastestDate = "";
            ElementVersionCurent = "";
            ElementVersionLastest = "";
            DimentionCurent = "";
            DimentionLastest = "";
            CoordinateCurent = "";
            CoordinateLastest = "";
            DirectionCurent = "";
            DirectionLastest = "";
            MarkLocationCurent = "";
            MarkLocationLastest = "";
            //set up button color
            _lightBrush = new SolidColorBrush();
            _lightBrush.Color = Colors.White;
            _darkBrush = new SolidColorBrush();
            _darkBrush.Color = Colors.LightGray;
            SetColorButton(true, false, false);
            IsDissablePreview = true;
            IsEnablePreview = true;
            IsSynch = true;
            IsOther = false;
            IsOnServer = false;
            PreviewVisibility = true;

            _username = Repository.GetUserNamebyMac();
            IsPushAllow = true;
            IsPullAllow = true;

            SelectedObj = null;
        }

        private void SetColorButton(bool isSynchEnable, bool isNewEnable, bool isOnserverEnable)
        {
            try {
                BackgroundSynchronizedColor = _lightBrush;
                BackgroundNewColor = _lightBrush;
                BackgroundOnseverColor = _lightBrush;
                if (isSynchEnable) {
                    BackgroundSynchronizedColor = _darkBrush;
                }
                if (isNewEnable) {
                    BackgroundNewColor = _darkBrush;
                }
                if (isOnserverEnable) {
                    BackgroundOnseverColor = _darkBrush;
                }
                OnPropertyChanged(nameof(BackgroundSynchronizedColor));
                OnPropertyChanged(nameof(BackgroundNewColor));
                OnPropertyChanged(nameof(BackgroundOnseverColor));
            }
            catch {
                UserView.Dispose();
            }
        }

        private void InitCommand()
        {
            try {
                // MouseDownCommand = new RelayCommand<object>(ListObj_MouseDownCommandInvoke);
                SynchCommand = new RelayCommand<object>(SynchCommandInvoke);
                NewCommand = new RelayCommand<object>(NewCommandInvoke);
                OnServerCommand = new RelayCommand<object>(OnServerCommandInvoke);
                CurentPreview = new RelayCommand<object>(CurentPreviewInvoke);
                LastestPreview = new RelayCommand<object>(LastestPreviewInvoke);
                //SwitchSynchronizedCommand = new RelayCommand<object>(SwitchSynchronizedCommandInvoke);
                PullAllCommand = new RelayCommand<object>(PullAllCommandInvoke);
                PushAllCommand = new RelayCommand<object>(PushAllCommandInvoke);
                DisconectAllCommand = new RelayCommand<object>(DisconectAllCommandInvoke);
                NoneAllCommand = new RelayCommand<object>(NoneAllCommandInvoke);
                CloseComand = new RelayCommand<object>(CloseComandInvoke);
                SendCommand = new RelayCommand<object>(SendCommandInvoke);
            }
            catch {
                UserView.Dispose();
            }
        }

        public ICommand CurentPreview { get; set; }

        private void CurentPreviewInvoke(object obj)
        {
            Transaction transaction = new Transaction(_document);
            try {
                Ultil.ClearUniqueIdState(transaction, _document, ref _stateUniquId);
                Ultil.VisibilityElement(_document, CurentView, RevitElementId, false);
                Ultil.HightLight(transaction, _document, CurentView.Id, RevitElementId, true);
            }
            catch {
                UserView.Dispose();
                transaction.RollBack();
            }
        }

        public ICommand LastestPreview { get; set; }

        private void LastestPreviewInvoke(object obj)
        {
            Transaction transaction = new Transaction(_document);
            try {
                Ultil.HightLight(transaction, _document, CurentView.Id, RevitElementId, false);
                Ultil.VisibilityElement(_document, CurentView, RevitElementId, true);
                Ultil.ClearUniqueIdState(transaction, _document, ref _stateUniquId);
                Obj selectedObject = SelectedObj as Obj;
                _stateUniquId = Ultil.CreateOpening(transaction, _document, _catergoryName, selectedObject);
                if (_stateUniquId == "") {
                    MessageBox.Show("Can't create this opening. Maybe this opening is not intersect with Wall");
                }
                Ultil.HightLight(transaction, _document, CurentView.Id, _stateUniquId, true);
            }
            catch {
                UserView.Dispose();
                transaction.RollBack();
            }
            //RevitElementId
        }

        public ICommand SendCommand { get; set; }

        private void SendCommandInvoke(object obj)
        {
            try {
                Comment += "\n" + _username + " :" + CurentComment;
                Obj selectedObject = _selectedObj as Obj;
                selectedObject.Comment = Comment;
                CurentComment = "";
                OnPropertyChanged(nameof(Comment));
                OnPropertyChanged(nameof(CurentComment));
            }
            catch {
                UserView.Dispose();
            }
        }

        public ICommand PullAllCommand { get; set; }

        private void PullAllCommandInvoke(object obj)
        {
            try {
                foreach (Obj item in ListObjectCurent) {
                    if (item.IsSelected == true) {
                        item._comparisonCoupleElement.Action = OpeningClient.Model.Action.PULL;
                        item.CurrentAction = item.ObjActions.Where(x => x.Name.Equals("PULL")).First() as StatusItem;
                        OnPropertyChanged(nameof(item.CurrentAction));
                    }
                }
                if (IsSynch) {
                    setCurentList(ListObjHasSynch);
                }
                if (IsOnServer) {
                    //ListObjectCurent = null;
                    setCurentList(ListObjOnStack);
                }
                if (IsSynch == false && IsOnServer == false) {
                    setCurentList(ListObjBelowLocal);
                }
            }
            catch {
                UserView.Dispose();
            }
        }

        public ICommand PushAllCommand { get; set; }

        private void PushAllCommandInvoke(object obj)
        {
            try {
                foreach (Obj item in ListObjectCurent) {
                    if (item.IsSelected == true) {
                        item._comparisonCoupleElement.Action = OpeningClient.Model.Action.PUSH;
                        item.CurrentAction = item.ObjActions.Where(x => x.Name.Equals("PUSH")).First() as StatusItem;
                        OnPropertyChanged(nameof(item.CurrentAction));
                    }
                }
                if (IsSynch) {
                    setCurentList(ListObjHasSynch);
                }
                if (IsOnServer) {
                    setCurentList(ListObjOnStack);
                }
                if (IsSynch == false && IsOnServer == false) {
                    setCurentList(ListObjBelowLocal);
                }
                OnPropertyChanged(nameof(ListObjectCurent));
            }
            catch {
                UserView.Dispose();
            }
        }

        public ICommand NoneAllCommand { get; set; }

        private void NoneAllCommandInvoke(object obj)
        {
            try {
                foreach (Obj item in ListObjectCurent) {
                    if (item.IsSelected == true) {
                        item._comparisonCoupleElement.Action = OpeningClient.Model.Action.NONE;
                        item.CurrentAction = item.ObjActions.Where(x => x.Name.Equals("NONE")).First() as StatusItem;
                        OnPropertyChanged(nameof(item.CurrentAction));
                    }
                }
                if (IsSynch) {
                    setCurentList(ListObjHasSynch);
                }
                if (IsOnServer) {
                    //ListObjectCurent = null;
                    setCurentList(ListObjOnStack);
                }
                if (IsSynch == false && IsOnServer == false) {
                    setCurentList(ListObjBelowLocal);
                }
                OnPropertyChanged(nameof(ListObjectCurent));
            }
            catch {
                UserView.Dispose();
            }
        }

        public ICommand DisconectAllCommand { get; set; }

        private void DisconectAllCommandInvoke(object obj)
        {
            try {
                foreach (Obj item in ListObjectCurent) {
                    if (item.IsSelected == true) {
                        item._comparisonCoupleElement.Action = OpeningClient.Model.Action.DISCONNECT;
                        item.CurrentAction = item.ObjActions.Where(x => x.Name.Equals("DISCONNECT")).First() as StatusItem;
                        OnPropertyChanged(nameof(item.CurrentAction));
                    }
                }
                if (IsSynch) {
                    setCurentList(ListObjHasSynch);
                }
                if (IsOnServer) {
                    setCurentList(ListObjOnStack);
                }
                if (IsSynch == false && IsOnServer == false) {
                    setCurentList(ListObjBelowLocal);
                }
                OnPropertyChanged(nameof(ListObjectCurent));
            }
            catch {
                UserView.Dispose();
            }
        }

        public ICommand CloseComand { get; set; }

        private void CloseComandInvoke(object obj)
        {
        }

        public System.Windows.Point GetMousePositionWindowsForms()
        {
            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
            return new System.Windows.Point(point.X, point.Y);
        }

        public ICommand SynchCommand { get; set; }

        private void SynchCommandInvoke(object obj)
        {
            Transaction transaction = new Transaction(_document);
            try {
                transaction.Start("Synch");
                if (IsSynch) {
                    Ultil.VisibilityElement(_document, CurentView, RevitElementId, false);
                }
                IsPushAllow = true;
                IsPullAllow = true;
                SelectedIndex = -1;
                SelectedObj = null;
                Ultil.HightLight(transaction, _document, CurentView.Id, _stateUniquId, false);
                PreviewVisibility = true;
                OnPropertyChanged(nameof(PreviewVisibility));
                IsSynch = true;
                IsOther = false;
                IsOnServer = false;
                Ultil.ClearUniqueIdState(transaction, _document, ref _stateUniquId);
                setCurentList(ListObjHasSynch);
                SetColorButton(true, false, false);
                OnPropertyChanged(nameof(ListObjHasSynch));
                Visible = System.Windows.Visibility.Collapsed;
                OnPropertyChanged(nameof(Visible));
                OnPropertyChanged(nameof(IsSynch));
                OnPropertyChanged(nameof(IsOther));
                OnPropertyChanged(nameof(IsOnServer));
                OnPropertyChanged(nameof(IsPushAllow));
                OnPropertyChanged(nameof(IsPullAllow));
                transaction.Commit();
            }
            catch {
                UserView.Dispose();
                transaction.RollBack();
            }
        }

        public ICommand NewCommand { get; set; }

        private void NewCommandInvoke(object obj)
        {
            Transaction transaction = new Transaction(_document);
            try {
                if (IsSynch) {
                    Ultil.VisibilityElement(_document, CurentView, RevitElementId, false);
                }
                IsPushAllow = true;
                IsPullAllow = false;
                SelectedIndex = -1;
                SelectedObj = null;
                Ultil.HightLight(transaction, _document, CurentView.Id, _stateUniquId, false);
                PreviewVisibility = false;
                IsSynch = false;
                IsOther = true;
                IsOnServer = false;
                Ultil.ClearUniqueIdState(transaction, _document, ref _stateUniquId);
                setCurentList(ListObjBelowLocal);
                SetColorButton(false, true, false);
                OnPropertyChanged(nameof(ListObjBelowLocal));
                OnPropertyChanged(nameof(Visible));
                OnPropertyChanged(nameof(IsSynch));
                OnPropertyChanged(nameof(IsOther));
                OnPropertyChanged(nameof(IsOnServer));
                OnPropertyChanged(nameof(PreviewVisibility));
                OnPropertyChanged(nameof(IsPushAllow));
                OnPropertyChanged(nameof(IsPullAllow));
            }
            catch {
                UserView.Dispose();
                transaction.RollBack();
            }
        }

        public ICommand OnServerCommand { get; set; }

        private void OnServerCommandInvoke(object obj)
        {
            Transaction transaction = new Transaction(_document);
            try {
                if (IsSynch) {
                    Ultil.VisibilityElement(_document, CurentView, RevitElementId, false);
                }
                IsPushAllow = false;
                OnPropertyChanged(nameof(IsPushAllow));
                IsPullAllow = true;
                OnPropertyChanged(nameof(IsPullAllow));
                SelectedObj = null;
                Ultil.HightLight(transaction, _document, CurentView.Id, _stateUniquId, false);
                PreviewVisibility = false;
                OnPropertyChanged(nameof(PreviewVisibility));
                IsSynch = false;
                IsOther = true;
                IsOnServer = true;
                OnPropertyChanged(nameof(IsSynch));
                OnPropertyChanged(nameof(IsOther));
                OnPropertyChanged(nameof(IsOnServer));
                Ultil.ClearUniqueIdState(transaction, _document, ref _stateUniquId);
                setCurentList(ListObjOnStack);
                SetColorButton(false, false, true);
                OnPropertyChanged(nameof(Visible));
            }
            catch {
                UserView.Dispose();
                transaction.RollBack();
            }
        }

        public void setCurentList(ObservableCollection<Obj> listObjCurent)
        {
            try {
                ListObjectCurent.Clear();
                IComparer<Obj> comparer = new MyOrderingObjClass();
                List<Obj> objs = listObjCurent.ToList();
                if (objs != null && objs.Count > 1)
                    objs.Sort(comparer);
                foreach (Obj obj in objs) {
                    ListObjectCurent.Add(obj);
                }
                if (_listObjHasSynch == null) {
                    _listObjHasSynch = new ObservableCollection<Obj>();
                }
                OnPropertyChanged(nameof(ListObjectCurent));
                OnPropertyChanged(nameof(ListObjBelowLocal));
                OnPropertyChanged(nameof(ListObjHasSynch));
                OnPropertyChanged(nameof(ListObjOnStack));
                CurentView = _document.ActiveView;
                _curentSelectedObj = null;
            }
            catch {
                if (UserView != null)
                    UserView.Dispose();
            }
        }

        private bool _isEnablePreview;

        public bool IsEnablePreview
        {
            get => _isEnablePreview;

            set
            {
                SetProperty(ref _isEnablePreview, value);
                if (IsDissablePreview == IsEnablePreview)
                    IsDissablePreview = !IsEnablePreview;
            }
        }

        private bool _isDissablePreview;

        public bool IsDissablePreview
        {
            get => _isDissablePreview;

            set
            {
                SetProperty(ref _isDissablePreview, value);
                if (IsDissablePreview == IsEnablePreview) {
                    IsEnablePreview = !IsDissablePreview;
                }
            }
        }

        private List<Autodesk.Revit.DB.View> _listView;
        public List<Autodesk.Revit.DB.View> ListView { get => _listView; set => SetProperty(ref _listView, value); }

        private Autodesk.Revit.DB.View _curentView;

        public Autodesk.Revit.DB.View CurentView
        {
            get => _curentView;
            set
            {
                try {
                    SetProperty(ref _curentView, value);
                    if (UserView != null)
                        UserView.Dispose();
                    UserView = new PreviewControl(_document, CurentView.Id);
                    OnPropertyChanged(nameof(UserView));

                    if (!IsOnServer) {
                        ObservableCollection<Obj> ObjsForQuery = new ObservableCollection<Obj>();
                        if (IsSynch) {
                            ObjsForQuery = ListObjHasSynch;
                        }
                        else {
                            ObjsForQuery = ListObjBelowLocal;
                        }
                        List<Element> elements = Ultil.GetAllOpenningByView(_document, CurentView, _catergoryName);
                        List<string> uniqueIds = elements.ConvertAll(x => x.UniqueId);
                        ObservableCollection<Obj> listCurentObjectTerm = new ObservableCollection<Obj>();
                        foreach (Obj obj in ObjsForQuery) {
                            if (obj.Status == "Nomal") {
                                foreach (string uniqueId in uniqueIds) {
                                    if (obj.revitElementId.Equals(uniqueId)) {
                                        listCurentObjectTerm.Add(obj);
                                    }
                                }
                            }
                            else {
                                listCurentObjectTerm.Add(obj);
                            }
                        }
                        ListObjectCurent = listCurentObjectTerm;
                        OnPropertyChanged(nameof(ListObjectCurent));
                    }
                }
                catch {
                    UserView.Dispose();
                }
            }
        }

        public System.Windows.Visibility Visible { get; set; }
        public bool PreviewVisibility { get; set; }

        private string _username;

        public bool IsOther { get; set; }
        public bool IsSynch { get; set; }
        public bool IsOnServer { get; set; }
        public bool IsPushAllow { get; set; }
        public bool IsPullAllow { get; set; }
        public SolidColorBrush BackgroundSynchronizedColor { get; set; }
        public SolidColorBrush BackgroundNewColor { get; set; }
        public SolidColorBrush BackgroundOnseverColor { get; set; }

        public PreviewControl UserView { get; set; }

        private ObservableCollection<Obj> _listObjectCurent;

        public ObservableCollection<Obj> ListObjectCurent
        {
            get => _listObjectCurent;
            set
            {
                SetProperty(ref _listObjectCurent, value);
            }
        }

        private ObservableCollection<string> _groupDrawings;
        public ObservableCollection<string> GroupDrawings { get => _groupDrawings; set => SetProperty(ref _groupDrawings, value); }

        private ObservableCollection<Obj> _listObjHasSynch;
        public ObservableCollection<Obj> ListObjHasSynch { get => _listObjHasSynch; set => SetProperty(ref _listObjHasSynch, value); }

        private ObservableCollection<Obj> _listObjBelowLocal;
        public ObservableCollection<Obj> ListObjBelowLocal { get => _listObjBelowLocal; set => SetProperty(ref _listObjBelowLocal, value); }

        private ObservableCollection<Obj> _listObjOnStack;
        public ObservableCollection<Obj> ListObjOnStack { get => _listObjOnStack; set => SetProperty(ref _listObjOnStack, value); }

        private object _curentSelectedObj;

        private object _selectedObj;

        public int SelectedIndex { get; set; }

        public object SelectedObj
        {
            get => _selectedObj;
            set
            {
                Transaction transaction = new Transaction(_document);
                try {
                    SetProperty(ref _selectedObj, value);
                    if (_selectedObj != null) {
                        Obj selectedObject = _selectedObj as Obj;

                        if (_curentSelectedObj != _selectedObj) {
                            if (IsSynch) {
                                Ultil.VisibilityElement(_document, CurentView, RevitElementId, false);
                            }
                            _curentSelectedObj = _selectedObj;
                            Id = selectedObject.Id;
                            RevitElementId = selectedObject.revitElementId;
                            ElementVersionCurentDate = selectedObject.ElementVersionCurentDate;
                            ElementVersionLastestDate = selectedObject.ElementVersionLastestDate;
                            ElementVersionCurent = selectedObject.ElementVersionCurent;
                            ElementVersionLastest = selectedObject.ElementVersionLastest;
                            DimentionCurent = selectedObject.DimentionCurent;
                            DimentionLastest = selectedObject.DimentionLastest;
                            CoordinateCurent = selectedObject.CoordinateCurent;
                            CoordinateLastest = selectedObject.CoordinateLastest;
                            DirectionCurent = selectedObject.DirectionCurent;
                            DirectionLastest = selectedObject.DirectionLastest;
                            MarkLocationCurent = selectedObject.MarkLocationCurent;
                            MarkLocationLastest = selectedObject.MarkLocationLastest;
                            Status = selectedObject.Status;
                            Comment = selectedObject.Comment;

                            Ultil.ClearUniqueIdState(transaction, _document, ref _stateUniquId);

                            if (IsSynch) {
                                if (selectedObject._comparisonCoupleElement.IsSameShapeAndLocation()) {
                                    PreviewVisibility = false;
                                }
                                else {
                                    PreviewVisibility = true;
                                }
                                OnPropertyChanged(nameof(PreviewVisibility));
                            }

                            if (IsOnServer) {
                                if (IsEnablePreview) {
                                    _stateUniquId = Ultil.CreateOpening(transaction, _document, _catergoryName, selectedObject);
                                    if (_stateUniquId == "") {
                                        MessageBox.Show("Can't create this opening. Maybe this opening is not intersect with Wall");
                                    }
                                    else {
                                        Ultil.HightLight(transaction, _document, CurentView.Id, _stateUniquId, true);
                                    }
                                }
                            }

                            if (RevitElementId != null)
                                Ultil.HightLight(transaction, _document, CurentView.Id, RevitElementId, true);
                        }
                        else {
                            SelectedIndex = -1;
                            Ultil.ClearUniqueIdState(transaction, _document, ref _stateUniquId);
                            _curentSelectedObj = null;
                        }
                    }
                    else {
                        ElementVersionCurentDate = "";
                        ElementVersionLastestDate = "";
                        ElementVersionCurent = "";
                        ElementVersionLastest = "";
                        DimentionCurent = "";
                        DimentionLastest = "";
                        CoordinateCurent = "";
                        CoordinateLastest = "";
                        DirectionCurent = "";
                        DirectionLastest = "";
                        MarkLocationCurent = "";
                        MarkLocationLastest = "";
                    }
                }
                catch (Exception ex) {
                    UserView.Dispose();
                    transaction.RollBack();
                }
            }
        }

        private string _elementVersionCurentDate;

        public string ElementVersionCurentDate
        {
            get => _elementVersionCurentDate; set => SetProperty(ref _elementVersionCurentDate, value);
        }

        private string _elementVersionLastestDate;
        public string ElementVersionLastestDate { get => _elementVersionLastestDate; set => SetProperty(ref _elementVersionLastestDate, value); }

        private string _elementVersionCurent;
        public string ElementVersionCurent { get => _elementVersionCurent; set => SetProperty(ref _elementVersionCurent, value); }

        private string _elementVersionLastest;
        public string ElementVersionLastest { get => _elementVersionLastest; set => SetProperty(ref _elementVersionLastest, value); }

        private string _dimentionCurent;
        public string DimentionCurent { get => _dimentionCurent; set => SetProperty(ref _dimentionCurent, value); }

        private string _dimentionLastest;
        public string DimentionLastest { get => _dimentionLastest; set => SetProperty(ref _dimentionLastest, value); }

        private string _coordinateCurent;
        public string CoordinateCurent { get => _coordinateCurent; set => SetProperty(ref _coordinateCurent, value); }

        private string _coordinateLastest;
        public string CoordinateLastest { get => _coordinateLastest; set => SetProperty(ref _coordinateLastest, value); }

        private string _directionCurent;
        public string DirectionCurent { get => _directionCurent; set => SetProperty(ref _directionCurent, value); }

        private string _directionLastest;
        public string DirectionLastest { get => _directionLastest; set => SetProperty(ref _directionLastest, value); }

        private string _markLocationCurent;
        public string MarkLocationCurent { get => _markLocationCurent; set => SetProperty(ref _markLocationCurent, value); }

        private string _markLocationLastest;
        public string MarkLocationLastest { get => _markLocationLastest; set => SetProperty(ref _markLocationLastest, value); }

        private string _comment;
        public string Comment { get => _comment; set => SetProperty(ref _comment, value); }

        private string _curentComment;
        public string CurentComment { get => _curentComment; set => SetProperty(ref _curentComment, value); }

        private string _id;
        public string Id { get => _id; set => SetProperty(ref _id, value); }

        private string _drawingId;
        public string RevitElementId { get => _drawingId; set => SetProperty(ref _drawingId, value); }

        private string _status;
        public string Status { get => _status; set => SetProperty(ref _status, value); }

        public void ClearStage()
        {
            Transaction transaction = new Transaction(_document);
            Ultil.ClearUniqueIdState(transaction, _document, ref _stateUniquId);
            Ultil.VisibilityElement(_document, _document.ActiveView, RevitElementId, false);
            Ultil.HightLight(transaction, _document, CurentView.Id, RevitElementId, false);
            UserView.Dispose();
        }
    }

    public class MyOrderingClass : IComparer<Element>
    {
        public int Compare(Element x, Element y)
        {
            int compareDate = x.Name.CompareTo(y.Name);
            if (compareDate == 0) {
                return x.Name.CompareTo(y.Name);
            }
            return compareDate;
        }
    }

    public class MyOrderingObjClass : IComparer<Obj>
    {
        public int Compare(Obj x, Obj y)
        {
            int compareDate = x.Status.CompareTo(y.Status);
            if (compareDate == 0) {
                return x.Status.CompareTo(y.Status);
            }
            return compareDate;
        }
    }
}