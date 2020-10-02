using CommonTools.OpeningClient.HandleMiddleware.ServerGroupStatus;
using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.HandleMiddleware.ExtensionProcess
{
    public static class OpeningExtension
    {
        public static void InitCurrenVersionOpeningOnLocal(this IClassify classify, List<VersionGeometryOfElementInRevision> versionGeometries)
        {
            if (classify.Data == null) {
                return;
            }
            var data = classify.Data;
            if (data.NormalLocal.Count > 0) {
                foreach (var eleCompare in data.NormalLocal) {
                    if (!string.IsNullOrEmpty(eleCompare.IdRevitElement)) {
                        var eleFind = versionGeometries.FirstOrDefault(x => Common.IsValidGuid(x.IdLocal)
                                                                            && Common.IsValidGuid(eleCompare.IdRevitElement)
                                                                            && x.IdLocal.Equals(eleCompare.IdRevitElement));
                        if (eleFind != null) {
                            eleCompare.CurrentVersionGeometryOfLocal = eleFind.VersionGeometry;
                        }
                    }
                }
            }
            if (data.DeletedLocal.Count > 0) {
                foreach (var eleCompare in data.DeletedLocal) {
                    if (!string.IsNullOrEmpty(eleCompare.IdRevitElement)) {
                        var eleFind = versionGeometries.FirstOrDefault(x => Common.IsValidGuid(x.IdLocal)
                                                                            && Common.IsValidGuid(eleCompare.IdRevitElement)
                                                                            && x.IdLocal.Equals(eleCompare.IdRevitElement));
                        if (eleFind != null) {
                            eleCompare.CurrentVersionGeometryOfLocal = eleFind.VersionGeometry;
                        }
                    }
                }
            }
        }
    }
}