using System;
using System.Runtime.Serialization;

using Octree.Tools.Vector;

namespace Octree.Tools.Point
{
    /// <summary>
    /// 3D double point
    /// </summary>
    [Serializable]
    public class Point3d : 
        IEquatable<Point3d>, 
        ISerializable
    {
        private double[] nxyz = new double[3];

        public static readonly Point3d NullPoint = new Point3d(
                                                                double.NegativeInfinity,
                                                                double.NegativeInfinity,
                                                                double.NegativeInfinity);

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public Point3d()
        {
        }

        /// <summary>
        /// Constructor - overload 1
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        public Point3d(double x, double y, double z)
        {
            nxyz[0] = x;
            nxyz[1] = y;
            nxyz[2] = z;
        }

        /// <summary>
        /// Constructor - overload 2
        /// </summary>
        /// <param name="XYZ">A double array for coordinates</param>
        public Point3d(double[] xyz)
        {
            nxyz = (double[])xyz.Clone();
        }

        /// <summary>
        /// Constructor - overload 3
        /// </summary>
        /// <param name="XYZ">A vector for coordinates</param>
        public Point3d(Vector3d vector)
        {
            nxyz = vector.ToArray();
        }

        #endregion

        #region ISerializable

        //Deserialization constructor
        public Point3d(SerializationInfo info, StreamingContext ctxt)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            nxyz = sr.ReadDoubleArray();
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
        /// <param name="XYZ">A double array for coordinates</param>
        public double this[byte i]
        {
            get
            {
                if (i < 3)
                    return nxyz[i];
                return Double.NaN;
            }
            set
            {
                if (i < 3)
                    nxyz[i] = value;
            }

        }
        public double this[int i]
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
        public double this[uint i]
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
        public double[] getxyz()
        {
            return nxyz;
        }
        /// <summary>
        /// get Max
        /// </summary>
        public double Max()
        {
            return Math.Max(nxyz[0], Math.Max(nxyz[1], nxyz[2]));
        }

        /// <summary>
        /// get Min
        /// </summary>
        public double Min()
        {
            return Math.Min(nxyz[0], Math.Min(nxyz[1], nxyz[2]));
        }
        

        /// <summary>
        /// Write coordinates
        /// </summary>
        /// <returns></returns>
        public string WriteCoordinate()
        {
            return new Vector3d(nxyz).ToString();
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
        public bool AlmostEquals(Point3d p2, double Error)
        {
            return Math.Abs(this.x - p2.x) <= Error &&
                   Math.Abs(this.y - p2.y) <= Error &&
                   Math.Abs(this.z - p2.z) <= Error;
        }
        public bool Equals(Point3d p2)
        {
            return this.x == p2.x && this.y == p2.y && this.z == p2.z;
        }
        #endregion

        #region Properties
        /// <summary>
        /// get/set x coordinate
        /// </summary>
        public double x
        {
            get { return nxyz[0]; }
            set { nxyz[0] = value; }
        }

        /// <summary>
        /// get/set y coordinate
        /// </summary>
        public double y
        {
            get { return nxyz[1]; }
            set { nxyz[1] = value; }
        }

        /// <summary>
        /// get/set z coordinate
        /// </summary>
        public double z
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
