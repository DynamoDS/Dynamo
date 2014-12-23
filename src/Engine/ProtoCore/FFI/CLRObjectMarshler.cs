using System;
using System.Collections;
using System.Collections.Generic;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using System.Reflection;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;
using System.Xml.Serialization;
using System.Text;
using System.IO;
using System.Xml;
using System.Linq;

namespace ProtoFFI
{
    abstract class PrimitiveMarshler : FFIObjectMarshler
    {
        private readonly ProtoCore.Type mType;
        public PrimitiveMarshler(ProtoCore.Type type)
        {
            mType = type;
        }

        public override void OnDispose(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi)
        {
            throw new NotImplementedException();
        }

        public override string GetStringValue(StackValue dsObject)
        {
            throw new NotImplementedException();
        }

        public override ProtoCore.Type GetMarshaledType(Type type)
        {
            return mType;
        }

        public static ProtoCore.Type CreateType(ProtoCore.PrimitiveType type)
        {
            ProtoCore.Type protoType = new ProtoCore.Type { rank = 0, UID = (int)type };
            switch (type)
            {
                case ProtoCore.PrimitiveType.kTypeDouble:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Double;
                    break;
                case ProtoCore.PrimitiveType.kTypeInt:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Int;
                    break;
                case ProtoCore.PrimitiveType.kTypeBool:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Bool;
                    break;
                case ProtoCore.PrimitiveType.kTypeChar:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Char;
                    break;
                case ProtoCore.PrimitiveType.kTypeString:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.String;
                    break;
                case ProtoCore.PrimitiveType.kTypePointer:
                case ProtoCore.PrimitiveType.kTypeVar:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Var;
                    break;
                case ProtoCore.PrimitiveType.kTypeVoid:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Void;
                    break;
                case ProtoCore.PrimitiveType.kTypeNull:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Null;
                    break;
                default:
                    throw new NotSupportedException(string.Format("Primitive type {0} is not supported for marshaling.", type));
            }

            return protoType;
        }

        public static bool IsPrimitiveRange(ProtoCore.PrimitiveType type)
        {
            return type > ProtoCore.PrimitiveType.kTypeNull && type < ProtoCore.PrimitiveType.kTypeVar;
        }

        public static bool IsPrimitiveDSType(ProtoCore.Type type)
        {
            if (IsPrimitiveRange((ProtoCore.PrimitiveType)type.UID) && type.IsIndexable == false)
                return true;
            else if (type.UID == (int)ProtoCore.PrimitiveType.kTypeChar && type.rank == 1)
                return true;

            return false;
        }

        public static bool IsPrimitiveObjectType(object obj, ProtoCore.Type type)
        {
            return (obj.GetType().IsValueType || obj.GetType() == typeof(String)) && type.UID == (int)ProtoCore.PrimitiveType.kTypeVar;
        }
    }

    /// <summary>
    /// Marshales integer based primitive types.
    /// </summary>
    class IntMarshaler : PrimitiveMarshler
    {
        public long MaxValue { get; private set; }
        public long MinValue { get; private set; }
        public Func<long, object> CastToObject { get; private set; }
        public Func<object, long> CastToLong { get; private set; }

        private static readonly ProtoCore.Type kType = CreateType(ProtoCore.PrimitiveType.kTypeInt);

        public IntMarshaler(long maxValue, long minValue, Func<long, object> castOperator)
            : base(kType)
        {
            MaxValue = maxValue;
            MinValue = minValue;
            CastToObject = castOperator;
        }

        public override StackValue Marshal(object obj, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type type)
        {
            return StackValue.BuildInt(System.Convert.ToInt64(obj));
        }

        public override object UnMarshal(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, Type type)
        {
            if (dsObject.opdata > MaxValue || dsObject.opdata < MinValue)
            {
                string message = String.Format(ProtoCore.StringConstants.kFFIInvalidCast, dsObject.opdata, type.Name, MinValue, MaxValue);
                dsi.LogWarning(ProtoCore.RuntimeData.WarningID.kTypeMismatch, message);
            }

            return CastToObject(dsObject.opdata);
        }
    }

    /// <summary>
    /// Marshales floating point primitive types.
    /// </summary>
    class FloatMarshaler : PrimitiveMarshler
    {
        public double MaxValue { get; private set; }
        public double MinValue { get; private set; }
        public Func<double, object> CastToDouble { get; private set; }
        private static readonly ProtoCore.Type kType = CreateType(ProtoCore.PrimitiveType.kTypeDouble);

        public FloatMarshaler(double maxValue, double minValue, Func<double, object> castOperator)
            : base(kType)
        {
            MaxValue = maxValue;
            MinValue = minValue;
            CastToDouble = castOperator;
        }

        public override StackValue Marshal(object obj, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type type)
        {
            return StackValue.BuildDouble(System.Convert.ToDouble(obj));
        }

        public override object UnMarshal(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, Type type)
        {
            if (dsObject.RawDoubleValue > MaxValue || dsObject.RawDoubleValue < MinValue)
            {
                string message = String.Format(ProtoCore.StringConstants.kFFIInvalidCast, dsObject.RawDoubleValue, type.Name, MinValue, MaxValue);
                dsi.LogWarning(ProtoCore.RuntimeData.WarningID.kTypeMismatch, message);
            }

            return CastToDouble(dsObject.RawDoubleValue);
        }
    }

