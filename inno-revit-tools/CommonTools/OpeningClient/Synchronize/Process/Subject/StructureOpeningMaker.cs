using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Synchronize.Process.Subject
{
    class StructureOpeningMaker
    {
        private bool _isVertical;
        public StructureOpeningMaker(XYZ direction)
        {
            if(direction == XYZ.BasisZ || direction == XYZ.BasisZ.Negate())
            {
                _isVertical = true;
            }
            else
            {
                _isVertical = false;
            }
        }

        void CreateOpenning()
        {
            if(_isVertical == true)
            {
                CreateVerticalOpenning();
            }
            else
            {
                CreateHorizontalOpenning();
            }
        }

        private void CreateVerticalOpenning()
        {
            throw new NotImplementedException();
        }
    }
}
