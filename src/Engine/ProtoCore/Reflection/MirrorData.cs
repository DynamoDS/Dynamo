using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoCore
{
    namespace Mirror
    {
        /// <summary>
        ///  An object that performs marshalling of all relevant data associated with this object
        /// </summary>
        public class MirrorData
        {

            /// <summary>
            ///  The stack value associated with this mirror data
            /// </summary>
            private StackValue svData;


            //
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
            /// <param name="sv"></param>
            public MirrorData(ProtoCore.Core core, StackValue sv)
            {
                this.core = core;
                svData = sv;
            }       
            
            /// <summary>
            /// Takes a runtime core object to read runtime data
            /// </summary>
            /// <param name="sv"></param>
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
                List<DSASM.StackValue> values = new List<DSASM.StackValue>();
                GetPointersRecursively(svData, values);

                List<IGraphicItem> graphicItems = new List<IGraphicItem>();
                foreach (var sv in values)
                {
                    List<IGraphicItem> items = dataProvider.GetGraphicItems(sv, this.runtimeCore);
                    if (items != null && (items.Count > 0))
                        graphicItems.AddRange(items);
                }
                if (graphicItems.Count > 0)
                    return graphicItems;

                return null;
            }

            /// <summary>
            /// Recursively finds all Pointers from the stack value
            /// </summary>
            /// <param name="sv">Stack value</param>
            /// <param name="values">Stack values</param>
            private void GetPointersRecursively(DSASM.StackValue sv, List<DSASM.StackValue> values)
            {
                switch (sv.optype)
                {
                    case ProtoCore.DSASM.AddressType.Pointer:
                        values.Add(sv);
                        break;
                    case ProtoCore.DSASM.AddressType.ArrayPointer:
                        var array = runtimeCore.Heap.ToHeapObject<DSArray>(sv);
                        foreach (var item in array.Values)
                            GetPointersRecursively(item, values);
                        break;
                    default:
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
            /// Returns the list of MirrorData if this data represents a collection,
            /// else null.
            /// </summary>
            /// <returns>List of MirrorData represented by this data.</returns>
            public IEnumerable<MirrorData> GetElements() 
            {
                //This is not a collection
                if (!this.IsCollection)
                    return null;

                var array = runtimeCore.Heap.ToHeapObject<DSArray>(svData);
                return array.Values.Select(x => new MirrorData(this.core, this.runtimeCore, x));
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
            /// <param name="sv">StackValue</param>
            /// <param name="core">ProtoCore.Core</param>
            /// <returns>System.Object</returns>
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

            /// <summary>
            /// Returns if this data points to null.
            /// </summary>
            public bool IsNull
            {
                get
                {
                    return svData.IsNull;
                }
            }

            /// <summary>
            /// Determines if this data points to a collection.
            /// </summary>
            public bool IsCollection
            {
                get
                {
                    return svData.IsArray;
                }
            }

            public bool IsDictionary => Data is DesignScript.Builtin.Dictionary;

            /// <summary>
            /// Determines if this data is a pointer
            /// </summary>
            public bool IsPointer
            {
                get
                {
                    return svData.IsPointer;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
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
