using Autodesk.Revit.DB;
using CommonTools.OpeningClient.ExtensibleStorage;
using CommonTools.OpeningClient.HandleMiddleware;
using CommonTools.OpeningClient.HandleMiddleware.ServerGroupStatus;
using CommonTools.OpeningClient.JsonObject;
using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Service;
using CommonTools.OpeningClient.Support;
using CommonTools.OpeningClient.Synchronize.UI.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CommonTools.OpeningClient.Synchronize.Process.Subject
{
    internal class StructureProcess : SubjectProcess
    {
        private Document _doc;
        public string _subject;
        private string _projectName;

        public ObservableCollection<Obj> ListObjHasSynch { get; private set; }
        public ObservableCollection<Obj> ListObjBelowLocal { get; private set; }
        public ObservableCollection<Obj> ListObjOnStack { get; private set; }

        public StructureProcess(Document doc, string projectName)
        {
            _doc = doc;
            _subject = "Structure";
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

        public bool Push(Handling handling)
        {
            // update opening below Local
            UpdateToRevit(handling);
            return true;
        }

        private bool UpdateToRevit(Handling handling)
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
            return true;
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
                            UpdateGeometryAndDirection(transaction, element, comparisonCoupleElement);
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
            transaction.Start("delete element");
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
            //tạo moi openning va tien hanh save data vao extensible va lay GUID luu vào ComparisonCoupleElement.RevitId
            StrAchOpenningMaker openningMaker = new StrAchOpenningMaker(_doc, openingData);
            openningMaker.CreateOpenning(transaction);
        }

        private void UpdateGeometryAndDirection(Transaction transaction, Element element, ComparisonCoupleElement openningModel)
        {
            StrAchOpenningMaker openningMaker = new StrAchOpenningMaker(_doc, openningModel);
            openningMaker.EditOpening(transaction, element);
            //saveDataToExtensibleStorage(openningModel, element.Id);
        }

        private void saveDataToExtensibleStorage(ComparisonCoupleElement openningModel, ElementId openningId)
        {
            Element openning = _doc.GetElement(openningId);
            if (openning != null)
                OpenningProcessExtensibleStorage.SetValueToExtensibleStorage(openningModel, openning, _subject);
        }

        private void UpdateDirection(ElementId id, OpeningModel openningModel)
        {
            XYZ basicZ = XYZ.BasisZ;
            XYZ direction = ConvertOpenningStringToObjectReVit.getDirection(openningModel.Geometry.Direction);
            XYZ axisVec = basicZ.CrossProduct(direction);
            Line axis = Line.CreateBound(axisVec, axisVec * 2);
            double angle = basicZ.AngleOnPlaneTo(direction, axisVec);
            ElementTransformUtils.RotateElement(_doc, id, axis, angle);
        }

        public override List<List<Element>> GetAllOpenning(Document doc)
        {
            List<List<Element>> GenericModels = new List<List<Element>>();
            FilteredElementCollector ShaftOpening = new FilteredElementCollector(doc);
            List<Element> listSharfOpening = ShaftOpening.OfCategory(BuiltInCategory.OST_ShaftOpening).ToList();
            FilteredElementCollector windownRectangle = new FilteredElementCollector(doc);
            List<Element> listRectangle = windownRectangle.OfCategory(BuiltInCategory.OST_Windows).Where(x => x.Name.Equals(Define.OPENNING_RECTANGLE_NAME_STRUCTURE) && x is FamilyInstance).ToList();
            FilteredElementCollector windownCylynder = new FilteredElementCollector(doc);
            List<Element> listCylynder = windownCylynder.OfCategory(BuiltInCategory.OST_Windows).Where(x => x.Name.Equals(Define.OPENNING_CYLYNDER_NAME_STRUCTURE) && x is FamilyInstance).ToList();
            foreach (Element opening in listSharfOpening) {
                if (IsRectangle(opening) == true) {
                    listRectangle.Add(opening);
                }
                else {
                    listCylynder.Add(opening);
                }
            }
            if (listRectangle != null && listCylynder != null) {
                GenericModels.Add(listRectangle);
                GenericModels.Add(listCylynder);
            }
            return GenericModels;
        }

        private bool IsRectangle(Element element)
        {
            if (element is Opening) {
                Opening opening = element as Opening;
                CurveArray curveArray = opening.BoundaryCurves;
                foreach (Curve curve in curveArray) {
                    if (curve is Arc) {
                        return false;
                    }
                }
                return true;
            }
            else {
                throw new System.Exception();
            }
        }

        #endregion push
    }
}