using CommonTools.OpeningClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.HandleMiddleware.ServerGroupStatus
{
    public class ServerNormalStatus : IClassify
    {
        private IFactory _factory;
        private List<OpeningModel> _openingNormalStatus;
        private CompareClassifyLocalStatus _localClassify;

        public ServerNormalStatus(IFactory factory)
        {
            _factory = factory;
            _openingNormalStatus = _factory.RetreiveOpeningsServerWithStatus(DefineStatus.NORMAL);
            _openingNormalStatus.AddRange(_factory.RetreiveOpeningsServerWithStatus(DefineStatus.DISCONNECT));
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
            _localClassify = _factory.RetreiveOpeningsLocalCoresponding(_openingNormalStatus);
            return this;
        }
    }
}