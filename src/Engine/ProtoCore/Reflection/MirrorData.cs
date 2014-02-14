using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;
using ProtoCore.Utils;
using ProtoCore.DSASM;

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
            private ProtoCore.Core core = null;

            /// <summary>
            /// 
            /// </summary>
            private static GraphicDataProvider dataProvider = new GraphicDataProvider();

            /// <summary>
            /// Experimental constructor that takes in a core object
            /// </summary>
            /// <param name="sv"></param>
            public MirrorData(ProtoCore.Core core, StackValue sv)
            {
                this.core = core;
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
                    List<IGraphicItem> items = dataProvider.GetGraphicItems(sv, this.core);
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
                        List<DSASM.StackValue> stackValues = GetArrayStackValues(sv);
                        foreach (var item in stackValues)
                            GetPointersRecursively(item, values);

                        break;
                    default:
                        break;
                }
            }

            private List<StackValue> GetArrayStackValues(DSASM.StackValue sv)
            {
                return ArrayUtils.GetValues<StackValue>(sv, this.core, (DSASM.StackValue s) => s);
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
            /// Gets the Class info mirror for this data.
            /// </summary>
            /// <returns></returns>
            private ClassMirror GetClass()
            {
                if (svData.optype != DSASM.AddressType.Pointer)
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
            /// Gets the list of MirrorData if this data represents a collection,
            /// else null.
            /// </summary>
            /// <returns>List of MirrorData represented by this data.</returns>
            public List<MirrorData> GetElements()
            {
                //This is not a collection
                if (!this.IsCollection)
                    return null;

                List<MirrorData> elements = ArrayUtils.GetValues<MirrorData>(svData, core, (StackValue sv) => new MirrorData(this.core, sv));
                return elements;
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
            internal static object GetData(StackValue sv, Core core)
            {
                switch (sv.optype)
                {
                    case AddressType.Int:
                        return sv.opdata;
                    case AddressType.Double:
                        return sv.opdata_d;
                    case AddressType.Boolean:
                        return sv.opdata != 0;
                    case AddressType.Char:
                        return ProtoCore.Utils.EncodingUtils.ConvertInt64ToCharacter(sv.opdata);
                    case AddressType.String:
                        return StringUtils.GetStringValue(sv, core);
                    case AddressType.Pointer:
                        return dataProvider.GetCLRObject(sv, core);
                    default:
                        break;
                }
                return null;
            }

            /// <summary>
            /// Gets the CLR object for all the value type or FFI objects, else null
            /// </summary>
            public object Data
            {
                get
                {
                    if (null == clrdata)
                        clrdata = GetData(svData, core);

                    return clrdata;
                }
            }

            /// <summary>
            /// Gets if this data points to null.
            /// </summary>
            public bool IsNull
            {
                get
                {
                    return svData.optype == AddressType.Null;
                }
            }

            /// <summary>
            /// Gets if this data points to a collection.
            /// </summary>
            public bool IsCollection
            {
                get
                {
                    return svData.optype == AddressType.ArrayPointer;
                }
            }
        }
    }
}
