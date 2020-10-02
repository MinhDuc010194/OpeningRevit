using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Model.RollbackModel
{
    public class RevisionGetModel
    {
        public string IdRevision { get; set; }
        public string Version { get; set; }
        public DateTime CreatedDate { get; set; }
        public IEnumerable<ElementManagerWithRevisionGeometry> Managers { get; set; }
    }
}