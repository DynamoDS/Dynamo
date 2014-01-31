using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class CoordinateEntity : DesignScriptEntity, ICoordinateSystemEntity
    {
        internal CoordinateEntity()
        {
            this.Origin = new PointEntity() { X = 0, Y = 0, Z = 0 };
            this.XAxis = DsVector.ByCoordinates(1, 0, 0);
            this.YAxis = DsVector.ByCoordinates(0, 1, 0);
            this.ZAxis = DsVector.ByCoordinates(0, 0, 1);
        }

        public CoordinateEntity(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis, IVectorEntity zAxis)
        {
            this.Origin = origin;
            this.XAxis = xAxis;
            this.YAxis = yAxis;
            this.ZAxis = zAxis;
        }

        public IDisplayable Display
        {
            get { return null; }
        }

        public double GetDeterminant()
        {
            return 1.0;
        }

        public double[] GetScaleFactors()
        {
            return new double[] { 1, 1, 1 };
        }

        public ICoordinateSystemEntity Inverse()
        {
            return this;
        }

        public bool IsEqualTo(ICoordinateSystemEntity other)
        {
            return false;
        }

        public bool IsScaledOrtho()
        {
            return true;
        }

        public bool IsSingular()
        {
            return true;
        }

        public bool IsUniscaledOrtho()
        {
            return true;
        }
        
        public ICoordinateSystemEntity PostMultiplyBy(ICoordinateSystemEntity other)
        {
            return other;
        }

        public ICoordinateSystemEntity PreMultiplyBy(ICoordinateSystemEntity other)
        {
            return this;
        }

        public ICoordinateSystemEntity Rotation(double rotationAngle, IVectorEntity axis, IPointEntity origin)
        {
            return new CoordinateEntity();
        }

        public ICoordinateSystemEntity Scale(double scaleX, double scaleY, double scaleZ)
        {
            CoordinateEntity cs = new CoordinateEntity();
            cs.Set(this.Origin, this.XAxis, this.YAxis, this.ZAxis);
            cs.XAxis.Scale(scaleX);
            cs.YAxis.Scale(scaleY);
            cs.ZAxis.Scale(scaleZ);
            return cs;
        }

        public void Set(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis, IVectorEntity zAxis)
        {
            this.Origin = origin;
            this.XAxis = xAxis;
            this.YAxis = yAxis;
            this.ZAxis = zAxis;
        }

        public ICoordinateSystemEntity Translate(IVectorEntity translationVector)
        {
            CoordinateEntity cs = new CoordinateEntity();
            PointEntity pt = new PointEntity() { X = cs.Origin.X + translationVector.X, Y = cs.Origin.Y + translationVector.Y, Z = cs.Origin.Z + translationVector.Z };
            cs.Set(pt, this.XAxis, this.YAxis, this.ZAxis);
            return cs;
        }

        public ICoordinateSystemEntity Transpose()
        {
            return this;
        }

        public IPointEntity Origin
        {
            get;
            protected set;
        }
        public IVectorEntity XAxis
        {
            get;
            protected set;
        }
        public IVectorEntity YAxis
        {
            get;
            protected set;
        }
        public IVectorEntity ZAxis
        {
            get;
            protected set;
        }

        public ICoordinateSystemEntity Clone()
        {
            throw new NotImplementedException();
        }

        public ICoordinateSystemEntity Mirror(IPlaneEntity mirror_plane)
        {
            throw new NotImplementedException();
        }

        bool ICoordinateSystemEntity.IsSingular
        {
            get { throw new NotImplementedException(); }
        }

        bool ICoordinateSystemEntity.IsScaledOrtho
        {
            get { throw new NotImplementedException(); }
        }

        bool ICoordinateSystemEntity.IsUniscaledOrtho
        {
            get { throw new NotImplementedException(); }
        }

        public double Determinant
        {
            get { throw new NotImplementedException(); }
        }

        public IVectorEntity ScaleFactor()
        {
            throw new NotImplementedException();
        }

        public double XScaleFactor
        {
            get { throw new NotImplementedException(); }
        }

        public double YScaleFactor
        {
            get { throw new NotImplementedException(); }
        }

        public double ZScaleFactor
        {
            get { throw new NotImplementedException(); }
        }

        public void Translate(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        void ITransformableEntity.Translate(IVectorEntity vec)
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

        public void Scale(double amount) { throw new NotImplementedException(); }

        void ITransformableEntity.Scale(double xamount, double yamount, double zamount)
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
}
