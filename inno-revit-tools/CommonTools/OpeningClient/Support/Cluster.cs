using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Support
{
    public enum Cluster
    {
        ExistOnLocalButNotOnServer,
        ExistOnLocalAndServer,
        ExistOnServerButNoOnLocal
    }
}