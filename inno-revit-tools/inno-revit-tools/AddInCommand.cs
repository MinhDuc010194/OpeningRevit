using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommonUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace OpeningTools
{
    /// <summary>
    /// Implements the Revit add-in interface IExternalCommand, create a wall
    /// all the properties for new wall comes from user selection in Ribbon
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class CreateWall : IExternalCommand
    {
        public static ElementSet CreatedWalls = new ElementSet(); //restore all the walls created by API.

        #region IExternalCommand Members Implementation

        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit,
                                               ref string message,
                                               ElementSet elements)
        {
            Transaction trans = new Transaction(revit.Application.ActiveUIDocument.Document, "CreateWall");
            trans.Start();
            Autodesk.Revit.UI.UIApplication app = revit.Application;

            WallType newWallType = GetNewWallType(app); //get WallType from RadioButtonGroup - WallTypeSelector
            Level newWallLevel = GetNewWallLevel(app); //get Level from Combobox - LevelsSelector
            List<Curve> newWallShape = GetNewWallShape(app); //get wall Curve from Combobox - WallShapeComboBox
            String newWallMark = GetNewWallMark(app); //get mark of new wall from Text box - WallMark

            Wall newWall = null;
            if ("CreateStructureWall" == this.GetType().Name) //decided by SplitButton
            { newWall = Wall.Create(app.ActiveUIDocument.Document, newWallShape, newWallType.Id, newWallLevel.Id, true); }
            else { newWall = Wall.Create(app.ActiveUIDocument.Document, newWallShape, newWallType.Id, newWallLevel.Id, false); }
            if (null != newWall) {
                newWall.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).Set(newWallMark); //set new wall's mark
                CreatedWalls.Insert(newWall);
            }
            trans.Commit();
            return Autodesk.Revit.UI.Result.Succeeded;
        }

        #endregion IExternalCommand Members Implementation

        #region protected methods

        protected WallType GetNewWallType(Autodesk.Revit.UI.UIApplication app)
        {
            //RibbonPanel myPanel = app.GetRibbonPanels()[0];
            RibbonPanel myPanel = app.GetRibbonPanels(Define.RevitToolRibbonTab)[0];
            if (!(GetRibbonItemByName(myPanel, "WallTypeSelector") is RadioButtonGroup radioGroupTypeSelector)) { throw new InvalidCastException("Cannot get Wall Type selector!"); }
            String wallTypeName = radioGroupTypeSelector.Current.ItemText;
            WallType newWallType = null;
            FilteredElementCollector collector = new FilteredElementCollector(app.ActiveUIDocument.Document);
            ICollection<Element> founds = collector.OfClass(typeof(WallType)).ToElements();
            foreach (Element elem in founds) {
                WallType wallType = elem as WallType;
                if (wallType.Name.StartsWith(wallTypeName)) {
                    newWallType = wallType; break;
                }
            }

            return newWallType;
        }

        protected Level GetNewWallLevel(Autodesk.Revit.UI.UIApplication app)
        {
            //RibbonPanel myPanel = app.GetRibbonPanels()[0];
            RibbonPanel myPanel = app.GetRibbonPanels(Define.RevitToolRibbonTab)[0];
            if (!(GetRibbonItemByName(myPanel, "LevelsSelector") is Autodesk.Revit.UI.ComboBox comboboxLevel)) { throw new InvalidCastException("Cannot get Level selector!"); }
            String wallLevel = comboboxLevel.Current.ItemText;
            //find wall type in document
            Level newWallLevel = null;
            FilteredElementCollector collector = new FilteredElementCollector(app.ActiveUIDocument.Document);
            ICollection<Element> founds = collector.OfClass(typeof(Level)).ToElements();
            foreach (Element elem in founds) {
                Level level = elem as Level;
                if (level.Name.StartsWith(wallLevel)) {
                    newWallLevel = level; break;
                }
            }

            return newWallLevel;
        }

        protected List<Curve> GetNewWallShape(Autodesk.Revit.UI.UIApplication app)
        {
            //RibbonPanel myPanel = app.GetRibbonPanels()[0];
            RibbonPanel myPanel = app.GetRibbonPanels(Define.RevitToolRibbonTab)[0];

            if (!(GetRibbonItemByName(myPanel, "WallShapeComboBox") is Autodesk.Revit.UI.ComboBox comboboxWallShape)) { throw new InvalidCastException("Cannot get Wall Shape Gallery!"); }
            String wallShape = comboboxWallShape.Current.ItemText;
            if ("SquareWall" == wallShape) { return GetSquareWallShape(app.Application.Create); }
            else if ("CircleWall" == wallShape) { return GetCircleWallShape(app.Application.Create); }
            else if ("TriangleWall" == wallShape) { return GetTriangleWallShape(app.Application.Create); }
            else { return GetRectangleWallShape(app.Application.Create); }
        }

        protected String GetNewWallMark(Autodesk.Revit.UI.UIApplication app)
        {
            //RibbonPanel myPanel = app.GetRibbonPanels()[0];
            RibbonPanel myPanel = app.GetRibbonPanels(Define.RevitToolRibbonTab)[0];

            if (!(GetRibbonItemByName(myPanel, "WallMark") is Autodesk.Revit.UI.TextBox textBox)) { throw new InvalidCastException("Cannot get Wall Mark TextBox!"); }
            String newWallMark;
            int newWallIndex = 0;
            FilteredElementCollector collector = new FilteredElementCollector(app.ActiveUIDocument.Document);
            ICollection<Element> founds = collector.OfClass(typeof(Wall)).ToElements();
            foreach (Element elem in founds) {
                Wall wall = elem as Wall;
                string wallMark = wall.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString();
                if (wallMark.StartsWith(textBox.Value.ToString()) && wallMark.Contains('_')) {
                    //get the index for new wall (wall_1, wall_2...)
                    char[] chars = { '_' };
                    string[] strings = wallMark.Split(chars);
                    if (strings.Length >= 2) {
                        try {
                            int index = Convert.ToInt32(strings[strings.Length - 1]);
                            if (index > newWallIndex) { newWallIndex = index; }
                        }
                        catch (System.Exception) {
                            continue;
                        }
                    }
                }
            }
            newWallMark = textBox.Value.ToString() + '_' + (newWallIndex + 1);
            return newWallMark;
        }

        protected List<Curve> GetRectangleWallShape(Autodesk.Revit.Creation.Application creApp)
        {
            //calculate size of Structural and NonStructural walls
            int WallsSize = CreateStructureWall.CreatedWalls.Size + CreatedWalls.Size;
            List<Curve> curves = new List<Curve>();
            //15: distance from each wall, 60: wall length , 60: wall width
            Line line1 = Line.CreateBound(new Autodesk.Revit.DB.XYZ(WallsSize * 15, 0, 0), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 60, 0));
            Line line2 = Line.CreateBound(new Autodesk.Revit.DB.XYZ(WallsSize * 15, 60, 0), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 60, 40));
            Line line3 = Line.CreateBound(new Autodesk.Revit.DB.XYZ(WallsSize * 15, 60, 40), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 0, 40));
            Line line4 = Line.CreateBound(new Autodesk.Revit.DB.XYZ(WallsSize * 15, 0, 40), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 0, 0));
            curves.Add(line1);
            curves.Add(line2);
            curves.Add(line3);
            curves.Add(line4);
            return curves;
        }

        protected List<Curve> GetSquareWallShape(Autodesk.Revit.Creation.Application creApp)
        {
            //calculate size of Structural and NonStructural walls
            int WallsSize = CreateStructureWall.CreatedWalls.Size + CreatedWalls.Size;
            List<Curve> curves = new List<Curve>();
            //15: distance from each wall, 40: wall length
            Line line1 = Line.CreateBound(new Autodesk.Revit.DB.XYZ(WallsSize * 15, 0, 0), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 40, 0));
            Line line2 = Line.CreateBound(new Autodesk.Revit.DB.XYZ(WallsSize * 15, 40, 0), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 40, 40));
            Line line3 = Line.CreateBound(new Autodesk.Revit.DB.XYZ(WallsSize * 15, 40, 40), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 0, 40));
            Line line4 = Line.CreateBound(new Autodesk.Revit.DB.XYZ(WallsSize * 15, 0, 40), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 0, 0));
            curves.Add(line1);
            curves.Add(line2);
            curves.Add(line3);
            curves.Add(line4);
            return curves;
        }

        protected List<Curve> GetCircleWallShape(Autodesk.Revit.Creation.Application creApp)
        {
            //calculate size of Structural and NonStructural walls
            int WallsSize = CreateStructureWall.CreatedWalls.Size + CreatedWalls.Size;
            List<Curve> curves = new List<Curve>();
            //15: distance from each wall, 40: diameter of circle
            Arc arc = Arc.Create(new Autodesk.Revit.DB.XYZ(WallsSize * 15, 20, 0), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 20, 40), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 40, 20));
            Arc arc2 = Arc.Create(new Autodesk.Revit.DB.XYZ(WallsSize * 15, 20, 0), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 20, 40), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 0, 20));
            curves.Add(arc);
            curves.Add(arc2);
            return curves;
        }

        protected List<Curve> GetTriangleWallShape(Autodesk.Revit.Creation.Application creApp)
        {
            //calculate size of Structural and NonStructural walls
            int WallsSize = CreateStructureWall.CreatedWalls.Size + CreatedWalls.Size;
            List<Curve> curves = new List<Curve>();
            //15: distance from each wall, 40: height of triangle
            Line line1 = Line.CreateBound(new Autodesk.Revit.DB.XYZ(WallsSize * 15, 0, 0), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 40, 0));
            Line line2 = Line.CreateBound(new Autodesk.Revit.DB.XYZ(WallsSize * 15, 40, 0), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 20, 40));
            Line line3 = Line.CreateBound(new Autodesk.Revit.DB.XYZ(WallsSize * 15, 20, 40), new Autodesk.Revit.DB.XYZ(WallsSize * 15, 0, 0));
            curves.Add(line1);
            curves.Add(line2);
            curves.Add(line3);
            return curves;
        }

        #endregion protected methods

        /// <summary>
        /// return the RibbonItem by the input name in a specific panel
        /// </summary>
        /// <param name="panelRibbon">RibbonPanel which contains the RibbonItem </param>
        /// <param name="itemName">name of RibbonItem</param>
        /// <return>RibbonItem whose name is same with input string</param>
        public RibbonItem GetRibbonItemByName(RibbonPanel panelRibbon, String itemName)
        {
            foreach (RibbonItem item in panelRibbon.GetItems()) {
                if (itemName == item.Name) {
                    return item;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Implements the Revit add-in interface IExternalCommand,create a structural wall
    /// all the properties for new wall comes from user selection in Ribbon
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class CreateStructureWall : CreateWall
    {
    }

    /// <summary>
    /// Implements the Revit add-in interface IExternalCommand,
    /// delete all the walls which create by Ribbon sample
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class DeleteWalls : IExternalCommand
    {
        #region IExternalCommand Members Implementation

        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit,
                                               ref string message,
                                               ElementSet elements)
        {
            // delete all the walls which create by RibbonSample
            ElementSet wallSet = CreateWall.CreatedWalls;
            Transaction trans = new Transaction(revit.Application.ActiveUIDocument.Document, "DeleteWalls");
            trans.Start();
            foreach (Element e in wallSet) {
                revit.Application.ActiveUIDocument.Document.Delete(e.Id);
            }
            CreateWall.CreatedWalls.Clear();
            trans.Commit();
            return Autodesk.Revit.UI.Result.Succeeded;
        }

        #endregion IExternalCommand Members Implementation
    }

    /// <summary>
    /// Implements the Revit add-in interface IExternalCommand,Move walls, X direction
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class XMoveWalls : IExternalCommand
    {
        #region IExternalCommand Members Implementation

        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit,
                                               ref string message,
                                               ElementSet elements)
        {
            Transaction trans = new Transaction(revit.Application.ActiveUIDocument.Document, "XMoveWalls");
            trans.Start();
            IEnumerator iter = CreateWall.CreatedWalls.GetEnumerator();
            iter.Reset();
            while (iter.MoveNext()) {
                if (iter.Current is Wall wall) {
                    wall.Location.Move(new Autodesk.Revit.DB.XYZ(12, 0, 0));
                }
            }
            trans.Commit();
            return Autodesk.Revit.UI.Result.Succeeded;
        }

        #endregion IExternalCommand Members Implementation
    }

    /// <summary>
    /// Implements the Revit add-in interface IExternalCommand,Move walls, Y direction
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class YMoveWalls : IExternalCommand
    {
        #region IExternalCommand Members Implementation

        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit,
                                               ref string message,
                                               ElementSet elements)
        {
            Transaction trans = new Transaction(revit.Application.ActiveUIDocument.Document, "YMoveWalls");
            trans.Start();
            IEnumerator iter = CreateWall.CreatedWalls.GetEnumerator();
            iter.Reset();
            while (iter.MoveNext()) {
                if (iter.Current is Wall wall) {
                    wall.Location.Move(new Autodesk.Revit.DB.XYZ(0, 12, 0));
                }
            }
            trans.Commit();
            return Autodesk.Revit.UI.Result.Succeeded;
        }

        #endregion IExternalCommand Members Implementation
    }

    /// <summary>
    /// Implements the Revit add-in interface IExternalCommand,
    /// Reset all the Ribbon options to default, such as level, wall type...
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class ResetSetting : IExternalCommand
    {
        #region IExternalCommand Members Implementation

        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit,
                                               ref string message,
                                               ElementSet elements)
        {
            //RibbonPanel myPanel = revit.Application.GetRibbonPanels()[0];
            RibbonPanel myPanel = revit.Application.GetRibbonPanels(Define.RevitToolRibbonTab)[0];
            //reset wall type
            if (!(GetRibbonItemByName(myPanel, "WallTypeSelector") is RadioButtonGroup radioGroupTypeSelector)) { throw new InvalidCastException("Cannot get Wall Type selector!"); }
            radioGroupTypeSelector.Current = radioGroupTypeSelector.GetItems()[0];

            //reset level
            if (!(GetRibbonItemByName(myPanel, "LevelsSelector") is Autodesk.Revit.UI.ComboBox comboboxLevel)) { throw new InvalidCastException("Cannot get Level selector!"); }
            comboboxLevel.Current = comboboxLevel.GetItems()[0];

            //reset wall shape
            Autodesk.Revit.UI.ComboBox comboboxWallShape =
                GetRibbonItemByName(myPanel, "WallShapeComboBox") as Autodesk.Revit.UI.ComboBox;
            if (null == comboboxLevel) { throw new InvalidCastException("Cannot get wall shape combo box!"); }
            comboboxWallShape.Current = comboboxWallShape.GetItems()[0];

            //get wall mark
            if (!(GetRibbonItemByName(myPanel, "WallMark") is Autodesk.Revit.UI.TextBox textBox)) { throw new InvalidCastException("Cannot get Wall Mark TextBox!"); }
            textBox.Value = "new wall";

            return Autodesk.Revit.UI.Result.Succeeded;
        }

        /// <summary>
        /// return the RibbonItem by the input name in a specific panel
        /// </summary>
        /// <param name="panelRibbon">RibbonPanel which contains the RibbonItem </param>
        /// <param name="itemName">name of RibbonItem</param>
        /// <return>RibbonItem whose name is same with input string</param>
        public RibbonItem GetRibbonItemByName(RibbonPanel panelRibbon, String itemName)
        {
            foreach (RibbonItem item in panelRibbon.GetItems()) {
                if (itemName == item.Name) {
                    return item;
                }
            }

            return null;
        }

        #endregion IExternalCommand Members Implementation
    }

    /// <summary>
    /// Do Nothing,
    /// Create this just because ToggleButton have to bind to a ExternalCommand
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class Dummy : IExternalCommand
    {
        #region IExternalCommand Members Implementation

        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit,
                                               ref string message,
                                               ElementSet elements)
        {
            return Autodesk.Revit.UI.Result.Succeeded;
        }

        #endregion IExternalCommand Members Implementation
    }

    /// <summary>
    /// Read information of app
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class Information : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try {
                string information = string.Empty;
                information += "Version: ";

                string revitVersion = commandData.Application.Application.VersionNumber;

                // get product version
                string productVersion = CommonUtils.Utils.ReadRegistryValue(revitVersion, "ProductVersion");
                productVersion = CommonUtils.Utils.GetProductVersion(productVersion);

                information += productVersion;

                // patch
                string productPatch = CommonUtils.Utils.ReadRegistryValue(revitVersion, "ProductPatch");
                if (!string.IsNullOrEmpty(productPatch) && !"-1".Equals(productPatch))
                    information += "\nPatch: " + productPatch;

                // product code
                string productCode = CommonUtils.Utils.ReadRegistryValue(revitVersion, "ProductCode");

                information += "\nCode: ";
                information += productCode;

                // Creates a Revit task dialog to communicate information to the user.
                TaskDialog mainDialog = new TaskDialog("About") {
                    MainContent = information
                };

                // Add commmandLink options to task dialog
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Inno revit tools website");

                // task dialog will show a Close button by default
                mainDialog.CommonButtons = TaskDialogCommonButtons.Close;
                mainDialog.DefaultButton = TaskDialogResult.Close;

                TaskDialogResult mainResult = mainDialog.Show();

                // If the user clicks the first command link, a simple Task Dialog
                if (TaskDialogResult.CommandLink1 == mainResult)
                    System.Diagnostics.Process.Start(ConfigUtils.GetSetting(DefineUtils.Lm_Url_Frontend));
            }
            catch (Exception ex) {
                //TaskDialog.Show("About", "Can't access information!");
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace.ToString());
            }

            return Result.Succeeded;
        }
    }
}