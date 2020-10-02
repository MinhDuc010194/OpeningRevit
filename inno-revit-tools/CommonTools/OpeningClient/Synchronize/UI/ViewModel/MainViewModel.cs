using Autodesk.Revit.DB;
using CommonTools.OpeningClient.Support;
using CommonTools.OpeningClient.Synchronize.UI.Model;
using CommonTools.OpeningClient.Synchronize.UI.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace CommonTools.OpeningClient.Synchronize.UI.ViewModel
{
    internal class MainViewModel : BindableBase
    {
        private Document _document;
        private CombineOpeningVM _combineOpeningVM;

        public MainViewModel(
            Document document,
            string catergoryName,
            CombineOpeningVM combineOpeningVM = null
            )
        {
            _document = document;
            if (_combineOpeningVM == null) {
                _combineOpeningVM = new CombineOpeningVM(_document, catergoryName);
            }
            else {
                _combineOpeningVM = combineOpeningVM;
            }
            CurrentViewModel = _combineOpeningVM;
            OkCommand = new RelayCommand<object>(OkCommandInvoke);
            CloseCommand = new RelayCommand<object>(CloseCommandInvoke);
        }

        public ICommand OkCommand { get; set; }

        private void OkCommandInvoke(object obj)
        {
            DialogResult = DialogResultType.Ok;
            _combineOpeningVM.ClearStage();
            Window mainView = obj as Window;
            mainView.Close();
        }

        public ICommand CloseCommand { get; set; }

        private void CloseCommandInvoke(object obj)
        {
            _combineOpeningVM.ClearStage();
        }

        public DialogResultType DialogResult { get; private set; }

        private BindableBase _currentViewModel;

        public BindableBase CurrentViewModel
        {
            get => _currentViewModel;
            set { SetProperty(ref _currentViewModel, value); }
        }

        public void SetDataVM(ObservableCollection<Obj> ListObjBelowLocal,
                       ObservableCollection<Obj> ListObjHasSynch,
                       ObservableCollection<Obj> ListObjOnStack,
                       ObservableCollection<string> GroupDrawings)
        {
            _combineOpeningVM.ListObjBelowLocal = ListObjBelowLocal;
            foreach (Obj obj in _combineOpeningVM.ListObjBelowLocal) {
                StatusItem statusItem = obj.CurrentAction;
                obj.ObjActions = Ultil.Setaction("OnLocal");
                obj.CurrentAction = obj.ObjActions.Where(x => x.Name.Equals(statusItem.Name)).FirstOrDefault();
            }
            _combineOpeningVM.ListObjHasSynch = ListObjHasSynch;
            foreach (Obj obj in _combineOpeningVM.ListObjHasSynch) {
                StatusItem statusItem = obj.CurrentAction;
                obj.ObjActions = Ultil.Setaction("OnSynch");
                obj.CurrentAction = obj.ObjActions.Where(x => x.Name.Equals(statusItem.Name)).FirstOrDefault();
            }
            _combineOpeningVM.ListObjOnStack = ListObjOnStack;
            foreach (Obj obj in _combineOpeningVM.ListObjOnStack) {
                StatusItem statusItem = obj.CurrentAction;
                obj.ObjActions = Ultil.Setaction("OnServe");
                obj.CurrentAction = obj.ObjActions.Where(x => x.Name.Equals(statusItem.Name)).FirstOrDefault();
            }
            _combineOpeningVM.GroupDrawings = GroupDrawings;
            _combineOpeningVM.setCurentList(_combineOpeningVM.ListObjHasSynch);
        }
    }
}