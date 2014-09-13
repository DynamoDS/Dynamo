using System;
using System.Runtime.Serialization;

namespace Octree.Tools.Vector
{

    /// <summary>
    /// 3D Vector (float)
    /// </summary>
    [Serializable]
    public class Vector3f : 
        IComparable<Vector3f>, 
        ICloneable, 
        ISerializable
    {
        #region Fileds

        private float[] nXYZ = new float[3];

        #endregion Fileds

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public Vector3f()
        {
            nXYZ[0] = 0;
            nXYZ[1] = 0;
            nXYZ[2] = 0;
        }

        /// <summary>
        /// Constructor - overload 1
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vector3f(float x, float y, float z)
        {
            nXYZ[0] = x;
            nXYZ[1] = y;
            nXYZ[2] = z;
        }

        /// <summary>
        /// Constructor - overload 2
        /// </summary>
        /// <param name="xyz">A float array for coordinates</param>
        public Vector3f(float[] xyz)
        {
            nXYZ = (float[])xyz.Clone();
        }

        /// <summary>
        /// Constructor - overload 2
        /// </summary>
        /// <param name="xyz">A float array for coordinates</param>
        public Vector3f(double[] xyz)
        {
            for (int i = 0; i < 3; i++)
                nXYZ[i] = (float)xyz[i];
        }

        #endregion

        #region Indexers
        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="XYZ">A double array for coordinates</param>
        public float this[byte i]
        {
            get
            {
                if (i < nXYZ.Length)
                    return nXYZ[i];
                return float.NaN;
            }
            set
            {
                if (i < nXYZ.Length)
                    nXYZ[i] = value;
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

        #region ISerializable

        //Deserialization constructor
        public Vector3f(SerializationInfo info, StreamingContext ctxt)
        {
            SerializationReader sr = SerializationReader.GetReader(info);
            nXYZ = sr.ReadFloatArray();
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
        //~Vector3f()
        //{
        //    // Do not re-create Dispose clean-up code here.
        //    // Calling Dispose(false) is optimal in terms of
        //    // readability and maintainability.
        //    Dispose(false);
        //}

        #endregion

        #region ICloneable
        /// <summary>
        /// deep copy of a Vector3f
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Vector3f((double[])nXYZ.Clone());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cross product of two vectors.
        /// </summary>
        /// <param name="v">Vector3f</param>
        /// <returns>a Vector3f representing the cross product of the current vector and vector v</returns>
        public Vector3f CrossProduct(Vector3f v)
        {
            return new Vector3f(
                (this.y * v.z) - (this.z * v.y),
                (this.z * v.x) - (this.x * v.z),
                (this.x * v.y) - (this.y * v.x));
        }

        /// <summary>
        /// dot product of two vectors.
        /// </summary>
        /// <param name="v">Vector3f</param>
        /// <returns>a float representing the dot product of the current vector and vector v</returns>
        public float DotProduct(Vector3f v)
        {
            return this.x * v.x + this.y * v.y + this.z * v.z;
        }

        /// <summary>
        /// returns the sorted xyz coordinates
        /// </summary>
        public float[] SortedList()
        {
            float[] xyz_s = (float[])nXYZ.Clone();
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
                if (float.IsNaN(nXYZ[i]) || float.IsInfinity(nXYZ[i]))
                    return true;

            return false;
        }

        /// <summary>
        /// get Max
        /// </summary>
        public float Max()
        {
            return (float)Math.Max(nXYZ[0], Math.Max(nXYZ[1], nXYZ[2]));
        }

        /// <summary>
        /// get Min
        /// </summary>
        public float Min()
        {
            return (float)Math.Min(nXYZ[0], Math.Min(nXYZ[1], nXYZ[2]));
        }

        /// <summary>
        /// Vector length
        /// </summary>
        public float Length()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// get xyz coordinates
        /// </summary>
        public float[] ToArray()
        {
            return nXYZ;
        }

        #endregion

        #region IComparable

        /// <summary>
        /// 0: This instance is equal to value.
        /// 1: All vectors components are greater. 
        /// -1: All vectors components are smaller. 
        /// -10: otherwise.
        /// </summary>
        /// <param name="other">Vector3f</param>
        /// <returns></returns>
        public int CompareTo(Vector3f other)
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
            return (float)x + ", " + (float)y + ", " + (float)z;
        }

        #endregion

        #region Operator Overloads
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3f operator +(Vector3f v1, Vector3f v2)
        {
            return new Vector3f(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v2"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vector3f operator -(Vector3f v2, Vector3f v1)
        {
            return new Vector3f(v2.x - v1.x, v2.y - v1.y, v2.z - v1.z);
        }

        /// <summary>
        /// Scalar Product
        /// </summary>
        /// <param name="v"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Vector3f operator *(Vector3f v, float s)
        {
            return new Vector3f(s * v.x, s * v.y, s * v.z);
        }

        public static Vector3f operator *(float s, Vector3f v)
        {
            return v * s;
        }
        public static Vector3f operator /(Vector3f v, float s)
        {
            return v * (1 / s);
        }

        /// <summary>
        /// Dot Product
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static float operator *(Vector3f v1, Vector3f v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        //public static bool operator ==(Vector3f v1, Vector3f v2)
        //{
        //    if (Math.Abs(v1.x - v2.x) < Epsilon &&
        //        Math.Abs(v1.y - v2.y) < Epsilon &&
        //        Math.Abs(v1.z - v2.z) < Epsilon)
        //        return true;
        //    else
        //        return false;
        //}

        //public static bool operator !=(Vector3f v1, Vector3f v2)
        //{
        //    return !(v1 == v2);
        //}

        //public override bool Equals(object v)
        //{
        //    return (this == (Vector3f)v);
        //}

        //public override int GetHashCode()
        //{
        //    return 0;
        //}

        #endregion Operator Overloads

        #region Properties

        /// <summary>
        /// get/set x coordinate
        /// </summary>
        public float x
        {
            get { return nXYZ[0]; }
            set { nXYZ[0] = value; }
        }

        /// <summary>
        /// get/set y coordinate
        /// </summary>
        public float y
        {
            get { return nXYZ[1]; }
            set { nXYZ[1] = value; }
        }

        /// <summary>
        /// get/set z coordinate
        /// </summary>
        public float z
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
        //public float this[uint i]
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
        //    Vector3f theVector;      // Vector object that this enumerato refers to 
        //    int location;   // which element of theVector the enumerator is currently referring to 

        //    public VectorEnumerator(Vector3f theVector)
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
