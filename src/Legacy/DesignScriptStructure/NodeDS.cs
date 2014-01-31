using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.DesignScript.Structure
{
    abstract public class NodeDS
    {
        public int Index { get; set; }

        public Point BasePoint { get; protected set; }
        protected double[] _x = { 0.0, 0.0, 0.0 };
        //protected double _y;
        //protected double _z;

        public LoadDS Load { get; set; }
        public FixityDS Fixity { get; set; }

        public Vertex Vertex { get; set; }

       // public static abstract NodeDS ByNode();

        protected void Copy(NodeDS node)
        {
            CreateFromPoint(node.BasePoint);

            Fixity = node.Fixity;

            BasePoint = node.BasePoint;

            Index = node.Index;
        }

        protected virtual void CreateFromPoint(Point pt)
        {
            _x[0] = pt.X;
            _x[1] = pt.Y;
            _x[2] = pt.Z;
            BasePoint = pt;
        }

        public double X
        {
            get { return _x[0]; }
            set
            {
                _x[0] = value;
                BasePoint = (Point)BasePoint.Translate(value, 0, 0);
            }

        }

        public double Y
        {
            get { return _x[1]; }
            set
            {
                _x[1] = value;
                BasePoint = (Point)BasePoint.Translate(0, value, 0);
            }
        }

        public double Z
        {
            get { return _x[2]; }
            set
            {
                _x[2] = value;
                BasePoint = (Point)BasePoint.Translate(0, 0, value);
            }
        }

        public void UpdateBasePoint()
        {
            //BasePoint.Dispose(); not working

            BasePoint = Point.ByCoordinates(_x[0], _x[1], _x[2]);
        }

        public bool SetFixity(FixityDS fixity)
        {
            Fixity = fixity;
            return true;
        }
        public bool SetLoad(LoadDS load)
        {
            Load = load;
            return true;
        }
    }
}
