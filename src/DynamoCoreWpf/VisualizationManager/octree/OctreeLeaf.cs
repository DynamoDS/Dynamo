using System;

namespace Octree.OctreeSearch
{

    [Serializable]
    public class OctreeLeaf
    {

        private float fx, fy, fz;
        private object objectValue;

        public OctreeLeaf(float x, float y, float z, object obj)
        {
            fx = x;
            fy = y;
            fz = z;
            objectValue = obj;
        }
        public OctreeLeaf(float x, float y, float z, int obj)
            : this(x, y, z, (object)obj)
        {
        }
        public OctreeLeaf(float x, float y, float z, uint obj)
            : this(x, y, z, (object)obj)
        {
        }
        public OctreeLeaf(float x, float y, float z, short obj)
            : this(x, y, z, (object)obj)
        {
        }
        public OctreeLeaf(float x, float y, float z, long obj)
            : this(x, y, z, (object)obj)
        {
        }
        public OctreeLeaf(float x, float y, float z, float obj)
            : this(x, y, z, (object)obj)
        {
        }
        public OctreeLeaf(float x, float y, float z, double obj)
            : this(x, y, z, (object)obj)
        {
        }
        public OctreeLeaf(float x, float y, float z, bool obj)
            : this(x, y, z, (object)obj)
        {
        }

        public OctreeLeaf(double x, double y, double z, object obj)
            : this((float)x, (float)y, (float)z, (object)obj)
        {
        }
        public OctreeLeaf(double x, double y, double z, int obj)
            : this(x, y, z, (object)obj)
        {
        }
        public OctreeLeaf(double x, double y, double z, uint obj)
            : this(x, y, z, (object)obj)
        {
        }
        public OctreeLeaf(double x, double y, double z, short obj)
            : this(x, y, z, (object)obj)
        {
        }
        public OctreeLeaf(double x, double y, double z, long obj)
            : this(x, y, z, (object)obj)
        {
        }
        public OctreeLeaf(double x, double y, double z, float obj)
            : this(x, y, z, (object)obj)
        {
        }
        public OctreeLeaf(double x, double y, double z, double obj)
            : this(x, y, z, (object)obj)
        {
        }
        public OctreeLeaf(double x, double y, double z, bool obj)
            : this(x, y, z, (object)obj)
        {
        }

        public object LeafObject
        {
            get
            {
                return objectValue;
            }
        }

        public float X
        {
            get
            {
                return fx;
            }
            set
            {
                fx = value; ;
            }
        }
        public float Y
        {
            get
            {
                return fy;
            }
            set
            {
                fy = value; ;
            }
        }
        public float Z
        {
            get
            {
                return fz;
            }
            set
            {
                fz = value; ;
            }
        }

    }
}