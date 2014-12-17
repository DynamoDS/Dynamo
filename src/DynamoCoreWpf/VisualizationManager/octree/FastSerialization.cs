using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Octree.Tools.Point;
using Octree.Tools.Vector;

namespace Octree.Tools
{
    #region FastSerialization

    // Enum for the standard types handled by Read/WriteObject()
    internal enum ObjType : byte
    {
        nullType,
        boolType,
        byteType,
        uint16Type,
        uint32Type,
        uint64Type,
        sbyteType,
        int16Type,
        int32Type,
        int64Type,
        charType,
        stringType,
        singleType,
        doubleType,
        decimalType,
        dateTimeType,
        byteArrayType,
        charArrayType,
        vector3dType,
        vector3fType,
        otherType
    }

    /// <summary> SerializationWriter.  Extends BinaryWriter to add additional data types,
    /// handle null strings and simplify use with ISerializable. </summary>
    public class SerializationWriter : BinaryWriter
    {
        private SerializationWriter(Stream s) : base(s) { }

        /// <summary> Static method to initialise the writer with a suitable MemoryStream. </summary>

        public static SerializationWriter GetWriter()
        {
            MemoryStream ms = new MemoryStream(1024);
            return new SerializationWriter(ms);
        }

        /// <summary> Writes a string to the buffer.  Overrides the base implementation so it can cope with nulls </summary>

        public override void Write(string str)
        {
            if (str == null)
            {
                Write((byte)ObjType.nullType);
            }
            else
            {
                Write((byte)ObjType.stringType);
                base.Write(str);
            }
        }

        /// <summary> Writes a byte array to the buffer.  Overrides the base implementation to
        /// send the length of the array which is needed when it is retrieved </summary>

        public override void Write(byte[] b)
        {
            if (b == null)
            {
                Write(-1);
            }
            else
            {
                int len = b.Length;
                Write(len);
                if (len > 0) base.Write(b);
            }
        }

        /// <summary> Writes a char array to the buffer.  Overrides the base implementation to
        /// sends the length of the array which is needed when it is read. </summary>

        public override void Write(char[] chars)
        {
            if (chars == null)
            {
                Write(-1);
            }
            else
            {
                int len = chars.Length;
                Write(len);
                if (len > 0) base.Write(chars);
            }
        }

        /// <summary> Writes a DateTime to the buffer. <summary>

        public void Write(DateTime dt) { Write(dt.Ticks); }

        /// <summary> Writes a generic ICollection (such as an IList<T>) to the buffer. </summary>

        public void Write<T>(ICollection<T> collection)
        {
            if (collection == null)
            {
                Write(-1);
            }
            else
            {
                Write(collection.Count);
                foreach (T item in collection) WriteObject(item);
            }
        }

        /// <summary> Writes a generic IDictionary to the buffer. </summary>

        public void Write<T, U>(IDictionary<T, U> dictionary)
        {
            if (dictionary == null)
            {
                Write(-1);
            }
            else
            {
                Write(dictionary.Count);
                foreach (KeyValuePair<T, U> kvp in dictionary)
                {
                    WriteObject(kvp.Key);
                    WriteObject(kvp.Value);
                }
            }
        }

        public void Write(uint[] UInt32Array)
        {
            if (UInt32Array == null)
            {
                Write(-1);
            }
            else
            {
                base.Write(UInt32Array.Length);
                for (int i = 0; i < UInt32Array.Length; i++)
                    base.Write(UInt32Array[i]);
            }
        }

        public void Write(int[] Int32Array)
        {
            if (Int32Array == null)
            {
                Write(-1);
            }
            else
            {
                base.Write(Int32Array.Length);
                for (int i = 0; i < Int32Array.Length; i++)
                    base.Write(Int32Array[i]);
            }
        }

        public void Write(double[] DoubleArray)
        {
            if (DoubleArray == null)
            {
                Write(-1);
            }
            else
            {
                base.Write(DoubleArray.Length);
                for (int i = 0; i < DoubleArray.Length; i++)
                    base.Write(DoubleArray[i]);
            }
        }

        public void Write(Vector3f vector)
        {
            if (vector == null)
            {
                Write(-1);
            }
            else
            {
                Write(vector.ToArray());
            }
        }

        public void Write(Vector3d vector)
        {
            if (vector == null)
            {
                Write(-1);
            }
            else
            {
                Write(vector.ToArray());
            }
        }

