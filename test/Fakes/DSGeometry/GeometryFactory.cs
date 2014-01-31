using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
namespace DSGeometry
{
    public class GeometryFactory : IGeometryFactory
    {
        public IArcEntity ArcByThreePoints(IPointEntity firstPoint, IPointEntity secondPoint, IPointEntity thirdPoint)
        {
            DSGeometryApplication.Check();
            return new ArcEntity();
        }

        public IArcEntity ArcByCenterPointRadiusAngle(IPointEntity center, double radius, double startAngle, double endAngle, IVectorEntity normal)
        {
            DSGeometryApplication.Check();
            double sweepAngle = endAngle - startAngle;
            return new ArcEntity(center, normal, radius, startAngle, sweepAngle);
        }

        public IArcEntity ArcByCenterPointStartPointSweepAngle(IPointEntity centerPoint, IPointEntity startPoint, double sweepAngle, IVectorEntity normal)
        {
            DSGeometryApplication.Check();
            double radius = startPoint.DistanceTo(centerPoint);
            double startAngle = 30;
            return new ArcEntity(centerPoint, normal, radius, startAngle, sweepAngle);
        }

        public IArcEntity ArcByCenterPointStartPointSweepPoint(IPointEntity centerPoint, IPointEntity startPoint, IPointEntity sweepPoint)
        {
            DSGeometryApplication.Check();
            Vector start_center = Vector.ByCoordinates(startPoint.X - centerPoint.X, startPoint.Y - centerPoint.Y, startPoint.Z - centerPoint.Z);
            Vector sweep_center = Vector.ByCoordinates(sweepPoint.X - centerPoint.X, sweepPoint.Y - centerPoint.Y, sweepPoint.Z - centerPoint.Z);
            Vector normal = start_center.Cross(sweep_center);
            double radius = start_center.GetLength();
            Vector Axis = Vector.ByCoordinates(1, 0, 0);
            double startAngle = Math.Acos(start_center.Dot(Axis) / (start_center.GetLength() * Axis.GetLength()));
            double sweepAngle = Math.Acos(start_center.Dot(sweep_center) / (start_center.GetLength() * sweep_center.GetLength()));
            return new ArcEntity(centerPoint, normal.ToIVector(), radius, startAngle, sweepAngle);
        }

        public IArcEntity ArcByCenterPointStartPointEndPoint(IPointEntity centerPoint, IPointEntity startPoint, IPointEntity endPoint) { throw new NotImplementedException(); }

        public IBoundingBoxEntity BoundingBoxByGeometry(IGeometryEntity geom) { throw new NotImplementedException(); }

        public IBoundingBoxEntity BoundingBoxByGeometry(IGeometryEntity[] geom) { throw new NotImplementedException(); }

        public IBoundingBoxEntity BoundingBoxByGeometryCoordinateSystem(IGeometryEntity geom, ICoordinateSystemEntity cs) { throw new NotImplementedException(); }

        public IBoundingBoxEntity BoundingBoxByGeometryCoordinateSystem(IGeometryEntity[] geom, ICoordinateSystemEntity cs) { throw new NotImplementedException(); }

        public ICircleEntity CircleByCenterPointRadius(IPointEntity center, double radius) { throw new NotImplementedException(); }

        public ICircleEntity CircleByCenterPointRadiusNormal(IPointEntity center, double radius, IVectorEntity normal)
        {
            DSGeometryApplication.Check();
            return new CircleEntity(center, radius, normal);
        }

        public ICircleEntity CircleByPlaneRadius(IPlaneEntity plane, double radius) { throw new NotImplementedException(); }

        public ICircleEntity CircleByThreePoints(IPointEntity p1, IPointEntity p2, IPointEntity p3)
        {
            DSGeometryApplication.Check();
            return new CircleEntity();
        }

        public IConeEntity ConeByPointsRadius(IPointEntity startPoint, IPointEntity endPoint, double startRadius) { throw new NotImplementedException(); }

        public IConeEntity ConeByPointsRadii(IPointEntity startPoint, IPointEntity endPoint, double startRadius, double endRadius)
        {
            DSGeometryApplication.Check();
            ConeEntity cone = new ConeEntity();
            cone.UpdateCone(startPoint, endPoint, startRadius, endRadius);
            return cone;
        }

        public IConeEntity ConeByCoordinateSystemHeightRadius(ICoordinateSystemEntity cs, double height, double startRadius)
        {
            throw new NotImplementedException();
        }

