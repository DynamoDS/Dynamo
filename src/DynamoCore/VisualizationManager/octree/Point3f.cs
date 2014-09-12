using System;
using System.Runtime.Serialization;

using Octree.Tools.Vector;

namespace Octree.Tools.Point
{

    /// <summary>
    /// 3D float point
    /// </summary>
    [Serializable]
    public class Point3f :
        IEquatable<Point3f>,
        ISerializable
    {
        private float[] nxyz = new float[3];

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public Point3f()
        {
        }

        /// <summary>
        /// Constructor - overload 1
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        public Point3f(float x, float y, float z)
        {
            nxyz[0] = x;
            nxyz[1] = y;
            nxyz[2] = z;
        }

        /// <summary>
        /// Constructor - overload 2
        /// </summary>
        /// <param name="XYZ">A float array for coordinates</param>
        public Point3f(float[] xyz)
        {
            nxyz = (float[])xyz.Clone();
        }

        /// <summary>
        /// Constructor - overload 3
        /// </summary>
        /// <param name="XYZ">A vector for coordinates</param>
        public Point3f(Vector3f vector)
        {
            nxyz = vector.ToArray();
        }

        #endregion

        #region ISerializable

        //Deserialization constructor
        public Point3f(SerializationInfo info, StreamingContext ctxt)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            nxyz = sr.ReadFloatArray();
        }

        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            sw.Write(nxyz);
            sw.AddToInfo(info);
        }

        #endregion

        #region Indexers
        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="XYZ">A float array for coordinates</param>
        public float this[byte i]
        {
            get
            {
                if (i < 3)
                    return nxyz[i];
                return float.NaN;
            }
            set
            {
                if (i < 3)
                    nxyz[i] = value;
            }

        }
        public float this[int i]
        {
            get
            {
                return this[(byte)i];
            }
            set
            {
                this[(byte)i] = value;
            }

        }
        public float this[uint i]
        {
            get
            {
                return this[(byte)i];
            }
            set
            {
                this[(byte)i] = value;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// get xyz coordinates
        /// </summary>
        public float[] getxyz()
        {
            return nxyz;
        }
        /// <summary>
        /// get Max
        /// </summary>
        public float Max()
        {
            return Math.Max(nxyz[0], Math.Max(nxyz[1], nxyz[2]));
        }

        /// <summary>
        /// get Min
        /// </summary>
        public float Min()
        {
            return Math.Min(nxyz[0], Math.Min(nxyz[1], nxyz[2]));
        }
        

        /// <summary>
        /// Write coordinates
        /// </summary>
        /// <returns></returns>
        public string WriteCoordinate()
        {
            return new Vector3f(nxyz).ToString();
        }
        /// <summary>
        /// Write one coordinate
        /// </summary>
        /// <returns></returns>
        public string WriteCoordinate(byte index)
        {
            return this.nxyz[index].ToString();
        }
        /// <summary>
        /// Write one coordinate
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string WriteCoordinate(int index)
        {
            return WriteCoordinate((byte)index);
        }
        /// <summary>
        /// Write one coordinate
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string WriteCoordinate(uint index)
        {
            return WriteCoordinate((byte)index);
        }
        public bool AlmostEquals(Point3f p2, float Error)
        {
            return Math.Abs(this.x - p2.x) <= Error &&
                   Math.Abs(this.y - p2.y) <= Error &&
                   Math.Abs(this.z - p2.z) <= Error;
        }
        public bool Equals(Point3f p2)
        {
            return this.x == p2.x && this.y == p2.y && this.z == p2.z;
        }
        
        public static float Area2(Point3f p0, Point3f p1, Point3f p2)
        {
            return 0; // p0.x * (p1.y - p2.y) + p1.x * (p2.y - p0.y) + p2.x * (p0.y - p1.y);
        }
        #endregion

        #region Properties
        /// <summary>
        /// get/set x coordinate
        /// </summary>
        public float x
        {
            get { return nxyz[0]; }
            set { nxyz[0] = value; }
        }

        /// <summary>
        /// get/set y coordinate
        /// </summary>
        public float y
        {
            get { return nxyz[1]; }
            set { nxyz[1] = value; }
        }

        /// <summary>
        /// get/set z coordinate
        /// </summary>
        public float z
        {
            get { return nxyz[2]; }
            set { nxyz[2] = value; }
        }
        #endregion

        #region Overrides

        /// <summary>
        /// Overrides ToString to print a point
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.x + " " + this.y + " " + this.z;
        }

        #endregion


    }

}
