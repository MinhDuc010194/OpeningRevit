using Autodesk.Revit.DB;
using CommonTools.OpeningClient.HandleMiddleware;
using CommonTools.OpeningClient.Service;
using CommonTools.OpeningClient.Synchronize.Process.Subject;
using CommonTools.OpeningClient.Synchronize.UI.Model;
using CommonTools.OpeningClient.Synchronize.UI.Support;
using CommonTools.OpeningClient.Synchronize.UI.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommonUtils;
using CommonTools.OpeningClient.Support;
using CommonTools.OpeningClient.Synchronize.UI.View;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CommonTools.OpeningClient.Synchronize.Process
{
    internal class ProcessData
    {
        private Document _document;
        private string _projectName;
        private string _categoryName;
        private string _drawingGroupName;
        private List<DrawingsName> _drawingInDrawingGroupName;
        private ObservableCollection<Obj> ListObjHasSynch { get; set; }
        private ObservableCollection<Obj> ListObjBelowLocal { get; set; }
        private ObservableCollection<Obj> ListObjOnStack { get; set; }
        private ObservableCollection<string> GroupDrawings { get; set; }
        public Func<bool> PushServerInvoke { get; set; }
        public Func<bool> ReleaseAuthen { get; set; }

        public Func<bool> IsTimeOut { get; set; }

        public ProcessData(Document document, string projectName, string categoryName)
        {
            _document = document;
            _projectName = projectName;
            _categoryName = categoryName;
        }

        public bool ShowDialog(Handling handling, string DrawingGroupName)
        {
            List<DrawingsName> DrawingInDrawingGroupName = Repository.GetDrawingInGroupName(DrawingGroupName);
            _drawingInDrawingGroupName = DrawingInDrawingGroupName;
            _drawingGroupName = DrawingGroupName;
            Transaction transaction = new Transaction(_document);
            bool isSuccess = Pull(ref handling);
            if (isSuccess == true) {
                MainViewModel mainViewModel = new MainViewModel(_document, _categoryName);
                mainViewModel.SetDataVM(ListObjBelowLocal, ListObjHasSynch, ListObjOnStack, GroupDrawings);
                mainView view = new mainView();
                view.DataContext = mainViewModel;

                view.ShowDialog();
                // Xu ly local
                //Push server
                if (mainViewModel.DialogResult == DialogResultType.Ok) {
                    if (!IsTimeOut.Invoke()) {
                        Push(handling);
                        if (PushServerInvoke != null && PushServerInvoke.Invoke()) {
                            _document.Save();
                            Utils.MessageInfor(Define.Ok);
                        }
                        else {
                            // Rollback Local
                        }
                    }
                    else {
                        Utils.MessageError(Define.TimeOut);
                    }
                }
                else {
                    ReleaseAuthen.Invoke();
                }
                return true;
            }
            else {
                Utils.MessageWarning(Define.MissingFamilyInstance);
                return false;
            }
        }

        public bool Pull(ref Handling handling)
        {
            bool isSuccess = false;
            if (IsMepSubject()) {
                MEPProcess mEPProcess = new MEPProcess(_document, _projectName);
                isSuccess = mEPProcess.Pull(ref handling);
                ListObjHasSynch = mEPProcess.ListObjHasSynch;
                ListObjBelowLocal = mEPProcess.ListObjBelowLocal;
                ListObjOnStack = mEPProcess.ListObjOnStack;
            }
            else if (IsStructure()) {
                StructureProcess structureProcess = new StructureProcess(_document, _projectName);
                isSuccess = structureProcess.Pull(ref handling);
                ListObjHasSynch = structureProcess.ListObjHasSynch;
                ListObjBelowLocal = structureProcess.ListObjBelowLocal;
                ListObjOnStack = structureProcess.ListObjOnStack;
            }
            GroupDrawings = new ObservableCollection<string>();
            GroupDrawings.Add("Group: " + _drawingGroupName);
            foreach (DrawingsName str in _drawingInDrawingGroupName) {
                GroupDrawings.Add(str.Name);
            }

            return isSuccess;
        }

        public void Push(Handling handling)
        {
            if (IsMepSubject()) {
                MEPProcess mEPProcess = new MEPProcess(_document, _projectName);
                mEPProcess.Push(handling);
            }
            else if (IsStructure()) {
                StructureProcess structureProcess = new StructureProcess(_document, _projectName);
                structureProcess.Push(handling);
            }
        }

        private bool IsMepSubject()
        {
            if (_categoryName == "MEP")
                return true;
            else
                return false;
        }

        private bool IsStructure()
        {
            if (_categoryName == "Structure" || _categoryName == "Architecture")
                return true;
            else
                return false;
        }
    }
}