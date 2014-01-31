using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
namespace DSGeometry
{
    class SurfaceEntity : GeometryEntity, ISurfaceEntity
    {
        public INurbsSurfaceEntity ApproxBSpline(double tol)
        {
            return new NurbsSurfaceEntity();
        }

        public INurbsSurfaceEntity[] ConvertToBSplineSurface()
        {
            return new INurbsSurfaceEntity[3] { new NurbsSurfaceEntity(), new NurbsSurfaceEntity(), new NurbsSurfaceEntity() };
        }

        public virtual double GetArea()
        {
            return 10;
        }

        public INurbsSurfaceEntity GetBsplineSurface()
        {
            return new NurbsSurfaceEntity();
        }

        public override IPointEntity GetClosestPoint(IPointEntity point)
        {
            return point;
        }

        public virtual bool GetIsClosed()
        {
            return false;
        }

        public virtual bool GetIsClosedInU()
        {
            return false;
        }

        public virtual bool GetIsClosedInV()
        {
            return false;
        }

        public IVectorEntity GetNormalAtPoint(IPointEntity point)
        {
            return DsVector.ByCoordinates(point.X, point.Y, point.Z);
        }

        public ISurfaceEntity GetOffsetSurface(double distance)
        {
            return this;
        }

        public double GetPerimeter()
        {
            return 50;
        }

        public Tuple<double, double> GetUVParameterAtPoint(IPointEntity point)
        {
            return new Tuple<double, double>(point.X,point.Y);
        }

        public IGeometryEntity[] IntersectWith(ISurfaceEntity geometry)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public IGeometryEntity[] IntersectWith(IPlaneEntity plane)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public IVectorEntity NormalAtPoint(IPointEntity point)
        {
            return DsVector.ByCoordinates(point.X, point.Y, point.Z);
        }

        public ISurfaceEntity Offset(double distance)
        {
            return new SurfaceEntity();
        }

        public IPointEntity PointAtParameter(double u, double v)
        {
            return new PointEntity();
        }

        public IGeometryEntity[] Project(IGeometryEntity geometry, IVectorEntity direction)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public ISurfaceEntity[] Split(ISurfaceEntity surface)
        {
            return new ISurfaceEntity[4] { new SurfaceEntity(), new SurfaceEntity(), new SurfaceEntity(), new SurfaceEntity() };
        }

        public void Split(IPlaneEntity plane, ref ISurfaceEntity[] negativeHalf, ref ISurfaceEntity[] positiveHalf)
        {
            //split complete
        }

        public ISurfaceEntity[] Split(IPlaneEntity plane)
        {
            return new ISurfaceEntity[4] { new SurfaceEntity(), new SurfaceEntity(), new SurfaceEntity(), new SurfaceEntity() };
        }

        public IGeometryEntity[] SubtractFrom(IGeometryEntity geometry)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public IVectorEntity TangentAtUParameter(double u, double v)
        {
            return DsVector.ByCoordinates(0, u, v);
        }

        public IVectorEntity TangentAtVParameter(double u, double v)
        {
            return DsVector.ByCoordinates(0, u, v);
        }

        public ISolidEntity Thicken(double thickness, bool bothSides)
        {
            return new SolidEntity();
        }

        public ISurfaceEntity Trim(ICurveEntity[] curves, IPlaneEntity[] planes, ISurfaceEntity[] surfaces, ISolidEntity[] solids, IPointEntity point, bool bAutoExtend)
        {
            return this;
        }

        public bool UpdateRevolve(ICurveEntity profile, IPointEntity origin, IVectorEntity revolveAxis, double startAngle, double sweepAngle)
        {
            return false;
        }

        public bool UpdateSurfaceByLoftFromCrossSectionsGuides(ICurveEntity[] crossSections, ICurveEntity[] guides)
        {
            return false;
        }

        public bool UpdateSurfaceByLoftedCrossSections(ICurveEntity[] crossSections)
        {
            return false;
        }

        public bool UpdateSurfaceByLoftedCrossSectionsPath(ICurveEntity[] crossSections, ICurveEntity path)
        {
            return false;
        }

        public bool UpdateSweep(ICurveEntity profile, ICurveEntity path)
        {
            return false;
        }

        public ICoordinateSystemEntity CurvatureAtParameter(double u, double v)
        {
            return null;
        }

        public IGeometryEntity[] Project(IPointEntity PointEntity, IVectorEntity dir)
        {
            throw new NotImplementedException();
        }

        public IGeometryEntity[] Difference(ISolidEntity iSolidEntity)
        {
            throw new NotImplementedException();
        }

        public IGeometryEntity[] SubtractFrom(ISolidEntity trimmingEntity)
        {
            throw new NotImplementedException();
        }

        public IUVEntity UVParameterAtPoint(IPointEntity point)
        {
            throw new NotImplementedException();
        }

        public INurbsSurfaceEntity ToNurbsSurface()
        {
            throw new NotImplementedException();
        }

        public INurbsSurfaceEntity ApproximateWithTolerance(double tolerance)
        {
            throw new NotImplementedException();
        }

        public ISolidEntity Thicken(double thickness)
        {
            throw new NotImplementedException();
        }

        public ICoordinateSystemEntity CoordinateSystemAtParameter(double u, double v)
        {
            throw new NotImplementedException();
        }

        public IVectorEntity NormalAtParameter(double u, double v)
        {
            throw new NotImplementedException();
        }

        public IVectorEntity[] DerivativesAtParameter(double u, double v)
        {
            throw new NotImplementedException();
        }

        public double GaussianCurvatureAtParameter(double u, double v)
        {
            throw new NotImplementedException();
        }

        public double[] PrincipalCurvaturesAtParameter(double u, double v)
        {
            throw new NotImplementedException();
        }

        public IVectorEntity[] PrincipalDirectionsAtParameter(double u, double v)
        {
            throw new NotImplementedException();
        }

        public double Area
        {
            get { throw new NotImplementedException(); }
        }

        public double Perimeter
        {
            get { throw new NotImplementedException(); }
        }

        public bool ClosedInU
        {
            get { throw new NotImplementedException(); }
        }

        public bool ClosedInV
        {
            get { throw new NotImplementedException(); }
        }

        public bool Closed
        {
            get { throw new NotImplementedException(); }
        }

        public ICurveEntity[] PerimeterCurves()
        {
            throw new NotImplementedException();
        }

        public ICurveEntity[] GetIsolines(int isoDirection, double parameter)
        {
            throw new NotImplementedException();
        }

        public ISurfaceEntity FlipNormalDirection()
        {
            throw new NotImplementedException();
        }
    }
}
