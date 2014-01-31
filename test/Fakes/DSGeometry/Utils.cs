using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class DsVector : DesignScriptEntity, IVectorEntity
    {
        public static DsVector ByCoordinates(double x, double y, double z)
        {
            return new DsVector { X = x, Y = y, Z = z };
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public bool IsAlmostEqualTo(IVectorEntity other)
        {
            throw new NotImplementedException();
        }

        public void Translate(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public void Translate(IVectorEntity vec)
        {
            throw new NotImplementedException();
        }

        public void TransformBy(ICoordinateSystemEntity cs)
        {
            throw new NotImplementedException();
        }

        public void TransformFromTo(ICoordinateSystemEntity from, ICoordinateSystemEntity to)
        {
            throw new NotImplementedException();
        }

        public void Rotate(IPointEntity origin, IVectorEntity axis, double degrees)
        {
            throw new NotImplementedException();
        }

        public void Rotate(IPlaneEntity origin, double degrees)
        {
            throw new NotImplementedException();
        }

        public void Scale(double amount)
        {
            throw new NotImplementedException();
        }

        public void Scale(double xamount, double yamount, double zamount)
        {
            throw new NotImplementedException();
        }

        public void Scale(IPointEntity from, IPointEntity to)
        {
            throw new NotImplementedException();
        }

        public void Scale1D(IPointEntity from, IPointEntity to)
        {
            throw new NotImplementedException();
        }

        public void Scale2D(IPointEntity from, IPointEntity to)
        {
            throw new NotImplementedException();
        }
    }

    class DsColor : IColor
    {
        public byte AlphaValue { get; set; }

        public byte RedValue { get; set; }

        public byte GreenValue { get; set; }

        public byte BlueValue { get; set; }
    }

    static class Utils
    {
        public static Vector ToVector(this IVectorEntity v)
        {
            return Vector.ByCoordinates(v.X, v.Y, v.Z);
        }

        public static IVectorEntity ToIVector(this Vector v)
        {
            return new DsVector { X = v.X, Y = v.Y, Z = v.Z };
        }

        public static IVectorEntity Scale(this IVectorEntity v, double scale)
        {
            return v.ToVector().Scale(scale).ToIVector();
        }

        public static double Length(this IVectorEntity v)
        {
            return v.ToVector().Length;
        }

        public static Color ToColor(this IColor c)
        {
            return Color.ByARGB(c.AlphaValue, c.RedValue, c.GreenValue, c.BlueValue);
        }

        public static IColor ToIColor(this Color c)
        {
            return new DsColor { AlphaValue = c.AlphaValue, RedValue = c.RedValue, GreenValue = c.GreenValue, BlueValue = c.BlueValue };
        }
    }
}
