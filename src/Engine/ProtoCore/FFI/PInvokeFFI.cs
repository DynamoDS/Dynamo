using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Autodesk.DesignScript.Runtime;
using ProtoCore.DSASM;
using ProtoCore.Exceptions;
using FFICppFunction = ProtoCore.Lang.FFICppFunction2;

namespace ProtoFFI
{

    public class PInvokeDLLModule : DLLModule
    {
        public readonly string Name;
        public readonly ModuleBuilder ModuleBuilder;
        public readonly AssemblyName AssemblyName;
        public readonly AssemblyBuilder AssemblyBuilder;

        private readonly Dictionary<string, List<FFIFunctionPointer>> functionPointers = new Dictionary<string, List<FFIFunctionPointer>>();
        
        public PInvokeDLLModule(string name)
        {
            Name = name;
            AssemblyName = new AssemblyName();
            AssemblyName.Name = name;
            AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                    AssemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule(AssemblyName.Name);
        }


        //this is incomplete todo: implement
        public override List<FFIFunctionPointer> GetFunctionPointers(string className, string name)
        {
            if(functionPointers.ContainsKey(name))
            {
                return functionPointers[name];
            }
            //todo: fix this
            return new List<FFIFunctionPointer>();
        }

        public override FFIFunctionPointer GetFunctionPointer(string className, string name, List<ProtoCore.Type> argTypes, ProtoCore.Type returnType)
        {
            List<FFIFunctionPointer> pointers;
            if (functionPointers.ContainsKey(name))
            {
                pointers = functionPointers[name];
                //todo this needs validating types. else this would be an overload
                return pointers[0];
            }
            else
            {
                pointers = new List<FFIFunctionPointer>();
                FFIFunctionPointer f = new PInvokeFunctionPointer(this, name, argTypes, returnType);
                pointers.Add(f);
                functionPointers[name] = pointers;
                return f;
            }
        }
    }

    public class PInvokeFunctionPointer : FFIFunctionPointer
    {
        public PInvokeDLLModule Module
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        private readonly FFICppFunction mFunction;
        private readonly List<ProtoCore.Type> mArgTypes;
        private ProtoCore.Type mReturnType;

        public PInvokeFunctionPointer(PInvokeDLLModule module, String name)
        {
            Module = module;
            Name = name;
            //mFunction = new FFICppFunction(module.Name, name);
            mFunction = new FFICppFunction(module.ModuleBuilder, module.AssemblyName, module.AssemblyBuilder, name);
        }

        public PInvokeFunctionPointer(PInvokeDLLModule module, String name, ProtoCore.Type returnType)
        {
            Module = module;
            Name = name;
            mReturnType = returnType;
            //mFunction = new FFICppFunction(module.Name, name);
            mFunction = new FFICppFunction(module.ModuleBuilder, module.AssemblyName, module.AssemblyBuilder, name);
        }

        #region DS_ARRAY_TO_CS_ARRAY
        private object GetArray(StackValue o,
                                ProtoCore.DSASM.Interpreter dsi,
                                out int size)
        {
            size = 0;
            var array = dsi.runtime.rmem.Heap.ToHeapObject<DSArray>(o);

            if (!array.Values.Any())
                return null;

            IList elements = null;
            var opType = array.GetValueFromIndex(0, dsi.runtime.RuntimeCore).optype;
            if (opType == AddressType.Boolean)
            {
                elements = new List<bool>();
            }
            else if (opType == AddressType.Double)
            {
                elements = new List<double>();
            }
            else if (opType == AddressType.Int)
            {
                elements = new List<int>();
            }
            else if (opType == AddressType.ArrayPointer)
            {
                throw new ArgumentException("FFI does not support nested arrays");
            }
            else
            {
                throw new ArgumentException(string.Format("Argument of type {0} is not supported for FFI Marshalling", opType.ToString()));
            }

            foreach (var op in array.Values)
            {
                if (opType == AddressType.Double)
                {
                    elements.Add(op.DoubleValue);
                }
                else if (opType == AddressType.Int)
                {
                    elements.Add(op.IntegerValue);
                }
                else if (opType == AddressType.Boolean)
                {
                    elements.Add(op.BooleanValue);
                }
            }


            //  now based on the type of element in ds-array
            //  create a new array and return it
            //
            if (opType == AddressType.Double)
            {
                double[] arr = new double[size];
                elements.CopyTo(arr, 0);
                return arr;
            }
            else if (opType == AddressType.Int)
            {
                int[] arr = new int[size];
                elements.CopyTo(arr, 0);
                return arr;
            }
            else if (opType == AddressType.Boolean)
            {
                bool[] arr = new bool[size];
                elements.CopyTo(arr, 0);
                return arr;
            }
            else
            {
                size = 0;
                return null;
            }
        }
        #endregion

        #region CS_ARRAY_TO_DS_ARRAY

        private object ConvertCSArrayToDSArray(double[] csArray, ProtoCore.DSASM.Interpreter dsi)
        {
            var runtimeCore = dsi.runtime.RuntimeCore;
            object retVal = null;

            var values = csArray.Select(x => StackValue.BuildDouble(x)).ToArray();

