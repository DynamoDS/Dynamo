using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class PersistentObject : IPersistentObject, IDisplayable
    {
        private IDesignScriptEntity mEntity;
        public PersistentObject(IDesignScriptEntity geometry)
        {
            mEntity = geometry;
            Visible = true;
            Color = null;
        }

        public bool Erase()
        {
            Visible = false;
            Color = null;
            return false;
        }

        public IGeometryEntity Geometry
        {
            get { return mEntity as IGeometryEntity; }
        }

        public IDisplayable Display
        {
            get { return this; }
        }

        public void Dispose()
        {
            //disposed
        }

        public bool Highlight(bool visibility)
        {
            throw new NotImplementedException();
        }

        public IDisplayable SetColor(IColor color)
        {
            Color = color;
            return this;
        }

        public IDisplayable SetVisibility(bool visible)
        {
            Visible = visible;
            return this;
        }

        public bool Visible
        {
            get;
            set;
        }

        public IColor Color
        {
            get;
            set;
        }
    }
}
