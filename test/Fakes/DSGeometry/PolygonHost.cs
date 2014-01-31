using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
namespace DSGeometry
{
    class PolygonEntity : GeometryEntity, IPolygonEntity
    {
        internal PolygonEntity()
        {
            Positions = new IPointEntity[2] { new PointEntity(), new PointEntity(1, 1, 1) };
        }

        public double GetOutOfPlane()
        {
            return 0;
        }

        public IPlaneEntity GetPlane()
        {
            return new PlaneEntity();
        }

        public IPointEntity[] GetVertices()
        {
            return new IPointEntity[2] { new PointEntity(), new PointEntity() };
        }

        public IPolygonEntity Trim(IPlaneEntity[] halfSpaces)
        {
            return this;
        }

        public void UpdateVertices(IPointEntity[] positions)
        {
            this.Positions = positions;
        }

        public IPointEntity[] Positions { get; protected set; }

        public IPointEntity[] GetPoints()
        {
            throw new NotImplementedException();
        }

        public IPlaneEntity Plane
        {
            get { throw new NotImplementedException(); }
        }

        public double PlaneDeviation
        {
            get { throw new NotImplementedException(); }
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

        public ISolidEntity Thicken(double thickness, bool bothSides)
        {
            throw new NotImplementedException();
        }

        public ISurfaceEntity Offset(double distance)
        {
            throw new NotImplementedException();
        }

        public ICoordinateSystemEntity CurvatureAtParameter(double u, double v)
        {
            throw new NotImplementedException();
        }

        public ICoordinateSystemEntity CoordinateSystemAtParameter(double u, double v)
        {
            throw new NotImplementedException();
        }

        public IVectorEntity TangentAtUParameter(double u, double v)
        {
            throw new NotImplementedException();
        }

        public IVectorEntity TangentAtVParameter(double u, double v)
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

        public IPointEntity PointAtParameter(double u, double v)
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
