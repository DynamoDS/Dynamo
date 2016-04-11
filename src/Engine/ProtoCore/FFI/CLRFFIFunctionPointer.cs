﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using Autodesk.DesignScript.Runtime;
using ProtoCore.Properties;
using ProtoCore.Exceptions;

namespace ProtoFFI
{
    /// <summary>
    /// FFIParameterInfo wraps ParameterInfo and attributes that applied to
    /// the parameter.
    /// </summary>
    class FFIParameterInfo
    {
        /// <summary>
        /// ParameterInfo
        /// </summary>
        public ParameterInfo Info { get; private set; }

        /// <summary>
        /// Create FFIParameterInfo by ParameterInfo
        /// </summary>
        /// <param name="info"></param>
        public FFIParameterInfo(ParameterInfo info)
        {
            Info = info;
        }

        private bool? keepReference;
        /// <summary>
        /// Indicate if the marshaller should keep a reference to this
        /// parameter in the return object of the corresponding function call.
        /// </summary>
        public bool KeepReference
        {
            get
            {
                if (keepReference.HasValue)
                {
                    return keepReference.Value;
                }

                keepReference = Info.GetCustomAttribute<KeepReferenceAttribute>() != null;
                return keepReference.Value;
            }
        }
    }

    abstract class FFIMemberInfo
    {
        protected MemberInfo Info { get; private set; }
        private AllowRankReductionAttribute mRankReducer;
        private bool? mAllowRankReduction;

        protected FFIMemberInfo(MemberInfo info)
        {
            Info = info;
        }

        public object ReduceReturnedCollectionToSingleton(object collection)
        {
            if (null == mAllowRankReduction)
            {
                object[] atts = Info.GetCustomAttributes(false);
                foreach (var item in atts)
                {
                    mRankReducer = item as AllowRankReductionAttribute;
                    if (null != mRankReducer)
                    {
                        mAllowRankReduction = true;
                        break;
                    }
                }
            }
            //If couldn't find the attribute, return the same object.
            if (null == mRankReducer || !AllowRankReductionAttribute.IsRankReducible(collection))
            {
                mAllowRankReduction = false;
                return collection;
            }
            return mRankReducer.ReduceRank(collection);
        }

        public virtual bool IsStatic { get { return true; } }

        public virtual FFIParameterInfo[] GetParameters() { return new FFIParameterInfo[0]; }

        public abstract object Invoke(object thisObject, object[] parameters);

        public Type DeclaringType { get { return Info.DeclaringType; } }

        public string Name { get { return Info.Name; } }

        public static FFIMemberInfo CreateFrom(MemberInfo info)
        {
            MethodInfo m = info as MethodInfo;
            if (null != m)
                return new FFIMethodInfo(m);
            ConstructorInfo c = info as ConstructorInfo;
            if (null != c)
                return new FFIConstructorInfo(c);
            FieldInfo f = info as FieldInfo;
            if (null != f)
                return new FFIFieldInfo(f);

            return null;
        }

        public bool IsWrapperOf(MemberInfo info)
        {
            return this.Info.Equals(info);
        }
    }

    class FFIFieldInfo : FFIMemberInfo
    {
        private FieldInfo mField;
        bool IsEnum;
        public FFIFieldInfo(FieldInfo field)
            : base(field)
        {
            mField = field;
            IsEnum = mField.DeclaringType.IsEnum;
        }

        public override object Invoke(object thisObject, object[] parameters)
        {
            if (IsEnum)
                return mField.GetValue(mField.DeclaringType);
            return mField.GetValue(thisObject);
        }

        public override bool IsStatic
        {
            get
            {
                return mField.IsStatic;
            }
        }
    }

    class FFIMethodInfo : FFIMemberInfo
    {
        private MethodInfo mMethod;
        private FFIParameterInfo[] mParameterInfos;

        public FFIMethodInfo(MethodInfo method)
            : base(method)
        {
            mMethod = method;
        }

        public override bool IsStatic
        {
            get
            {
                return mMethod.IsStatic;
            }
        }

        public override FFIParameterInfo[] GetParameters()
        {
            if (mParameterInfos == null)
            {
                mParameterInfos = mMethod.GetParameters().Select(p => new FFIParameterInfo(p)).ToArray();
            }
            return mParameterInfos;
        }

        public override object Invoke(object thisObject, object[] parameters)
        {
            return mMethod.Invoke(thisObject, parameters);
        }
    }

    class FFIConstructorInfo : FFIMemberInfo
    {
        private FFIParameterInfo[] mParameterInfos;
        ConstructorInfo mCInfo;
        public FFIConstructorInfo(ConstructorInfo c)
            : base(c)
        {
            mCInfo = c;
        }

