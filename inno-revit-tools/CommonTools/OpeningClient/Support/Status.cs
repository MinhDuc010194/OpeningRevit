using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Support
{
    public enum Status
    {
        NORMAL,
        PENDING_DELETE,
        PENDING_CREATE,
        DELETED,
        NONE
    }
}