using CommonTools.OpeningClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Service
{
    public class ElementSendServer
    {
        public string Id { get; set; } = new Guid().ToString();
        public string IdManager { get; set; } = new Guid().ToString();
        public string IdRevitElement { get; set; } = string.Empty;
        public GeometryDetail Geometry { get; set; }
        public string ServerStatus { get; set; } = string.Empty;
        public string LocalStatus { get; set; } = string.Empty;
        public bool DifferenceGeometry { get; set; }
        public string Comment { get; set; }

        public static ElementSendServer Create(ComparisonCoupleElement coupleElement)
        {
            return new ElementSendServer() {
                Id = coupleElement.Id,
                IdManager = coupleElement.IdManager,
                IdRevitElement = coupleElement.IdRevitElement,
                Geometry = coupleElement.Action == Model.Action.PUSH ? coupleElement.LocalGeometry : coupleElement.ServerGeometry,
                ServerStatus = coupleElement.ServerStatus,
                LocalStatus = coupleElement.LocalStatus,
                Comment = coupleElement.Comment,
                DifferenceGeometry = !coupleElement.IsSameShapeAndLocation()
            };
        }

        public static ElementSendServer CreateWithStack(ComparisonCoupleElement coupleElement)
        {
            return new ElementSendServer() {
                Id = coupleElement.Id,
                IdManager = coupleElement.IdManager,
                IdRevitElement = coupleElement.IdRevitElement,
                Geometry = coupleElement.ServerGeometry,
                ServerStatus = DefineStatus.NONE,
                LocalStatus = DefineStatus.NONE,
                Comment = coupleElement.Comment,
                DifferenceGeometry = !coupleElement.IsSameShapeAndLocation()
            };
        }
    }
}