        public override FFIParameterInfo[] GetParameters()
        {
            if (mParameterInfos == null)
            {
                mParameterInfos = mCInfo.GetParameters().Select(p => new FFIParameterInfo(p)).ToArray();
            }
            return mParameterInfos;
        }

        public override object Invoke(object thisObject, object[] parameters)
        {
            return mCInfo.Invoke(parameters);
        }
    }

    class CLRFFIFunctionPointer : FFIFunctionPointer
    {
        public CLRDLLModule Module
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            private set;
        }
        public FFIMemberInfo ReflectionInfo
        {
            get;
            private set;
        }

        readonly ProtoCore.Type[] mArgTypes;
        readonly ProtoCore.Type mReturnType;

        public CLRFFIFunctionPointer(CLRDLLModule module, string name, MemberInfo info, List<ProtoCore.Type> argTypes, ProtoCore.Type returnType)
        {
            Module = module;
            Name = name;
            ReflectionInfo = FFIMemberInfo.CreateFrom(info);

            if (argTypes == null)
                mArgTypes = GetArgumentTypes(ReflectionInfo);
            else
                mArgTypes = argTypes.ToArray();

            mReturnType = returnType;
        }

        private ProtoCore.Type[] GetArgumentTypes(FFIMemberInfo member)
        {
            return member.GetParameters().Select(
                pi => CLRModuleType.GetProtoCoreType(pi.Info.ParameterType, Module)
                ).ToArray();
        }

        public bool Contains(string name, List<ProtoCore.Type> argTypes, ProtoCore.Type returnType)
        {
            return Contains(name, argTypes.ToArray(), returnType);
        }

        public bool Contains(string name, ProtoCore.Type[] argTypes, ProtoCore.Type returnType)
        {
            if (name != Name)
                return false;
            if (!SameTypes(mReturnType, returnType))
                return false;
            if (argTypes.Length != mArgTypes.Length)
                return false;
            for (int i = 0; i < argTypes.Length; ++i)
            {
                if (!SameTypes(argTypes[i], mArgTypes[i]))
                    return false;
            }
            return true;
        }

