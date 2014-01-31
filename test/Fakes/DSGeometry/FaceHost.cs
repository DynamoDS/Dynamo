using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class FaceEntity : TopologyEntity, IFaceEntity
    {
        public double GetArea()
        {
            return 50;
        }

        public int GetCellCount()
        {
            return 4;
        }

        public int GetCellFaceCount()
        {
            return 6;
        }

        public ICellFaceEntity[] GetCellFaces()
        {
            return new ICellFaceEntity[4] { new CellFaceEntity(), new CellFaceEntity(), new CellFaceEntity(), new CellFaceEntity() };
        }

        public ICellEntity[] GetCells()
        {
            return new ICellEntity[4] { new CellEntity(), new CellEntity(), new CellEntity(), new CellEntity() };
        }

        public IPointEntity GetCentroid()
        {
            return new PointEntity();
        }

        public int GetEdgeCount()
        {
            return 4;
        }

        public IEdgeEntity[] GetEdges()
        {
            return new IEdgeEntity[4] { new EdgeEntity(), new EdgeEntity(), new EdgeEntity(), new EdgeEntity() };
        }

        public string GetFaceType()
        {
            return "Plane";
        }

        public IShellEntity GetShell()
        {
            return new ShellEntity();
        }

        public ISurfaceEntity GetSurfaceGeometry()
        {
            return new SurfaceEntity();
        }

        public int GetVertexCount()
        {
            return 10;
        }

        public IVertexEntity[] GetVertices()
        {
            return new IVertexEntity[3] { new VertexEntity(), new VertexEntity(), new VertexEntity() };
        }


        public bool IsPlanar()
        {
            return false;
        }
    }
}
