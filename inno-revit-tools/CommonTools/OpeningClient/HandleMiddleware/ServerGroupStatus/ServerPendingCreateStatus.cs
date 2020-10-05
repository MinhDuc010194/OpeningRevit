using CommonTools.OpeningClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.HandleMiddleware.ServerGroupStatus
{
    public class ServerPendingCreateStatus : IClassify
    {
        private IFactory _factory;
        private List<OpeningModelDTO> _openingPendingCreateStatus;
        private CompareClassifyLocalStatus _localClassify;

        public ServerPendingCreateStatus(IFactory factory)
        {
            _factory = factory;
            _openingPendingCreateStatus = _factory.RetreiveOpeningsServerWithStatus(DefineStatus.PENDING_CREATE);
        }

        public CompareClassifyLocalStatus Data
        {
            get
            {
                if (_localClassify == null) {
                    ImplementClassifyLocal();
                }
                return _localClassify;
            }
        }

        public IClassify ImplementClassifyLocal()
        {
            _localClassify = _factory.RetreiveOpeningsLocalCoresponding(_openingPendingCreateStatus);
            return this;
        }
    }
}