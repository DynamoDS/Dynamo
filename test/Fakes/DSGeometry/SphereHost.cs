using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class SphereEntity : SolidEntity, ISphereEntity
    {
        internal SphereEntity()
        {
            this.CenterPoint = new PointEntity();
            this.Radius = 1;
        }

        public SphereEntity(IPointEntity centerPoint, double radius)
        {
            this.CenterPoint = centerPoint;
            this.Radius = radius;
        }

        public IPointEntity GetCenterPoint()
        {
            return CenterPoint;
        }

        public double GetRadius()
        {
            return Radius;
        }

        public void UpdateSphere(IPointEntity centerPoint, double radius)
        {
            this.CenterPoint = centerPoint;
            this.Radius = radius;
        }

        public override IPointEntity GetCentroid()
        {
            return this.CenterPoint;
        }

        public override int GetEdgeCount()
        {
            return 0;
        }

        public override int GetFaceCount()
        {
            return 1;
        }

        public override int GetVertexCount()
        {
            return 0;
        }

        public override double Area
        {
            get { return Radius * Radius * Math.PI * 4; }
        }

        public override double Volume
        {
            get { return (4 / 3) * Math.PI * Radius * Radius * Radius; }
        }

        public IPointEntity CenterPoint { get; protected set; }

        public double Radius { get; protected set; }

        public ICoordinateSystemEntity GetCoordinateSystem()
        {
            throw new NotImplementedException();
        }
    }
}
