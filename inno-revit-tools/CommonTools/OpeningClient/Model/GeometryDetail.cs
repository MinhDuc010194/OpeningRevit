using CommonTools.OpeningClient.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Model
{
    public class GeometryDetail
    {
        public string Geometry { get; set; }
        public string Original { get; set; }
        public string Direction { get; set; }
        public string Version { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}