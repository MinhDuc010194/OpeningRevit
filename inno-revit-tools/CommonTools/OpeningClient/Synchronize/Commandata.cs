using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommonTools.OpeningClient.HandleMiddleware;
using CommonTools.OpeningClient.Service;
using CommonTools.OpeningClient.Synchronize.Process;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using CommonTools.OpeningClient.Support;
using System.Collections.Generic;
using CommonUtils;

namespace CommonTools.OpeningClient.Synchronize
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class Commandata : IExternalCommand
    {
        private Document _document;
        private Handling _handlingGlobal;
        private AuthenticationInfo _info;
        private string drawingName;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            _document = uidoc.Document;
            string userName = commandData.Application.Application.Username;
            drawingName = _document.Title.Replace($"_{userName}", "");
            drawingName = drawingName.Replace($"-{userName}", "");
            _info = GetInfo();

            #region get data from server

            var category = Repository.GetCategoryName(drawingName);
            if (string.IsNullOrEmpty(category)) {
                Utils.MessageError(Define.DrawingNotFound);
                return Result.Failed;
            }

            var timeOut = Repository.GetTimeout();
            if (timeOut != "Ok") {
                Utils.MessageError("Expiry date");
                return Result.Failed;
            }

            var DrawingGroupName = Repository.GetDrawingGroupName(drawingName);

            // get data form server
            var openingsOnStack = Repository.GetOpeningOnServer(drawingName);

            var openingsSync = Repository.GetManagerExept(drawingName, DrawingGroupName);

            var versionOpeningsGeometry = Repository.GetElementWithVersionCurrent(drawingName);

            // init data for UI
            _handlingGlobal = new Handling(openingsOnStack, openingsSync, versionOpeningsGeometry);

            ProcessData processData = new ProcessData(_document, drawingName, category);
            processData.PushServerInvoke += PushDataFromLocal;
            processData.IsTimeOut += CheckTimeOut;
            processData.ReleaseAuthen += ReleaseAuthen;
            processData.ShowDialog(_handlingGlobal, DrawingGroupName);

            //PushDataFromLocal();
            //123

            #endregion get data from server

            return Result.Succeeded;
        }

        private bool PushDataFromLocal()
        {
            #region init data local

            PreprocessingDataPost preprocessingDataPost = new PreprocessingDataPost(_handlingGlobal);
            var dataPush = preprocessingDataPost.ProduceDataPostRequest(_info);
            var validation = new Validation.ValidationData(dataPush);
            validation.ImpelementValidate();

            #endregion init data local

            return Repository.PostDataToServer(dataPush);
        }

        private AuthenticationInfo GetInfo()
        {
            var macAddress = NetworkInterface
                              .GetAllNetworkInterfaces()
                              .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                              .Select(nic => nic.GetPhysicalAddress().ToString())
                              .FirstOrDefault();
            return new AuthenticationInfo() { DrawingName = drawingName, MacAddress = macAddress };
        }

        private bool CheckTimeOut()
        {
            return Repository.IsTimeOut(_info);
        }

        private bool ReleaseAuthen()
        {
            return Repository.ReleaseAuthen(_info);
        }

        private bool IsExitFamilyInstance(string category)
        {
            if (category == "Structure" || category == "Architecture") {
                FilteredElementCollector windownRectangle = new FilteredElementCollector(_document);
                List<Element> listRectangle = windownRectangle.OfCategory(BuiltInCategory.OST_Windows).Where(x => x.Name.Equals(Define.OPENNING_RECTANGLE_NAME_STRUCTURE) && x is FamilySymbol).ToList();
                FilteredElementCollector windownCylynder = new FilteredElementCollector(_document);
                List<Element> listCylynder = windownCylynder.OfCategory(BuiltInCategory.OST_Windows).Where(x => x.Name.Equals(Define.OPENNING_CYLYNDER_NAME_STRUCTURE) && x is FamilySymbol).ToList();
                if (listRectangle.Count == 0 || listCylynder.Count == 0) {
                    return false;
                }
                else {
                    return true;
                }
            }
            else if (category == "MEP") {
                FilteredElementCollector collector = new FilteredElementCollector(_document);
                List<Element> listRectangle = collector.OfCategory(BuiltInCategory.OST_GenericModel).Where(x => x.Name.Equals(Define.FLOOR_OPENNING_RECTANGLE_NAME_MEP) || x.Name.Equals(Define.WALL_OPENNING_RECTANGLE_NAME_MEP)).Where(x => x is FamilySymbol).ToList();
                List<Element> listCylynder = collector.OfCategory(BuiltInCategory.OST_GenericModel).Where(x => x.Name.Equals(Define.FLOOR_OPENNING_CYLYNDER_NAME_MEP) || x.Name.Equals(Define.WALL_OPENNING_CYLYNDER_NAME_MEP)).Where(x => x is FamilySymbol).ToList();
                if (listRectangle.Count == 0 || listCylynder.Count == 0) {
                    return false;
                }
                else {
                    return true;
                }
            }
            return false;
        }
    }
}