using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Model.RollbackModel;
using CommonUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommonTools.OpeningClient.Support;
using System.Net.NetworkInformation;
using CommonTools.OpeningClient.Synchronize.UI.Model;

namespace CommonTools.OpeningClient.Service
{
    public class Repository
    {
        public static List<ElementManager> GetManagerExept(string drawingName, string groupDrawing)
        {
            List<ElementManager> manangers = new List<ElementManager>();
            string url = Support.Define.BASE_URL + "api/ElementManagement/GetAllElementManagementOnGroupExeptAsync/" + drawingName + "/" + groupDrawing;
            HttpClient client = new HttpClient();

            var task = client.GetAsync(url).ContinueWith(taskResponse => {
                var response = taskResponse.Result;
                if (response.IsSuccessStatusCode) {
                    var jsonString = response.Content.ReadAsStringAsync();
                    jsonString.Wait();
                    manangers = JsonConvert.DeserializeObject<List<ElementManager>>(jsonString.Result);
                }
            });
            task.Wait();
            return manangers;
        }

        public static List<OpeningModel> GetOpeningOnServer(string drawingName)
        {
            List<OpeningModel> openings = new List<OpeningModel>();
            string url = Support.Define.BASE_URL + "api/Element/GetAllElementInDrawing/" + drawingName;
            HttpClient client = new HttpClient();
            var task = client.GetAsync(url).ContinueWith(taskResponse => {
                var response = taskResponse.Result;
                if (response.IsSuccessStatusCode) {
                    var jsonString = response.Content.ReadAsStringAsync();
                    jsonString.Wait();
                    openings = JsonConvert.DeserializeObject<List<OpeningModel>>(jsonString.Result);
                }
            });
            task.Wait();
            return openings;
        }

        public static List<RevisionGetModel> GetRevision(string drawingName)
        {
            List<RevisionGetModel> revisions = new List<RevisionGetModel>();
            string url = Support.Define.BASE_URL + "api/Revision/GetAllRevisionOnDrawingAsync/" + drawingName;
            HttpClient client = new HttpClient();
            var task = client.GetAsync(url).ContinueWith(taskResponse => {
                var response = taskResponse.Result;
                if (response.IsSuccessStatusCode) {
                    var jsonString = response.Content.ReadAsStringAsync();
                    jsonString.Wait();
                    revisions = JsonConvert.DeserializeObject<List<RevisionGetModel>>(jsonString.Result);
                }
            });
            task.Wait();
            return revisions;
        }

        public static List<VersionGeometryOfElementInRevision> GetElementWithVersionCurrent(string drawingName)
        {
            List<VersionGeometryOfElementInRevision> elementsWithCurrentVersionOnDrawing = new List<VersionGeometryOfElementInRevision>();
            string url = Support.Define.BASE_URL + "api/Element/GetElementWithGeometryInLatestRevisionOfDrawingAsync/" + drawingName;
            HttpClient client = new HttpClient();
            var task = client.GetAsync(url).ContinueWith(taskResponse => {
                var response = taskResponse.Result;
                if (response.IsSuccessStatusCode) {
                    var jsonString = response.Content.ReadAsStringAsync();
                    jsonString.Wait();
                    elementsWithCurrentVersionOnDrawing = JsonConvert.DeserializeObject<List<VersionGeometryOfElementInRevision>>(jsonString.Result);
                }
            });
            task.Wait();
            return elementsWithCurrentVersionOnDrawing;
        }

        public static bool PostDataToServer(LocalDataPushModel dataLocal)
        {
            var json = JsonConvert.SerializeObject(dataLocal);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var url = Support.Define.BASE_URL + "api/SyncRevitClient/SyncDataOfLocalAsync";
            string message = string.Empty;
            bool status = true;
            HttpClient client = new HttpClient();
            var task = client.PutAsync(url, data).ContinueWith(taskResponse => {
                var response = taskResponse.Result;
                if (!response.IsSuccessStatusCode) {
                    status = response.IsSuccessStatusCode;
                    var jsonString = response.Content.ReadAsStringAsync();
                    jsonString.Wait();
                    message = jsonString.Result;
                }
            });
            task.Wait();
            if (!status) {
                MessageBox.Show(message);
            }
            return status;
        }

        public static bool PostDataRollbackToServerAsync(LocalDataRollbackPushModel dataLocal)
        {
            var json = JsonConvert.SerializeObject(dataLocal);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var url = Support.Define.BASE_URL + "api/SyncRevitClient/RollbackAsync";
            string message = string.Empty;
            bool status = true;
            HttpClient client = new HttpClient();
            var task = client.PutAsync(url, data).ContinueWith(taskResponse => {
                var response = taskResponse.Result;
                if (!response.IsSuccessStatusCode) {
                    status = response.IsSuccessStatusCode;
                    var jsonString = response.Content.ReadAsStringAsync();
                    jsonString.Wait();
                    message = jsonString.Result;
                }
            });
            task.Wait();
            if (!status) {
                MessageBox.Show(message);
            }
            return status;
        }

