using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
namespace DSGeometry
{
    class ConeEntity : SolidEntity, IConeEntity
    {
        internal ConeEntity()
        {
            this.StartPoint = new PointEntity();
            this.EndPoint = new PointEntity(1, 1, 1);
            this.StartRadius = 2;
            this.EndRadius = 0;
            this.Height = EndPoint.DistanceTo(StartPoint);
        }

        public ICoordinateSystemEntity GetCoordinateSystem()
        {
            return new CoordinateEntity();
        }

        public IPointEntity GetEndPoint()
        {
            return EndPoint;
        }

        public double GetEndRadius()
        {
            return EndRadius;
        }

        public double GetHeight()
        {
            return Height;
        }

        public IPointEntity GetStartPoint()
        {
            return StartPoint;
        }

        public double GetStartRadius()
        {
            return StartRadius;
        }

        public void UpdateCone(IPointEntity startPoint, IPointEntity endPoint, double startRadius, double endRadius)
        {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this.StartRadius = startRadius;
            this.EndRadius = endRadius; 
        }

        public void UpdateCylinder(double[] data, double startRadius, double endRadius, double length)
        {

            this.StartRadius = startRadius;
            this.EndRadius = endRadius;
            this.Height = length;
        }

        public override int GetEdgeCount()
        {
            return 1;
        }

        public override int GetFaceCount()
        {
            return 2;
        }

        public override int GetVertexCount()
        {
            return 1;
        }

        public override double Area
        {
            get { return (Math.PI * (StartRadius + EndRadius) * Math.Sqrt((StartRadius - EndRadius) * (StartRadius - EndRadius) + Height * Height))
                + (Math.PI * (StartRadius * StartRadius + EndRadius * EndRadius)); }
        }

        public override double Volume
        {
            get { return (1 / 3) * Math.PI * (StartRadius * StartRadius + StartRadius * EndRadius + EndRadius * EndRadius) * Height; }
        }

        public double StartRadius { get; protected set; }

        public IPointEntity StartPoint { get; protected set; }

        public double Height { get; protected set; }

        public double EndRadius { get; protected set; }

        public IPointEntity EndPoint { get; protected set; }

        public double RadiusRatio
        {
            get { throw new NotImplementedException(); }
        }
    }
}
