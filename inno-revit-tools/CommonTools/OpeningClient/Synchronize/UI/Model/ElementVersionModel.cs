using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Synchronize.UI.Model
{
    public class DrawingsName
    {
        public string Name { get; set; }
    }

    public class ChilrendTreview
    {
        public ChilrendTreview()
        {
            DrawingsNames = new ObservableCollection<ChilrendTreview>();
        }

        public string GroupName { get; set; }
        public ObservableCollection<ChilrendTreview> DrawingsNames { get; set; }
    }
}