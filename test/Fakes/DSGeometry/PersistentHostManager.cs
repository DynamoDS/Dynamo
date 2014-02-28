using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class PersistentHostManager : IPersistenceManager
    {
        public IPersistentObject Persist(IDesignScriptEntity entity)
        {
            return new PersistentObject(entity);
        }

        public void UpdateDisplay()
        {
            throw new NotImplementedException();
        }

        public IPersistentObject GetPersistentObjectFromHandle(object handle)
        {
            throw new NotImplementedException();
        }

        public IGeometryFactory GeometryFactory
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool SupportsGeometryCapture()
        {
            throw new NotImplementedException();
        }

        public IDesignScriptEntity[] CaptureGeometry()
        {
            throw new NotImplementedException();
        }

        public IPersistentObject FromObject(long ptr)
        {
            throw new NotImplementedException();
        }
    }
}
