using CommonTools.OpeningClient.HandleMiddleware;
using CommonTools.OpeningClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Service
{
    public class PreprocessingDataPost
    {
        private Handling _handling;
        private List<ElementSendServer> _openingLocalPullAction;
        private List<ElementSendServer> _openingLocalPushAction;
        private List<ElementSendServer> _openingLocalDisconnect;

        public PreprocessingDataPost(Handling handling)
        {
            _handling = handling;
            _openingLocalPullAction = new List<ElementSendServer>();
            _openingLocalPushAction = new List<ElementSendServer>();
            _openingLocalDisconnect = new List<ElementSendServer>();
            ImplementClassifyData();
        }

        public LocalDataPushModel ProduceDataPostRequest(AuthenticationInfo info)
        {
            return new LocalDataPushModel() {
                DrawingName = info.DrawingName,
                MacAddress = info.MacAddress,
                OpeningsDisconnect = _openingLocalDisconnect,
                OpeningsLocalPullAction = _openingLocalPullAction,
                OpeningsLocalPushAction = _openingLocalPushAction
            };
        }

        public void ImplementClassifyData()
        {
            foreach (var opening in _handling.ServerNormalStatus.Data.NormalLocal) {
                AddOpeningToSpecificGroup(opening);
            }

            foreach (var opening in _handling.ServerNormalStatus.Data.DeletedLocal) {
                AddOpeningToSpecificGroup(opening);
            }

            foreach (var opening in _handling.ServerPendingCreateStatus.Data.NormalLocal) {
                AddOpeningToSpecificGroup(opening);
            }
            foreach (var opening in _handling.ServerPendingCreateStatus.Data.DeletedLocal) {
                AddOpeningToSpecificGroup(opening);
            }

            foreach (var opening in _handling.ServerPendingDeleteStatus.Data.NormalLocal) {
                AddOpeningToSpecificGroup(opening);
            }
            foreach (var opening in _handling.ServerPendingDeleteStatus.Data.DeletedLocal) {
                AddOpeningToSpecificGroup(opening);
            }
            foreach (var opening in _handling.NewOpeningsBelowLocal) {
                AddOpeningToSpecificGroup(opening);
            }

            foreach (var opening in _handling.NewOpeningFromStack) {
                AddOpeningFromStack(opening);
            }
        }

        private void AddOpeningToSpecificGroup(ComparisonCoupleElement coupleElement)
        {
            if (coupleElement.Action.Equals(Model.Action.PULL)) {
                _openingLocalPullAction.Add(ElementSendServer.Create(coupleElement));
            }
            else if (coupleElement.Action.Equals(Model.Action.PUSH)) {
                _openingLocalPushAction.Add(ElementSendServer.Create(coupleElement));
            }
            else if (coupleElement.Action.Equals(Model.Action.DISCONNECT)) {
                _openingLocalDisconnect.Add(ElementSendServer.Create(coupleElement));
            }
        }

        private void AddOpeningFromStack(ComparisonCoupleElement coupleElement)
        {
            if (coupleElement.Action.Equals(Model.Action.PULL)) {
                _openingLocalPullAction.Add(ElementSendServer.CreateWithStack(coupleElement));
            }
        }
    }
}