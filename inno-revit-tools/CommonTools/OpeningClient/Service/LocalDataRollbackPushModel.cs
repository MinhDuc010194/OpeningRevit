using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Service
{
    public class LocalDataRollbackPushModel : AuthenticationInfo
    {
        public IEnumerable<ElementRollbackSendServer> ExistOnCurrentServer { get; set; }
        public IEnumerable<ElementRollbackSendServer> NoExistOnCurrentServer { get; set; }
    }
}