    /// <summary>
    /// Marshales boolean
    /// </summary>
    class BoolMarshaler : PrimitiveMarshler
    {
        private static readonly ProtoCore.Type kType = CreateType(ProtoCore.PrimitiveType.kTypeBool);
        public BoolMarshaler() : base(kType) { }

        public override StackValue Marshal(object obj, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type type)
        {
            return StackValue.BuildBoolean((bool)obj);
        }

        public override object UnMarshal(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, Type type)
        {
            return dsObject.opdata == 0 ? false : true;
        }
    }

    /// <summary>
    /// Marshales char
    /// </summary>
    class CharMarshaler : PrimitiveMarshler
    {
        private static readonly ProtoCore.Type kType = CreateType(ProtoCore.PrimitiveType.kTypeChar);
        public CharMarshaler() : base(kType) { }

        public override StackValue Marshal(object obj, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type type)
        {
            return StackValue.BuildChar((char)obj);
        }

        public override object UnMarshal(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, Type type)
        {
            return ProtoCore.Utils.EncodingUtils.ConvertInt64ToCharacter(dsObject.opdata);
        }
    }

    /// <summary>
    /// Marshals collection
    /// </summary>
    class CollectionMarshaler : PrimitiveMarshler
    {
        private FFIObjectMarshler primitiveMarshaler;

