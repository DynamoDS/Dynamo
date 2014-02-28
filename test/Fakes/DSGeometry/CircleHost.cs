using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class CircleEntity : CurveEntity, ICircleEntity
    {
        internal CircleEntity()
        {
            this.CenterPoint = new PointEntity(0, 0, 0);
            this.Radius = 3;
            this.Normal = DsVector.ByCoordinates(0, 0, 1);
        }

        internal CircleEntity(IPointEntity center, double radius, IVectorEntity normal)
        {
            this.CenterPoint = center;
            this.Radius = radius;
            this.Normal = normal;
        }

        public IPointEntity CenterPoint
        {
            get;
            protected set;
        }

        public IVectorEntity Normal
        {
            get;
            protected set;
        }

        public double Radius
        {
            get;
            protected set;
        }

        public override bool IsClosed
        {
            get { return true; }
        }

        public override bool IsPlanar
        {
            get { return true; }
        }
    }
}
