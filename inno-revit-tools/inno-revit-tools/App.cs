using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace OpeningTools
{
    internal class AppOpening : IExternalApplication
    {
        private void CreateRibbonPanel(UIControlledApplication application)
        {
            string resourceDir = CommonUtils.Utils.GetResourceDir();
            string buttonIcons32 = Path.Combine(resourceDir, CommonUtils.DefineUtils.ICONS_32_DIR);
            string buttonIcons16 = Path.Combine(resourceDir, CommonUtils.DefineUtils.ICONS_16_DIR);

            // Create a custom ribbon tab
            application.CreateRibbonTab(Define.RevitToolRibbonTab);

            #region About

            string assembly = @"C:\ProgramData\Autodesk\Revit\Addins\2019\OpeningCombiler" + @"\OpeningTools.dll";

            // Add Combine ribbon panel
            RibbonPanel combineRibbonPanel = application.CreateRibbonPanel(Define.RevitToolRibbonTab, Define.CombinePanel);
            assembly = @"C:\ProgramData\Autodesk\Revit\Addins\2019\OpeningCombiler" + @"\CommonOpeningTools.dll";

            // create pull down button for Combine opening
            PulldownButtonData combineDownButton = new PulldownButtonData("Combine Opening", "Combine \nOpening");
            PulldownButton groupCombine = combineRibbonPanel.AddItem(combineDownButton) as PulldownButton;
            combineDownButton.LargeImage = new BitmapImage(new Uri(Path.Combine(buttonIcons32, "Combine_Opening32x32.png"), UriKind.Absolute));
            combineDownButton.Image = new BitmapImage(new Uri(Path.Combine(buttonIcons16, "Combine_Opening16X16.png"), UriKind.Absolute));
            groupCombine.LargeImage = new BitmapImage(new Uri(Path.Combine(buttonIcons32, "Combine_Opening32x32.png"), UriKind.Absolute));
            groupCombine.Image = new BitmapImage(new Uri(Path.Combine(buttonIcons16, "Combine_Opening16X16.png"), UriKind.Absolute));

            // add button for synchronize
            PushButtonData synchronoursButtonData = new PushButtonData("Synchronous", "Synchronous", assembly, "CommonTools.OpeningClient.Synchronize.Commandata");
            PushButton synchronoursBtn = groupCombine.AddPushButton(synchronoursButtonData) as PushButton;
            synchronoursBtn.LargeImage = new BitmapImage(new Uri(Path.Combine(buttonIcons32, "Combine_Opening_Synchronous32x32.png"), UriKind.Absolute));
            synchronoursBtn.Image = new BitmapImage(new Uri(Path.Combine(buttonIcons16, "Combine_Opening_Synchronous16x16.png"), UriKind.Absolute));
            synchronoursBtn.ToolTip = "Synchronous";

            #endregion About
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try {
                // read file input
                CommonUtils.ConfigUtils.LoadFileConfig();
                CreateRibbonPanel(application);
            }
            catch (Exception ex) {
                TaskDialog.Show("Common tool start-up failure", ex.Message + "\n" + ex.StackTrace);
            }

            return Result.Succeeded;
        }
    }
}