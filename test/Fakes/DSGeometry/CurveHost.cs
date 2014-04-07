using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    abstract class CurveEntity : GeometryEntity, ICurveEntity
    {
        public virtual double DistanceAtParameter(double param)
        {
            return param * 2;
        }

        public virtual double ParameterAtDistance(double distance)
        {
            return distance * 0.5;
        }

        public virtual double EndParameter()
        {
            return 1;
        }

        public IPointEntity EndPoint
        {
            get;
            protected set;
        }

        public virtual ISurfaceEntity Extrude(IVectorEntity direction, double distance)
        {
            return new SurfaceEntity();
        }

        public virtual IPointEntity GetClosestPointTo(ICurveEntity iCurveEntity)
        {
            return this.StartPoint;
        }

        public virtual IPointEntity GetClosestPointTo(IPointEntity point, IVectorEntity direction, bool extend)
        {
            return this.StartPoint;
        }

        public virtual IPointEntity GetClosestPointTo(IPointEntity point, bool extend)
        {
            return this.StartPoint;
        }

        public virtual double GetLength()
        {
            return Length;
        }

        public virtual ICurveEntity GetOffsetCurve(double distance)
        {
            return new LineEntity();
        }

        public virtual IPointEntity[] IntersectWith(ICurveEntity otherCurve)
        {
            return new IPointEntity[2] { new PointEntity(), new PointEntity() };
        }

        public virtual IGeometryEntity[] IntersectWith(IPlaneEntity plane)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public virtual IGeometryEntity[] IntersectWith(ISurfaceEntity surface)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public virtual IGeometryEntity[] IntersectWith(ISolidEntity solid)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public abstract bool IsClosed
        {
            get;
        }

        public abstract bool IsPlanar
        {
            get; 
        }

        public virtual ICurveEntity[] JoinWith(ICurveEntity[] curves, double bridgeTolerance)
        {
            return new ICurveEntity[3] { new LineEntity(), new LineEntity(), new LineEntity() };
        }

        public virtual INurbsCurveEntity JoinWith(ICurveEntity[] curves)
        {
            return new NurbsCurveEntity();
        }

        public virtual IVectorEntity NormalAtParameter(double param)
        {
            return DsVector.ByCoordinates(param, param, param);
        }

        public virtual double ParameterAtPoint(IPointEntity point)
        {
            return 1;
        }

        public virtual IPointEntity PointAtDistance(double distance)
        {
            return new PointEntity();
        }

        public virtual IPointEntity PointAtParameter(double param)
        {
            return new PointEntity();
        }

        public virtual IPointEntity Project(IPointEntity point, IVectorEntity direction)
        {
            return new PointEntity();
        }

        public virtual IGeometryEntity[] ProjectOn(ISurfaceEntity surface, IVectorEntity direction)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public virtual ICurveEntity ProjectOn(IPlaneEntity plane, IVectorEntity direction)
        {
            return new LineEntity();
        }

        public virtual ICurveEntity Reverse()
        {
            return new LineEntity();
        }

        public virtual ICurveEntity[] Split(double[] parameters)
        {
            return new ICurveEntity[3] { new LineEntity(), new LineEntity(), new LineEntity() };
        }

        public virtual double StartParameter()
        {
            return 0;
        }

        public virtual IPointEntity StartPoint
        {
            get;
            protected set;
        }

        public virtual IVectorEntity TangentAtParameter(double param)
        {
            if(null != StartPoint && null != EndPoint)
                return DsVector.ByCoordinates(EndPoint.X - StartPoint.X, EndPoint.Y - StartPoint.Y, EndPoint.Z - StartPoint.Z);

            return DsVector.ByCoordinates(param, param, param);
        }

        public virtual ICurveEntity[] Trim(double startParameter, double endParameter, bool discardBetweenParams)
        {
            return new ICurveEntity[3] { new LineEntity(), new LineEntity(), new LineEntity() };
        }

        public virtual ICurveEntity[] Trim(double[] parameters, bool discardEvenSegments)
        {
            return new ICurveEntity[3] { new LineEntity(), new LineEntity(), new LineEntity() };
        }

        public ICoordinateSystemEntity CoordinateSystemAtParameter(double param) { throw new NotImplementedException(); }

        public ICoordinateSystemEntity HorizontalFrameAtParameter(double param) { throw new NotImplementedException(); }

        public ICoordinateSystemEntity GetNoTwistFrameAtParameter(double param) { throw new NotImplementedException(); }

        public double Length
        {
            get { return 1; }
        }

        public double GetLength(double startParam, double endParam) { throw new NotImplementedException(); }

        public IVectorEntity Normal
        {
            get { throw new NotImplementedException(); }
        }



        public ICurveEntity Offset(double distance)
        {
            throw new NotImplementedException();
        }

        public bool IsAlmostEqualTo(IPointEntity other)
        {
            throw new NotImplementedException();
        }

        public ICurveEntity ParameterTrimStart(double startParameter)
        {
            throw new NotImplementedException();
        }

        public ICurveEntity ParameterTrimEnd(double endParameter)
        {
            throw new NotImplementedException();
        }

        public ICurveEntity ParameterTrim(double startParameter, double endParameter)
        {
            throw new NotImplementedException();
        }

        public ICurveEntity[] ParameterTrimInterior(double startParameter, double endParameter)
        {
            throw new NotImplementedException();
        }

        public ICurveEntity[] ParameterTrimSegments(double[] parameters)
        {
            throw new NotImplementedException();
        }

        public ICurveEntity[] ParameterTrimSegments(double[] parameters, bool discardEvenSegments)
        {
            throw new NotImplementedException();
        }

        public ICurveEntity[] ParameterSplit(double parameter)
        {
            throw new NotImplementedException();
        }

        public ICurveEntity[] ParameterSplit(double[] parameters)
        {
            throw new NotImplementedException();
        }

        public IPolyCurveEntity Join(ICurveEntity curve)
        {
            throw new NotImplementedException();
        }

        public INurbsCurveEntity Merge(ICurveEntity curve)
        {
            throw new NotImplementedException();
        }

        public ISurfaceEntity Extrude(double distance)
        {
            throw new NotImplementedException();
        }

        public ISurfaceEntity Extrude(IVectorEntity direction)
        {
            throw new NotImplementedException();
        }

        public ISolidEntity ExtrudeAsSolid(double distance)
        {
            throw new NotImplementedException();
        }

        public ISolidEntity ExtrudeAsSolid(IVectorEntity direction)
        {
            throw new NotImplementedException();
        }

        public ISolidEntity ExtrudeAsSolid(IVectorEntity direction, double distance)
        {
            throw new NotImplementedException();
        }

        public ICurveEntity Extend(double distance, IPointEntity pickSide)
        {
            throw new NotImplementedException();
        }

        public ICurveEntity ExtendStart(double distance)
        {
            throw new NotImplementedException();
        }

        public ICurveEntity ExtendEnd(double distance)
        {
            throw new NotImplementedException();
        }

        public ICurveEntity[] ApproximateWithArcAndLineSegments()
        {
            throw new NotImplementedException();
        }

        public INurbsCurveEntity ToNurbsCurve()
        {
            throw new NotImplementedException();
        }

        public ICurveEntity PullOntoPlane(IPlaneEntity plane)
        {
            throw new NotImplementedException();
        }

        public ISurfaceEntity Patch()
        {
            throw new NotImplementedException();
        }
    }
}
