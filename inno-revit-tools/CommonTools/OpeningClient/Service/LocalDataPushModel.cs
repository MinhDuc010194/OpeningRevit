using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Service
{
    public class LocalDataPushModel : AuthenticationInfo
    {
        public List<ElementSendServer> OpeningsLocalPullAction { get; set; }

        public List<ElementSendServer> OpeningsLocalPushAction { get; set; }

        public List<ElementSendServer> OpeningsDisconnect { get; set; }
    }
}