using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;
using ProtoCore.Utils;
using ProtoCore.DSASM;
using System.Linq;
using System;
using System.Globalization;

namespace ProtoCore
{
    namespace Mirror
    {
        /// <summary>
        /// An object allowing inspection of a StackValue inside of the virtual machine
        /// </summary>
        public class MirrorData
        {
            /// <summary>
            ///  The stack value associated with this mirror data
            /// </summary>
            private StackValue svData;

            // Comment Jun:
            // Experimental - have a copy of the core so the data marshaller has access to it
            // The proper solution is to either:
            //      1. Move the MirrorData properties in the RuntimeMirror class or ...
            //      2. Do the data analysis of the MirrorData in the MirrorData class itself
            //
            private ProtoCore.Core core;
            private ProtoCore.RuntimeCore runtimeCore;

            /// <summary>
            /// 
            /// </summary>
            private static GraphicDataProvider dataProvider = new GraphicDataProvider();

            /// <summary>
            /// Experimental constructor that takes in a core object
            /// Takes a core object to read static data
            /// </summary>
            public MirrorData(ProtoCore.Core core, StackValue sv)
            {
                this.core = core;
                svData = sv;
            }       
            
            /// <summary>
            /// Takes a runtime core object to read runtime data
            /// </summary>
            public MirrorData(ProtoCore.Core core, ProtoCore.RuntimeCore runtimeCore, StackValue sv)
            {
                this.core = core;
                this.runtimeCore = runtimeCore;
                svData = sv;
            }

            /// <summary>
            ///  Retrieves list of IGraphicItem to get the graphic 
            ///  representation/preview of this Data.
            /// </summary>
            /// <returns>List of IGraphicItem</returns>
            /// <remarks>This method is marked as obsolete because it's possible
            /// to get the CLR object from this mirror data and client can handle
            /// any query to IGraphicItem on the CLR object directly.</remarks>
            [System.Obsolete("Query IGraphicItem from Data property of this class")]
            public List<IGraphicItem> GetGraphicsItems()
            {
                var graphicItems = new List<IGraphicItem>();
                foreach (var sv in GetPointersRecursively(svData))
                {
                    var items = dataProvider.GetGraphicItems(sv, this.runtimeCore);
                    if (items != null && (items.Count > 0))
                        graphicItems.AddRange(items);
                }
                if (graphicItems.Count > 0)
                    return graphicItems;

                return null;
            }

            private IEnumerable<StackValue> GetPointersRecursively(DSASM.StackValue sv)
            {
                switch (sv.optype)
                {
                    case AddressType.Pointer:
                        yield return sv;
                        break;
                    case AddressType.ArrayPointer:
                        var array = runtimeCore.Heap.ToHeapObject<DSArray>(sv);
                        foreach (var item in array.Values)
                            GetPointersRecursively(item);
                        break;
                    case AddressType.DictionaryPointer:
                        var dict = runtimeCore.Heap.ToHeapObject<DSDictionary>(sv);
                        foreach (var item in dict.Values)
                            GetPointersRecursively(item);
                        break;
                }
            }

            /// <summary>
            ///  Retrieve the stack value for this mirror
            /// </summary>
            /// <returns></returns>
            [System.Obsolete("Use Data property of this class")]
            public StackValue GetStackValue()
            {
                return svData;
            }

            /// <summary>
            /// The DesignScript Class info mirror.
            /// </summary>
            private ClassMirror classMirror = null;
            
            /// <summary>
            /// Returns the Class info mirror for this data.
            /// </summary>
            /// <returns></returns>
            private ClassMirror GetClass()
            {
                if (!svData.IsPointer)
                    return null;

                return new ClassMirror(svData, this.core);
            }

            /// <summary>
            /// Returns ClassMirror if this data is an instance of a DesignScript Class.
            /// </summary>
            public ClassMirror Class
            {
                get 
                {
                    if (null == classMirror)
                        classMirror = GetClass();
                    return classMirror;
                }
            }