            try
            {
                retVal = runtimeCore.RuntimeMemory.Heap.AllocateArray(values);
                return retVal;
            }
            catch (RunOutOfMemoryException)
            {
                dsi.runtime.RuntimeCore.RuntimeStatus.LogWarning(ProtoCore.Runtime.WarningID.RunOutOfMemory, ProtoCore.Properties.Resources.RunOutOfMemory);
                return StackValue.Null;
            }
        }

        #endregion

        #region RETRUN_TYPE_MASSAGING

        [StructLayout(LayoutKind.Sequential)]
        struct _Array
        {
            public readonly int numElems;
            public readonly IntPtr elements;
        }

        //  return type is a bit special, first of all you cannot return back an array from C++ to C#
        //  this needs a little massaging
        //
        private static Type GetMarshalledReturnType(ProtoCore.Type t)
        {
            if (t.IsIndexable && t.rank > 1)
            {
                throw new ArgumentException("FFI does not support nested arrays");
            }

            if (t.IsIndexable && (t.Name == "int" || t.Name == "double") )
            {
                //  now that we have ensured that the array is of rank 1 (1d array) 
                //  it is safe to marshall it back at _Array
                //
                return typeof(IntPtr);
            }

            if (t.Name == "int")
            {
                return typeof(Int64);
            }
            else if (t.Name == "double")
            {
                return typeof(double);
            }
            else if (t.Name == "bool")
            {
                return typeof(bool);
            }
            else
            {
                throw new ArgumentException(string.Format("FFI: unknown type {0} to marshall", t.Name));
            }
        }

        private object ConvertReturnValue(object retVal, ProtoCore.Runtime.Context context, ProtoCore.DSASM.Interpreter dsi)
        {
            object returnValue = retVal;
            //  these are arrays!
            if (mReturnType.IsIndexable)
            {
                //  we have already asserted that these can be of rank 1 only, for now
                //
                IntPtr arrPtr = (IntPtr)retVal;
                if (arrPtr == IntPtr.Zero)
                {
                    return StackValue.Null;
                }

                _Array arr = (_Array)Marshal.PtrToStructure(arrPtr, typeof(_Array));
                
                if (mReturnType.Name == "double")
                {
                    double[] elements = new double[arr.numElems];
                    Marshal.Copy(arr.elements, elements, 0, arr.numElems);

                    //  free up the memory
                    Marshal.FreeCoTaskMem(arr.elements);
                    Marshal.FreeCoTaskMem(arrPtr);

                    returnValue = ConvertCSArrayToDSArray(elements, dsi);
                }
                else if (mReturnType.Name == "int")
                {
                    int[] elements = new int[arr.numElems];
                    Marshal.Copy(arr.elements, elements, 0, arr.numElems);
                    return elements;
                }
                else
                {
                    throw new ArgumentException(string.Format("FFI: unknown type {0} to marshall", mReturnType.Name));
                }
            }

            return returnValue;
        }

    #endregion

        public PInvokeFunctionPointer(PInvokeDLLModule module, String name, List<ProtoCore.Type> argTypes, ProtoCore.Type returnType)
        {
            Module = module;
            Name = name;
            mReturnType = returnType;
            mArgTypes = argTypes;
            mFunction = new FFICppFunction(module.ModuleBuilder, module.AssemblyName, module.AssemblyBuilder, name, GetMarshalledReturnType(returnType));
        }

        public override object Execute(ProtoCore.Runtime.Context context, Interpreter dsi, List<StackValue> newStack)
        {
            List<Object> parameters = new List<object>();
            if (IsDNI)
            {
                parameters.Add(DLLFFIHandler.Env);
                parameters.Add((Int64)0);
            }
            for (int i = 0; i < mArgTypes.Count; ++i)
            {

                // Comment Jun: FFI function stack frames do not contain locals
                int locals = 0;
                int relative = 0 - StackFrame.StackFrameSize - locals - i - 1;
                StackValue o = dsi.runtime.rmem.GetAtRelative(relative);

                if (o.IsInteger)
                {
                    if (mArgTypes[i].Name == "double")
                    {
                        //  if the function expects a double and we have passed an int
                        //  in an int then promote it to be a double!
                        //
                        parameters.Add(o.IntegerValue);
                    }
                    else
                    {
                        parameters.Add(o.RawData);
                    }
                }
                else if (o.IsDouble)
                {
                    parameters.Add(o.DoubleValue);
                }
                else if (o.IsArray)
                {
                    int size = 0;
                    object array = GetArray(o, dsi, out size);
                    parameters.Add(array);
                    parameters.Add(size);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            //object ret = mFunction.Invoke(parameters);
            object ret = mFunction.Execute(parameters.ToArray());
            return ConvertReturnValue(ret, context, dsi);
        }

        [IsObsolete("Remove in 3.0. Use Execute(ProtoCore.Runtime.Context c, ProtoCore.DSASM.Interpreter dsi, List<StackValue> stack) instead")]
        public override object Execute(ProtoCore.Runtime.Context context, Interpreter dsi)
        {
            return Execute(context, dsi, null);
        }
    }

    public class PInvokeModuleHelper : ModuleHelper
    {
        public override FFIObjectMarshaler GetMarshaler(ProtoCore.RuntimeCore runtimeCore)
        {
            return null;
        }

        public override DLLModule getModule(String name)
        {
            return new PInvokeDLLModule(name);
        }
    }
}