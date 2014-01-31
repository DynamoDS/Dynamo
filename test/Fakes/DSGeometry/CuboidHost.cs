using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
namespace DSGeometry
{
    class CuboidEntity : SolidEntity, ICuboidEntity
    {
        public ICoordinateSystemEntity GetCoordinateSystem()
        {
            return ContextCoordinateSystem;
        }

        public CuboidEntity(double[] data, double length, double width, double height)
        {
            PointEntity origin = new PointEntity(data[0], data[1], data[2]);
            ContextCoordinateSystem = new CoordinateEntity(origin, DsVector.ByCoordinates(length, 0, 0), DsVector.ByCoordinates(0, width, 0), DsVector.ByCoordinates(0, 0, height));
        }

        public void UpdateCuboid(double[] data, double length, double width, double height)
        {
            PointEntity origin = new PointEntity(data[0], data[1], data[2]);
            ContextCoordinateSystem = new CoordinateEntity(origin, DsVector.ByCoordinates(length, 0, 0), DsVector.ByCoordinates(0, width, 0), DsVector.ByCoordinates(0, 0, height));
        }

        public override int GetEdgeCount()
        {
            return 12;
        }

        public override int GetFaceCount()
        {
            return 6;
        }

        public override int GetVertexCount()
        {
            return 8;
        }

        public override double Area
        {
            get { return (2 * Length * Width + 2 * Length * Height + 2 * Width * Height); }
        }

        public override double Volume
        {
            get { return Length * Width * Height; }
        }

        private ICoordinateSystemEntity mContextCoordinateSystem = null;
        public ICoordinateSystemEntity ContextCoordinateSystem
        {
            get
            {
                if (mContextCoordinateSystem == null)
                    mContextCoordinateSystem = new CoordinateEntity();
                return mContextCoordinateSystem;
            }
            protected set { mContextCoordinateSystem = value; }
        }

        public double Length
        {
            get { return ContextCoordinateSystem.XAxis.Length(); }
        }

        public double Width
        {
            get { return ContextCoordinateSystem.YAxis.Length(); }
        }

        public double Height
        {
            get { return ContextCoordinateSystem.ZAxis.Length(); }
        }
    }
}