        public IConeEntity ConeByCoordinateSystemHeightRadii(ICoordinateSystemEntity cs, double height, double startRadius, double endRadius)
        {
            throw new NotImplementedException();
        }
        
        public ICoordinateSystemEntity CoordinateSystemByMatrix(double[] matrix) { throw new NotImplementedException(); }

        public ICoordinateSystemEntity CoordinateSystemByOrigin(double x, double y) { throw new NotImplementedException(); }

        public ICoordinateSystemEntity CoordinateSystemByOrigin(double x, double y, double z) { throw new NotImplementedException(); }

        public ICoordinateSystemEntity CoordinateSystemByOrigin(IPointEntity origin) { throw new NotImplementedException(); }

        public ICoordinateSystemEntity CoordinateSystemByPlane(IPlaneEntity plane) { throw new NotImplementedException(); }

        public ICoordinateSystemEntity CoordinateSystemByOriginVectors(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis) { throw new NotImplementedException(); }

        public ICoordinateSystemEntity CoordinateSystemByOriginVectors(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis, IVectorEntity zAxis) { throw new NotImplementedException(); }

        public ICoordinateSystemEntity CoordinateSystemByCylindricalCoordinates(ICoordinateSystemEntity contextCS, double radius, double theta, double height)
        {
            DSGeometryApplication.Check();
            CoordinateEntity cs = new CoordinateEntity();
            cs.Set(new PointEntity() { X = radius * Math.Cos(DegreeToRadian(theta)), Y = radius * Math.Sin(DegreeToRadian(theta)), Z = height }, contextCS.XAxis, contextCS.YAxis, contextCS.ZAxis);
            return cs;
           // return new CoordinateEntity() { Origin = new PointEntity() { X = radius*Math.Sin(theta), Y = radius*Math.Cos(theta), Z = height } };
           // return new CoordinateEntity();
        }

        public ICoordinateSystemEntity CoordinateSystemBySphericalCoordinates(ICoordinateSystemEntity contextCS, double radius, double theta, double phi)
        {
            DSGeometryApplication.Check();
            CoordinateEntity cs = new CoordinateEntity();
            cs.Set(new PointEntity() { X = radius * Math.Sin(DegreeToRadian(theta)) * Math.Cos(DegreeToRadian(phi)), Y = radius * Math.Sin(DegreeToRadian(theta)) * Math.Sin(DegreeToRadian(phi)), Z = radius * Math.Cos(DegreeToRadian(theta)) }, contextCS.XAxis, contextCS.YAxis, contextCS.ZAxis);
            return cs;
        }

        public ICuboidEntity CuboidByLengths(double width, double length, double height) { throw new NotImplementedException(); }

        public ICuboidEntity CuboidByLengths(IPointEntity originPoint, double width, double length, double height) { throw new NotImplementedException(); }

        public ICuboidEntity CuboidByLengths(ICoordinateSystemEntity cs, double length, double width, double height)
        {
            DSGeometryApplication.Check();
            ICuboidEntity cub = new CuboidEntity(new double[] { cs.Origin.X, cs.Origin.Y, cs.Origin.Z }, length, width, height);
            return cub;
        }

        public ICuboidEntity CuboidByCorners(IPointEntity lowPoint, IPointEntity highPoint) { throw new NotImplementedException(); }

        public ICurveEntity CurveByParameterLineOnSurface(ISurfaceEntity baseSurface, IUVEntity startParams, IUVEntity endParams) { throw new NotImplementedException(); }

        public IEllipseEntity EllipseByOriginRadii(IPointEntity origin, double xAxisRadius, double yAxisRadius) { throw new NotImplementedException(); }

        public IEllipseEntity EllipseByOriginVectors(IPointEntity origin, IVectorEntity xAxisRadius, IVectorEntity yAxisRadius) { throw new NotImplementedException(); }

        public IEllipseEntity EllipseByCoordinateSystemRadii(ICoordinateSystemEntity origin, double xAxisRadius, double yAxisRadius) { throw new NotImplementedException(); }

        public IEllipseEntity EllipseByPlaneRadii(IPlaneEntity plane, double xAxisRadius, double yAxisRadius) { throw new NotImplementedException(); }

        public IHelixEntity HelixByAxis(IPointEntity axisPoint, IVectorEntity axisDirection, IPointEntity startPoint, double pitch, double angleTurns) { throw new NotImplementedException(); }

        public IIndexGroupEntity IndexGroupByIndices(uint a, uint b, uint c, uint d) { throw new NotImplementedException(); }

        public IIndexGroupEntity IndexGroupByIndices(uint a, uint b, uint c) { throw new NotImplementedException(); }

