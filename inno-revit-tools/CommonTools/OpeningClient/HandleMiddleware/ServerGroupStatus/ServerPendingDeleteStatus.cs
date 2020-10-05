using CommonTools.OpeningClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.HandleMiddleware.ServerGroupStatus
{
    public class ServerPendingDeleteStatus : IClassify
    {
        private IFactory _factory;
        private List<OpeningModelDTO> _openingPendingDeleteStatus;
        private CompareClassifyLocalStatus _localClassify;

        public ServerPendingDeleteStatus(IFactory factory)
        {
            _factory = factory;
            _openingPendingDeleteStatus = _factory.RetreiveOpeningsServerWithStatus(DefineStatus.PENDING_DELETE);
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
            _localClassify = _factory.RetreiveOpeningsLocalCoresponding(_openingPendingDeleteStatus);
            return this;
        }
    }
}