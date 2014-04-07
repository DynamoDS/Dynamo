using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class EdgeEntity : TopologyEntity, IEdgeEntity
    {
        public int GetAdjacentFaceCount()
        {
            return 4;
        }

        public IFaceEntity[] GetAdjacentFaces()
        {
            return new IFaceEntity[3] { new FaceEntity(), new FaceEntity(), new FaceEntity() };
        }

        public ICurveEntity GetCurveGeometry()
        {
            return new LineEntity();
        }

        public IVertexEntity GetEndVertex()
        {
            return new VertexEntity();
        }

        public IVertexEntity GetStartVertex()
        {
            return new VertexEntity();
        }
    }
}
