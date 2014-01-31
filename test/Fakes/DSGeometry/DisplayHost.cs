using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class DisplayEntity : IDisplayable
    {
        IColor mColor = null;
        bool mVisible = true;
        public bool Highlight(bool visibility)
        {
            return true;
        }
        
        public void Dispose()
        {
            //disposed
        }

        bool IDisplayable.Highlight(bool visibility)
        {
            return true;
        }

        IDisplayable IDisplayable.SetColor(IColor color)
        {
            mColor = color;
            return this;
        }

        IDisplayable IDisplayable.SetVisibility(bool visible)
        {
            mVisible = visible;
            return this;
        }

        bool IDisplayable.Visible
        {
            get
            {
                return mVisible;
            }
            set
            {
                mVisible = value;
            }
        }

        IColor IDisplayable.Color
        {
            get
            {
                return mColor;
            }
            set
            {
                mColor = value;
            }
        }
    }
}
