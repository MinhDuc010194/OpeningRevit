using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using CommonTools.OpeningClient.ExtensibleStorage;
using CommonTools.OpeningClient.HandleMiddleware;
using CommonTools.OpeningClient.HandleMiddleware.ServerGroupStatus;
using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Service;
using CommonTools.OpeningClient.Support;
using CommonTools.OpeningClient.Synchronize.UI.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CommonTools.OpeningClient.Synchronize.Process.Subject
{
    internal class MEPProcess : SubjectProcess
    {
        private Document _doc;
        private string _subject;
        private string _projectName;

        public ObservableCollection<Obj> ListObjHasSynch { get; private set; }
        public ObservableCollection<Obj> ListObjBelowLocal { get; private set; }
        public ObservableCollection<Obj> ListObjOnStack { get; private set; }

        public MEPProcess(Document document, string projectName)
        {
            _doc = document;
            _subject = "MEP";
            _projectName = projectName;
        }

        #region pull

        public bool Pull(ref Handling handling)
        {
            List<List<Element>> elements = GetAllOpenning(_doc);
            if (elements.Count == 0) {
                return false;
            }
            List<OpeningModel> openningModelsLocal = GetDataFromListElement(elements, _subject);
            handling.RevitElementOnLocal = openningModelsLocal;
            handling.InitData();

            ListObjHasSynch = UpdateOpenningModelHasSynchToListObject(_doc, handling);
            ListObjBelowLocal = UpdateOpeningModelBelowLocalToListObj(_doc, handling);
            ListObjOnStack = UpdateOpeningModelOnStackToListObj(_doc, handling);
            return true;
        }

        #endregion pull

        #region push

        public void Push(Handling handling)
        {
            // update opening below Local
            UpdateToRevit(handling);
            // Push handling to Severver
        }

        private void UpdateToRevit(Handling handling)
        {
            Transaction transaction = new Transaction(_doc);
            //ServerNormalStatus
            UpdateNomalStatus(transaction, handling);
            //ServerPendingCreateStatus
            UpdatePendingCreateStatus(transaction, handling);
            //ServerPendingDeleteStatus
            UpdatePendingDeleteStatus(transaction, handling);
            //CreateOpeningOnStack
            CreateOpeningOnStack(transaction, handling);
        }

        private void CreateOpeningOnStack(Transaction transaction, Handling handling)
        {
            List<CompareCoupleWithManager> compareCoupleWithManagers = handling.NewOpeningFromStack;
            foreach (CompareCoupleWithManager compareCoupleWithManager in compareCoupleWithManagers) {
                if (compareCoupleWithManager.Action == Model.Action.PULL) {
                    LoadFammilyInstance(transaction, compareCoupleWithManager);
                }
            }
        }

        private void UpdateNomalStatus(Transaction transaction, Handling handling)
        {
            IClassify classifyNomalStatus = handling.ServerNormalStatus;
            CompareClassifyLocalStatus compareClassifyLocalStatusNomal = classifyNomalStatus.Data;
            //get local status
            IEnumerable<ComparisonCoupleElement> deletedLocal = compareClassifyLocalStatusNomal.DeletedLocal;
            foreach (ComparisonCoupleElement comparisonCoupleElement in deletedLocal) {
                // tuân theo sever
                if (comparisonCoupleElement.Action == Model.Action.PULL) {
                    LoadFammilyInstance(transaction, comparisonCoupleElement);
                }
            }
            IEnumerable<ComparisonCoupleElement> nomalLocal = compareClassifyLocalStatusNomal.NormalLocal;
            foreach (ComparisonCoupleElement comparisonCoupleElement in nomalLocal) {
                if (!comparisonCoupleElement.IsSameShapeAndLocation()) {
                    if (comparisonCoupleElement.Action == Model.Action.PULL
                        && Common.IsValidGuid(comparisonCoupleElement.IdRevitElement)) {
                        if (comparisonCoupleElement.IsSameTypeGeometry()) {
                            Element element = _doc.GetElement(comparisonCoupleElement.IdRevitElement);
                            UpdateGeometryAndDirection(transaction, element.Id, comparisonCoupleElement);
                        }
                        else {
                            Element element = _doc.GetElement(comparisonCoupleElement.IdRevitElement);
                            transaction.Start("Delete Opening");
                            _doc.Delete(element.Id);
                            transaction.Commit();
                            LoadFammilyInstance(transaction, comparisonCoupleElement);
                        }
                    }
                }
            }
        }

        private void UpdatePendingCreateStatus(Transaction transaction, Handling handling)
        {
            IClassify classifyNomalStatus = handling.ServerPendingCreateStatus;
            CompareClassifyLocalStatus compareClassifyLocalStatusNomal = classifyNomalStatus.Data;
            //get local status
            IEnumerable<ComparisonCoupleElement> deletedLocal = compareClassifyLocalStatusNomal.DeletedLocal;
            foreach (ComparisonCoupleElement comparisonCoupleElement in deletedLocal) {
                // tuân theo sever
                if (comparisonCoupleElement.Action == Model.Action.PULL) {
                    LoadFammilyInstance(transaction, comparisonCoupleElement);
                }
            }
        }

        private void UpdatePendingDeleteStatus(Transaction transaction, Handling handling)
        {
            transaction.Start("delete");
            IClassify classifyNomalStatus = handling.ServerPendingDeleteStatus;
            CompareClassifyLocalStatus compareClassifyLocalStatusNomal = classifyNomalStatus.Data;
            //get local status
            IEnumerable<ComparisonCoupleElement> nomalLocal = compareClassifyLocalStatusNomal.NormalLocal;
            foreach (ComparisonCoupleElement comparisonCoupleElement in nomalLocal) {
                if (!comparisonCoupleElement.IsSameShapeAndLocation()) {
                    if (comparisonCoupleElement.Action == Model.Action.PULL
                        && Common.IsValidGuid(comparisonCoupleElement.IdRevitElement)) {
                        Element element = _doc.GetElement(comparisonCoupleElement.IdRevitElement);
                        _doc.Delete(element.Id);
                    }
                }
            }
            transaction.Commit();
        }

        private void LoadFammilyInstance(Transaction transaction, ComparisonCoupleElement openingData)
        {
            MEPOpeningMaker mEPOpeningMaker = new MEPOpeningMaker(_doc, openingData, _projectName);
            mEPOpeningMaker.LoadFammilyInstance(transaction);
        }

        private void UpdateGeometryAndDirection(Transaction transaction, ElementId id, ComparisonCoupleElement openningModel)
        {
            MEPOpeningMaker mEPOpeningMaker = new MEPOpeningMaker(_doc, openningModel, _projectName);
            mEPOpeningMaker.UpdateGeometryAndDirection(transaction, id);
        }

        private void UpdateDirection(ElementId id, ComparisonCoupleElement openningModel)
        {
            XYZ basicZ = XYZ.BasisZ;
            XYZ direction = ConvertOpenningStringToObjectReVit.getDirection(openningModel.ServerGeometry.Direction);
            XYZ axisVec = basicZ.CrossProduct(direction);
            Line axis = Line.CreateBound(axisVec, axisVec * 2);
            double angle = basicZ.AngleOnPlaneTo(direction, axisVec);
            ElementTransformUtils.RotateElement(_doc, id, axis, angle);
        }

        public override List<List<Element>> GetAllOpenning(Document doc)
        {
            List<List<Element>> GenericModels = new List<List<Element>>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<Element> listRectangle = collector.OfCategory(BuiltInCategory.OST_GenericModel).Where(x => x.Name.Equals(Define.FLOOR_OPENNING_RECTANGLE_NAME_MEP) || x.Name.Equals(Define.WALL_OPENNING_RECTANGLE_NAME_MEP)).Where(x => x is FamilyInstance).ToList();
            List<Element> listCylynder = collector.OfCategory(BuiltInCategory.OST_GenericModel).Where(x => x.Name.Equals(Define.FLOOR_OPENNING_CYLYNDER_NAME_MEP) || x.Name.Equals(Define.WALL_OPENNING_CYLYNDER_NAME_MEP)).Where(x => x is FamilyInstance).ToList();
            if (listRectangle != null && listCylynder != null) {
                GenericModels.Add(listRectangle);
                GenericModels.Add(listCylynder);
            }
            return GenericModels;
        }

        #endregion push
    }
}