using CommonTools.OpeningClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Service
{
    public class AutoConverter
    {
        public static List<CompareCoupleWithManager> ConvertManagerToCompareCouple(IEnumerable<ElementManager> elementManagers)
        {
            List<CompareCoupleWithManager> listCompare = new List<CompareCoupleWithManager>();
            foreach (var manager in elementManagers) {
                listCompare.Add(new CompareCoupleWithManager() {
                    Id = new Guid().ToString(),
                    IdManager = manager.Id,
                    ServerGeometry = manager.Geometry,
                    DrawingsContain = manager.DrawingsContain,
                    Comment = manager.Comment
                });
            }
            return listCompare;
        }
    }
}