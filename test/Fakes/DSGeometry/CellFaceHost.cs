using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class CellFaceEntity  : TopologyEntity, ICellFaceEntity
    {
        public ICellEntity GetCell()
        {
            return new CellEntity();
        }

        public IFaceEntity GetFace()
        {
            return new FaceEntity();
        }
    }
}