        public void Write(Point3d point)
        {
            if (point == null)
            {
                Write(-1);
            }
            else
            {
                Write(point.getxyz());
            }
        }

        /// <summary> Writes an arbitrary object to the buffer.  Useful where we have something of type "object"
        /// and don't know how to treat it.  This works out the best method to use to write to the buffer. </summary>
        public void WriteObject(object obj)
        {
            if (obj == null)
            {
                Write((byte)ObjType.nullType);
            }
            else
            {
                switch (obj.GetType().Name)
                {
                    case "Boolean":
                        Write((byte)ObjType.boolType);
                        Write((bool)obj);
                        break;

                    case "Byte":
                        Write((byte)ObjType.byteType);
                        Write((byte)obj);
                        break;

                    case "UInt16":
                        Write((byte)ObjType.uint16Type);
                        Write((ushort)obj);
                        break;

                    case "UInt32":
                        Write((byte)ObjType.uint32Type);
                        Write((uint)obj);
                        break;

                    case "UInt64":
                        Write((byte)ObjType.uint64Type);
                        Write((ulong)obj);
                        break;

                    case "SByte":
                        Write((byte)ObjType.sbyteType);
                        Write((sbyte)obj);
                        break;

                    case "Int16":
                        Write((byte)ObjType.int16Type);
                        Write((short)obj);
                        break;

                    case "Int32":
                        Write((byte)ObjType.int32Type);
                        Write((int)obj);
                        break;

                    case "Int64":
                        Write((byte)ObjType.int64Type);
                        Write((long)obj);
                        break;

                    case "Char":
                        Write((byte)ObjType.charType);
                        base.Write((char)obj);
                        break;

                    case "String":
                        Write((byte)ObjType.stringType);
                        base.Write((string)obj);
                        break;

                    case "Single":
                        Write((byte)ObjType.singleType);
                        Write((float)obj);
                        break;

                    case "Double":
                        Write((byte)ObjType.doubleType);
                        Write((double)obj);
                        break;

                    case "Decimal":
                        Write((byte)ObjType.decimalType);
                        Write((decimal)obj);
                        break;

                    case "DateTime":
                        Write((byte)ObjType.dateTimeType);
                        Write((DateTime)obj);
                        break;

                    case "Byte[]":
                        Write((byte)ObjType.byteArrayType);
                        base.Write((byte[])obj);
                        break;

                    case "Char[]":
                        Write((byte)ObjType.charArrayType);
                        base.Write((char[])obj);
                        break;

                    default:
                        Write((byte)ObjType.otherType);
                        new BinaryFormatter().Serialize(BaseStream, obj);
                        break;
                } // switch
            } // if obj==null
        } // WriteObject

        /// <summary> Adds the SerializationWriter buffer to the SerializationInfo at the end of GetObjectData(). </summary>
        public void AddToInfo(SerializationInfo info)
        {
            byte[] b = ((MemoryStream)BaseStream).ToArray();
            info.AddValue("X", b, typeof(byte[]));
        }
    } // SerializationWriter

    /// <summary> SerializationReader.  Extends BinaryReader to add additional data types,
    /// handle null strings and simplify use with ISerializable. </summary>
    public class SerializationReader : BinaryReader
    {
        private SerializationReader(Stream s)
            : base(s)
        {
        }

        /// <summary> Static method to take a SerializationInfo object (an input to an ISerializable constructor)
        /// and produce a SerializationReader from which serialized objects can be read </summary>.
        public static SerializationReader GetReader(SerializationInfo info)
        {
            byte[] byteArray = (byte[])info.GetValue("X", typeof(byte[]));
            MemoryStream ms = new MemoryStream(byteArray);
            return new SerializationReader(ms);
        }

        /// <summary> Reads a string from the buffer.  Overrides the base implementation so it can cope with nulls. </summary>
        public override string ReadString()
        {
            ObjType t = (ObjType)ReadByte();
            if (t == ObjType.stringType) return base.ReadString();
            return null;
        }

        /// <summary> Reads a byte array from the buffer, handling nulls and the array length. </summary>
        public byte[] ReadByteArray()
        {
            int len = ReadInt32();
            if (len > 0) return ReadBytes(len);
            if (len < 0) return null;
            return new byte[0];
        }

