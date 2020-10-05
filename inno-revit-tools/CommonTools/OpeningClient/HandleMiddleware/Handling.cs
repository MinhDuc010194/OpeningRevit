using CommonTools.OpeningClient.HandleMiddleware.ExtensionProcess;
using CommonTools.OpeningClient.HandleMiddleware.ServerGroupStatus;
using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.HandleMiddleware
{
    public class Handling
    {
        private List<OpeningModelDTO> _revitElementsOnServer;
        private List<OpeningModel> _revitElementsOnLocal;
        private List<ElementManager> _elementOnStack;
        private List<VersionGeometryOfElementInRevision> _revitElementsWithCurrentVersionGeometry;

        #region HasSynch

        public IClassify ServerNormalStatus { get; set; }
        public IClassify ServerPendingCreateStatus { get; set; }
        public IClassify ServerPendingDeleteStatus { get; set; }

        #endregion HasSynch

        #region BelowLocal

        public List<ComparisonCoupleElement> NewOpeningsBelowLocal { get; set; }

        #endregion BelowLocal

        #region Sever

        public List<CompareCoupleWithManager> NewOpeningFromStack { get; set; } = new List<CompareCoupleWithManager>();

        #endregion Sever

        public Handling(List<OpeningModelDTO> revitElementsOnServer, List<OpeningModel> revitElementsOnLocal, List<ElementManager> elementOnStack)
        {
            _revitElementsOnServer = revitElementsOnServer;
            _revitElementsOnLocal = revitElementsOnLocal;
            _elementOnStack = elementOnStack;
            InitData();
        }

        public Handling(List<OpeningModelDTO> revitElementsOnServer, List<ElementManager> elementOnStack, List<VersionGeometryOfElementInRevision> elementsWithCurrentVersionGeometry)
        {
            _revitElementsOnServer = revitElementsOnServer;
            _elementOnStack = elementOnStack;
            _revitElementsWithCurrentVersionGeometry = elementsWithCurrentVersionGeometry;
        }

        public List<OpeningModel> RevitElementOnLocal { get => _revitElementsOnLocal; set => _revitElementsOnLocal = value; }

        public void InitData()
        {
            IFactory factory = new ClusterFactory(_revitElementsOnServer, _revitElementsOnLocal);
            ServerNormalStatus = new ServerNormalStatus(factory);
            ServerPendingCreateStatus = new ServerPendingCreateStatus(factory);
            ServerPendingDeleteStatus = new ServerPendingDeleteStatus(factory);
            ServerNormalStatus.ImplementClassifyLocal().InitCurrenVersionOpeningOnLocal(_revitElementsWithCurrentVersionGeometry);
            ServerPendingCreateStatus.ImplementClassifyLocal();
            ServerPendingDeleteStatus.ImplementClassifyLocal().InitCurrenVersionOpeningOnLocal(_revitElementsWithCurrentVersionGeometry);

            if (_elementOnStack != null && _elementOnStack.Count() > 0) {
                NewOpeningFromStack = AutoConverter.ConvertManagerToCompareCouple(_elementOnStack);
            }
            NewOpeningsBelowLocal = factory.RetreiveNewOpeningLocal();
        }

        public Handling()
        {
        }

        private void InitCurrentVersionGeometry()
        {
        }

        public void HandleImplement()
        {
        }
    }
}