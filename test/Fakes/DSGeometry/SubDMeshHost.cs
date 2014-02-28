using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using ProtoGeometry = Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class SubDMeshEntity : GeometryEntity, ISubDMeshEntity
    {
        public double Area
        {
            get { return 100; }
        }

        public double Volume
        {
            get { return 400; }
        }

        public double ComputeSurfaceArea()
        {
            return Area;
        }

        public double ComputeVolume()
        {
            return Volume;
        }

        public ISolidEntity ConvertToSolid(bool bConvertAsSmooth)
        {
            return new SolidEntity();
        }

        public ISurfaceEntity ConvertToSurface(bool bConvertAsSmooth)
        {
            return new SurfaceEntity();
        }

        public ILineEntity[] GetEdges()
        {
            return new ILineEntity[2] { new LineEntity(), new LineEntity() };
        }

        public int[][] GetFaceIndices()
        {
            return new int[][] { new int[] { 1, 2, 3 }, new int[] { 3, 2, 1 } };
        }

        public bool GetIsClosed()
        {
            return false;
        }

        public int NumVertices
        {
            get { return 8; }
        }

        public int NumFaces
        {
            get { return 4; }
        }

        public int NumResultVertices
        {
            get { return 6; }
        }

        public int NumResultFaces
        {
            get { return 2; }
        }

        public int[][] GetResultFaceIndices()
        {
            return new int[][] { new int[] { 3, 2, 1 }, new int[] { 1, 2, 3 } };
        }

        public IPointEntity[] GetResultVertices()
        {
            return new IPointEntity[3] { new PointEntity(), new PointEntity(1,1,1), new PointEntity(2,2,2) };
        }

        public IColor[] GetVertexColors()
        {
            return new IColor[2] { ProtoGeometry.Color.Black.ToIColor(), ProtoGeometry.Color.Blue.ToIColor() };
        }

        public IVectorEntity[] GetVertexNormals()
        {
            return new DsVector[2] { DsVector.ByCoordinates(0, 0, 1), DsVector.ByCoordinates(1, 0, 0) };
        }

        public IPointEntity[] GetVertices()
        {
            return new IPointEntity[3] { new PointEntity(), new PointEntity(0,7,6), new PointEntity(3,2,1) };
        }

        public bool UpdateByVerticesFaceIndices(IPointEntity[] vertices, int[][] faceIndices, int subDLevel)
        {
            return false;
        }

        public bool UpdateSubDMeshColors(IColor[] colors)
        {
            return false;
        }

        public bool UpdateSubDMeshColors(IPointEntity[] vertices, IVectorEntity[] normals, IColor[] colors, int[][] faceIndices, int subDLevel)
        {
            return false;
        }

        public bool UpdateSubDMeshNormals(IVectorEntity[] normals)
        {
            return false;
        }

        public IGeometryEntity Geometry
        {
            get { throw new NotImplementedException(); }
        }
    }
}
