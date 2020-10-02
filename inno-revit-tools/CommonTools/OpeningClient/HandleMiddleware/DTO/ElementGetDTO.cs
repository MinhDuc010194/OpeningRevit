using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.HandleMiddleware.DTO
{
    public class ElementGetDTO
    {
        public Guid Id { get; set; }
        public Guid IdManager { get; set; }
        public Guid IdRevitElement { get; set; }
        public string Status { get; set; }
        public GeometryDTO Geometry { get; set; }
    }
}