using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class LineEntity : CurveEntity, ILineEntity
    {

        public void UpdateEndPoints(IPointEntity start, IPointEntity end)
        {
            StartPoint = start;
            EndPoint = end;
        }
        internal LineEntity()
        {
            StartPoint = new PointEntity();
            EndPoint = new PointEntity(1, 1, 1);
        }
        /*
        public double DistanceAtParameter(double param)
        {
            Vector dir = Vector.ByCoordinates(this.EndPoint.X - this.StartPoint.X, this.EndPoint.Y - this.StartPoint.Y, this.EndPoint.Z - this.StartPoint.Z, true);
            dir.MultiplyBy(param);
            return dir.GetLength();
        }
        */

        public override bool IsClosed
        {
            get { return false; }
        }

        public override bool IsPlanar
        {
            get { return true; }
        }
    }
}