        /// <summary> Reads a char array from the buffer, handling nulls and the array length. </summary>
        public char[] ReadCharArray()
        {
            int len = ReadInt32();
            if (len > 0) return ReadChars(len);
            if (len < 0) return null;
            return new char[0];
        }

        /// <summary> Reads a DateTime from the buffer. </summary>
        public DateTime ReadDateTime()
        {
            return new DateTime(ReadInt64());
        }

        /// <summary> Reads a generic list from the buffer. </summary>
        public IList<T> ReadList<T>()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            IList<T> d = new List<T>();
            for (int i = 0; i < count; i++) 
                d.Add((T)ReadObject());
            return d;
        }

        /// <summary> Reads a generic Dictionary from the buffer. </summary>
        public IDictionary<T, U> ReadDictionary<T, U>()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            IDictionary<T, U> d = new Dictionary<T, U>();
            for (int i = 0; i < count; i++) 
                d[(T)ReadObject()] = (U)ReadObject();
            return d;
        }

        /// <summary>
        /// UInt32 Array
        /// </summary>
        /// <returns></returns>
        public uint[] ReadUInt32Array()
        {
            int len = ReadInt32();
            if (len < 0) return null;

            uint[] xyz = new uint[len];
            for (int i = 0; i < len; i++)
                xyz[i] = ReadUInt32();

            return xyz;
        }

        /// <summary>
        /// Int32 Array
        /// </summary>
        /// <returns></returns>
        public double[] ReadInt32Array()
        {
            int len = ReadInt32();
            if (len < 0) return null;

            double[] xyz = new double[len];
            for (int i = 0; i < len; i++)
                xyz[i] = ReadInt32();

            return xyz;
        }

        /// <summary>
        /// Float Array
        /// </summary>
        /// <returns></returns>
        public float[] ReadFloatArray()
        {
            int len = ReadInt32();
            if (len < 0) return null;

            float[] xyz = new float[len];
            for (int i = 0; i < len; i++)
                xyz[i] = ReadSingle();

            return xyz;
        }

        /// <summary>
        /// Double Array
        /// </summary>
        /// <returns></returns>
        public double[] ReadDoubleArray()
        {
            int len = ReadInt32();
            if (len < 0) return null;

            double[] xyz = new double[len];
            for (int i = 0; i < len; i++)
                xyz[i] = ReadDouble();

            return xyz;
        }

        /// <summary>
        /// Read Vector3f
        /// </summary>
        /// <returns></returns>
        public Vector3f ReadVector3f()
        {
            return new Vector3f(ReadFloatArray());
        }

        /// <summary>
        /// Read Vector3d
        /// </summary>
        /// <returns></returns>
        public Vector3d ReadVector3d()
        {
            return new Vector3d(ReadDoubleArray());
        }

        /// <summary>
        /// Read Point3d
        /// </summary>
        /// <returns></returns>
        public Point3d ReadPoint3d()
        {
            return new Point3d(ReadDoubleArray());
        }
        /// <summary> Reads an object which was added to the buffer by WriteObject. </summary>
        public object ReadObject()
        {
            ObjType t = (ObjType)ReadByte();
            switch (t)
            {
                case ObjType.boolType:
                    return ReadBoolean();
                case ObjType.byteType:
                    return ReadByte();
                case ObjType.uint16Type:
                    return ReadUInt16();
                case ObjType.uint32Type:
                    return ReadUInt32();
                case ObjType.uint64Type:
                    return ReadUInt64();
                case ObjType.sbyteType:
                    return ReadSByte();
                case ObjType.int16Type:
                    return ReadInt16();
                case ObjType.int32Type:
                    return ReadInt32();
                case ObjType.int64Type:
                    return ReadInt64();
                case ObjType.charType:
                    return ReadChar();
                case ObjType.stringType:
                    return base.ReadString();
                case ObjType.singleType:
                    return ReadSingle();
                case ObjType.doubleType:
                    return ReadDouble();
                case ObjType.decimalType:
                    return ReadDecimal();
                case ObjType.dateTimeType:
                    return ReadDateTime();
                case ObjType.byteArrayType:
                    return ReadByteArray();
                case ObjType.charArrayType:
                    return ReadCharArray();
                case ObjType.vector3dType:
                    return ReadVector3d();
                case ObjType.vector3fType:
                    return ReadVector3f();
                case ObjType.otherType:
                    return new BinaryFormatter().Deserialize(BaseStream);
                default:
                    return null;
            }
        }
    } // SerializationReader

    #endregion
}
