using CommonTools.OpeningClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.HandleMiddleware.ServerGroupStatus
{
    public class CompareClassifyLocalStatus
    {
        public List<ComparisonCoupleElement> DeletedLocal { get; set; }
        public List<ComparisonCoupleElement> NormalLocal { get; set; }

        public CompareClassifyLocalStatus()
        {
            DeletedLocal = new List<ComparisonCoupleElement>();
            NormalLocal = new List<ComparisonCoupleElement>();
        }
    }
}