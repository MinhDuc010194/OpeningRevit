using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools.OpeningClient.Synchronize.Process
{
    class Common
    {
        /// <summary>
        /// Convert number from milimeter to foot
        /// </summary>
        /// <param name="mili"></param>
        /// <returns></returns>
        public static double MilimeterToFeet(double mili)
        {
            if (double.IsNaN(mili))
                return double.NaN;

            return mili / Define.UNIT_CONVERSION_RATIO;
        }

        /// <summary>
        /// Convert a point or a vector from milimeter to feet
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static XYZ MilimeterToFeet(XYZ point)
        {
            if (point == null)
                return null;

            double x = MilimeterToFeet(point.X);
            double y = MilimeterToFeet(point.Y);
            double z = MilimeterToFeet(point.Z);
            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Convert number from foot to milimeter
        /// </summary>
        /// <param name="feet"></param>
        /// <returns></returns>
        public static double FeetToMilimeter(double feet)
        {
            if (double.IsNaN(feet))
                return double.NaN;

            return feet * Define.UNIT_CONVERSION_RATIO;
        }

        /// <summary>
        /// Convert a point or a vector from feet to milimeter
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static XYZ FeetToMilimeter(XYZ point)
        {
            if (point == null)
                return null;

            double x = FeetToMilimeter(point.X);
            double y = FeetToMilimeter(point.Y);
            double z = FeetToMilimeter(point.Z);
            return new XYZ(x, y, z);
        }
    }
}
