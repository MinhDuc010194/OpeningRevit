using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Model.RollbackModel;
using System;
using System.Collections.Generic;

namespace CommonTools.OpeningClient.Service
{
    public class ProcessingDataRollback
    {
        private RollbackData _handingRollback;

        public ProcessingDataRollback(RollbackData handingRollback)
        {
            _handingRollback = handingRollback;
        }

        public LocalDataRollbackPushModel ProduceDataPostRollbackRequest(AuthenticationInfo info)
        {
            return new LocalDataRollbackPushModel() {
                DrawingName = info.DrawingName,
                MacAddress = info.MacAddress,
                ExistOnCurrentServer = RetreiveDataBeRollback(),
                NoExistOnCurrentServer = RetreiveDataWithElementDeletedCurrentRollback()
            };
        }

        private IEnumerable<ElementRollbackSendServer> RetreiveDataBeRollback()
        {
            foreach (var ele in _handingRollback.CompareMultipleRevisions) {
                if (ele.IsSelected) {
                    yield return new ElementRollbackSendServer() {
                        Id = ele.IdServer,
                        IdManager = ele.IdManager,
                        ServerStatus = ele.ServerStatus,
                        IdRevitElement = ele.CurentRevisionGeometryHasSelect.IdRevitElement,
                        Geometry = ele.CurentRevisionGeometryHasSelect.Geometry,
                        RevisionStatus = ele.CurentRevisionGeometryHasSelect.Geometry != null ? DefineStatus.NORMAL : DefineStatus.DELETED,
                    };
                }
            }
        }

        private IEnumerable<ElementRollbackSendServer> RetreiveDataWithElementDeletedCurrentRollback()
        {
            if (_handingRollback.RevisionNameSelected != "none") {
                var elems = _handingRollback.RevisionsWithManagersThatServerNoExist[_handingRollback.RevisionNameSelected];
                foreach (var element in elems) {
                    if (element.IsSelected) {
                        yield return new ElementRollbackSendServer() {
                            Id = Guid.NewGuid().ToString(),
                            IdManager = element.IdManager,
                            IdRevitElement = element.IdRevitElement,
                            Geometry = element.Geometry
                        };
                    }
                }
            }
        }
    }
}