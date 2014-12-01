using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    public class DummyPoint : IDisposable
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

        public DummyPoint Translate(double dx, double dy, double dz)
        {
            return DummyPoint.ByCoordinates(X + dx, Y + dy, Z + dz);
        }

        public static DummyPoint Centroid(IList<DummyPoint> points)
        {
            return ByCoordinates(points.Average(p => p.X), points.Average(p => p.Y), points.Average(p => p.Z));
        }

        public DummyPoint UnknownPoint()
        {
            return new UnknownPoint(this.X, this.Y, this.Z);
        }

        public override bool Equals(object obj)
        {
            DummyPoint other = obj as DummyPoint;
            if (null == other)
                return false;

            return this.DirectionTo(other).GetLengthSquare() < 0.00001;
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }

    public class UnknownPoint : DummyPoint
    {
        internal UnknownPoint(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
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

        public DummyVector Scale(double value)
        {
            return DummyVector.ByCoordinates(X * value, Y * value, Z * value);
        }

        public double GetLengthSquare()
        {
            return X * X + Y * Y + Z * Z;
        }
    }

    public class DummyLine : IDisposable
    {
        public DummyPoint Start { get; private set; }
        public DummyPoint End { get; private set; }

        public static DummyLine ByStartPointEndPoint(DummyPoint a, DummyPoint b)
        {
            DummyLine ln = new DummyLine();
            ln.Start = a;
            ln.End = b;

            return ln;
        }

        public void Dispose()
        {
            //Don't do anything
        }
        
    }


}
