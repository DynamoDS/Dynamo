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
using DesignScript.Builtin;
using ProtoCore.Properties;
using ProtoCore.Exceptions;
using ProtoCore.Runtime;

namespace ProtoFFI
{
    abstract class PrimitiveMarshaler : FFIObjectMarshaler
    {
        private readonly ProtoCore.Type mType;
        public PrimitiveMarshaler(ProtoCore.Type type)
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
                case ProtoCore.PrimitiveType.Double:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Double;
                    break;
                case ProtoCore.PrimitiveType.Integer:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Int;
                    break;
                case ProtoCore.PrimitiveType.Bool:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Bool;
                    break;
                case ProtoCore.PrimitiveType.Char:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Char;
                    break;
                case ProtoCore.PrimitiveType.String:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.String;
                    break;
                case ProtoCore.PrimitiveType.Pointer:
                case ProtoCore.PrimitiveType.Var:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Var;
                    break;
                case ProtoCore.PrimitiveType.Void:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Void;
                    break;
                case ProtoCore.PrimitiveType.Null:
                    protoType.Name = ProtoCore.DSDefinitions.Keyword.Null;
                    break;
                default:
                    throw new NotSupportedException(string.Format("Primitive type {0} is not supported for marshaling.", type));
            }

            return protoType;
        }

        public static bool IsPrimitiveRange(ProtoCore.PrimitiveType type)
        {
            return type > ProtoCore.PrimitiveType.Null && type < ProtoCore.PrimitiveType.Var;
        }

        public static bool IsPrimitiveDSType(ProtoCore.Type type)
        {
            if (IsPrimitiveRange((ProtoCore.PrimitiveType)type.UID) && type.IsIndexable == false)
                return true;
            else if (type.UID == (int)ProtoCore.PrimitiveType.Char && type.rank == 1)
                return true;

            return false;
        }

        public static bool IsPrimitiveObjectType(object obj, ProtoCore.Type type)
        {
            return (obj.GetType().IsValueType || obj.GetType() == typeof(String)) && type.UID == (int)ProtoCore.PrimitiveType.Var;
        }
    }

    /// <summary>
    /// Marshales integer based primitive types.
    /// </summary>
    class IntMarshaler : PrimitiveMarshaler
    {
        public long MaxValue { get; private set; }
        public long MinValue { get; private set; }
        public Func<long, object> CastToObject { get; private set; }
        public Func<object, long> CastToLong { get; private set; }

        private static readonly ProtoCore.Type kType = CreateType(ProtoCore.PrimitiveType.Integer);

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
            if (dsObject.IntegerValue > MaxValue || dsObject.IntegerValue< MinValue)
            {
                string message = String.Format(Resources.kFFIInvalidCast, dsObject.IntegerValue, type.Name, MinValue, MaxValue);
                dsi.LogWarning(ProtoCore.Runtime.WarningID.TypeMismatch, message);
            }

            return CastToObject(dsObject.IntegerValue);
        }
    }

    /// <summary>
    /// Marshales floating point primitive types.
    /// </summary>
    class FloatMarshaler : PrimitiveMarshaler
    {
        public double MaxValue { get; private set; }
        public double MinValue { get; private set; }
        public Func<double, object> CastToDouble { get; private set; }
        private static readonly ProtoCore.Type kType = CreateType(ProtoCore.PrimitiveType.Double);

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
            if (dsObject.DoubleValue > MaxValue || dsObject.DoubleValue < MinValue)
            {
                string message = String.Format(Resources.kFFIInvalidCast, dsObject.DoubleValue, type.Name, MinValue, MaxValue);
                dsi.LogWarning(ProtoCore.Runtime.WarningID.TypeMismatch, message);
            }

            return CastToDouble(dsObject.DoubleValue);
        }
    }

    /// <summary>
    /// Marshales boolean
    /// </summary>
    class BoolMarshaler : PrimitiveMarshaler
    {
        private static readonly ProtoCore.Type kType = CreateType(ProtoCore.PrimitiveType.Bool);
        public BoolMarshaler() : base(kType) { }

        public override StackValue Marshal(object obj, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type type)
        {
            return StackValue.BuildBoolean((bool)obj);
        }

        public override object UnMarshal(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, Type type)
        {
            return dsObject.BooleanValue;
        }
    }

    /// <summary>
    /// Marshales char
    /// </summary>
    class CharMarshaler : PrimitiveMarshaler
    {
        private static readonly ProtoCore.Type kType = CreateType(ProtoCore.PrimitiveType.Char);
        public CharMarshaler() : base(kType) { }

        public override StackValue Marshal(object obj, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type type)
        {
            return StackValue.BuildChar((char)obj);
        }

        public override object UnMarshal(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, Type type)
        {
            return Convert.ToChar(dsObject.CharValue);
        }
    }

    class ArrayMarshaler : PrimitiveMarshaler
    {
        private readonly CLRObjectMarshaler primitiveMarshaler;

        /// <summary>
        /// Constructor for the ArrayMarshaler
        /// </summary>
        /// <param name="primitiveMarshaler">Marshaler to marshal primitive type</param>
        /// <param name="type">Expected DS type for marshaling</param>
        public ArrayMarshaler(CLRObjectMarshaler primitiveMarshaler, ProtoCore.Type type)
            : base(type)
        {
            this.primitiveMarshaler = primitiveMarshaler;
        }

        public override StackValue Marshal(object obj, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type type)
        {
            var collection = obj as IEnumerable;
            Validity.Assert(null != collection, "Expected IEnumerable object for marshaling as collection");
            if (null == collection) 
                return StackValue.Null;

            if (collection is ICollection)
                return ToDSArray(collection as ICollection, context, dsi, type);

            return ToDSArray(collection, context, dsi, type);
        }

        public override object UnMarshal(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, Type expectedCLRType)
        {
            // If expected type is an IDictionary, log warning and return
            if (expectedCLRType == typeof(IDictionary) ||
                expectedCLRType.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Select(i => i.GetGenericTypeDefinition())
                    .Contains(typeof(IDictionary<,>)))
            {
                dsi.LogWarning(WarningID.TypeMismatch, Resources.FailedToConvertArrayToDictionary);
                return null;
            }

            var arrayType = expectedCLRType;
            var elementType = expectedCLRType.GetElementType() ?? typeof(object);

            if (expectedCLRType.IsGenericType)
            {
                elementType = expectedCLRType.GetGenericArguments().First();
                arrayType = elementType.MakeArrayType();
            }

            ICollection collection = null;
            if (dsObject.IsArray)
            {
                collection = ToICollection(dsObject, context, dsi, arrayType);
            }
            else
            {
                // If dsObject is non array pointer but the expectedCLRType is IEnumerable, promote the dsObject to a collection.
                Validity.Assert(typeof(IEnumerable).IsAssignableFrom(expectedCLRType));
                var obj = primitiveMarshaler.UnMarshal(dsObject, context, dsi, elementType);
                collection = new ArrayList(new object[] { obj });
            }

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
                var list = collection as ArrayList;
                if (null != list)
                    return list.ToArray(elementType);
            }

            return collection;
        }

        private ProtoCore.Type GetApproxDSType(object obj)
        {
            if (null == obj)
                return CreateType(ProtoCore.PrimitiveType.Null);

            Type type = obj.GetType();
            if(type == typeof(string))
                return StringMarshaler.kType;
            ProtoCore.Type dsType;
            if (CLRModuleType.TryGetImportedDSType(type, out dsType))
                return dsType;
            if (typeof(IEnumerable).IsAssignableFrom(type)) //It's a collection
            {
                dsType = CreateType(ProtoCore.PrimitiveType.Var);
                dsType.rank = ProtoCore.DSASM.Constants.kArbitraryRank;
                return dsType;
            }

            return CreateType(ProtoCore.PrimitiveType.Var); 
        }

        protected StackValue MarshalToStackValue(object obj, ProtoCore.Runtime.Context context, Interpreter dsi)
        {
            if (obj is StackValue)
            {
                return (StackValue)obj;
            }

            return primitiveMarshaler.Marshal(obj, context, dsi, GetApproxDSType(obj));
        }

        protected StackValue ToDSArray(ICollection collection, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type expectedDSType)
        {
            var sv = new StackValue[collection.Count];
            var index = 0;

            foreach (var item in collection)
            {
                sv[index] = MarshalToStackValue(item, context, dsi);
                ++index;
            }

            try
            {
                return dsi.runtime.rmem.Heap.AllocateArray(sv);
            }
            catch (RunOutOfMemoryException)
            {
                dsi.runtime.RuntimeCore.RuntimeStatus.LogWarning(ProtoCore.Runtime.WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                return StackValue.Null;
            }
        }

        protected StackValue ToDSArray(IEnumerable enumerable, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type expectedDSType)
        {
            var svs = new List<StackValue>();

            foreach (var item in enumerable)
            {
                svs.Add(MarshalToStackValue(item, context, dsi));
            }

            try
            {
                return dsi.runtime.rmem.Heap.AllocateArray(svs.ToArray());
            }
            catch (RunOutOfMemoryException)
            {
                dsi.runtime.RuntimeCore.RuntimeStatus.LogWarning(ProtoCore.Runtime.WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                return StackValue.Null;
            }
        }

        protected T[] UnMarshal<T>(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi)
        {
            var result = new List<T>();
            var heap = dsi.runtime.RuntimeCore.Heap;
            if (!dsObject.IsArray)
                return result.ToArray();

            var dsElements = heap.ToHeapObject<DSArray>(dsObject).Values;
            Type objType = typeof(T);

            foreach (var elem in dsElements)
            {
                object obj = primitiveMarshaler.UnMarshal(elem, context, dsi, objType);
                if (null == obj)
                {
                    if (objType.IsValueType)
                        throw new System.InvalidCastException(
                            string.Format(Resources.FailedToCastFromNull, objType.Name));

                    result.Add(default(T));
                }
                else
                {
                    result.Add((T)obj);
                }
            }

            return result.ToArray();
        }

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

            var dsArray = dsi.runtime.rmem.Heap.ToHeapObject<DSArray>(dsObject);

            //  use arraylist instead of object[], this allows us to correctly capture 
            //  the type of objects being passed
            //
            var arrList = new ArrayList();
            var elementType = arrayType.GetElementType();
            if (elementType == null)
                elementType = typeof(object);
            foreach (var sv in dsArray.Values)
            {
                object obj = primitiveMarshaler.UnMarshal(sv, context, dsi, elementType);
                arrList.Add(obj);
            }

            return arrList;

        }
    }

    class DictionaryMarshaler : PrimitiveMarshaler
    {
        private readonly CLRObjectMarshaler primitiveMarshaler;

        public DictionaryMarshaler(CLRObjectMarshaler primitiveMarshaler, ProtoCore.Type type)
            : base(type)
        {
            this.primitiveMarshaler = primitiveMarshaler;
        }

        public override StackValue Marshal(object obj, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type type)
        {
            List<string> keys = null;
            List<object> values = null;
            
            if (obj is IDictionary)
            {
                var dict = (IDictionary) obj;
                keys = dict.Keys.Cast<string>().ToList();
                values = dict.Values.Cast<object>().ToList();
            }
            else if (obj is Dictionary)
            {
                var dict = (Dictionary) obj;
                keys = dict.Keys.ToList();
                values = dict.Values.ToList();
            }
            // TODO(pboyer) what if keys are not strings?
            var dsdict = Dictionary.ByKeysValues(keys, values);
            return MarshalToStackValue(dsdict, context, dsi);
        }

        public override object UnMarshal(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, Type expectedCLRType)
        {
            if (expectedCLRType.IsGenericType)
            {
                var isGenericDict = expectedCLRType.GetInterfaces()
                                                   .Where(i => i.IsGenericType)
                                                   .Select(i => i.GetGenericTypeDefinition())
                                                   .Contains(typeof(IDictionary<,>));

                if (isGenericDict)
                {
                    return ToGenericIDictionary(dsObject, context, dsi, expectedCLRType);
                }
            }

            // If the target type is an IDictionary, marshal to that type
            if (expectedCLRType == typeof(IDictionary))
            {
                return ToIDictionary(dsObject, context, dsi, expectedCLRType);
            }

            // If it's possible to assign a builtin Dictionary to the target type, then all we need to do is provide
            // the CLR object.
            if (expectedCLRType.IsAssignableFrom(typeof(DesignScript.Builtin.Dictionary)))
            {
                return primitiveMarshaler.GetDictionary(dsObject);
            }

            return null;
        }

        internal static bool IsAssignableFromDictionary(Type expectedCLRType)
        {
            return expectedCLRType == typeof(IDictionary) ||
                expectedCLRType.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Select(i => i.GetGenericTypeDefinition())
                    .Contains(typeof(IDictionary<,>)) ||
                expectedCLRType.IsAssignableFrom(typeof(DesignScript.Builtin.Dictionary));
        }

        private object ToIDictionary(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, System.Type expectedType)
        {
            var targetDict = Activator.CreateInstance(typeof(Dictionary<object, object>)) as IDictionary;

            var d = primitiveMarshaler.GetDictionary(dsObject);
            foreach (var key in d.Keys)
            {
                targetDict[key] = d.ValueAtKey(key);
            }

            return targetDict;
        }

        private object ToGenericIDictionary(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, System.Type expectedType) {
            var keyType = expectedType.GetGenericArguments().First();
            var valueType = expectedType.GetGenericArguments().Last();
            var instanceType = expectedType.GetGenericTypeDefinition().MakeGenericType(keyType, valueType);

            var targetDict = Activator.CreateInstance(instanceType) as IDictionary;
            var d = primitiveMarshaler.GetDictionary(dsObject);
            foreach (var key in d.Keys)
            {
                try
                {
                    if (valueType != typeof(object))
                    {
                        targetDict[key] = Convert.ChangeType(d.ValueAtKey(key), valueType);
                    }
                    else
                    {
                        targetDict[key] = d.ValueAtKey(key);
                    }
                }
                catch (Exception e)
                {
                    dsi.LogWarning(WarningID.TypeMismatch, e.Message);
                }
                
            }

            return targetDict;
        }

        private ProtoCore.Type GetApproxDSType(object obj)
        {
            if (null == obj)
                return CreateType(ProtoCore.PrimitiveType.Null);

            Type type = obj.GetType();
            if (type == typeof(string))
                return StringMarshaler.kType;
            ProtoCore.Type dsType;
            if (CLRModuleType.TryGetImportedDSType(type, out dsType))
                return dsType;
            if (typeof(IEnumerable).IsAssignableFrom(type)) //It's a collection
            {
                dsType = CreateType(ProtoCore.PrimitiveType.Var);
                dsType.rank = ProtoCore.DSASM.Constants.kArbitraryRank;
                return dsType;
            }

            return CreateType(ProtoCore.PrimitiveType.Var);
        }

        protected StackValue MarshalToStackValue(object obj, ProtoCore.Runtime.Context context, Interpreter dsi)
        {
            if (obj is StackValue)
            {
                return (StackValue)obj;
            }

            return primitiveMarshaler.Marshal(obj, context, dsi, GetApproxDSType(obj));
        }
    }

    /// <summary>
    /// Marshales string as array of chars
    /// </summary>
    class StringMarshaler : PrimitiveMarshaler 
    {
        public static readonly ProtoCore.Type kType = CreateType(ProtoCore.PrimitiveType.String);

        public StringMarshaler() : base(kType) { }

        public override StackValue Marshal(object obj, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type type)
        {
            string str = (string)obj;
            return dsi.runtime.rmem.Heap.AllocateString(str);
        }

        public override object UnMarshal(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, Type type)
        {
            var dsString = dsi.runtime.rmem.Heap.ToHeapObject<DSString>(dsObject);
            if (dsString == null)
                return null;
            return dsString.Value;
        }
    }

    /// <summary>
    /// This class marshals CLR Objects to DS Object and vice-versa.
    /// </summary>
    class CLRObjectMarshaler : FFIObjectMarshaler
    {
        private static readonly Dictionary<Type, FFIObjectMarshaler> mPrimitiveMarshalers;
        static CLRObjectMarshaler()
        {
            mPrimitiveMarshalers = new Dictionary<Type, FFIObjectMarshaler>();
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
        /// Returns instance of the CLRObjectMarshler for a given core. If marshler
        /// is not already created, it creates a new one.
        /// </summary>
        /// <param name="core">Core object.</param>
        /// <returns>CLRObjectMarshler</returns>
        public static CLRObjectMarshaler GetInstance(ProtoCore.RuntimeCore core)
        {
            CLRObjectMarshaler marshaller = null;
            if (!mObjectMarshlers.TryGetValue(core, out marshaller))
            {
                IDisposable[] disposables = null;
                lock (syncroot)
                {
                    if (mObjectMarshlers.TryGetValue(core, out marshaller))
                        return marshaller;

                    marshaller = new CLRObjectMarshaler(core);

                    object value;
                    if (core.DSExecutable.Configurations.TryGetValue(ConfigurationKeys.GeometryXmlProperties, out value))
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
            var marshaler = GetMarshalerForDsType(expectedDSType, obj.GetType());

            //3. Got a marshaler, now marshal it.
            if (marshaler != null)
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
            var marshaler = GetMarshalerForCLRType(expectedCLRType, dsObject);
            if (null != marshaler)
                return marshaler.UnMarshal(dsObject, context, dsi, expectedCLRType);

            //The dsObject must be of pointer type
            Validity.Assert(dsObject.IsPointer || dsObject.IsFunctionPointer, 
                string.Format("Operand type {0} not supported for marshalling", dsObject.optype));

            if (dsObject.IsFunctionPointer)
            {
                return dsObject;
            }

            //Search in the DSObjectMap, for corresponding clrObject.
            object clrObject = null;
            if (DSObjectMap.TryGetValue(dsObject, out clrObject))
                return clrObject;

            
            return CreateCLRObject(dsObject, context, dsi, expectedCLRType);
        }

        /// <summary>
        /// If clrType is IEnumerable, returns a CollectionMarshaler, otherwise
        /// gets marshaler for the given clrType and if it fails
        /// to get one, it tries to get primitive marshaler based on dsType.
        /// 
        /// We want to get correct marshaler specific to the input type because
        /// more than one type gets map to same type in DS.
        /// </summary>
        /// <param name="clrType">System.Type to which DS object needs to be 
        /// marshaled.</param>
        /// <param name="dsType">DS Object type, that needs to be marshaled.
        /// </param>
        /// <returns>FFIObjectMarshler or null</returns>
        private FFIObjectMarshaler GetMarshalerForCLRType(Type clrType, StackValue value)
        {
            var dsType = value.optype;

            FFIObjectMarshaler marshaler = null;
            //Expected CLR type is object, get marshaled clrType from dsType
            Type expectedType = clrType;
            if (expectedType == typeof(object))
                expectedType = GetPrimitiveType(dsType);
            else if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>))
                expectedType = Nullable.GetUnderlyingType(clrType);

            //If DS Type is array, it needs to be marshaled as collection.
            bool isArray = dsType == AddressType.ArrayPointer;

            //Expected CLR type is not string, but is derived from IEnumerable
            isArray = isArray || (typeof(string) != expectedType && typeof(IEnumerable).IsAssignableFrom(expectedType));

            // If the source is a Dictionary and the target allows assignment of a Dictionary
            if (IsDictionary(value) && DictionaryMarshaler.IsAssignableFromDictionary(expectedType))
            {
                var type = PrimitiveMarshaler.CreateType(ProtoCore.PrimitiveType.Var);
                type.rank = ProtoCore.DSASM.Constants.kArbitraryRank;
                return new DictionaryMarshaler(this, type);
            }

            if (isArray)
            {
                var type = PrimitiveMarshaler.CreateType(ProtoCore.PrimitiveType.Var);
                type.rank = ProtoCore.DSASM.Constants.kArbitraryRank;
                return new ArrayMarshaler(this, type);
            }

            // If the input ds object is pointer type then it can't be marshaled as primitive.
            if (dsType == AddressType.Pointer)
            {
                return null;
            }

            if (!mPrimitiveMarshalers.TryGetValue(expectedType, out marshaler))
                mPrimitiveMarshalers.TryGetValue(GetPrimitiveType(dsType), out marshaler);

            return marshaler;
        }

        /// <summary>
        /// Returns appropriate marshaler for given DS Type.
        /// </summary>
        /// <param name="dsType">DS Type to which given objType needs to be marshaled.</param>
        /// <param name="objType">CLR object type that needs to marshal.</param>
        /// <returns>FFIObjectMarshler or null</returns>
        private FFIObjectMarshaler GetMarshalerForDsType(ProtoCore.Type dsType, Type objType)
        {
            //Expected DS Type is pointer, so there is no primitive marshaler available.
            if (!dsType.IsIndexable && dsType.UID == (int)ProtoCore.PrimitiveType.Pointer)
                return null;

            FFIObjectMarshaler marshaler = null;

            if (DictionaryMarshaler.IsAssignableFromDictionary(objType))
            {
                return new DictionaryMarshaler(this, dsType);
            }

            bool marshalAsArray = false;

            //0. String needs special handling becuase it's derived from IEnumerable.
            if (typeof(string) == objType)
            {
                marshalAsArray = false;
            }  
            //1. If expectedDSType is fixed rank collection, objType must be a collection of same rank
            else if (dsType.rank > 0 && typeof(IEnumerable).IsAssignableFrom(objType))
            {
                marshalAsArray = true;
            }
            //2. If dsType is arbitrary rank collection, marshal based on objType
            //3. If dsType is var, marshal based on objType.
            else if ((dsType.rank == ProtoCore.DSASM.Constants.kArbitraryRank ||
                dsType.UID == (int)ProtoCore.PrimitiveType.Var) && typeof(IEnumerable).IsAssignableFrom(objType))
            {
                marshalAsArray = true;
            }

            //4. Else get primitive marshaler for given objType
            if (marshalAsArray)
                marshaler = new ArrayMarshaler(this, dsType);
            else if(dsType.UID != (int)ProtoCore.PrimitiveType.Pointer) //Not exported as pointer type
                mPrimitiveMarshalers.TryGetValue(objType, out marshaler);

            return marshaler;
        }

        internal bool IsDictionary(StackValue value)
        {
            object obj;
            return value.IsPointer && DSObjectMap.TryGetValue(value, out obj) && obj is DesignScript.Builtin.Dictionary;
        }

        internal DesignScript.Builtin.Dictionary GetDictionary(StackValue value)
        {
            return DSObjectMap[value] as DesignScript.Builtin.Dictionary;
        }

        /// <summary>
        /// Returns a primitive System.Type for the given DS type.
        /// </summary>
        /// <param name="addressType">DS AddressType</param>
        /// <returns>System.Type</returns>
        private Type GetPrimitiveType(AddressType addressType)
        {
            switch (addressType)
            {
                case AddressType.Int:
                    return typeof(long);
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
        /// Returns marshaled DS type for the given System.Type
        /// </summary>
        /// <param name="type">System.Type</param>
        /// <returns>ProtoCore.Type as equivalent DS type for input System.Type</returns>
        public override ProtoCore.Type GetMarshaledType(Type type)
        {
            return CLRObjectMarshaler.GetProtoCoreType(type);
        }

        /// <summary>
        /// Returns equivalent DS type for the input System.Type
        /// </summary>
        /// <param name="type">System.Type</param>
        /// <returns>ProtoCore.Type</returns>
        public static ProtoCore.Type GetProtoCoreType(Type type)
        {
            ProtoCore.Type retype = PrimitiveMarshaler.CreateType(ProtoCore.PrimitiveType.Var);
            ComputeDSType(type, ref retype);
            return retype;
        }

        /// <summary>
        /// Returns the marshaled type for input System.Type as DS Pointer type
        /// </summary>
        /// <param name="type">System.Type</param>
        /// <returns>ProtoCore.Type</returns>
        public static ProtoCore.Type GetUserDefinedType(Type type)
        {
            ProtoCore.Type retype = PrimitiveMarshaler.CreateType(ProtoCore.PrimitiveType.Pointer);
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
            FFIObjectMarshaler marshaler;
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
                    protoCoreType.UID = (int)ProtoCore.PrimitiveType.Pointer;
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
                protoCoreType = PrimitiveMarshaler.CreateType(ProtoCore.PrimitiveType.Var);
                protoCoreType.rank = 0; //Initially setup zero rank
            }
            else if (type == typeof(void))
                protoCoreType = PrimitiveMarshaler.CreateType(ProtoCore.PrimitiveType.Void);
            else if (protoCoreType.UID == (int)ProtoCore.PrimitiveType.Pointer)
                protoCoreType.Name = GetTypeName(type);
            else if (mPrimitiveMarshalers.TryGetValue(type, out marshaler))
                protoCoreType = marshaler.GetMarshaledType(type);
            else
            {
                protoCoreType.Name = GetTypeName(type);
                protoCoreType.UID = (int)ProtoCore.PrimitiveType.Pointer;
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
            var runtimeCore = dsi.runtime.RuntimeCore;
            var classTable = runtimeCore.DSExecutable.classTable;
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
            StackValue retval = runtimeCore.RuntimeMemory.Heap.AllocatePointer(classTable.ClassNodes[type].Size, metadata);
            BindObjects(obj, retval);
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

            var runtimeCore = dsi.runtime.RuntimeCore;
            StackValue[] svs = dsi.runtime.rmem.Heap.ToHeapObject<DSObject>(dsObject).Values.ToArray();
            for (int ix = 0; ix < svs.Length; ++ix)
            {
                SymbolNode symbol = runtimeCore.DSExecutable.classTable.ClassNodes[classIndex].Symbols.symbolList[ix];
                object prop = null;
                if (properties.TryGetValue(symbol.name, out prop) && null != prop)
                    svs[ix] = Marshal(prop, context, dsi, GetMarshaledType(prop.GetType()));
            }
        }

        /// <summary>
        /// Returns all the properties of input object, that are marked with 
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
                BindObjects(dsObject, dsObject);
                return dsObject;
            }

            throw new InvalidOperationException("Unable to locate managed object for given dsObject.");
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Core object for marshler.</param>
        private CLRObjectMarshaler(ProtoCore.RuntimeCore runtimeCore)
        {
            runtimeCore.Dispose += core_Dispose;
        }

        /// <summary>
        /// Dispose event handler.
        /// </summary>
        /// <param name="sender">Core object being disposed.</param>
        void core_Dispose(ProtoCore.RuntimeCore sender)
        {
            CLRObjectMarshaler marshaller = null;
            if (!mObjectMarshlers.TryGetValue(sender, out marshaller))
                throw new System.Collections.Generic.KeyNotFoundException();

            mObjectMarshlers.Remove(sender);

            //Detach from core.
            sender.Dispose -= core_Dispose;

            //Dispose all disposable CLR objects.
            foreach (var item in DSObjectMap)
            {
                IDisposable disposable = item.Value as IDisposable;

                if (null != disposable)
                    disposable.Dispose();
            }

            //Clear the maps.
            DSObjectMap.Clear();
            CLRObjectMap.Clear();
        }

        private readonly Dictionary<StackValue, Object> DSObjectMap = new Dictionary<StackValue, object>(new PointerValueComparer());
        private readonly Dictionary<Object, StackValue> CLRObjectMap = new Dictionary<object, StackValue>(new ReferenceEqualityComparer());
        private static readonly Dictionary<ProtoCore.RuntimeCore, CLRObjectMarshaler> mObjectMarshlers = new Dictionary<ProtoCore.RuntimeCore, CLRObjectMarshaler>();
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
            return x.Pointer == y.Pointer && x.metaData.type == y.metaData.type;
        }

        public int GetHashCode(StackValue obj)
        {
            unchecked
            {
                var hash = 0;
                hash = (hash * 397) ^ obj.Pointer.GetHashCode();
                hash = (hash * 397) ^ obj.metaData.type.GetHashCode();
                return hash;
            }
        }
    }
}
