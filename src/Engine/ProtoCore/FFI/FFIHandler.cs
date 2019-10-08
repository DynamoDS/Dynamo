using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;

namespace ProtoFFI
{
    public abstract class FFIFunctionPointer
    {
        public bool IsDNI { get; set; }

        [IsObsolete("Remove in 3.0. Use Execute(ProtoCore.Runtime.Context c, ProtoCore.DSASM.Interpreter dsi, List<StackValue> stack) instead")]
        public abstract object Execute(ProtoCore.Runtime.Context c, Interpreter dsi);
        public abstract Object Execute(ProtoCore.Runtime.Context c, ProtoCore.DSASM.Interpreter dsi, List<StackValue> stack);
        public static T[] GetUnderlyingArray<T>(List<T> list)
        {
            return list.ToArray();
        }
    }

    abstract public class FFIHandler
    {
        public string ModuleType;
        public abstract FFIFunctionPointer GetFunctionPointer(string moduleName, string className, string functionName, List<ProtoCore.Type> parameterTypes, ProtoCore.Type returnType);
    }

    abstract public class DLLModule
    {
        public abstract List<FFIFunctionPointer> GetFunctionPointers(string className, string name);
        public abstract FFIFunctionPointer GetFunctionPointer(string className, string name, List<ProtoCore.Type> argTypes, ProtoCore.Type returnType);
        public virtual CodeBlockNode ImportCodeBlock(string typeName, string alias, CodeBlockNode refnode) { return null; } //All modules don't support import
        public virtual FFIObjectMarshaler GetMarshaler(ProtoCore.RuntimeCore runtimeCore) { return null; }
        public virtual Type GetExtensionAppType() { return null; }
    }

    public abstract class ModuleHelper
    {
        public abstract DLLModule getModule(String name);
        public abstract FFIObjectMarshaler GetMarshaler(ProtoCore.RuntimeCore runtimeCore);
    }

    /// <summary>
    /// This class is responsible for marshaling of FFI objects to DS world and 
    /// vice-versa.
    /// </summary>
    public abstract class FFIObjectMarshaler
    {
        /// <summary>
        /// Marshales a given FFI object to DS Object of given ProtoCore.Type
        /// </summary>
        /// <param name="obj">The FFI object</param>
        /// <param name="context">DS execution context</param>
        /// <param name="dsi">The current runtime interpreter</param>
        /// <param name="type">Type of DS Object expected</param>
        /// <returns>Marshaled object as Operand</returns>
        public abstract StackValue Marshal(object obj, ProtoCore.Runtime.Context context, Interpreter dsi, ProtoCore.Type type);

        /// <summary>
        /// UnMarshales a given DS object to FFI object of given System.Type
        /// </summary>
        /// <param name="dsObject">The DS object</param>
        /// <param name="context">DS execution context</param>
        /// <param name="dsi">The current runtime interpreter</param>
        /// <param name="type">Type of FFI object expected</param>
        /// <returns>Unmarshaled FFI object</returns>
        public abstract object UnMarshal(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi, System.Type type);

        /// <summary>
        /// Returns the marshaled Type of DS object for a given FFI object type
        /// </summary>
        /// <param name="type">Type of FFI object</param>
        /// <returns>Type of DS object</returns>
        public abstract ProtoCore.Type GetMarshaledType(System.Type type);

        /// <summary>
        /// This is a callback method called when the given DS object is disposed. 
        /// Marshaler gets an opportunity to clear the cache related to the given
        /// DS object.
        /// </summary>
        /// <param name="dsObject">DS object being disposed</param>
        /// <param name="context">DS execution context</param>
        /// <param name="dsi">The current runtime interpreter</param>
        public abstract void OnDispose(StackValue dsObject, ProtoCore.Runtime.Context context, Interpreter dsi); //callback method

        /// <summary>
        /// Returns a string representation for given DS object
        /// </summary>
        /// <param name="dsObject">DS Object</param>
        /// <returns>string representation of a DS object</returns>
        public abstract string GetStringValue(StackValue dsObject);
    }

    public enum FFILanguage
    {
        CPlusPlus,
        CSharp
    }

    public class DLLFFIHandler : FFIHandler
    {
        static DLLFFIHandler()
        {
            Register(); //Do the default initialization for CSharp FFI handlers
        }

        private DLLFFIHandler() { }
        private static readonly Dictionary<FFILanguage, ModuleHelper> helpers = new Dictionary<FFILanguage, ModuleHelper>();
        private static readonly DLLFFIHandler Instance = new DLLFFIHandler();
        private static bool registered;
        public static Dictionary<string, DLLModule> Modules = new Dictionary<string, DLLModule>(StringComparer.CurrentCultureIgnoreCase);
        public static IntPtr Env
        {
            get;
            set;
        }
        public static void Register()
        {
            if (!ProtoCore.Lang.FFIFunctionEndPoint.FFIHandlers.ContainsKey("dll"))
            {
                ProtoCore.Lang.FFIFunctionEndPoint.FFIHandlers.Add("dll", Instance);
                helpers.Add(FFILanguage.CSharp, new CSModuleHelper());
            }
            registered = true;
        }

        public static void Register(FFILanguage type, ModuleHelper helper)
        {
            if (!registered)
                Register();

            if (!helpers.ContainsKey(type))
                helpers.Add(type, helper);
        }

        public override FFIFunctionPointer GetFunctionPointer(string dllModuleName, string className, string functionName, List<ProtoCore.Type> parameterTypes, ProtoCore.Type returnType)
        {
            DLLModule dllModule = GetModule(dllModuleName);
            return dllModule.GetFunctionPointer(className, functionName, parameterTypes, returnType);
        }

        public static ModuleHelper GetModuleHelper(FFILanguage language)
        {
            if (language == FFILanguage.CPlusPlus || language == FFILanguage.CSharp)
            {
                return helpers[language];
            }
            return null;
        }

        public static DLLModule GetModule(string dllModuleName)
        {
            string moduleFileName = System.IO.Path.GetFileName(dllModuleName);
            if (!Modules.ContainsKey(moduleFileName))
            {
                try
                {
                    Modules.Add(moduleFileName, helpers[FFILanguage.CSharp].getModule(dllModuleName));
                }
                catch
                {
                    //try loading c++
                    try
                    {
                        Modules.Add(moduleFileName, helpers[FFILanguage.CPlusPlus].getModule(dllModuleName));
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            DLLModule dllModule = Modules[moduleFileName];
            return dllModule;
        }
    }

    //public class PYthonFFIHandler
    //{
    //}
}

