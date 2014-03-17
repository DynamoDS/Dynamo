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

        public DummyVector DirectionTo(DummyPoint p1)
        {
            return DummyVector.ByCoordinates(
                p1.X - X, p1.Y - Y, p1.Z - Z);
        }

        public DummyPoint Translate(DummyVector direction)
        {
            return DummyPoint.ByCoordinates(X + direction.X, Y + direction.Y, Z + direction.Z);
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
