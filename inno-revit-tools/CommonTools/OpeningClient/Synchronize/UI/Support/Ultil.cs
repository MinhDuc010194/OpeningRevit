using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommonTools.OpeningClient.Support;
using CommonTools.OpeningClient.Synchronize.Process.Subject;
using CommonTools.OpeningClient.Synchronize.UI.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CommonTools.OpeningClient.Synchronize.UI.Support
{
    static public class Ultil
    {
        static public List<Element> GetAllOpenningByView(Document doc, Autodesk.Revit.DB.View view, string categoryName)
        {
            if (categoryName == "MEP") {
                FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id);
                List<Element> listOpening = collector.OfCategory(BuiltInCategory.OST_GenericModel).Where(x => x.Name.Equals(Define.FLOOR_OPENNING_RECTANGLE_NAME_MEP) || x.Name.Equals(Define.WALL_OPENNING_RECTANGLE_NAME_MEP)).Where(x => x is FamilyInstance).ToList();
                List<Element> listCylynder = collector.OfCategory(BuiltInCategory.OST_GenericModel).Where(x => x.Name.Equals(Define.FLOOR_OPENNING_CYLYNDER_NAME_MEP) || x.Name.Equals(Define.WALL_OPENNING_CYLYNDER_NAME_MEP)).Where(x => x is FamilyInstance).ToList();
                listOpening.AddRange(listCylynder);
                return listOpening;
            }
            else {
                FilteredElementCollector ShaftOpening = new FilteredElementCollector(doc);
                List<Element> listSharfOpening = ShaftOpening.OfCategory(BuiltInCategory.OST_ShaftOpening).ToList();
                FilteredElementCollector windownRectangle = new FilteredElementCollector(doc);
                List<Element> listRectangle = windownRectangle.OfCategory(BuiltInCategory.OST_Windows).Where(x => x.Name.Equals(Define.OPENNING_RECTANGLE_NAME_STRUCTURE) && x is FamilyInstance).ToList();
                FilteredElementCollector windownCylynder = new FilteredElementCollector(doc);
                List<Element> listCylynder = windownCylynder.OfCategory(BuiltInCategory.OST_Windows).Where(x => x.Name.Equals(Define.OPENNING_CYLYNDER_NAME_STRUCTURE) && x is FamilyInstance).ToList();
                listSharfOpening.AddRange(listRectangle);
                listSharfOpening.AddRange(listCylynder);
                return listSharfOpening;
            }
        }

        static public string CreateOpening(Transaction transaction, Document doc, string catergoryName, Obj obj)
        {
            if (catergoryName == "MEP") {
                MEPOpeningMaker mEPOpeningMaker = new MEPOpeningMaker(doc, obj._comparisonCoupleElement);
                return mEPOpeningMaker.LoadFammilyInstanceForPreview(transaction);
            }
            else {
                StrAchOpenningMaker openningMaker = new StrAchOpenningMaker(doc, obj._comparisonCoupleElement);
                return openningMaker.CreateOpenningForPreview(transaction);
            }
        }

        static public void HightLight(Transaction transaction, Document document, ElementId view, string RevitElementId, bool isEnable)
        {
            ICollection<ElementId> ids = new List<ElementId>();
            UIDocument uiDoc = new UIDocument(document);
            if (isEnable) {
                if (RevitElementId != null && RevitElementId != string.Empty) {
                    Element element = document.GetElement(RevitElementId);

                    FilteredElementCollector collector = new FilteredElementCollector(document, view);
                    List<Element> listOpening = collector.Where(
                        x => x.Name.Equals(Define.FLOOR_OPENNING_RECTANGLE_NAME_MEP) ||
                        x.Name.Equals(Define.WALL_OPENNING_RECTANGLE_NAME_MEP) ||
                        x.Name.Equals(Define.FLOOR_OPENNING_CYLYNDER_NAME_MEP) ||
                        x.Name.Equals(Define.WALL_OPENNING_CYLYNDER_NAME_MEP) ||
                        x.Name.Equals(Define.WINDOW_OPENNING_RECTANGLE_NAME) ||
                        x.Name.Equals(Define.WINDOW_OPENNING_CYLYNDER_NAME)).Where(x => x is FamilyInstance).ToList();
                    FilteredElementCollector collector2 = new FilteredElementCollector(document, view);
                    List<Element> listOpening2 = collector2.OfCategory(BuiltInCategory.OST_ShaftOpening).ToList();
                    List<string> elementIds = listOpening.ConvertAll(x => x.UniqueId);
                    foreach (Element element1 in listOpening2) {
                        elementIds.Add(element1.UniqueId);
                    }
                    if (elementIds.Contains(RevitElementId)) {
                        ids.Add(element.Id);
                        if (ids.Count != 0) {
                            uiDoc.Selection.SetElementIds(ids);
                            uiDoc.ShowElements(ids);
                        }
                    }
                    else {
                        MessageBox.Show("Can't hightLight this opening. Maybe this opening is not exit in this view");
                    }
                }
            }
            else {
                ids.Clear();
                uiDoc.Selection.SetElementIds(ids);
            }
        }

        static public void VisibilityElement(Document document, Autodesk.Revit.DB.View view, string RevitElementId, bool isEnable)
        {
            if (RevitElementId != null && RevitElementId != "") {
                Transaction transaction = new Transaction(document);
                transaction.Start("Preview");
                ICollection<ElementId> ids = new List<ElementId>();
                UIDocument uiDoc = new UIDocument(document);
                if (isEnable) {
                    Element element = document.GetElement(RevitElementId);
                    ids.Add(element.Id);
                    view.HideElements(ids);
                }
                else {
                    Element element = document.GetElement(RevitElementId);
                    if (element.IsHidden(view)) {
                        ids.Add(element.Id);
                        view.UnhideElements(ids);
                    }
                }

                transaction.Commit();
            }
        }

        static public void ClearUniqueIdState(Transaction transaction, Document document, ref string UniqueId)
        {
            if (UniqueId != null && UniqueId != "") {
                transaction.Start("Clear State");
                Element element = document.GetElement(UniqueId);
                document.Delete(element.Id);
                UniqueId = "";
                transaction.Commit();
            }
        }

        static public ObservableCollection<StatusItem> Setaction(string state)
        {
            ObservableCollection<StatusItem> ObjActions;
            if (state == "OnSynch") {
                ObjActions = new ObservableCollection<StatusItem>();
                StatusItem pull = new StatusItem("PUSH");
                StatusItem push = new StatusItem("PULL");
                StatusItem none = new StatusItem("NONE");
                StatusItem disconnect = new StatusItem("DISCONNECT");

                ObjActions.Add(pull);
                ObjActions.Add(push);
                ObjActions.Add(none);
                ObjActions.Add(disconnect);
                return ObjActions;
            }
            else if (state == "OnServe") {
                ObjActions = new ObservableCollection<StatusItem>();
                StatusItem pull = new StatusItem("PULL");
                StatusItem none = new StatusItem("NONE");

                ObjActions.Add(pull);
                ObjActions.Add(none);
                return ObjActions;
            }
            else if (state == "OnLocal") {
                ObjActions = new ObservableCollection<StatusItem>();
                StatusItem push = new StatusItem("PUSH");
                StatusItem none = new StatusItem("NONE");

                ObjActions.Add(push);
                ObjActions.Add(none);
                return ObjActions;
            }

            return null;
        }
    }
}