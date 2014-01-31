using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class ShellEntity : TopologyEntity, IShellEntity
    {
        public int GetFaceCount()
        {
            return 4;
        }

        public IFaceEntity[] GetFaces()
        {
            return new IFaceEntity[2] { new FaceEntity(), new FaceEntity() };
        }

        public ISolidEntity GetSolidGeometry()
        {
            return new SolidEntity();
        }
    }
}
