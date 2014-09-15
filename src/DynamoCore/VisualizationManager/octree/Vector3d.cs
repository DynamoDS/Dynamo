using System;
using System.Runtime.Serialization;

using Octree.Tools.Point;

namespace Octree.Tools.Vector
{

    ///<summary>
    ///3D Vector (double)
    ///</summary>
    [Serializable]
    public class Vector3d : 
        IComparable<Vector3d>, 
        ICloneable, 
        ISerializable
    {
        #region Fileds

        protected double[] nXYZ = new double[3];

        #endregion Fileds

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public Vector3d()
        {
            nXYZ[0] = 0.0;
            nXYZ[1] = 0.0;
            nXYZ[2] = 0.0;
        }

        /// <summary>
        /// Constructor - overload 1
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        public Vector3d(double x, double y, double z)
        {
            nXYZ[0] = x;
            nXYZ[1] = y;
            nXYZ[2] = z;
        }

        public Vector3d(Point3d point)
        {
            nXYZ[0] = point.x;
            nXYZ[1] = point.y;
            nXYZ[2] = point.z;
        }

        /// <summary>
        /// Constructor - overload 2
        /// </summary>
        /// <param name="XYZ">A double array for coordinates</param>
        public Vector3d(double[] xyz)
        {
            nXYZ = (double[])xyz.Clone();
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
                    return nXYZ[i];
                return Double.NaN;
            }
            set 
            {
                if (i < 3)
                    nXYZ[i] = value;
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

        #region ISerializable

        //Deserialization constructor
        public Vector3d(SerializationInfo info, StreamingContext ctxt)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            nXYZ = sr.ReadDoubleArray();
        }

        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            SerializationWriter sw = SerializationWriter.GetWriter();
            sw.Write(nXYZ);
            sw.AddToInfo(info);
        }

        #endregion

        #region IDisposable

        //// Pointer to an external unmanaged resource.
        //// Other managed resource this class uses.
        //private Component component = new Component();
        //// Track whether Dispose has been called.
        //private bool disposed = false;
        //private IntPtr handle;

        //// Implement IDisposable.
        //// Do not make this method virtual.
        //// A derived class should not be able to override this method.
        //public void Dispose()
        //{
        //    Dispose(true);
        //    // This object will be cleaned up by the Dispose method.
        //    // Therefore, you should call GC.SupressFinalize to
        //    // take this object off the finalization queue 
        //    // and prevent finalization code for this object
        //    // from executing a second time.
        //    GC.SuppressFinalize(this);
        //}

        //// Dispose(bool disposing) executes in two distinct scenarios.
        //// If disposing equals true, the method has been called directly
        //// or indirectly by a user's code. Managed and unmanaged resources
        //// can be disposed.
        //// If disposing equals false, the method has been called by the 
        //// runtime from inside the finalizer and you should not reference 
        //// other objects. Only unmanaged resources can be disposed.
        //private void Dispose(bool disposing)
        //{
        //    // Check to see if Dispose has already been called.
        //    if (!disposed)
        //    {
        //        // If disposing equals true, dispose all managed 
        //        // and unmanaged resources.
        //        if (disposing)
        //        {
        //            // Dispose managed resources.
        //            component.Dispose();
        //        }

        //        // Call the appropriate methods to clean up 
        //        // unmanaged resources here.
        //        // If disposing is false, 
        //        // only the following code is executed.
        //        CloseHandle(handle);
        //        handle = IntPtr.Zero;
        //    }
        //    disposed = true;
        //}

        //// Use interop to call the method necessary  
        //// to clean up the unmanaged resource.
        //[DllImport("Kernel32")]
        //private static extern Boolean CloseHandle(IntPtr handle);

        //// Use C# destructor syntax for finalization code.
        //// This destructor will run only if the Dispose method 
        //// does not get called.
        //// It gives your base class the opportunity to finalize.
        //// Do not provide destructors in types derived from this class.
        //~Vector3d()
        //{
        //    // Do not re-create Dispose clean-up code here.
        //    // Calling Dispose(false) is optimal in terms of
        //    // readability and maintainability.
        //    Dispose(false);
        //}

        #endregion

        #region ICloneable
        /// <summary>
        /// deep copy of a Vector3d
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Vector3d((double[])nXYZ.Clone());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cross product of two vectors.
        /// </summary>
        /// <param name="v">Vector3d</param>
        /// <returns>a Vector3d representing the cross product of the current vector and vector v</returns>
        public Vector3d CrossProduct(Vector3d v)
        {
            return new Vector3d(
                (this.y * v.z) - (this.z * v.y),
                (this.z * v.x) - (this.x * v.z),
                (this.x * v.y) - (this.y * v.x));
        }

        /// <summary>
        /// dot product of two vectors.
        /// </summary>
        /// <param name="v">Vector3d</param>
        /// <returns>a float representing the dot product of the current vector and vector v</returns>
        public double DotProduct(Vector3d v)
        {
            return this.x * v.x + this.y * v.y + this.z * v.z;
        }

        public Vector3f ToVector3f()
        {
            return new Vector3f(nXYZ);
        }

        /// <summary>
        /// returns the sorted xyz coordinates
        /// </summary>
        public double[] SortedList()
        {
            double[] xyz_s = (double[])nXYZ.Clone();
            Array.Sort(xyz_s);
            return xyz_s;
        }

        /// <summary>
        /// check if the vector is NaN or Infinity
        /// </summary>
        /// <returns></returns>
        private bool IsNan()
        {
            for (int i = 0; i < 3; i++)
                if (Double.IsNaN(nXYZ[i]) || Double.IsInfinity(nXYZ[i]))
                    return true;

            return false;
        }

