using Autodesk.Revit.DB;
using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.HandleMiddleware.ServerGroupStatus
{
    public class ClusterFactory : IFactory
    {
        private List<OpeningModel> _openingsServer;
        private List<OpeningModel> _openingsLocal;

        public ClusterFactory(List<OpeningModel> openingsServer, List<OpeningModel> openingsLocal)
        {
            _openingsLocal = openingsLocal;
            _openingsServer = openingsServer;
        }

        public List<OpeningModel> RetreiveOpeningsServerWithStatus(string status)
        {
            return _openingsServer.Where(x => x.Status.Equals(status)).ToList();
        }

        public List<ComparisonCoupleElement> RetreiveNewOpeningLocal()
        {
            List<ComparisonCoupleElement> newOpeningFromLocal = new List<ComparisonCoupleElement>();
            foreach (var opening in _openingsLocal) {
                if (!_openingsServer.Any(x => Common.IsValidGuid(x.IdLocal)
                                            && Common.IsValidGuid(opening.IdLocal)
                                            && x.IdLocal.Equals(opening.IdLocal))) {
                    newOpeningFromLocal.Add(new ComparisonCoupleElement(opening, false));
                }
            }
            return newOpeningFromLocal;
        }

       

        public List<ComparisonCoupleElement> RetreiveNewOpeningFromStack(List<OpeningModel> stackOpenings)
        {
            List<ComparisonCoupleElement> newOpeningsFromStack = new List<ComparisonCoupleElement>();
            foreach (var opening in stackOpenings) {
                newOpeningsFromStack.Add(new ComparisonCoupleElement() { IdManager = opening.IdManager, ServerGeometry = opening.Geometry });
            }
            return newOpeningsFromStack;
        }

        public CompareClassifyLocalStatus RetreiveOpeningsLocalCoresponding(List<OpeningModel> opennings)
        {
            CompareClassifyLocalStatus localClassifyStatus = new CompareClassifyLocalStatus();

            foreach (var opening in opennings) {
                var openingLocalFind = _openingsLocal.FirstOrDefault(x => x.IdLocal.Equals(opening.IdLocal));
                if (openingLocalFind != null) {
                    if (localClassifyStatus.NormalLocal == null) {
                        localClassifyStatus.NormalLocal = new List<ComparisonCoupleElement>();
                    }
                    localClassifyStatus.NormalLocal.Add(new ComparisonCoupleElement(opening, openingLocalFind));
                }
                else {
                    if (localClassifyStatus.DeletedLocal == null) {
                        localClassifyStatus.DeletedLocal = new List<ComparisonCoupleElement>();
                    }
                    localClassifyStatus.DeletedLocal.Add(new ComparisonCoupleElement(opening) { LocalStatus = DefineStatus.DELETED });
                }
            }
            return localClassifyStatus;
        }
    }
}