            /// <summary>
            /// A list of MirrorData objects if this object represents an array or dictionary,
            /// otherwise null.
            /// </summary>
            public IEnumerable<MirrorData> GetValues() 
            {
                switch (svData.optype)
                {
                    case AddressType.ArrayPointer:
                        var array = runtimeCore.Heap.ToHeapObject<DSArray>(svData);
                        return array.Values.Select(x => new MirrorData(this.core, this.runtimeCore, x));
                    case AddressType.DictionaryPointer:
                        var dict = runtimeCore.Heap.ToHeapObject<DSDictionary>(svData);
                        return dict.Values.Select(x => new MirrorData(this.core, this.runtimeCore, x));
                }

                return null;
            }

            /// <summary>
            /// A list of MirrorData objects if this object represents a dictionary,
            /// otherwise null.
            /// </summary>
            public IEnumerable<MirrorData> GetDictionaryKeys()
            {
                if (!svData.IsDictionary)
                {
                    return null;
                }

                var dict = runtimeCore.Heap.ToHeapObject<DSDictionary>(svData);
                return dict.Keys.Select(x => new MirrorData(this.core, this.runtimeCore, x));
            }

            /// <summary>
            /// The CLR object represented by this MirrorData
            /// </summary>
            private object clrdata = null;

            /// <summary>
            /// Returns clr object represented by given StackValue. It handles
            /// DS primitive types such as Int, Double, Bool, Char, String and 
            /// Pointer if the pointer represents an FFI object. For other cases
            /// it returns null.
            /// </summary>
            internal static object GetData(StackValue sv, RuntimeCore runtimeCore)
            {
                switch (sv.optype)
                {
                    case AddressType.Int:
                        return sv.IntegerValue;
                    case AddressType.Double:
                        return sv.DoubleValue;
                    case AddressType.Boolean:
                        return sv.BooleanValue;
                    case AddressType.Char:
                        return Convert.ToChar(sv.CharValue); 
                    case AddressType.String:
                        return StringUtils.GetStringValue(sv, runtimeCore);
                    case AddressType.Pointer:
                        return dataProvider.GetCLRObject(sv, runtimeCore);
                    default:
                        break;
                }
                return null;
            }

            /// <summary>
            /// Returns string representation of data
            /// </summary>
            public string StringData
            {
                get
                {
                    if (object.ReferenceEquals(Data, null) || this.IsNull)
                    {
                        return "null";
                    }
                    else if (Data is bool)
                    {
                        return Data.ToString().ToLower();
                    }
                    else if (Data is IFormattable)
                    {
                        // Object.ToString() by default will use the current 
                        // culture to do formatting. For example, Double.ToString()
                        // https://msdn.microsoft.com/en-us/library/3hfd35ad(v=vs.110).aspx
                        // We should always use invariant culture format for formattable 
                        // object.
                        return (Data as IFormattable).ToString(null, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        return Data.ToString();
                    }
                }
            }

            /// <summary>
            /// Returns the CLR object for all the value type or FFI objects, else null
            /// </summary>
            public object Data
            {
                get
                {
                    if (null == clrdata)
                        clrdata = GetData(svData, runtimeCore);

                    return clrdata;
                }
            }

            public bool IsNull
            {
                get
                {
                    return svData.IsNull;
                }
            }

            public bool IsCollection
            {
                get
                {
                    return svData.IsArray || svData.IsDictionary;
                }
            }

            public bool IsArray
            {
                get
                {
                    return svData.IsArray;
                }
            }


            public bool IsDictionary
            {
                get
                {
                    return svData.IsDictionary;
                }
            }

            public bool IsPointer
            {
                get
                {
                    return svData.IsPointer;
                }
            }

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(obj, this))
                    return true;

                MirrorData data = obj as MirrorData;
                if (null == data)
                    return false;

                return StackUtils.CompareStackValues(this.svData, data.svData, this.runtimeCore, data.runtimeCore);
            }
        }
    }
}
