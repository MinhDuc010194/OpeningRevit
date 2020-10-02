using CommonTools.OpeningClient.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Service
{
    public class ElementManager
    {
        [JsonProperty("IdManager")]
        public string Id { get; set; }

        public GeometryDetail Geometry { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }

        public IEnumerable<string> DrawingsContain { get; set; }
    }
}