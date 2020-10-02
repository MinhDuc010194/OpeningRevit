using Autodesk.Revit.DB;
using CommonTools.OpeningClient.ExtensibleStorage;
using CommonTools.OpeningClient.HandleMiddleware;
using CommonTools.OpeningClient.HandleMiddleware.ServerGroupStatus;
using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Support;
using CommonTools.OpeningClient.Synchronize.UI.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CommonTools.OpeningClient.Synchronize.Process.Subject
{
    abstract internal class SubjectProcess
    {
        #region pull

        private IEnumerable<ComparisonCoupleElement> _listBelowLocal;

        abstract public List<List<Element>> GetAllOpenning(Document doc);

        public List<OpeningModel> GetDataFromListElement(List<List<Element>> gennericModelOpennings, string subject)
        {
            List<OpeningModel> openningModels = new List<OpeningModel>();
            bool isRectangle = true;
            foreach (List<Element> shapeLists in gennericModelOpennings) {
                foreach (Element shapeOpenning in shapeLists) {
                    OpeningModel openningModel = GetNewOpenningModelFromNewOpenning(shapeOpenning, subject, isRectangle);
                    openningModels.Add(openningModel);
                }
                // item dau tien trong list là list rectangle Openning
                isRectangle = false;
            }
            return openningModels;
        }

        public OpeningModel GetNewOpenningModelFromNewOpenning(Element openning, string suject, bool isRectangle)
        {
            OpeningModel openningModel = new OpeningModel();
            openningModel.Geometry.Geometry = ConvertObjectReVitToOpenningString.GetGeometry(openning, isRectangle, suject);
            openningModel.Geometry.Direction = ConvertObjectReVitToOpenningString.GetDirection(openning, isRectangle, suject);
            openningModel.Geometry.Original = ConvertObjectReVitToOpenningString.GetOrigin(openning, isRectangle, suject);
            openningModel.IdLocal = openning.UniqueId;
            return openningModel;
        }

        public ObservableCollection<Obj> UpdateOpenningModelHasSynchToListObject(Document document, Handling handling)
        {
            ObservableCollection<Obj> objs = new ObservableCollection<Obj>();
            // ServerNormalStatus
            IClassify ServerNormalStatus = handling.ServerNormalStatus;
            CompareClassifyLocalStatus compareClassifyLocalStatusNomal = ServerNormalStatus.Data;
            List<ComparisonCoupleElement> deletedLocal = compareClassifyLocalStatusNomal.DeletedLocal;
            FindAndAddListComparisonCoupleElement(document, deletedLocal, objs);
            List<ComparisonCoupleElement> normalLocal = compareClassifyLocalStatusNomal.NormalLocal;
            FindAndAddListComparisonCoupleElement(document, normalLocal, objs);
            // ServerPendingCreateStatus
            IClassify ServerPendingCreateStatus = handling.ServerPendingCreateStatus;
            CompareClassifyLocalStatus compareClassifyLocalStatusPendingCreate = ServerPendingCreateStatus.Data;
            deletedLocal = compareClassifyLocalStatusPendingCreate.DeletedLocal;
            FindAndAddListComparisonCoupleElement(document, deletedLocal, objs);
            normalLocal = compareClassifyLocalStatusPendingCreate.NormalLocal;
            FindAndAddListComparisonCoupleElement(document, normalLocal, objs);
            // ServerPendingDeleteStatus
            IClassify ServerPendingDeleteStatus = handling.ServerPendingDeleteStatus;
            CompareClassifyLocalStatus compareClassifyLocalStatusPendingDelete = ServerPendingDeleteStatus.Data;
            deletedLocal = compareClassifyLocalStatusPendingDelete.DeletedLocal;
            FindAndAddListComparisonCoupleElement(document, deletedLocal, objs);
            normalLocal = compareClassifyLocalStatusPendingDelete.NormalLocal;
            FindAndAddListComparisonCoupleElement(document, normalLocal, objs);
            return objs;
        }

        public ObservableCollection<Obj> UpdateOpeningModelBelowLocalToListObj(Document document, Handling handling)
        {
            ObservableCollection<Obj> objs = new ObservableCollection<Obj>();
            _listBelowLocal = handling.NewOpeningsBelowLocal;
            FindAndAddListComparisonCoupleElement(document, handling.NewOpeningsBelowLocal, objs);
            return objs;
        }

        public ObservableCollection<Obj> UpdateOpeningModelOnStackToListObj(Document document, Handling handling)
        {
            ObservableCollection<Obj> objs = new ObservableCollection<Obj>();
            if (handling.NewOpeningFromStack != null) {
                List<CompareCoupleWithManager> newOpeningFromStack = handling.NewOpeningFromStack;
                FindAndAddListComparisonCoupleElement(document, newOpeningFromStack, objs);
            }

            return objs;
        }

        public void FindAndAddListComparisonCoupleElement(Document document, List<ComparisonCoupleElement> listComparisonCoupleElement, ObservableCollection<Obj> Objs)
        {
            if (listComparisonCoupleElement != null) {
                for (int i = 0; i < listComparisonCoupleElement.Count(); i++) {
                    ComparisonCoupleElement comparisonCoupleElement = listComparisonCoupleElement.ElementAt(i);
                    Obj objNew = new Obj(ref comparisonCoupleElement, document);
                    if (objNew != null) {
                        Objs.Add(objNew);
                    }
                }
            }
        }

        public void FindAndAddListComparisonCoupleElement(Document document, List<CompareCoupleWithManager> listComparisonCoupleElement, ObservableCollection<Obj> Objs)
        {
            if (listComparisonCoupleElement != null) {
                for (int i = 0; i < listComparisonCoupleElement.ToList().Count; i++) {
                    ComparisonCoupleElement comparisonCoupleElement = listComparisonCoupleElement.ToList()[i];
                    CompareCoupleWithManager compareCoupleWithManager = listComparisonCoupleElement.ToList()[i];
                    //comparisonCoupleElement.Action = Action.NONE;
                    Obj objNew = new Obj(ref comparisonCoupleElement, document);
                    if (objNew != null) {
                        ObservableCollection<InfileObject> InfileObjects = new ObservableCollection<InfileObject>();
                        foreach (string fileName in compareCoupleWithManager.DrawingsContain) {
                            InfileObject infileObject = new InfileObject(fileName);
                            InfileObjects.Add(infileObject);
                        }
                        objNew.Infile = InfileObjects;
                        if (objNew.Infile != null && objNew.Infile.Count > 0)
                            objNew.CurrentInfile = objNew.Infile[0];
                        Objs.Add(objNew);
                    }
                }
            }
        }

        #endregion pull

        #region push

        public void saveDataToExtensibleStorage(Document document, ComparisonCoupleElement openningModel, ElementId openningId, string subject)
        {
            Element openning = document.GetElement(openningId);
            if (openning != null)
                OpenningProcessExtensibleStorage.SetValueToExtensibleStorage(openningModel, openning, subject);
        }

        #endregion push
    }
}