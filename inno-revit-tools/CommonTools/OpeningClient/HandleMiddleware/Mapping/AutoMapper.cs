using CommonTools.OpeningClient.HandleMiddleware.DTO;
using CommonTools.OpeningClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.HandleMiddleware.Mapping
{
    public static class AutoMapper
    {
        public static ElementGetDTO ElementManagerToElement(this ElementManagerDTO elementManagerDTO)
        {
            return new ElementGetDTO() {
                Id = Guid.Empty,
                IdManager = elementManagerDTO.Id,
                IdRevitElement = Guid.Empty,
                Status = string.Empty,
                Geometry = elementManagerDTO.Geometry
            };
        }

        public static ElementSendDTO ComparisonToElementToSend(this ComparisonCoupleElement comparisonCoupleElement)
        {
            return new ElementSendDTO() {
            };
        }
    }
}