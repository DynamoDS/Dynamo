using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class TextEntity : GeometryEntity, ITextEntity
    {
        internal TextEntity()
        {
            Height = 10;
            Text = "Hello World";
            Orientation = 0;
        }

        private double height;
        public double Height
        {
            get { return height; }
            protected set { height = value; }
        }

        private string text;
        public string Text
        {
            get { return text; }
            protected set { text = value; }
        }

        private int orientation;
        public int Orientation
        {
            get { return orientation; }
            protected set { orientation = value; }
        }

        public double GetFontSize()
        {
            return Height;
        }

        public string GetString()
        {
            return Text;
        }

        public bool UpdateByCoordinateSystem(ICoordinateSystemEntity cs, int orientation, string textString, double fontSize)
        {
            this.Orientation = orientation;
            this.Text = textString;
            this.Height = fontSize;
            return true;
        }

        public Encoding GetEncoding()
        {
            throw new NotImplementedException();
        }
    }
}
