using CommonTools.OpeningClient.Model;
using CommonTools.OpeningClient.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.HandleMiddleware.ServerGroupStatus
{
    public interface IFactory
    {
        List<OpeningModelDTO> RetreiveOpeningsServerWithStatus(string status);

        CompareClassifyLocalStatus RetreiveOpeningsLocalCoresponding(List<OpeningModelDTO> opennings);

        List<ComparisonCoupleElement> RetreiveNewOpeningLocal();
    }
}