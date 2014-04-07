using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class ArcEntity : CurveEntity, IArcEntity
    {
        internal ArcEntity()
        {
            CenterPoint = new PointEntity();
            Normal = DsVector.ByCoordinates(0, 0, 1);
            Radius = 1;
            StartAngle = 30;
            SweepAngle = 60;
        }

        internal ArcEntity(IPointEntity center, IVectorEntity normal, double radius, double startAngle, double sweepAngle)
        {
            this.CenterPoint = center;
            this.Normal = normal;
            this.Radius = radius;
            this.StartAngle = startAngle;
            this.SweepAngle = sweepAngle;
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

        public double StartAngle
        {
            get;
            protected set;
        }

        public double SweepAngle
        {
            get;
            protected set;
        }

        public override bool IsClosed
        {
            get { return false; }
        }

        public override bool IsPlanar
        {
            get { return true; }
        }

        public override double GetLength()
        {
            return Radius * SweepAngle/180*Math.PI;
        }
    }
}
