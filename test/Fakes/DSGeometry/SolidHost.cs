using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
namespace DSGeometry
{
    class SolidEntity : GeometryEntity, ISolidEntity
    {
        public virtual int GetCellCount()
        {
            return 1;
        }

        public virtual ICellEntity[] GetCells()
        {
            return new ICellEntity[6] { new CellEntity(), new CellEntity(), new CellEntity(), new CellEntity(), new CellEntity(), new CellEntity() };
        }

        public virtual IPointEntity GetCentroid()
        {
            return new PointEntity();
        }

        public virtual int GetShellCount()
        {
            return 1;
        }

        public IShellEntity[] GetShells()
        {
            return new IShellEntity[3] { new ShellEntity(), new ShellEntity(), new ShellEntity() };
        }

        public IGeometryEntity[] IntersectWith(ISurfaceEntity geometry)
        {
            return new IGeometryEntity[2]{new GeometryEntity(),new GeometryEntity()};
        }

        public IGeometryEntity[] IntersectWith(IPlaneEntity geometry)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public IGeometryEntity[] IntersectWith(ISolidEntity geometry)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public virtual bool IsNonManifold()
        {
            return false;
        }

        public ISolidEntity NonRegularImpose(ISolidEntity geometry)
        {
            return new SolidEntity();
        }

        public ISolidEntity NonRegularSliceWithPlane(IPlaneEntity plane)
        {
            return new SolidEntity();
        }

        public ISolidEntity NonRegularSliceWithPlanes(IPlaneEntity[] planes)
        {
            return new SolidEntity();
        }

        public ISolidEntity NonRegularSliceWithSurface(ISurfaceEntity surface)
        {
            return new SolidEntity();
        }

        public ISolidEntity NonRegularSliceWithSurfaces(ISurfaceEntity[] surfaces)
        {
            return new SolidEntity();
        }

        public IGeometryEntity[] NonRegularUnionWith(IGeometryEntity geometry)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public ISolidEntity NonRegularUnionWithMany(IGeometryEntity[] solids)
        {
            return new SolidEntity();
        }

        public IGeometryEntity[] Project(IGeometryEntity geometry, IVectorEntity direction)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public ISolidEntity Regularise()
        {
            return new SolidEntity();
        }

        public IGeometryEntity[] SeparateSolid()
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public IGeometryEntity[] SliceWithPlane(IPlaneEntity plane)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public IGeometryEntity[] SliceWithPlanes(IPlaneEntity[] planeHosts)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public IGeometryEntity[] SliceWithSurface(ISurfaceEntity surface)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public IGeometryEntity[] SliceWithSurfaces(ISurfaceEntity[] surfaces)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public IGeometryEntity[] SubtractFrom(IGeometryEntity geometry)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public ISolidEntity ThinShell(double internalFaceThickness, double externalFaceThickness)
        {
            return new SolidEntity();
        }

        public ISolidEntity Trim(IPlaneEntity[] planes, ISurfaceEntity[] surfaces, ISolidEntity[] solids, IPointEntity point)
        {
            return new SolidEntity();
        }

        public IGeometryEntity[] UnionWith(IGeometryEntity geometry)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public bool UpdateSolidByLoftFromCrossSectionsGuides(ICurveEntity[] crossSections, ICurveEntity[] guides)
        {
            return false;
        }

        public bool UpdateSolidByLoftedCrossSections(ICurveEntity[] crossSections)
        {
            return false;
        }

        public bool UpdateSolidByLoftedCrossSectionsPath(ICurveEntity[] crossSections, ICurveEntity path)
        {
            return false;
        }

        public bool UpdateSolidByRevolve(ICurveEntity profileCurve, IPointEntity originPoint, IVectorEntity revolveAxis, double startAngle, double sweepAngle)
        {
            return false;
        }

        public bool UpdateSolidBySweep(ICurveEntity profile, ICurveEntity path)
        {
            return false;
        }

        public virtual int GetEdgeCount()
        {
            return 4;
        }

        public IEdgeEntity[] GetEdges()
        {
            return new IEdgeEntity[4] { new EdgeEntity(), new EdgeEntity(), new EdgeEntity(), new EdgeEntity() };
        }

        public virtual int GetFaceCount()
        {
            return 6;
        }

        public IFaceEntity[] GetFaces()
        {
            return new IFaceEntity[2] { new FaceEntity(), new FaceEntity() };
        }

        public virtual int GetVertexCount()
        {
            return 12;
        }

        public IVertexEntity[] GetVertices()
        {
            return new IVertexEntity[4] { new VertexEntity(), new VertexEntity(), new VertexEntity(), new VertexEntity() };
        }

        public virtual double Area
        {
            get { return 100; }
        }

        public virtual double Volume
        {
            get { return 400; }
        }

        public IPointEntity GetCenterOfGravity()
        {
            throw new NotImplementedException();
        }

        public ISolidEntity CSGUnion(ISolidEntity geometry)
        {
            throw new NotImplementedException();
        }

        public ISolidEntity CSGDifference(ISolidEntity geometry)
        {
            throw new NotImplementedException();
        }

        public ISolidEntity CSGIntersect(ISolidEntity geometry)
        {
            throw new NotImplementedException();
        }

        ISolidEntity[] ISolidEntity.ThinShell(double internalFaceThickness, double externalFaceThickness)
        {
            throw new NotImplementedException();
        }

        public IGeometryEntity[] Project(IPointEntity PointEntity, IVectorEntity dir)
        {
            throw new NotImplementedException();
        }
    }
}