        /// <summary>
        /// get Max
        /// </summary>
        public double Max()
        {
            return Math.Max(nXYZ[0], Math.Max(nXYZ[1], nXYZ[2]));
        }

        /// <summary>
        /// get Min
        /// </summary>
        public double Min()
        {
            return Math.Min(nXYZ[0], Math.Min(nXYZ[1], nXYZ[2]));
        }

        /// <summary>
        /// Vector length
        /// </summary>
        public double Length()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// get xyz coordinates
        /// </summary>
        public double[] ToArray()
        {
            return nXYZ;
        }

        /// <summary>
        /// Converts coordinates to a Point3d
        /// </summary>
        public Point3d ToPoint3d()
        {
            return new Point3d(this.x, this.y, this.z);
        }
       
        #endregion

        #region IComparable

        /// <summary>
        /// 0: Vectors are identical.
        /// 1: All vectors components are greater. 
        /// -1: All vectors components are smaller. 
        /// -10: otherwise.
        /// </summary>
        /// <param name="other">Vector3d</param>
        /// <returns></returns>
        public int CompareTo(Vector3d other)
        {
            int result = 0;
            for (int i = 0; i < 3; i++)
                result += nXYZ[i].CompareTo(other.nXYZ[i]);

            switch (result)
            {
                case 0:
                    return 0;
                case 3:
                    return 1;
                case -3:
                    return -1;
                default:
                    return -10;
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Overrides ToString to print a vector
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.x + " " + this.y + " " + this.z;
        }

        #endregion

        #region Operator Overloads

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3d operator +(Vector3d v1, Vector3d v2)
        {
            return new Vector3d(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3d operator +(Vector3d v1, Point3d v2)
        {
            return new Vector3d(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v2"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vector3d operator -(Vector3d v1, Vector3d v2)
        {
            return new Vector3d(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3d operator -(Vector3d v1, Point3d v2)
        {
            return new Vector3d(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }
        /// <summary>
        /// Scalar Product
        /// </summary>
        /// <param name="v"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Vector3d operator *(Vector3d v, double s)
        {
            return new Vector3d(s * v.x, s * v.y, s * v.z);
        }
        public static Vector3d operator *(double s, Vector3d v)
        {
            return v * s;
        }
        public static Vector3d operator /(Vector3d v, double s)
        {
            return v * (1 / s);
        }

        /// <summary>
        /// Dot Product
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double operator *(Vector3d v1, Vector3d v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        //public static bool operator ==(Vector3d v1, Vector3d v2)
        //{

        //    double[] v1Sorted = v1.SortedList();
        //    double[] v2Sorted = v2.SortedList();

        //    for (int i = 0; i < 3; i++)
        //    {
        //        if (Math.Abs(v1Sorted[i] - v2Sorted[i]) > Epsilon)
        //            return false;
        //    }

        //    return true;
        //}

        //public static bool operator !=(Vector3d v1, Vector3d v2)
        //{
        //    return !(v1 == v2);
        //}
        #region IEquatable
        //public override bool Equals(object v)
        //{
        //    return (this.CompareTo((Vector3d)v) == 0 ? true : false);
        //}
        public bool Equals(Vector3d v)
        {
            return (this.CompareTo(v) == 0 ? true : false);
        }
        #endregion
        //public override int GetHashCode()
        //{
        //    return 0;
        //}

        #endregion Operator Overloads

        #region Properties

        /// <summary>
        /// get/set x coordinate
        /// </summary>
        public double x
        {
            get { return nXYZ[0]; }
            set { nXYZ[0] = value; }
        }

        /// <summary>
        /// get/set y coordinate
        /// </summary>
        public double y
        {
            get { return nXYZ[1]; }
            set { nXYZ[1] = value; }
        }

        /// <summary>
        /// get/set z coordinate
        /// </summary>
        public double z
        {
            get { return nXYZ[2]; }
            set { nXYZ[2] = value; }
        }

        #endregion

        #region IEnumerator

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //public IEnumerator GetEnumerator()
        //{
        //    return new VectorEnumerator(this);
        //}

        /// <summary>
        /// this enable a vector to be looped in a foreach
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        //public double this[uint i]
        //{
        //    get
        //    {
        //        switch (i)
        //        {
        //            case 0:
        //                return x;
        //            case 1:
        //                return y;
        //            case 2:
        //                return z;
        //            default:
        //                throw new IndexOutOfRangeException(
        //                   "Attempt to retrieve Vector element" + i);
        //        }
        //    }
        //    set
        //    {
        //        switch (i)
        //        {
        //            case 0:
        //                x = value;
        //                break;
        //            case 1:
        //                y = value;
        //                break;
        //            case 2:
        //                z = value;
        //                break;
        //            default:
        //                throw new IndexOutOfRangeException(
        //                   "Attempt to set Vector element" + i);
        //        }
        //    }
        //}
        //private class VectorEnumerator : IEnumerator
        //{
        //    Vector3d theVector;      // Vector object that this enumerato refers to 
        //    int location;   // which element of theVector the enumerator is currently referring to 

        //    public VectorEnumerator(Vector3d theVector)
        //    {
        //        this.theVector = theVector;
        //        location = -1;
        //    }

        //    public bool MoveNext()
        //    {
        //        ++location;
        //        return (location > 2) ? false : true;
        //    }

        //    public object Current
        //    {
        //        get
        //        {
        //            if (location < 0 || location > 2)
        //                throw new InvalidOperationException(
        //                   "The enumerator is either before the first element or " +
        //                   "after the last element of the Vector");
        //            return theVector[(uint)location];
        //        }
        //    }

        //    public void Reset()
        //    {
        //        location = -1;
        //    }
        //}

        #endregion
    }


}
