using CommonTools.OpeningClient.Support;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Model
{
    public class OpeningModel
    {
        public OpeningModel()
        {
            Geometry = new GeometryDetail();
        }

        // RevitElement
        [JsonProperty("Id")]
        public string IdServer { get; set; }

        public string IdManager { get; set; }

        [JsonProperty("IdRevitElement")]
        public string IdLocal { get; set; }

        public string IdDrawing { get; set; }

        public string Comment { get; set; }

        [JsonProperty("ServerStatus")]
        public string Status { get; set; }

        //
        public GeometryDetail Geometry { get; set; }
    }

    public class OpeningModelDTO
    {
        public OpeningModelDTO()
        {
            Geometry = new GeometryDetail();
        }

        // RevitElement
        public string IdServer { get; set; }

        public string NameManager { get; set; }
        public string IdManager { get; set; }
        public string IdRevitElement { get; set; }
        public string IdDrawing { get; set; }
        public string Comment { get; set; }
        public string ServerStatus { get; set; }
        public GeometryDetail Geometry { get; set; }
    }
}