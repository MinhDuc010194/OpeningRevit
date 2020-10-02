using CommonTools.OpeningClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Service
{
    public class ElementRollbackSendServer
    {
        public string Id { get; set; }
        public string IdManager { get; set; }
        public string IdRevitElement { get; set; }
        public string ServerStatus { get; set; }
        public GeometryDetail Geometry { get; set; }
        public string RevisionStatus { get; set; }
    }
}