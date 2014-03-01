using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    public class DummyPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public static DummyPoint ByCoordinates(double x, double y, double z)
        {
            DummyPoint ret = new DummyPoint()
                {
                    X = x,
                    Y = y,
                    Z = z
                };

            return ret;
        }
    }

    public class DummyVector
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public static DummyVector ByCoordinates(double x, double y, double z)
        {
            DummyVector ret = new DummyVector()
            {
                X = x,
                Y = y,
                Z = z
            };

            return ret;
        }
    }

}
