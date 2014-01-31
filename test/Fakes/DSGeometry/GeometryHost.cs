using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class DesignScriptEntity : IDesignScriptEntity
    {
        private Object mOwner = null;
        public virtual Object Owner
        {
            get { return mOwner; }
        }

        public virtual void SetOwner(Object owner)
        {
            mOwner = owner;
        }

        public void Dispose()
        {
            //Disposed
        }
    }

    class GeometryEntity : DesignScriptEntity, IGeometryEntity
    {
        public virtual IGeometryEntity Clone()
        {
            return this;
        }

        public virtual IGeometryEntity CopyAndTransform(ICoordinateSystemEntity fromCS, ICoordinateSystemEntity toCS)
        {
            return this;
        }

        public virtual IGeometryEntity CopyAndTranslate(IVectorEntity offset)
        {
            return this;
        }

        public virtual IDisplayable Display
        {
            get { return new DisplayEntity(); }
        }

        public virtual double DistanceTo(IPointEntity point)
        {
            return 10;
        }

        public virtual IPointEntity GetClosestPoint(IPointEntity otherPoint)
        {
            throw new System.NotImplementedException("ClosestPointTo");
        }

        public IGeometryEntity CopyAndTransform(ICoordinateSystemEntity toCS)
        {
            throw new NotImplementedException();
        }

        public double DistanceTo(IGeometryEntity entity)
        {
            throw new NotImplementedException();
        }

        public IPointEntity GetClosestPoint(IGeometryEntity entity)
        {
            throw new NotImplementedException();
        }

        public bool DoesIntersect(IGeometryEntity entity)
        {
            throw new NotImplementedException();
        }

        public bool IsWithin(IPointEntity point)
        {
            throw new NotImplementedException();
        }

        public ICoordinateSystemEntity CoordinateSystem
        {
            get { throw new NotImplementedException(); }
        }

        public IGeometryEntity[] Intersect(IGeometryEntity entity)
        {
            throw new NotImplementedException();
        }

        public IGeometryEntity[] Intersect(IGeometryEntity[] entity)
        {
            throw new NotImplementedException();
        }

        public IGeometryEntity[] Split(IGeometryEntity tool)
        {
            throw new NotImplementedException();
        }

        public IGeometryEntity[] Split(IGeometryEntity[] tools)
        {
            throw new NotImplementedException();
        }

        public IGeometryEntity[] Trim(IGeometryEntity tool, IPointEntity pick)
        {
            throw new NotImplementedException();
        }

        public IGeometryEntity[] Trim(IGeometryEntity[] tools, IPointEntity pick)
        {
            throw new NotImplementedException();
        }

        public IGeometryEntity[] Explode()
        {
            throw new NotImplementedException();
        }

        public IColor Color
        {
            get { throw new NotImplementedException(); }
        }

        public IBoundingBoxEntity BoundingBox
        {
            get { throw new NotImplementedException(); }
        }

        public IGeometryEntity[] Project(IGeometryEntity iGeometryEntity, IVectorEntity iVectorEntity)
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
}