        public static string GetCategoryName(string drawingName)
        {
            string url = Support.Define.BASE_URL + "api/Drawing/GetDrawingInfoAsync/" + drawingName;
            string categoryName = string.Empty;
            HttpClient client = new HttpClient();
            var task = client.GetAsync(url).ContinueWith(taskResponse => {
                var response = taskResponse.Result;
                if (response.IsSuccessStatusCode) {
                    var jsonString = response.Content.ReadAsStringAsync();
                    jsonString.Wait();
                    categoryName = jsonString.Result;
                }
            });
            task.Wait();
            return categoryName;
        }

        public static string GetTimeout()
        {
            string url = Support.Define.BASE_URL + "api/AspNetBLSTT/GetTimeOut/";
            string categoryName = string.Empty;
            HttpClient client = new HttpClient();
            var task = client.GetAsync(url).ContinueWith(taskResponse => {
                var response = taskResponse.Result;
                if (response.IsSuccessStatusCode) {
                    var jsonString = response.Content.ReadAsStringAsync();
                    jsonString.Wait();
                    categoryName = jsonString.Result;
                }
            });
            task.Wait();
            return categoryName;
        }

        public static string GetUserNamebyMac()
        {
            var macAddress = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault();

            string url = Support.Define.BASE_URL + "api/Project/GetUserByMacAddress/" + macAddress;
            string UserName = string.Empty;
            HttpClient client = new HttpClient();
            var task = client.GetAsync(url).ContinueWith(taskResponse => {
                var response = taskResponse.Result;
                if (response.IsSuccessStatusCode) {
                    var jsonString = response.Content.ReadAsStringAsync();
                    jsonString.Wait();
                    UserName = jsonString.Result;
                }
            });
            task.Wait();
            return UserName;
        }

        public static string GetDrawingGroupName(string drawingName)
        {
            string url = Support.Define.BASE_URL + "api/DrawingGroup/GetDrawingGroupNameByDrawingName/" + drawingName;
            string drawingGroupName = string.Empty;
            HttpClient client = new HttpClient();
            var task = client.GetAsync(url).ContinueWith(taskResponse => {
                var response = taskResponse.Result;
                if (response.IsSuccessStatusCode) {
                    var jsonString = response.Content.ReadAsStringAsync();
                    jsonString.Wait();
                    drawingGroupName = jsonString.Result;
                }
            });
            task.Wait();
            return drawingGroupName;
        }

        public static List<DrawingsName> GetDrawingInGroupName(string GroupName)
        {
            string url = Support.Define.BASE_URL + "api/Drawing/GetDrawingInGroupName/" + GroupName;
            List<DrawingsName> drawingGroupName = new List<DrawingsName>();
            HttpClient client = new HttpClient();
            var task = client.GetAsync(url).ContinueWith(taskResponse => {
                var response = taskResponse.Result;
                if (response.IsSuccessStatusCode) {
                    var jsonString = response.Content.ReadAsStringAsync();
                    jsonString.Wait();
                    drawingGroupName = JsonConvert.DeserializeObject<List<DrawingsName>>(jsonString.Result);
                }
            });
            task.Wait();
            return drawingGroupName;
        }

        public static bool IsTimeOut(AuthenticationInfo info)
        {
            var json = JsonConvert.SerializeObject(info);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var url = Support.Define.BASE_URL + "api/Project/TimeOutCheckingAsync";
            string result = string.Empty;
            bool status = false;
            HttpClient client = new HttpClient();
            var task = client.PutAsync(url, data).ContinueWith(res => {
                var response = res.Result;
                status = response.IsSuccessStatusCode;
                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                result = jsonString.Result;
            });
            task.Wait();
            if (status) {
                //return true;
                return result.Equals("TimeOut");
            }
            else {
                // return true;
                Utils.MessageWarning(Define.MissingFamilyInstance);
                return false;
            }
        }

        public static bool AuthenticationUpdate(AuthenticationInfo info)
        {
            var json = JsonConvert.SerializeObject(info);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var url = Support.Define.BASE_URL + "api/Project/AuthenticationUpdatingAsync";
            string result = string.Empty;
            bool status = false;
            HttpClient client = new HttpClient();
            var task = client.PutAsync(url, data).ContinueWith(res => {
                var response = res.Result;
                status = response.IsSuccessStatusCode;
                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                result = jsonString.Result;
            });
            task.Wait();
            if (status) {
                if (!result.Equals("Updated")) {
                    Utils.MessageWarning("Server is busy.");
                    return false;
                }
                return true;
            }
            else {
                Utils.MessageInfor(result);
                return false;
            }
        }

        public static bool ReleaseAuthen(AuthenticationInfo info)
        {
            var json = JsonConvert.SerializeObject(info);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var url = Support.Define.BASE_URL + "api/Project/ReleaseAuthenticationAysnc";
            string result = string.Empty;
            bool status = false;
            HttpClient client = new HttpClient();
            var task = client.PutAsync(url, data).ContinueWith(res => {
                var response = res.Result;
                status = response.IsSuccessStatusCode;
                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                result = jsonString.Result;
            });
            task.Wait();
            if (status) {
                return result.Equals("Updated");
            }
            else {
                MessageBox.Show(result);
                return false;
            }
        }
    }
}