        private static bool SameTypes(ProtoCore.Type type1, ProtoCore.Type type2)
        {
            if (type1.Name != type2.Name)
                return false;
            if (type1.IsIndexable != type2.IsIndexable)
                return false;
            if (type1.rank != type2.rank)
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
                return true;
            CLRFFIFunctionPointer fp = obj as CLRFFIFunctionPointer;
            if (fp == null)
                return false;

            return Contains(fp.Name, fp.mArgTypes, fp.mReturnType);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        protected virtual object InvokeFunctionPointer(object thisObject, object[] parameters)
        {
            return ReflectionInfo.Invoke(thisObject, parameters);
        }

        protected object InvokeFunctionPointerNoThrow(ProtoCore.Runtime.Context c, Interpreter dsi, object thisObject, object[] parameters)
        {
            object ret = null;
            StackValue dsRetValue = StackValue.Null;
            try
            {
                FFIObjectMarshler marshaller = Module.GetMarshaller(dsi.runtime.RuntimeCore);
                ret = InvokeFunctionPointer(thisObject, parameters);
                //Reduce to singleton if the attribute is specified.
                ret = ReflectionInfo.ReduceReturnedCollectionToSingleton(ret);
                dsRetValue = marshaller.Marshal(ret, c, dsi, mReturnType);
            }
            catch (DllNotFoundException ex)
            {
                if (ex.InnerException != null)
                {
                    dsi.LogSemanticError(ex.InnerException.Message);
                }
                dsi.LogSemanticError(ex.Message);
            }
            catch (System.Reflection.TargetException ex)
            {
                if (ex.InnerException != null)
                {
                    dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ex.InnerException.Message);
                }
                dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ex.Message);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    System.Exception exc = ex.InnerException;
                    var exception = exc as ArgumentNullException;
                    if (exception != null)
                    {
                        var innerMessage = string.Format(Resources.ArgumentNullException, exception.ParamName);
                        var msg = string.Format(Resources.OperationFailType2, 
                            ReflectionInfo.DeclaringType.Name, 
                            ReflectionInfo.Name, 
                            innerMessage);

                        dsi.LogWarning(ProtoCore.Runtime.WarningID.InvalidArguments, msg);
                    }
                    else if (exc is System.ArgumentException)
                        dsi.LogWarning(ProtoCore.Runtime.WarningID.InvalidArguments, ErrorString(exc));
                    else if (exc is System.NullReferenceException)
                        dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ErrorString(null));
                    else
                        dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ErrorString(exc));
                }
                else
                    dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ErrorString(ex));
            }
            catch (System.Reflection.TargetParameterCountException ex)
            {
                if (ex.InnerException != null)
                {
                    dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ex.InnerException.Message);
                }
                dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ex.Message);
            }
            catch (System.MethodAccessException ex)
            {
                if (ex.InnerException != null)
                {
                    dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ex.InnerException.Message);
                }
                dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ex.Message);
            }
            catch (System.InvalidOperationException ex)
            {
                if (ex.InnerException != null)
                {
                    dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ex.InnerException.Message);
                }
                dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ex.Message);
            }
            catch (System.NotSupportedException ex)
            {
                if (ex.InnerException != null)
                {
                    dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ex.InnerException.Message);
                }
                dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                var innerMessage = string.Format(Resources.ArgumentNullException, ex.ParamName);
                var msg = string.Format(Resources.OperationFailType2,
                    ReflectionInfo.DeclaringType.Name,
                    ReflectionInfo.Name,
                    innerMessage);

                dsi.LogWarning(ProtoCore.Runtime.WarningID.InvalidArguments, msg);
            }
            catch (System.ArgumentException ex)
            {
                if (ex.InnerException != null)
                {
                    dsi.LogWarning(ProtoCore.Runtime.WarningID.InvalidArguments, ErrorString(ex.InnerException));
                }
                else
                    dsi.LogWarning(ProtoCore.Runtime.WarningID.InvalidArguments, ErrorString(ex));
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    dsi.LogWarning(ProtoCore.Runtime.WarningID.Default, ErrorString(ex.InnerException));
                }
                dsi.LogWarning(ProtoCore.Runtime.WarningID.Default, ErrorString(ex));
            }

            return dsRetValue;
        }

        private string ErrorString(System.Exception ex)
        {
            if (ex is System.InvalidOperationException)
                return ex.Message;

            string msg = (ex == null) ? "" : ex.Message;
            if (string.IsNullOrEmpty(msg) || msg.Contains("operation failed"))
                return string.Format(Resources.OperationFailType1, ReflectionInfo.DeclaringType.Name, ReflectionInfo.Name);

            return string.Format(Resources.OperationFailType2, ReflectionInfo.DeclaringType.Name, ReflectionInfo.Name, msg);
        }

        public override object Execute(ProtoCore.Runtime.Context c, Interpreter dsi)
        {
            List<Object> parameters = new List<object>();
            List<StackValue> s = dsi.runtime.rmem.Stack;
            Object thisObject = null;
            FFIObjectMarshler marshaller = Module.GetMarshaller(dsi.runtime.RuntimeCore);
            if (!ReflectionInfo.IsStatic)
            {
                try
                {
                    thisObject = marshaller.UnMarshal(s.Last(), c, dsi, ReflectionInfo.DeclaringType);
                }
                catch (InvalidOperationException)
                {
                    string message = String.Format(Resources.kFFIFailedToObtainThisObject, ReflectionInfo.DeclaringType.Name, ReflectionInfo.Name);
                    dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, message);
                    return null;
                }

                if (thisObject == null)
                    return null; //Can't call a method on null object.
            }

            FFIParameterInfo[] paraminfos = ReflectionInfo.GetParameters();
            List<StackValue> referencedParameters = new List<StackValue>();

            for (int i = 0; i < mArgTypes.Length; ++i)
            {
                // Comment Jun: FFI function stack frames do not contain locals
                int locals = 0;
                int relative = 0 - ProtoCore.DSASM.StackFrame.StackFrameSize - locals - i - 1;
                StackValue opArg = dsi.runtime.rmem.GetAtRelative(relative);
                try
                {
                    Type paramType = paraminfos[i].Info.ParameterType;
                    object param = null;
                    if (opArg.IsDefaultArgument)
                        param = Type.Missing;
                    else 
                        param = marshaller.UnMarshal(opArg, c, dsi, paramType);

                    if (paraminfos[i].KeepReference && opArg.IsReferenceType)
                    {
                        referencedParameters.Add(opArg);
                    }

                    //null is passed for a value type, so we must return null 
                    //rather than interpreting any value from null. fix defect 1462014 
                    if (!paramType.IsGenericType && paramType.IsValueType && param == null)
                    {
                        //This is going to cause a cast exception. This is a very frequently called problem, so we want to short-cut the execution

                        dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation,
                            string.Format(Resources.FailedToCastFromNull, paraminfos[i].Info.ParameterType.Name));
                        
                            return null;
                        //throw new System.InvalidCastException(string.Format("Null value cannot be cast to {0}", paraminfos[i].ParameterType.Name));
                        
                    }

                    parameters.Add(param);
                }
                catch (System.InvalidCastException ex)
                {
                    dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ex.Message);
                    return null;
                }
                catch (InvalidOperationException)
                {
                    string message = String.Format(Resources.kFFIFailedToObtainObject, paraminfos[i].Info.ParameterType.Name, ReflectionInfo.DeclaringType.Name, ReflectionInfo.Name);
                    dsi.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, message);
                    return null;
                }
            }

            var ret =  InvokeFunctionPointerNoThrow(c, dsi, thisObject, parameters.Count > 0 ? parameters.ToArray() : null);
            int count = referencedParameters.Count;
            if (count > 0 && (ret is StackValue))
            {
                // If there is a parameter who has attribute [KeepReference],
                // it means this parameter will cross the DesignScript boundary
                // and be referenced by C# object. Therefore, when its DS
                // wrapper object is out of scope, we shouldn't dispose it;
                // otherwise that C# object will reference to an invalid object.
                //
                // The hack here is to treat it like a property in the return
                // object. Note all DS wrapper objects are dummy objects who
                // haven't any members. By allocating extra space on the heap,
                // we store the reference in the return object so that the
                // parameter will have the same lifecycle as the return object.
                var pointer = (StackValue)ret;
                if (pointer.IsPointer)
                {
                    var dsObject = dsi.runtime.rmem.Heap.ToHeapObject<DSObject>(pointer);
                    if (dsObject != null)
                    {
                        int startIndex = dsObject.Count;
                        try
                        {
                            dsObject.ExpandBySize(count);
                            Validity.Assert(dsObject.Count >= referencedParameters.Count);

                            for (int i = 0; i < referencedParameters.Count; i++)
                            {
                                dsObject.SetValueAtIndex(startIndex + i, referencedParameters[i], dsi.runtime.RuntimeCore);
                            }
                        }
                        catch (RunOutOfMemoryException)
                        {
                            dsi.runtime.RuntimeCore.RuntimeStatus.LogWarning(ProtoCore.Runtime.WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                            return StackValue.Null;
                        }
                    }
                } 
            }
            return ret;
        }
    }

    /// <summary>
    /// Implements a special _Dispose method for all IDisposable objects.
    /// </summary>
    class DisposeFunctionPointer : CLRFFIFunctionPointer
    {
        public DisposeFunctionPointer(CLRDLLModule module, MemberInfo method, ProtoCore.Type retType)
            : base(module, ProtoCore.DSDefinitions.Keyword.Dispose, method, default(List<ProtoCore.Type>), retType)
        {
        }

        public override object Execute(ProtoCore.Runtime.Context c, Interpreter dsi)
        {
            List<StackValue> s = dsi.runtime.rmem.Stack;
            FFIObjectMarshler marshaller = Module.GetMarshaller(dsi.runtime.RuntimeCore);

            var thisObject = marshaller.UnMarshal(s.Last(), c, dsi, ReflectionInfo.DeclaringType);
            //Notify marshler for dispose.
            marshaller.OnDispose(s.Last(), c, dsi);

            Object retVal = null;
            if (ReflectionInfo.IsWrapperOf(CLRModuleType.DisposeMethod))
            {
                // For those FFI objects that are disposable but don't provide 
                // Dispose() method in their classes, they will share a same
                // Dispose() method from CLRModuleType.DisposeMethod. We need
                // to manually dispose them.

                if (thisObject != null && thisObject is IDisposable)
                {
                    var disposable = thisObject as IDisposable;
                    disposable.Dispose();
                }
            }
            else
            {
                retVal = InvokeFunctionPointerNoThrow(c, dsi, thisObject, null);
            }

            return retVal;
        }
    }
        
    class GetterFunctionPointer : CLRFFIFunctionPointer
    {
        private string PropertyName
        {
            get;
            set;
        }

        public GetterFunctionPointer(CLRDLLModule module, String functionName, MemberInfo method, ProtoCore.Type retType)
            : base(module, functionName, method, default(List<ProtoCore.Type>), retType)
        {
            string property;
            if (CoreUtils.TryGetPropertyName(functionName, out property))
            {
                PropertyName = property;
            }
        }

        public override object Execute(ProtoCore.Runtime.Context c, Interpreter dsi)
        {
            Object retVal = base.Execute(c, dsi);
            if (retVal == null)
            {
                return null;
            }

            StackValue propValue = (StackValue)retVal;
            StackValue thisObject = dsi.runtime.rmem.Stack.Last();

            bool isValidPointer = thisObject.IsPointer && thisObject.Pointer != Constants.kInvalidIndex;
            if (isValidPointer && propValue.IsReferenceType)
            {
                int classIndex = thisObject.metaData.type;
                if (classIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    var runtimeCore = dsi.runtime.RuntimeCore;
                    int idx = runtimeCore.DSExecutable.classTable.ClassNodes[classIndex].Symbols.IndexOf(PropertyName);

                    var obj = runtimeCore.Heap.ToHeapObject<DSObject>(thisObject);
                    StackValue oldValue = obj.GetValueFromIndex(idx, runtimeCore);
                    if (!StackUtils.Equals(oldValue, propValue))
                    {
                        obj.SetValueAtIndex(idx, propValue, runtimeCore);
                    }
                }
            }

            return retVal;
        }
    }
}
