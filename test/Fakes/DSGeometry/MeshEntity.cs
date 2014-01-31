using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using System.Collections;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    /*
    class MeshTopologyEntity : ITopologyEntity, IBRepEntity
    {
        private ISubDMeshEntity mesh;
        private IBRepEntity brep = null;
        private bool mbAutoDispose;

        public MeshTopologyEntity(ISubDMeshEntity mesh, bool bAutoDispose = true)
        {
            // TODO: Complete member initialization
            this.mesh = mesh;
            mbAutoDispose = bAutoDispose;
        }

        public object Owner
        {
            get;
            private set;
        }

        public void SetOwner(object owner)
        {
            this.Owner = owner;
        }

        public void Dispose()
        {
            if (mbAutoDispose && mesh != null)
                mesh.Dispose();
            mesh = null;
        }

        private IBRepEntity GetBRepEntity()
        {
            if (brep == null)
            {
                ISurfaceEntity geometry = mesh.ConvertToSurface(false);
                brep = geometry as IBRepEntity;
            }
            return brep;
        }

        public IFaceEntity[] GetFaces()
        {
            return GetBRepEntity().GetFaces();
        }

        public IEdgeEntity[] GetEdges()
        {
            return GetBRepEntity().GetEdges();
        }

        public IVertexEntity[] GetVertices()
        {
            return GetBRepEntity().GetVertices();
        }

        public int GetVertexCount()
        {
            return GetBRepEntity().GetVertexCount();
        }

        public int GetEdgeCount()
        {
            return GetBRepEntity().GetEdgeCount();
        }

        public int GetFaceCount()
        {
            return GetBRepEntity().GetFaceCount();
        }
    }

    class PointComparer : IEqualityComparer<IPointEntity>, IEqualityComparer
    {
        public bool Equals(IPointEntity x, IPointEntity y)
        {
            return GeometryExtension.EqualsTo(x.X, y.X) &&
                GeometryExtension.EqualsTo(x.Y, y.Y) &&
                GeometryExtension.EqualsTo(x.Z, y.Z);
        }

        public int GetHashCode(IPointEntity obj)
        {
            unchecked
            {
                var hash = 0;
                hash = (hash * 397) ^ obj.X.GetHashCode();
                hash = (hash * 397) ^ obj.Y.GetHashCode();
                hash = (hash * 397) ^ obj.Z.GetHashCode();
                return hash;
            }
        }

        public bool Equals(object x, object y)
        {
            IPointEntity first = x as IPointEntity;
            IPointEntity second = y as IPointEntity;
            if (first != null && second != null)
                return this.Equals(first, second);

            return x.Equals(y);
        }

        public int GetHashCode(object obj)
        {
            IPointEntity pt = obj as IPointEntity;
            if (pt != null)
                return GetHashCode(pt);

            return obj.GetHashCode();
        }
    }*/

    class MeshEntity : TopologyEntity, IPolyMeshEntity
    {
        ISolidEntity mSolid = new SolidEntity();
        public IGeometryEntity Geometry
        {
            get { return mSolid; }
        }

        public IFaceEntity[] GetFaces()
        {
            return mSolid.GetFaces();
        }

        public IEdgeEntity[] GetEdges()
        {
            return mSolid.GetEdges();
        }

        public IVertexEntity[] GetVertices()
        {
            return mSolid.GetVertices();
        }

        /*public int GetVertexCount()
        {
            return mSolid.GetVertexCount();
        }*/

        public int GetEdgeCount()
        {
            return mSolid.GetEdgeCount();
        }

        /*public int GetFaceCount()
        {
            return mSolid.GetFaceCount();
        }*/

        public int NumVertices
        {
            get { return mSolid.GetVertexCount(); }
        }

        public int NumFaces
        {
            get { return mSolid.GetFaceCount(); }
        }

        public int NumResultVertices
        {
            get { throw new NotImplementedException(); }
        }

        public int NumResultFaces
        {
            get { throw new NotImplementedException(); }
        }

        public IColor[] GetVertexColors()
        {
            throw new NotImplementedException();
        }

        public IVectorEntity[] GetVertexNormals()
        {
            throw new NotImplementedException();
        }

        public int[][] GetFaceIndices()
        {
            throw new NotImplementedException();
        }

        public double Area
        {
            get { throw new NotImplementedException(); }
        }

        public double Volume
        {
            get { throw new NotImplementedException(); }
        }

        public bool GetIsClosed()
        {
            throw new NotImplementedException();
        }

        public ISurfaceEntity ConvertToSurface(bool bConvertAsSmooth)
        {
            throw new NotImplementedException();
        }

        public ISolidEntity ConvertToSolid(bool bConvertAsSmooth)
        {
            throw new NotImplementedException();
        }
    }
}
