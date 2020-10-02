using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.HandleMiddleware.DTO
{
    public class ElementManagerDTO
    {
        public Guid Id { get; set; }
        public GeometryDTO Geometry { get; set; }
        public IEnumerable<string> DrawingsContain { get; set; }
    }
}