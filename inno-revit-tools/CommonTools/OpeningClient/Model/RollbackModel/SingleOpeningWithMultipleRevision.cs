using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Model.RollbackModel
{
    /// <summary>
    /// Thang nay su dung cho viec 1 opening -> nhieu revision trong rollback
    /// </summary>
    public class SingleOpeningWithMultipleRevision
    {
        public string IdServer { get; set; }
        public string IdLocal { get; set; }
        public string IdManager { get; set; }
        public string IdDrawing { get; set; }

        public string ServerStatus { get; set; }
        public string RevisionStatus { get; set; }
        public GeometryDetail ServerGeometry { get; set; }
        public List<ElementManagerWithRevisionSpecify> RevisionsGeometry { get; set; }
        public ElementManagerWithRevisionSpecify CurentRevisionGeometryHasSelect { get; set; }
        public GeometryDetail NewGeometryFromRevision { get; set; }

        public bool IsSelected { get; set; }
    }
}