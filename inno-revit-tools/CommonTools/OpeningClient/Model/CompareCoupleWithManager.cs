using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Model
{
    public class CompareCoupleWithManager : ComparisonCoupleElement
    {
        public IEnumerable<string> DrawingsContain { get; set; }
    }
}