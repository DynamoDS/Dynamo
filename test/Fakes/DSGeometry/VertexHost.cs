using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class VertexEntity : TopologyEntity, IVertexEntity
    {
        public int GetAdjacentEdgeCount()
        {
            return 6;
        }

        public IEdgeEntity[] GetAdjacentEdges()
        {
            return new IEdgeEntity[4] { new EdgeEntity(), new EdgeEntity(), new EdgeEntity(), new EdgeEntity() };
        }

        public int GetAdjacentFaceCount()
        {
            return 6;
        }

        public IFaceEntity[] GetAdjacentFaces()
        {
            return new IFaceEntity[3] { new FaceEntity(), new FaceEntity(), new FaceEntity() };
        }

        public IPointEntity GetPointGeometry()
        {
            return new PointEntity();
        }
    }
}