        public ILineEntity LineByStartPointEndPoint(IPointEntity startPoint, IPointEntity endPoint)
        {
            DSGeometryApplication.Check();
            LineEntity lnhost = new LineEntity();
            lnhost.UpdateEndPoints(startPoint, endPoint);
            return lnhost;
        }

        public ILineEntity LineByBestFit(IPointEntity[] bestFitPoints) { throw new NotImplementedException(); }

        public ILineEntity LineByTangency(ICurveEntity curve, double parameter) { throw new NotImplementedException(); }

        public INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points) { throw new NotImplementedException(); }

        public INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points, int degree) { throw new NotImplementedException(); }

        public INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points, int degree, bool close_curve)
        {
            DSGeometryApplication.Check();
            return new NurbsCurveEntity(points, degree, close_curve);
        }

        public INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points, int degree, double[] weights, double[] knots) { throw new NotImplementedException(); }

        public INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] points) { throw new NotImplementedException(); }

        public INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] hosts, bool makePeriodic)
        {
            DSGeometryApplication.Check();
            return new NurbsCurveEntity();
        }

        public INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] points, int degree) { throw new NotImplementedException(); }

        public INurbsCurveEntity NurbsCurveByPointsTangents(IPointEntity[] pts, IVectorEntity startTangent, IVectorEntity endTangent)
        {
            DSGeometryApplication.Check();
            return new NurbsCurveEntity();
        }

        public INurbsSurfaceEntity NurbsSurfaceByPoints(IPointEntity[][] points, int uDegree, int vDegree)
        {
            DSGeometryApplication.Check();
            INurbsSurfaceEntity surface = new NurbsSurfaceEntity(points, uDegree, vDegree, 0);
            return surface;
        }

        public INurbsSurfaceEntity NurbsSurfaceByControlVertices(IPointEntity[][] controlVertices, int uDegree, int vDegree)
        {
            DSGeometryApplication.Check();
            INurbsSurfaceEntity surface = new NurbsSurfaceEntity(controlVertices, uDegree, vDegree, 1);
            return surface;
        }

        public INurbsSurfaceEntity NurbsSurfaceByControlVertices(IPointEntity[][] controlVertices, int uDegree, int vDegree, double[][] weights, double[][] knots) { throw new NotImplementedException(); }

        public IPlaneEntity PlaneByOriginNormal(IPointEntity origin, IVectorEntity normal)
        {
            DSGeometryApplication.Check();
            IPlaneEntity plane = new PlaneEntity(origin, normal);
            return plane;
        }

        public IPlaneEntity PlaneByOriginNormalXAxis(IPointEntity origin, IVectorEntity normal, IVectorEntity xAxis) { throw new NotImplementedException(); }

        public IPlaneEntity PlaneByOriginXAxisYAxis(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis) { throw new NotImplementedException(); }

        public IPlaneEntity PlaneByBestFitThroughPoints(IPointEntity[] points) { throw new NotImplementedException(); }

        public IPlaneEntity PlaneByLineAndPoint(ILineEntity line, IPointEntity point) { throw new NotImplementedException(); }

        public IPlaneEntity PlaneByThreePoints(IPointEntity p1, IPointEntity p2, IPointEntity p3) { throw new NotImplementedException(); }

        public IPointEntity PointByCoordinates(double x, double y) { throw new NotImplementedException(); }

        public IPointEntity PointByCoordinates(double x, double y, double z)
        {
            DSGeometryApplication.Check();
            return new PointEntity(x, y, z);
        }

        public IPointEntity PointByCartesianCoordinates(ICoordinateSystemEntity cs, double x, double y, double z)
        {
            DSGeometryApplication.Check();
            return new PointEntity(cs.Origin.X + x, cs.Origin.Y + y, cs.Origin.Z + z);
        }

        public IPointEntity PointByCylindricalCoordinates(ICoordinateSystemEntity cs, double angle, double elevation, double offset) { throw new NotImplementedException(); }

        public IPointEntity PointBySphericalCoordinates(ICoordinateSystemEntity cs, double phi, double theta, double radius) { throw new NotImplementedException(); }
        
        public IPolygonEntity PolygonByPoints(IPointEntity[] points)
        {
            DSGeometryApplication.Check();
            return new PolygonEntity();
        }

        public IPolyCurveEntity PolyCurveByJoinedCurves(ICurveEntity[] curves)
        {
            throw new NotImplementedException();
        }

        public IRectangleEntity RectangleByCornerPoints(IPointEntity[] points) { throw new NotImplementedException(); }

        public IRectangleEntity RectangleByCornerPoints(IPointEntity p1, IPointEntity p2, IPointEntity p3, IPointEntity p4) { throw new NotImplementedException(); }

        public IRectangleEntity RectangleByWidthHeight(double width, double length) { throw new NotImplementedException(); }

        public IRectangleEntity RectangleByWidthHeight(IPlaneEntity plane, double width, double length) { throw new NotImplementedException(); }

        public IRectangleEntity RectangleByWidthHeight(ICoordinateSystemEntity cs, double width, double length) { throw new NotImplementedException(); }

        public ISurfaceEntity SurfaceByLoft(ICurveEntity[] crossSections)
        {
            DSGeometryApplication.Check();
            return new SurfaceEntity();
        }

        public ISurfaceEntity SurfaceByLoft(ICurveEntity[] crossSections, ICurveEntity guides)
        {
            DSGeometryApplication.Check();
            return new SurfaceEntity();
        }

        public ISurfaceEntity SurfaceByLoftGuides(ICurveEntity[] hostXCurves, ICurveEntity[] hostGuides)
        {
            throw new NotImplementedException();
        }

        public ISurfaceEntity SurfaceBySweep(ICurveEntity profile, ICurveEntity path)
        {
            DSGeometryApplication.Check();
            return new SurfaceEntity();
        }

        public ISurfaceEntity SurfaceBySweep2Rails(ICurveEntity rail1, ICurveEntity rail2, ICurveEntity profile) { throw new NotImplementedException(); }

        public ISurfaceEntity SurfaceByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVectorEntity axisDirection, double startAngle, double sweepAngle)
        {
            DSGeometryApplication.Check();
            return new SurfaceEntity();
        }

        public ISurfaceEntity SurfaceByPatch(ICurveEntity iCurveEntity) { throw new NotImplementedException(); }

        public ISolidEntity SolidByLoft(ICurveEntity[] crossSections)
        {
            DSGeometryApplication.Check();
            return new SolidEntity();
        }

        public ISolidEntity SolidByLoft(ICurveEntity[] crossSections, ICurveEntity path)
        {
            DSGeometryApplication.Check();
            return new SolidEntity();
        }

        public ISolidEntity SolidByLoftGuides(ICurveEntity[] crossSections, ICurveEntity[] guides)
        {
            DSGeometryApplication.Check();
            return new SolidEntity();
        }

        public ISolidEntity SolidBySweep(ICurveEntity profile, ICurveEntity path)
        {
            DSGeometryApplication.Check();
            return new SolidEntity();
        }

        public ISolidEntity SolidBySweep2Rails(ICurveEntity rail1, ICurveEntity rail2, ICurveEntity profile) { throw new NotImplementedException(); }

        public ISolidEntity SolidByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVectorEntity axisDirection, double startAngle, double sweepAngle)
        {
            DSGeometryApplication.Check();
            return new SolidEntity();
        }

        public ISphereEntity SphereByCenterPointRadius(IPointEntity centerPoint, double radius)
        {
            DSGeometryApplication.Check();
            ISphereEntity sph = new SphereEntity(centerPoint, radius);
            return sph;
        }

        public ITextEntity TextByPoint(IPointEntity origin, string textString, double textHeight) { throw new NotImplementedException(); }
        
        public ITextEntity TextByCoordinateSystem(ICoordinateSystemEntity cs, string textString, double textHeight) { throw new NotImplementedException(); }

        public IUVEntity UVByCoordinates(double u, double v) { throw new NotImplementedException(); }

        public IVectorEntity VectorByCoordinates(double x, double y, double z) { throw new NotImplementedException(); }

        public IVectorEntity VectorByCoordinates(double x, double y, double z, bool normalized) { throw new NotImplementedException(); }

        public IPolyMeshEntity PolyMeshByVerticesFaceIndices(IPointEntity[] vertices, IIndexGroupEntity[] indices) { throw new NotImplementedException(); }

        public IBlockHelper GetBlockHelper()
        {
            throw new NotImplementedException();
        }
        
        public IGeometryEntity[] LoadSat(string satFile)
        {
            return new IGeometryEntity[] {new SolidEntity(), new SolidEntity(), new SolidEntity()};
        }

        public bool SaveSat(string satFile, object[] ffiObjects)
        {
            return false;
        }

        public IGeometrySettings GetSettings()
        {
            return new Settings();
        }

        internal static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }

    class Settings : IGeometrySettings
    {
        public bool PointVisibility { get; set; }
    }
}
