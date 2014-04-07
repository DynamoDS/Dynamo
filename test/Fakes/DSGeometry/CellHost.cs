using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class CellEntity : TopologyEntity, ICellEntity
    {
        public ICellEntity[] GetAdjacentCells()
        {
            return new ICellEntity[2]{new CellEntity(),new CellEntity()};
        }

        public double GetArea()
        {
            return 30;
        }

        public IPointEntity GetCentroid()
        {
            return new PointEntity();
        }

        public int GetFaceCount()
        {
            return 5;
        }

        public ICellFaceEntity[] GetFaces()
        {
            return new ICellFaceEntity[2]{new CellFaceEntity(), new CellFaceEntity()};
        }

        public ISolidEntity GetSolidGeometry()
        {
            return new SolidEntity();
        }

        public double GetVolume()
        {
            return 350;
        }
    }
}
