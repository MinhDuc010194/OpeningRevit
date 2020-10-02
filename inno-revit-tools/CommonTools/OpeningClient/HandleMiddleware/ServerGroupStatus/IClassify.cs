using CommonTools.OpeningClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.HandleMiddleware.ServerGroupStatus
{
    public interface IClassify
    {
        IClassify ImplementClassifyLocal();

        CompareClassifyLocalStatus Data { get; }
    }
}