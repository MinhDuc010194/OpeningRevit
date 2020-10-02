using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Model.RollbackModel
{
    public class ElementManagerWithRevisionSpecify : ElementManagerWithRevisionGeometry
    {
        public string IdRevision { get; set; }
        public string IdRevitElement { get; set; }
        public string Version { get; set; }
        public string Date { get => "v" + Version + ":" + String.Format("{0:f}", Geometry.UpdatedDate); }

        /// <summary>
        /// property nay chi su dung khi no nam trong danh sach revision co nhung server thi k co manager do.
        /// </summary>
        public bool IsSelected { get; set; }
    }
}