        /// <summary>
        /// Constructor for the CollectionMarshaler
        /// </summary>
        /// <param name="primitiveMarshaler">Marshaler to marshal primitive type</param>
        /// <param name="type">Expected DS type for marshaling</param>
        public CollectionMarshaler(FFIObjectMarshler primitiveMarshaler, ProtoCore.Type type)
            : base(type)
        {
            this.primitiveMarshaler = primitiveMarshaler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="context"></param>
        /// <param name="dsi"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override StackValue Marshal(object obj, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type type)
        {
            IEnumerable collection = obj as IEnumerable;
            Validity.Assert(null != collection, "Expected IEnumerable object for marshaling as collection");
            if (null == collection) 
                return StackValue.Null;

            if (collection is IDictionary)
                return ToDSArray(collection as IDictionary, context, dsi, type);
            if (collection is ICollection)
                return ToDSArray(collection as ICollection, context, dsi, type);

            return ToDSArray(collection, context, dsi, type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsObject"></param>
        /// <param name="context"></param>
        /// <param name="dsi"></param>
        /// <param name="expectedCLRType"></param>
        /// <returns></returns>
        public override object UnMarshal(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, Type expectedCLRType)
        {
            Type arrayType = expectedCLRType;
            Type elementType = GetElementType(expectedCLRType);
            if (expectedCLRType.IsGenericType)
            {
                elementType = expectedCLRType.GetGenericArguments()[0];
                arrayType = elementType.MakeArrayType();
            }

            ICollection collection = null;
            //If dsObject is non array pointer but the expectedCLRType is IEnumerable, promote the dsObject to a collection.
            if (!dsObject.IsArray)
            {
                Validity.Assert(typeof(IEnumerable).IsAssignableFrom(expectedCLRType));
                object obj = primitiveMarshaler.UnMarshal(dsObject, context, dsi, elementType);
                collection = new ArrayList(new object[] { obj });
            }
            else //Convert DS Array to CS Collection
                collection = ToICollection(dsObject, context, dsi, arrayType);

            if (expectedCLRType.IsGenericType && !expectedCLRType.IsInterface)
            {
                if (!collection.GetType().IsArray)
                {
                    Validity.Assert(collection is ArrayList);
                    collection = (collection as ArrayList).ToArray(elementType);
                }
                return Activator.CreateInstance(expectedCLRType, new[] { collection });
            }

            if (expectedCLRType.IsArray || expectedCLRType.IsGenericType)
            {
                ArrayList list = collection as ArrayList;
                if (null != list)
                    return list.ToArray(elementType);
            }

            return collection;
        }

        private static Type GetElementType(Type expectedCLRType)
        {
            var elementType = expectedCLRType.GetElementType();
            if (elementType == null)
                elementType = typeof(object);
            return elementType;
        }

        #region CS_ARRAY_TO_DS_ARRAY

        private ProtoCore.Type GetApproxDSType(object obj)
        {
            if (null == obj)
                return CreateType(ProtoCore.PrimitiveType.kTypeNull);

            Type type = obj.GetType();
            if(type == typeof(string))
                return StringMarshaler.kType;
            ProtoCore.Type dsType;
            if (CLRModuleType.TryGetImportedDSType(type, out dsType))
                return dsType;
            if (typeof(IEnumerable).IsAssignableFrom(type)) //It's a collection
            {
                dsType = CreateType(ProtoCore.PrimitiveType.kTypeVar);
                dsType.rank = ProtoCore.DSASM.Constants.kArbitraryRank;
                return dsType;
            }

            //Else return var type
            return CreateType(ProtoCore.PrimitiveType.kTypeVar); 
        }

        /// <summary>
        /// Marshal an object to a StackValue
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="context"></param>
        /// <param name="dsi"></param>
        /// <returns></returns>
        protected StackValue MarshalToStackValue(object obj, ProtoCore.Runtime.Context context, Interpreter dsi)
        {
            if (obj is StackValue)
            {
                return (StackValue)obj;
            }
            else
            {
                ProtoCore.Type dsType = GetApproxDSType(obj);
                return primitiveMarshaler.Marshal(obj, context, dsi, dsType);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="context"></param>
        /// <param name="dsi"></param>
        /// <param name="expectedDSType"></param>
        /// <returns></returns>
        protected StackValue ToDSArray(ICollection collection, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type expectedDSType)
        {
            StackValue[] sv = new StackValue[collection.Count];
            int index = 0;

            foreach (var item in collection)
            {
                sv[index] = MarshalToStackValue(item, context, dsi);
                dsi.runtime.rmem.Heap.IncRefCount(sv[index]);
                ++index;
            }

            var retVal = dsi.runtime.rmem.Heap.AllocateArray(sv);
            return retVal;
        }

        protected StackValue ToDSArray(IEnumerable enumerable, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type expectedDSType)
        {
            List<StackValue> svs = new List<StackValue>();

            foreach (var item in enumerable)
            {
                svs.Add(MarshalToStackValue(item, context, dsi));
            }

            var heap = dsi.runtime.rmem.Heap;
            svs.ForEach(sv => heap.IncRefCount(sv));
            var retVal = heap.AllocateArray(svs);
            return retVal;
        }

        protected StackValue ToDSArray(IDictionary dictionary, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type expectedDSType)
        {
            var core = dsi.runtime.Core;

            var array = dsi.runtime.rmem.Heap.AllocateArray(Enumerable.Empty<StackValue>());
            HeapElement ho = ArrayUtils.GetHeapElement(array, core);
            ho.Dict = new Dictionary<StackValue, StackValue>(new StackValueComparer(core));

            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];

                StackValue dsKey = MarshalToStackValue(key, context, dsi);
                GCUtils.GCRetain(dsKey, core);

                StackValue dsValue = MarshalToStackValue(value, context, dsi);
                GCUtils.GCRetain(dsValue, core);

                ho.Dict[dsKey] = dsValue;
            }

            return array;
        }

        #endregion

        #region DS_ARRAY_TO_CS_ARRAY
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dsObject"></param>
        /// <param name="context"></param>
        /// <param name="dsi"></param>
        /// <returns></returns>
        protected T[] UnMarshal<T>(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi)
        {
            var dsElements = ArrayUtils.GetValues(dsObject, dsi.runtime.Core);
            var result = new List<T>();
            Type objType = typeof(T);

            foreach (var elem in dsElements)
            {
                object obj = primitiveMarshaler.UnMarshal(elem, context, dsi, objType);
                if (null == obj)
                {
                    if (objType.IsValueType)
                        throw new System.InvalidCastException(
                            string.Format("Null value cannot be cast to {0}", objType.Name));

                    result.Add(default(T));
                }
                else
                {
                    result.Add((T)obj);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsObject"></param>
        /// <param name="context"></param>
        /// <param name="dsi"></param>
        /// <param name="arrayType"></param>
        /// <returns></returns>
        private ICollection ToICollection(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, System.Type arrayType)
        {
            if (arrayType.IsArray)
            {
                //  processing only for the primitive types
                //  anything else will be dealt with as it was earlier
                //
                if (arrayType.UnderlyingSystemType == typeof(int[]))
                {
                    return UnMarshal<int>(dsObject, context, dsi);
                }
                else if (arrayType.UnderlyingSystemType == typeof(double[]))
                {
                    return UnMarshal<double>(dsObject, context, dsi);
                }
                else if (arrayType.UnderlyingSystemType == typeof(bool[]))
                {
                    return UnMarshal<bool>(dsObject, context, dsi);
                }
            }

            HeapElement hs = dsi.runtime.rmem.Heap.GetHeapElement(dsObject);

            //  use arraylist instead of object[], this allows us to correctly capture 
            //  the type of objects being passed
            //
            ArrayList arrList = new ArrayList();
            var elementType = arrayType.GetElementType();
            if (elementType == null)
                elementType = typeof(object);
            foreach (var sv in hs.VisibleItems)
            {
                object obj = primitiveMarshaler.UnMarshal(sv, context, dsi, elementType);
                arrList.Add(obj);
            }

            return arrList;

        }
        #endregion
    }

    /// <summary>
    /// Marshales string as array of chars
    /// </summary>
    class StringMarshaler : CollectionMarshaler
    {
        public static readonly ProtoCore.Type kType = CreateType(ProtoCore.PrimitiveType.kTypeString);
        private static readonly CharMarshaler kCharMarshaler = new CharMarshaler();

        public StringMarshaler() : base(kCharMarshaler, kType) { }

        public override StackValue Marshal(object obj, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type type)
        {
            string str = (string)obj;
            StackValue dsarray = base.Marshal(str.ToCharArray(), context, dsi, type);
            return StackValue.BuildString(dsarray.opdata);
        }

        public override object UnMarshal(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, Type type)
        {
            char[] array = UnMarshal<char>(dsObject, context, dsi);
            return new string(array);
        }
    }

    /// <summary>
    /// This class marshales CLR Objects to DS Object and vice-versa.
    /// </summary>
    class CLRObjectMarshler : FFIObjectMarshler
    {
        private static readonly Dictionary<Type, FFIObjectMarshler> mPrimitiveMarshalers;
        static CLRObjectMarshler()
        {
            mPrimitiveMarshalers = new Dictionary<Type, FFIObjectMarshler>();
            mPrimitiveMarshalers.Add(typeof(Int16), new IntMarshaler(Int16.MaxValue, Int16.MinValue, (long value) => (Int16)value));
            mPrimitiveMarshalers.Add(typeof(Int32), new IntMarshaler(Int32.MaxValue, Int32.MinValue, (long value) => (Int32)value));
            mPrimitiveMarshalers.Add(typeof(Int64), new IntMarshaler(Int64.MaxValue, Int64.MinValue, (long value) => value));
            mPrimitiveMarshalers.Add(typeof(UInt16), new IntMarshaler(UInt16.MaxValue, UInt16.MinValue, (long value) => (UInt16)value));
            mPrimitiveMarshalers.Add(typeof(UInt32), new IntMarshaler(UInt32.MaxValue, UInt32.MinValue, (long value) => (UInt32)value));
            mPrimitiveMarshalers.Add(typeof(UInt64), new IntMarshaler(Int64.MaxValue, Int64.MinValue, (long value) => (UInt64)value));
            mPrimitiveMarshalers.Add(typeof(byte), new IntMarshaler(byte.MaxValue, byte.MinValue, (long value) => (byte)value));
            mPrimitiveMarshalers.Add(typeof(sbyte), new IntMarshaler(sbyte.MaxValue, sbyte.MinValue, (long value) => (sbyte)value));
            mPrimitiveMarshalers.Add(typeof(double), new FloatMarshaler(double.MaxValue, double.MinValue, (double value) => value));
            mPrimitiveMarshalers.Add(typeof(float), new FloatMarshaler(float.MaxValue, float.MinValue, (double value) => (float)value));
            mPrimitiveMarshalers.Add(typeof(bool), new BoolMarshaler());
            mPrimitiveMarshalers.Add(typeof(char), new CharMarshaler());
            mPrimitiveMarshalers.Add(typeof(string), new StringMarshaler());
        }

        /// <summary>
        /// Gets instance of the CLRObjectMarshler for a given core. If marshler
        /// is not already created, it creates a new one.
        /// </summary>
        /// <param name="core">Core object.</param>
        /// <returns>CLRObjectMarshler</returns>
        public static CLRObjectMarshler GetInstance(ProtoCore.Core core)
        {
            CLRObjectMarshler marshaller = null;
            if (!mObjectMarshlers.TryGetValue(core, out marshaller))
            {
                IDisposable[] disposables = null;
                lock (syncroot)
                {
                    if (mObjectMarshlers.TryGetValue(core, out marshaller))
                        return marshaller;

                    marshaller = new CLRObjectMarshler(core);

                    object value;
                    if (core.Configurations.TryGetValue(ConfigurationKeys.GeometryXmlProperties, out value))
                        marshaller.DumpXmlProperties = value == null ? false : (bool)value;
                    else
                        marshaller.DumpXmlProperties = false;
                    mObjectMarshlers[core] = marshaller;

                    disposables = mPendingDisposables.ToArray();
                    mPendingDisposables.Clear();
                }
                if (null != disposables)
                {
                    //Dispose pending disposals
                    foreach (IDisposable obj in disposables)
                    {
                        if (null != obj)
                        {
                            obj.Dispose();
                        }
                    }
                }
            }
            return marshaller;
        }

        /// <summary>
        /// Marshals the given CLR object to expectedDSType StackValue
        /// </summary>
        /// <param name="obj">Input object for marshaling</param>
        /// <param name="context">Runtime context, not being used</param>
        /// <param name="dsi">Runtime Interpreter</param>
        /// <param name="expectedDSType">Expected ProtoCore.Type to marshal as</param>
        /// <returns>StackValue</returns>
        public override StackValue Marshal(object obj, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type expectedDSType)
        {
            //1. Null object is marshaled as null
            if (obj == null)
                return StackValue.Null;

            //2. Get appropriate marshaler for the expectedDSType and objType
            Type objType = obj.GetType();
            FFIObjectMarshler marshaler = GetMarshalerForDsType(expectedDSType, objType);

            //3. Got a marshaler, now marshal it.
            if (null != marshaler)
                return marshaler.Marshal(obj, context, dsi, expectedDSType);

            //4. Didn't get the marshaler, could be a pointer or var type, check from map
            StackValue retVal;
            if (CLRObjectMap.TryGetValue(obj, out retVal))
                return retVal;

            //5. If it is a StackValue, simply return it.
            if (obj is StackValue)
            {
                return (StackValue)obj;
            }

            //6. Seems like a new object create a new DS object and bind it.
            return CreateDSObject(obj, context, dsi);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsObject"></param>
        /// <param name="context"></param>
        /// <param name="dsi"></param>
        /// <param name="expectedCLRType"></param>
        /// <returns></returns>
        public override object UnMarshal(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, System.Type expectedCLRType)
        {
            if (dsObject.IsNull)
                return null;

            //Get the correct marshaler to unmarshal
            FFIObjectMarshler marshaler = GetMarshalerForCLRType(expectedCLRType, dsObject.optype);
            if (null != marshaler)
                return marshaler.UnMarshal(dsObject, context, dsi, expectedCLRType);

            //The dsObject must be of pointer type
            Validity.Assert(dsObject.IsPointer || dsObject.IsFunctionPointer, 
                string.Format("Operand type {0} not supported for marshalling", 
                dsObject.optype));
            
            //Search in the DSObjectMap, for corresponding clrObject.
            object clrObject = null;
            if (DSObjectMap.TryGetValue(dsObject, out clrObject))
                return clrObject;

            if (dsObject.IsFunctionPointer)
            {
                return dsObject;
            }

            return CreateCLRObject(dsObject, context, dsi, expectedCLRType);
        }

        /// <summary>
        /// Gets marshaler for the given clrType and if it fails
        /// to get one, it tries to get primitive marshaler based on dsType.
        /// We want to get correct marshaler specific to the input type because
        /// more than one type gets map to same type in DS.
        /// </summary>
        /// <param name="clrType">System.Type to which DS object needs to be 
        /// marshaled.</param>
        /// <param name="dsType">DS Object type, that needs to be marshaled.
        /// </param>
        /// <returns>FFIObjectMarshler or null</returns>
        private FFIObjectMarshler GetMarshalerForCLRType(Type clrType, AddressType dsType)
        {
            //If the input ds object is pointer type then it can't be marshaled as primitive.
            if (dsType == AddressType.Pointer)
                return null;

            FFIObjectMarshler marshaler = null;
            //Expected CLR type is object, get marshaled clrType from dsType
            Type expectedType = clrType;
            if (expectedType == typeof(object))
                expectedType = GetPrimitiveType(dsType);
            else if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>))
                expectedType = Nullable.GetUnderlyingType(clrType);

            //If Ds Type is array pointer, it needs to be marshaled as collection.
            bool collection = (dsType == AddressType.ArrayPointer);

            //Expected CLR type is not string, but is derived from IEnumerable
            if (typeof(string) != expectedType && typeof(IEnumerable).IsAssignableFrom(expectedType))
                collection = true;

            if (collection)
            {
                ProtoCore.Type type = PrimitiveMarshler.CreateType(ProtoCore.PrimitiveType.kTypeVar);
                type.rank = ProtoCore.DSASM.Constants.kArbitraryRank;
                return new CollectionMarshaler(this, type);
            }

            if (!mPrimitiveMarshalers.TryGetValue(expectedType, out marshaler))
                mPrimitiveMarshalers.TryGetValue(GetPrimitiveType(dsType), out marshaler);

            return marshaler;
        }

        /// <summary>
        /// Get appropriate marshaler for given DS Type.
        /// </summary>
        /// <param name="dsType">DS Type to which given objType needs to be marshaled.</param>
        /// <param name="objType">CLR object type that needs to marshal.</param>
        /// <returns>FFIObjectMarshler or null</returns>
        private FFIObjectMarshler GetMarshalerForDsType(ProtoCore.Type dsType, Type objType)
        {
            //Expected DS Type is pointer, so there is no primitive marshaler available.
            if (!dsType.IsIndexable && dsType.UID == (int)ProtoCore.PrimitiveType.kTypePointer)
                return null;

            FFIObjectMarshler marshaler = null;
            bool marshalAsCollection = false;
            //0. String needs special handling becuase it's derived from IEnumerable.
            if (typeof(string) == objType)
                marshalAsCollection = false;
            //1. If expectedDSType is fixed rank collection, objType must be a collection of same rank
            else if (dsType.rank > 0 && typeof(IEnumerable).IsAssignableFrom(objType))
                marshalAsCollection = true;
            //2. If dsType is arbitrary rank collection, marshal based on objType
            //3. If dsType is var, marshal based on objType.
            else if ((dsType.rank == ProtoCore.DSASM.Constants.kArbitraryRank ||
                dsType.UID == (int)ProtoCore.PrimitiveType.kTypeVar) && typeof(IEnumerable).IsAssignableFrom(objType))
                marshalAsCollection = true;

            //4. Else get primitive marshaler for given objType
            if (marshalAsCollection)
                marshaler = new CollectionMarshaler(this, dsType);
            else if(dsType.UID != (int)ProtoCore.PrimitiveType.kTypePointer) //Not exported as pointer type
                mPrimitiveMarshalers.TryGetValue(objType, out marshaler);

            return marshaler;
        }

        /// <summary>
        /// Gets a primitive System.Type for the given DS type.
        /// </summary>
        /// <param name="addressType">DS AddressType</param>
        /// <returns>System.Type</returns>
        private Type GetPrimitiveType(AddressType addressType)
        {
            switch (addressType)
            {
                case AddressType.Int:
                    return typeof(int);
                case AddressType.Double:
                    return typeof(double);
                case AddressType.Boolean:
                    return typeof(bool);
                case AddressType.Char:
                    return typeof(char);
                case AddressType.String:
                    return typeof(string);
                default:
                    return typeof(object);
            }
        }

        /// <summary>
        /// Gets marshaled DS type for the given System.Type
        /// </summary>
        /// <param name="type">System.Type</param>
        /// <returns>ProtoCore.Type as equivalent DS type for input System.Type</returns>
        public override ProtoCore.Type GetMarshaledType(Type type)
        {
            return CLRObjectMarshler.GetProtoCoreType(type);
        }

        /// <summary>
        /// Gets equivalent DS type for the input System.Type
        /// </summary>
        /// <param name="type">System.Type</param>
        /// <returns>ProtoCore.Type</returns>
        public static ProtoCore.Type GetProtoCoreType(Type type)
        {
            ProtoCore.Type retype = PrimitiveMarshler.CreateType(ProtoCore.PrimitiveType.kTypeVar);
            ComputeDSType(type, ref retype);
            return retype;
        }

        /// <summary>
        /// Gets the marshaled type for input System.Type as DS Pointer type
        /// </summary>
        /// <param name="type">System.Type</param>
        /// <returns>ProtoCore.Type</returns>
        public static ProtoCore.Type GetUserDefinedType(Type type)
        {
            ProtoCore.Type retype = PrimitiveMarshler.CreateType(ProtoCore.PrimitiveType.kTypePointer);
            ComputeDSType(type, ref retype);
            return retype;
        }

        /// <summary>
        /// Computes an equivalent ProtoCore.Type for a given System.Type 
        /// recursively.
        /// </summary>
        /// <param name="type">System.Type</param>
        /// <param name="protoCoreType">ref ProtoCore.Type</param>
        private static void ComputeDSType(Type type, ref ProtoCore.Type protoCoreType)
        {
            FFIObjectMarshler marshaler;
            Type arrayType = ComputeArrayType(type);
            if (arrayType != null)
            {
                Type elemType = arrayType.GetElementType();
                //Get ProtoCoreType by importing elemType properly.
                protoCoreType = CLRModuleType.GetProtoCoreType(elemType, null);

                if (protoCoreType.rank != Constants.kArbitraryRank)
                {
                    protoCoreType.rank += arrayType.GetArrayRank(); //set the rank.
                }
            }
            else if (typeof(System.Collections.IDictionary).IsAssignableFrom(type))
            {
                protoCoreType.rank = ProtoCore.DSASM.Constants.kArbitraryRank;
            }
            else if (type.IsInterface && typeof(IEnumerable).IsAssignableFrom(type))
            {
                protoCoreType.rank = ProtoCore.DSASM.Constants.kArbitraryRank;
            }
            else if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
            {
                Type[] args = type.GetGenericArguments();
                int nArgs = args.Length;
                if (nArgs != 1)
                {
                    protoCoreType.Name = GetTypeName(type);
                    protoCoreType.UID = (int)ProtoCore.PrimitiveType.kTypePointer;
                    return;
                }
                Type elemType = args[0];
                //TODO: Ideally we shouldn't be calling this method on CLRModuleType,
                //but we want to import this elemType, hence we do this.
                protoCoreType = CLRModuleType.GetProtoCoreType(elemType, null);
                if (protoCoreType.rank != Constants.kArbitraryRank)
                {
                    protoCoreType.rank += 1;
                }
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                protoCoreType = CLRModuleType.GetProtoCoreType(Nullable.GetUnderlyingType(type), null);
            }
            else if (type == typeof(object))
            {
                protoCoreType = PrimitiveMarshler.CreateType(ProtoCore.PrimitiveType.kTypeVar);
                protoCoreType.rank = 0; //Initially setup zero rank
            }
            else if (type == typeof(void))
                protoCoreType = PrimitiveMarshler.CreateType(ProtoCore.PrimitiveType.kTypeVoid);
            else if (protoCoreType.UID == (int)ProtoCore.PrimitiveType.kTypePointer)
                protoCoreType.Name = GetTypeName(type);
            else if (mPrimitiveMarshalers.TryGetValue(type, out marshaler))
                protoCoreType = marshaler.GetMarshaledType(type);
            else
            {
                protoCoreType.Name = GetTypeName(type);
                protoCoreType.UID = (int)ProtoCore.PrimitiveType.kTypePointer;
            }
        }

        /// <summary>
        /// Tries to compute an array type from a given system type if it was 
        /// IEnumerable derived and a generic type.
        /// </summary>
        /// <param name="collectionType">Input type</param>
        /// <returns>An equivalent array type or null</returns>
        private static Type ComputeArrayType(Type collectionType)
        {
            if (collectionType.IsArray)
                return collectionType; //already an array type
            if (typeof(IEnumerable).IsAssignableFrom(collectionType) && collectionType.IsGenericType)
            {
                Type[] args = collectionType.GetGenericArguments();
                if (args == null || args.Length != 1)
                    return null;
                return args[0].MakeArrayType();
            }
            return null; //Can't be converted to array type
        }

        /// <summary>
        /// Checks if the given System.Type is marshaled as native type in DS.
        /// </summary>
        /// <param name="type">System.Type</param>
        /// <returns>True if marshaled as native DS type</returns>
        public static bool IsMarshaledAsNativeType(Type type)
        {
            if (type.IsPrimitive || type.IsArray || typeof(System.Collections.IDictionary).IsAssignableFrom(type) || type == typeof(string) || type == typeof(object) || type == typeof(void))
                return true;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return true;
            if (type.IsInterface && typeof(IEnumerable).IsAssignableFrom(type))
                return true;
            if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
            {
                Type[] args = type.GetGenericArguments();
                return args != null && args.Length == 1;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsObject"></param>
        /// <param name="context"></param>
        /// <param name="dsi"></param>
        public override void OnDispose(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi)
        {
            lock (DSObjectMap)
            {
                Object clrobject;
                if (DSObjectMap.TryGetValue(dsObject, out clrobject))
                {
                    DSObjectMap.Remove(dsObject);
                    CLRObjectMap.Remove(clrobject);
                    dsi.runtime.Core.FFIPropertyChangedMonitor.RemoveFFIObject(clrobject);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTypeName(Type type)
        {
            return GetTypeName(type, true);
        }

        private static string GetTypeName(Type type, bool fullName)
        {
            if (!type.IsGenericType)
            {
                if (type.IsNestedPublic)
                    return string.Format("{0}_{1}", type.DeclaringType.FullName, type.Name);

                return fullName ? type.FullName : type.Name;
            }

            string name = fullName ? type.GetGenericTypeDefinition().FullName : type.GetGenericTypeDefinition().Name;
            int trim = name.IndexOf('`');
            if (trim > 0)
                name = name.Substring(0, trim);
            Type[] args = type.GetGenericArguments();
            for (int i = 0; i < args.Length; ++i)
            {
                string prefix = (i == 0) ? "Of" : "And";
                name = name + prefix + GetTypeName(args[i], false);
            }

            return name;
        }

        //recursively gets a public type from the given type
        public static Type GetPublicType(Type type)
        {
            if (type.IsPublic || type.IsNestedPublic)
                return type;

            return GetPublicType(type.BaseType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="context"></param>
        /// <param name="dsi"></param>
        /// <returns></returns>
        private StackValue CreateDSObject(object obj, ProtoCore.Runtime.Context context, Interpreter dsi)
        {
            //We are here, because we want to create DS object of user defined type.
            var core = dsi.runtime.Core;
            var classTable = core.DSExecutable.classTable;
            Type objType = GetPublicType(obj.GetType());
            int type = classTable.IndexOf(GetTypeName(objType));
            //Recursively get the base class type if available.
            while (type == -1 && objType != null)
            {
                objType = objType.BaseType;
                if (null != objType)
                    type = classTable.IndexOf(GetTypeName(objType));
            }

            MetaData metadata;
            metadata.type = type;
            StackValue retval = core.Heap.AllocatePointer(classTable.ClassNodes[type].size, metadata);
            BindObjects(obj, retval);
            dsi.runtime.Core.FFIPropertyChangedMonitor.AddFFIObject(obj);
            return retval;
        }

        /// <summary>
        /// Initializes primary properties on the DS object for given FFI
        /// object.
        /// </summary>
        /// <param name="ffiObject">FFI object in context</param>
        /// <param name="dsObject">Design script object</param>
        /// <param name="context">Execution context</param>
        /// <param name="dsi">Interpreter</param>
        /// <param name="classIndex">Class index of design script data type</param>
        private void PopulatePrimaryProperties(object ffiObject, StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, int classIndex)
        {
            Dictionary<string, object> properties = GetPrimaryProperties(ffiObject);
            if (null == properties || properties.Count == 0)
                return;

            var core = dsi.runtime.Core;
            StackValue[] svs = core.Heap.GetHeapElement(dsObject).Stack;
            for (int ix = 0; ix < svs.Length; ++ix)
            {
                SymbolNode symbol = core.ClassTable.ClassNodes[classIndex].symbols.symbolList[ix];
                object prop = null;
                if (properties.TryGetValue(symbol.name, out prop) && null != prop)
                    svs[ix] = Marshal(prop, context, dsi, GetMarshaledType(prop.GetType()));
            }
        }

        /// <summary>
        /// Get all the properties of input object, that are marked with 
        /// "Primary" Category.
        /// </summary>
        /// <param name="obj">Input FFI object</param>
        /// <returns>Map of properties and its values</returns>
        private Dictionary<string, object> GetPrimaryProperties(object obj)
        {
            Type objType = obj.GetType();
            PropertyInfo[] properties = objType.GetProperties();
            Dictionary<string, object> propertyBag = new Dictionary<string, object>();
            foreach (var item in properties)
            {
                if ("Primary" == GetCategory(item))
                {
                    try
                    {
                        propertyBag[item.Name] = item.GetValue(obj, null);
                    }
                    catch
                    {
                    }
                }
            }

            return propertyBag;
        }

        /// <summary>
        /// Looks for CategoryAttribute on the given member and returns
        /// Category value.
        /// </summary>
        /// <param name="member">MemberInfo for querying attribute</param>
        /// <returns>Category name for the member if any, else empty string</returns>
        public static string GetCategory(MemberInfo member)
        {
            object[] atts = member.GetCustomAttributes(false);
            foreach (var item in atts)
            {
                CategoryAttribute category = item as CategoryAttribute;
                if (null != category)
                    return category.Category;
            }

            return string.Empty;
        }

        private void GeneratePrimaryPropertiesAsXml(object obj, XmlWriter xw)
        {
            Dictionary<string, object> properties = GetPrimaryProperties(obj);
            string elementName = obj.GetType().ToString();
            elementName = XmlConvert.EncodeName(elementName);
            xw.WriteStartElement(elementName);
            foreach (var item in properties)
            {
                if (null == item.Value)
                {
                    xw.WriteElementString(item.Key, "null");
                }
                else if (item.Value.GetType().IsPrimitive)
                {
                    xw.WriteElementString(item.Key, item.Value.ToString());
                }
                else
                {
                    xw.WriteStartElement(item.Key);
                    GeneratePrimaryPropertiesAsXml(item.Value, xw);
                    xw.WriteEndElement();
                }
            }
            xw.WriteEndElement();
        }

        /// <summary>
        /// Controls whether GetStringValue() to dump Xml properties or not
        /// </summary>
        private bool DumpXmlProperties
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsObject"></param>
        /// <returns></returns>
        public override string GetStringValue(StackValue dsObject)
        {
            object clrObject = null;
            if (!DSObjectMap.TryGetValue(dsObject, out clrObject))
            {
                return string.Empty;
            }

            if (clrObject != null)
            {
                if (!DumpXmlProperties)
                {
                    return clrObject.ToString();
                }
                else
                {
                    XmlWriterSettings settings = new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true };
                    using (StringWriter sw = new StringWriter())
                    {
                        using (XmlWriter xw = XmlTextWriter.Create(sw, settings))
                        {
                            GeneratePrimaryPropertiesAsXml(clrObject, xw);
                        }
                        return sw.ToString();
                    }
                }
            }
            else
            {
                return string.Empty;
            }
        }

        #region PRIVATE_SPACE

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="dsobj"></param>
        private void BindObjects(object obj, StackValue dsobj)
        {
#if DEBUG
            if (DSObjectMap.ContainsKey(dsobj))
                throw new InvalidOperationException("Object reference already mapped");

            if (CLRObjectMap.ContainsKey(obj))
                throw new InvalidOperationException("Object reference already mapped");
#endif
            lock (DSObjectMap)
            {
                DSObjectMap[dsobj] = obj;
                CLRObjectMap[obj] = dsobj;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dsObject"></param>
        /// <param name="context"></param>
        /// <param name="dsi"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private object CreateCLRObject(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, System.Type type)
        {
            //Must be a user defined type, and expecting a var object
            if (type == typeof(object) && dsObject.IsPointer)
            {
                //TOD: Fix GC issue, don't know how/when this will get GCed??
                dsi.runtime.rmem.Heap.IncRefCount(dsObject);
                BindObjects(dsObject, dsObject);
                return dsObject;
            }

            throw new InvalidOperationException("Unable to locate managed object for given dsObject.");
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Core object for marshler.</param>
        private CLRObjectMarshler(ProtoCore.Core core)
        {
            core.Dispose += core_Dispose;
        }

        /// <summary>
        /// Dispose event handler.
        /// </summary>
        /// <param name="sender">Core object being disposed.</param>
        void core_Dispose(ProtoCore.Core sender)
        {
            CLRObjectMarshler marshaller = null;
            if (!mObjectMarshlers.TryGetValue(sender, out marshaller))
                throw new KeyNotFoundException();

            mObjectMarshlers.Remove(sender);

            //Detach from core.
            sender.Dispose -= core_Dispose;

            //Dispose all disposable CLR objects.
            foreach (var item in DSObjectMap)
            {
                IDisposable disposable = item.Value as IDisposable;
                sender.FFIPropertyChangedMonitor.RemoveFFIObject(item.Value);

                if (null != disposable)
                    disposable.Dispose();
            }

            //Clear the maps.
            DSObjectMap.Clear();
            CLRObjectMap.Clear();
        }

        private readonly Dictionary<StackValue, Object> DSObjectMap = new Dictionary<StackValue, object>(new PointerValueComparer());
        private readonly Dictionary<Object, StackValue> CLRObjectMap = new Dictionary<object, StackValue>(new ReferenceEqualityComparer());
        private static readonly Dictionary<ProtoCore.Core, CLRObjectMarshler> mObjectMarshlers = new Dictionary<ProtoCore.Core, CLRObjectMarshler>();
        private static List<IDisposable> mPendingDisposables = new List<IDisposable>();
        private static readonly Object syncroot = new Object();
        #endregion
    }

    /// <summary>
    /// This class compares two CLR objects. It is used in CLRObjectMap to 
    /// avoid hash collision. 
    /// </summary>
    public class ReferenceEqualityComparer: IEqualityComparer<object>
    {
        public bool Equals(object x, object y)
        {
            return object.ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    /// This class compares two Pointer type StackValue objects.
    /// </summary>
    public class PointerValueComparer : IEqualityComparer<StackValue>
    {
        public bool Equals(StackValue x, StackValue y)
        {
            return x.opdata == y.opdata && x.metaData.type == y.metaData.type;
        }

        public int GetHashCode(StackValue obj)
        {
            unchecked
            {
                var hash = 0;
                hash = (hash * 397) ^ obj.opdata.GetHashCode();
                hash = (hash * 397) ^ obj.metaData.type.GetHashCode();
                return hash;
            }
        }
    }
}
