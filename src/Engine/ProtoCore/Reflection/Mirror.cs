
using System;
using System.Collections.Generic;
using ProtoCore.Utils;
using ProtoCore.DSASM;
using ProtoCore.Lang;

namespace ProtoCore
{
    namespace Mirror
    {
        /// <summary>
        /// An abstract MirrorObject that represents a generic DesignScript object that can reflected
        /// Reflection on this object can be done at compiletime or runtime
        /// </summary>
        public abstract class MirrorObject
        {
            protected ProtoCore.Core core = null;
            protected static ProtoCore.Core staticCore = null;

            protected MirrorObject() { }
            
            //protected MirrorObject(ProtoCore.Core core)
            //{
            //    this.core = core;
            //}

            protected MirrorObject(ProtoCore.Core core, ProtoCore.Core staticCore = null)
            {
                this.core = core;
                MirrorObject.staticCore = staticCore;
            }
        }


        /// <summary>
        ///  A RuntimeMirror object is used to reflect on the runtime status of a single designsript variable
        /// </summary>
        public class RuntimeMirror : MirrorObject
        {
            /// <summary>
            /// This is the generic data associated with this mirror
            /// </summary>
            private MirrorData mirrorData;

            //private string assemblyName = "";
            //private string type = "";

            Dictionary<string, List<string>> AssemblyType = new Dictionary<string, List<string>>();

            //
            // TODO Jun: 
            //      Retire the mirror from DSASM.Mirror and migrat them to ProtoCore.Mirror
            //
            private ProtoCore.DSASM.Mirror.ExecutionMirror deprecateThisMirror; 


            /// <summary>
            ///  The runtime executive that we are reflecting on
            /// </summary>
            public DSASM.Executive TargetExecutive { get; private set; }

            //
            // TODO Jun: Determin if these properties can just be retrived from the symbolNode associated with the stackvalue
            private string variableName = string.Empty;
            private int blockDeclaration = ProtoCore.DSASM.Constants.kInvalidIndex;

            /// <summary>
            /// This consutructor is for instantiating a Runtime mirror object where we already have the mirrorData
            /// </summary>
            /// <param name="mirrorData"></param>
            /// <param name="core"></param>
            public RuntimeMirror(MirrorData mirrorData, ProtoCore.Core core, ProtoCore.Core staticCore = null) : base(core, staticCore)
            {
                Validity.Assert(this.core != null);
                TargetExecutive = core.CurrentExecutive.CurrentDSASMExec;
                deprecateThisMirror = new DSASM.Mirror.ExecutionMirror(TargetExecutive, core);
                this.mirrorData = mirrorData;
            }

            public RuntimeMirror(string varname, int blockDecl, ProtoCore.Core core) : base(core)
            {
                TargetExecutive = core.CurrentExecutive.CurrentDSASMExec;
                deprecateThisMirror = new DSASM.Mirror.ExecutionMirror(TargetExecutive, core);
                
                Validity.Assert(this.core != null);

                variableName = varname;
                blockDeclaration = blockDecl;
                StackValue svData = deprecateThisMirror.GetValue(variableName, blockDeclaration).DsasmValue;

                mirrorData = new MirrorData(this.core, svData);
            }

            /// <summary>
            ///  This is a helper function to be able to retrive the data inspection capabilities of the 
            ///  soon to be deprecated ExecutionMirror
            /// </summary>
            /// <returns></returns>
            public ProtoCore.DSASM.Mirror.ExecutionMirror GetUtils()
            {
                Validity.Assert(deprecateThisMirror != null);
                return deprecateThisMirror;
            }


            /// <summary>
            ///  Retrieve the data associated with this RuntimeMirror
            /// </summary>
            /// <returns></returns>
            public MirrorData GetData()
            {
                Validity.Assert(mirrorData != null);
                return mirrorData;
            }

            /// <summary>
            /// This method will return the string representation of the mirror data if it is available
            /// </summary>
            public string GetStringData()
            {
                Validity.Assert(this.core != null);
                Validity.Assert(TargetExecutive != null);
                return deprecateThisMirror.GetStringValue(mirrorData.GetStackValue(), TargetExecutive.rmem.Heap, blockDeclaration); 
            }

            // Concatenates the list of strings into a single string of comma separated types
            /*private string GetTypesHelper(List<string> types)
            {
                string type = "";
                foreach(string s in types)
                {
                    if(string.IsNullOrEmpty(type))
                        type = s;
                    else
                        type += "," + s;
                }
                return type;
            }*/

            /*private string GetAssembly(StackValue sv)
            {
                if (sv.optype == AddressType.Pointer)
                {
                    ClassNode classNode = core.ClassTable.ClassNodes[(int)sv.metaData.type];
                    assemblyName = classNode.ExternLib;
                    
                }
                else if (sv.optype == AddressType.ArrayPointer)
                {
                    assemblyName = GetTypesHelper();                    
                }
                return assemblyName;
            }*/

            // Returns a list of unique types in the input array
            //private List<string> GetArrayTypes(StackValue svData)
            private Dictionary<string, List<string>> GetArrayTypes(StackValue svData)
            {
                Dictionary<string, List<string>> asmTypes = new Dictionary<string, List<string>>();
                //List<string> types = new List<string>();

                Validity.Assert(svData.optype == AddressType.ArrayPointer);

                int ptr = (int)svData.opdata;
                HeapElement hs = core.Heap.Heaplist[ptr];
                for (int n = 0; n < hs.VisibleSize; ++n)
                {
                    StackValue sv = hs.Stack[n];
                    if (sv.optype == AddressType.ArrayPointer)
                    {
                        /*List<string> arrtypes = new List<string>();
                        arrtypes = GetArrayTypes(sv);
                        foreach (string type in arrtypes)
                        {
                            if (!types.Contains(type))
                                types.Add(type);
                        }*/
                        Dictionary<string, List<string>> types = GetArrayTypes(sv);
                        foreach (var kvp in types)
                        {
                            if (!asmTypes.ContainsKey(kvp.Key))
                            {
                                asmTypes.Add(kvp.Key, kvp.Value);
                            }
                            else
                            {
                                List<string> cTypes = asmTypes[kvp.Key];
                                // Check if each type in kvp.Value is not in cTypes
                                foreach (string s in kvp.Value)
                                {
                                    if (!cTypes.Contains(s))
                                        cTypes.Add(s);
                                }
                                //cTypes.AddRange(kvp.Value);
                            }
                        }
                    }
                    else
                    {
                        //string type = GetType(sv);
                        //if (!types.Contains(type))
                          //  types.Add(type);
                        
                        Dictionary<string, List<string>> asmType = GetType(sv);
                        var iter = asmType.GetEnumerator();
                        iter.MoveNext();
                        KeyValuePair<string, List<string>> kvp = iter.Current;
                        if (!asmTypes.ContainsKey(kvp.Key))
                        {
                            asmTypes.Add(kvp.Key, kvp.Value);
                        }
                        else
                        {
                            List<string> cTypes = asmTypes[kvp.Key];
                            cTypes.AddRange(kvp.Value);
                        }
                    }
                }

                //return types;
                return asmTypes;
            }

            // If the type is an array, it returns a list of unique types in the array
            // corresponding to one assembly and a list of assemblies if the types belong to more than one assembly
            private Dictionary<string, List<string>> GetType(StackValue sv)
            {
                Dictionary<string, List<string>> asmType = new Dictionary<string, List<string>>();
                if (sv.optype == AddressType.Pointer)
                {
                    ClassNode classNode = core.ClassTable.ClassNodes[(int)sv.metaData.type];
                    //assemblyName = classNode.ExternLib;                    
                    //return classNode.name;
                    /*if (!asmType.ContainsKey(classNode.ExternLib))
                    {
                        List<string> types = new List<string>();
                        types.Add(classNode.name);
                        asmType.Add(classNode.ExternLib, types);
                    }
                    else
                    {
                        List<string> types = asmType[classNode.ExternLib];
                        if (!types.Contains(classNode.name))
                            types.Add(classNode.name);
                    }*/
                    List<string> types = new List<string>();
                    types.Add(classNode.name);
                    asmType.Add(classNode.ExternLib, types);

                    return asmType;
                }
                else
                {
                    List<string> type = new List<string>();
                    switch (sv.optype)
                    {
                        case AddressType.ArrayPointer:
                            {
                                //List<string> types = GetArrayTypes(sv);
                                //return "array";
                                //return GetTypesHelper(types);
                                asmType = GetArrayTypes(sv);
                                break;
                            }
                        case AddressType.Int:
                            //return "int";                            
                            type.Add("int");
                            asmType.Add("", type);
                            break;
                        case AddressType.Double:
                            //return "double";
                            type.Add("double");
                            asmType.Add("", type);
                            break;
                        case AddressType.Null:
                            //return "null";
                            type.Add("null");
                            asmType.Add("", type);
                            break;
                        case AddressType.Boolean:
                            //return "bool";
                            type.Add("bool");
                            asmType.Add("", type);
                            break;
                        case AddressType.String:
                            //return "string";
                            type.Add("string");
                            asmType.Add("", type);
                            break;
                        case AddressType.Char:
                            //return "char";
                            type.Add("char");
                            asmType.Add("", type);
                            break;
                        case AddressType.FunctionPointer:
                            //return "function pointer";
                            type.Add("function pointer");
                            asmType.Add("", type);
                            break;
                        default:
                            break;   
                        
                    }
                    return asmType;
                }
            }

            public Dictionary<string, List<string>> GetDynamicAssemblyType()
            {
                Validity.Assert(mirrorData != null);
                
                return GetType(mirrorData.GetStackValue());
            }

            public string GetDynamicType()
            {
                Validity.Assert(mirrorData != null);
                ProtoCore.DSASM.Mirror.ExecutionMirror mirror = GetUtils();

                Obj obj = new Obj(mirrorData.GetStackValue());
                return mirror.GetType(obj);
            }
        }

        public abstract class StaticMirror : MirrorObject
        {
            //protected static ProtoCore.Core staticCore = null;

            protected static int numBuiltInMethods = 0;
            private static List<MethodMirror> builtInMethods = null;
            /// <summary>
            /// List of built-in methods that are preloaded by default
            /// </summary>
            public static List<MethodMirror> BuiltInMethods
            {
                get
                {
                    return GetBuiltInMethods();
                }
            }

            protected StaticMirror() { }

            protected StaticMirror(ProtoCore.Core core)
            {
                MirrorObject.staticCore = core;
            }

            protected static MethodMirror FindMethod(string methodName, List<ProtoCore.Type> arguments, List<ProcedureNode> procNodes)
            {
                foreach (var procNode in procNodes)
                {
                    if (procNode.name == methodName)
                    {
                        if (procNode.argInfoList.Count == arguments.Count)
                        {
                            bool isEqual = true;
                            for (int i = 0; i < arguments.Count; ++i)
                            {
                                if (!arguments[i].Equals(procNode.argTypeList[i]))
                                {
                                    isEqual = false;
                                    break;
                                }
                            }
                            if (isEqual)
                                return new MethodMirror(procNode);
                        }
                    }
                }
                return null;
            }

            private static List<MethodMirror> GetBuiltInMethods()
            {
                Validity.Assert(staticCore != null);

                Validity.Assert(staticCore.CodeBlockList.Count > 0);

                if (builtInMethods == null)
                {
                    List<ProcedureNode> procNodes = staticCore.CodeBlockList[0].procedureTable.procList;
                    numBuiltInMethods = procNodes.Count;
                    //List<ProcedureNode> builtIns = new List<ProcedureNode>();
                    builtInMethods = new List<MethodMirror>();

                    foreach (ProcedureNode procNode in procNodes)
                    {
                        if (!procNode.name.StartsWith(ProtoCore.DSASM.Constants.kInternalNamePrefix) && !procNode.name.Equals("Break"))
                        {
                            MethodMirror builtIn = new MethodMirror(procNode);
                            builtInMethods.Add(builtIn);
                        }
                    }
                    //builtInMethods = new MethodMirror(builtIns);

                }
                Validity.Assert(builtInMethods.Count > 0);

                return builtInMethods;
            }

            /// <summary>
            /// Returns the static return type of a function given its class, name and arguments
            /// </summary>
            /// <param name="className"></param>
            /// <param name="methodName"></param>
            /// <param name="arguments"></param>
            /// <returns></returns>
            public static ProtoCore.Type? GetType(string className, string methodName, List<ProtoCore.Type> arguments)
            {
                if (!string.IsNullOrEmpty(className))
                {
                    ClassTable classTable = staticCore.ClassTable;
                    int ci = classTable.IndexOf(className);

                    if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        ClassNode classNode = classTable.ClassNodes[ci];
                        
                        MethodMirror mm = FindMethod(methodName, arguments, classNode.vtable.procList);
                        if (mm != null)
                            return mm.ReturnType;
                    }
                }
                else // Check for global functions
                {
                    Validity.Assert(staticCore.CodeBlockList.Count > 0);

                    List<ProcedureNode> procNodes = staticCore.CodeBlockList[0].procedureTable.procList;

                    MethodMirror mm = FindMethod(methodName, arguments, procNodes);
                    if (mm != null)
                        return mm.ReturnType;
                }

                return null;
            }
        }

        /// <summary>
        ///  A ClassMirror object reflects upon the type of a single designscript variable
        ///  The information here is populated during the code generation phase
        /// </summary>
        public class ClassMirror : StaticMirror
        {
            public string ClassName { get; private set; }

            private LibraryMirror libraryMirror = null;
            private ClassNode classNode = null;

            //public ClassMirror()
            //{
            //}

            public ClassMirror(ProtoCore.Type type, ProtoCore.Core core)
            {
                if (core != null)
                {
                    ClassName = type.Name;
                    if (classNode == null)
                    {
                        ProtoCore.DSASM.ClassTable classTable = core.ClassTable;
                        classNode = classTable.ClassNodes[type.UID];
                    }
                    libraryMirror = new LibraryMirror(classNode.ExternLib, core);
                }
            }

            public ClassMirror(string className, ProtoCore.Core core)
                : base(core)
            {
                if (core == null)
                    return;

                ClassName = className;
                
                if (classNode == null)
                {

                    ProtoCore.DSASM.ClassTable classTable = core.ClassTable;
                    int ci = classTable.IndexOf(ClassName);

                    if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        classNode = classTable.ClassNodes[ci];                        
                    }
                }
                libraryMirror = new LibraryMirror(classNode.ExternLib, core);
            }

            //public ClassMirror(string className)
            //{
            //    ClassName = className;
            //}

            internal ClassMirror(ProtoCore.Core core, ProtoCore.DSASM.ClassNode classNode, LibraryMirror libraryMirror) : base(core)
            {
                ClassName = classNode.name;
                this.libraryMirror = libraryMirror;
                this.classNode = classNode;
            }

            /// <summary>
            /// Constructor to construct ClassMirror from runtime data i.e. StackValue
            /// </summary>
            /// <param name="svData">StackValue</param>
            /// <param name="core">ProtoCore.Core</param>
            internal ClassMirror(StackValue svData, ProtoCore.Core core)
                : base(core)
            {
                Validity.Assert(svData.optype == AddressType.Pointer);
                Validity.Assert(null != core);
                Validity.Assert(null != core.DSExecutable.classTable);

                IList<ClassNode> classNodes = core.DSExecutable.classTable.ClassNodes;
                Validity.Assert(classNodes != null && classNodes.Count > 0);

                this.classNode = classNodes[(int)svData.metaData.type];
                this.ClassName = this.classNode.name;
                libraryMirror = new LibraryMirror(classNode.ExternLib, core);
            }

            /// <summary>
            /// Returns the library mirror of the assembly that the class belongs to
            /// </summary>
            /// <returns></returns>
            public LibraryMirror GetAssembly()
            {
                return libraryMirror;
            }

            /// <summary>
            ///  Get the super class of this class
            /// </summary>
            /// <returns></returns>
            public ClassMirror GetSuperClass()
            {
                Validity.Assert(!string.IsNullOrEmpty(ClassName));
                Validity.Assert(staticCore != null);

                int ci;
                if (classNode == null)
                {
                    
                    ProtoCore.DSASM.ClassTable classTable = staticCore.ClassTable;
                    ci = classTable.IndexOf(ClassName);

                    if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        classNode = classTable.ClassNodes[ci];
                    }
                }              
                
                ci = classNode.baseList[0];
                Validity.Assert(ci != ProtoCore.DSASM.Constants.kInvalidIndex);

                return new ClassMirror(staticCore, staticCore.ClassTable.ClassNodes[ci], this.libraryMirror);                

            }

            /// <summary>
            /// Returns the base class hierarchy for the given class
            /// </summary>
            /// <returns></returns>
            public List<ClassMirror> GetClassHierarchy()
            {
                List<ClassMirror> baseClasses = new List<ClassMirror>();
                
                Validity.Assert(!string.IsNullOrEmpty(ClassName));
                Validity.Assert(staticCore != null);

                int ci;
                if (classNode == null)
                {
                    
                    ProtoCore.DSASM.ClassTable classTable = staticCore.ClassTable;
                    ci = classTable.IndexOf(ClassName);

                    if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        classNode = classTable.ClassNodes[ci];
                    }
                }

                ClassNode cNode = classNode;
                while (cNode.baseList.Count > 0)
                {

                    ci = cNode.baseList[0];
                    Validity.Assert(ci != ProtoCore.DSASM.Constants.kInvalidIndex);

                    baseClasses.Add(new ClassMirror(staticCore, staticCore.ClassTable.ClassNodes[ci], this.libraryMirror));

                    cNode = staticCore.ClassTable.ClassNodes[ci];
                }
                return baseClasses;
            }

            /// <summary>
            ///  Returns the list of class properties of this class 
            /// </summary>
            /// <returns> symbol nodes</returns>
            public List<PropertyMirror> GetProperties()
            {
                List<PropertyMirror> properties = new List<PropertyMirror>();
                
                Dictionary<string, ProcedureNode> setterMap = new Dictionary<string, ProcedureNode>();
                string name = string.Empty;

                if (classNode == null)
                {
                    Validity.Assert(staticCore != null);
                
                    ProtoCore.DSASM.ClassTable classTable = staticCore.ClassTable;
                    int ci = classTable.IndexOf(ClassName);

                    
                    if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        classNode = classTable.ClassNodes[ci];
                    }
                }
                
                ProcedureTable procedureTable = classNode.vtable;
                List<ProcedureNode> procList = procedureTable.procList;
                string getterPrefix = ProtoCore.DSASM.Constants.kGetterPrefix;
                string setterPrefix = ProtoCore.DSASM.Constants.kSetterPrefix;

                foreach (ProcedureNode pNode in procList)
                {
                    name = pNode.name;
                    if (name.Contains(getterPrefix) && pNode.argInfoList.Count == 0 )
                    {
                        properties.Add(new PropertyMirror(pNode));
                    }
                    else if (name.Contains(setterPrefix) && pNode.argInfoList.Count == 1 && !pNode.isAutoGeneratedThisProc)
                    {
                        if (setterMap.ContainsKey(name))
                        {
                            ProcedureNode proc = setterMap[name];
                            if (proc.argTypeList[0].UID == (int)ProtoCore.PrimitiveType.kTypeVar && pNode.argTypeList[0].UID != (int)ProtoCore.PrimitiveType.kTypeVar)
                            {
                                setterMap.Remove(name);
                                setterMap.Add(name, pNode);
                            }
                        }
                        else
                        {
                            setterMap.Add(name, pNode);          
                        }
                    }
                }
                foreach (var kvp in setterMap)
                {
                    properties.Add(new PropertyMirror(kvp.Value, true));
                }
                
                return properties;
                
            }

            /// <summary>
            /// Returns the list of constructors defined for the given class
            /// </summary>
            /// <returns></returns>
            public List<MethodMirror> GetConstructors()
            {
                List<MethodMirror> constructors = new List<MethodMirror>();
                
                if(classNode == null)
                {
                    Validity.Assert(staticCore != null);
                    ProtoCore.DSASM.ClassTable classTable = staticCore.ClassTable;
                    int ci = classTable.IndexOf(ClassName);

                    if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        classNode = classTable.ClassNodes[ci];
                    }
                }
                
                ProcedureTable procedureTable = classNode.vtable;
                List<ProcedureNode> procList = procedureTable.procList;
                foreach (ProcedureNode pNode in procList)
                {
                    if (pNode.isConstructor == true)
                        constructors.Add(new MethodMirror(pNode));
                }
                                
                return constructors;
            }

            /// <summary>
            ///  Returns the list of function properties of the class only
            /// </summary>
            /// <returns> function nodes </returns>
            public List<MethodMirror> GetFunctions()
            {
                List<MethodMirror> methods = new List<MethodMirror>();
                string name = string.Empty;
                
                if(classNode == null)
                {
                    Validity.Assert(staticCore != null);

                    ProtoCore.DSASM.ClassTable classTable = staticCore.ClassTable;
                    int ci = classTable.IndexOf(ClassName);

                    if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        classNode = classTable.ClassNodes[ci];
                    }
                }
                
                ProcedureTable procedureTable = classNode.vtable;
                List<ProcedureNode> procList = procedureTable.procList;
                    
                foreach (ProcedureNode pNode in procList)
                {
                    name = pNode.name;
                    if (!pNode.isAssocOperator && !pNode.isAutoGenerated && !pNode.isAutoGeneratedThisProc && !pNode.isConstructor)
                    {
                        methods.Add(new MethodMirror(pNode));
                    }
                }
                
                return methods;                
            }

            public List<MethodMirror> GetOverloads(string methodName)
            {
                List<MethodMirror> methods = new List<MethodMirror>();
                string name = string.Empty;

                if(classNode == null)
                {
                    Validity.Assert(staticCore != null);

                    ProtoCore.DSASM.ClassTable classTable = staticCore.ClassTable;
                    int ci = classTable.IndexOf(ClassName);
                    
                    if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        classNode = classTable.ClassNodes[ci];
                    }
                }
                
                ProcedureTable procedureTable = classNode.vtable;
                List<ProcedureNode> procList = procedureTable.procList;

                foreach (ProcedureNode pNode in procList)
                {
                    name = pNode.name;
                    if (name == methodName)
                    {
                        methods.Add(new MethodMirror(pNode));
                    }
                }
                

                return methods;
            }

            public MethodMirror GetDeclaredMethod(string methodName, List<ProtoCore.Type> argumentTypes)
            {
                if (classNode == null)
                {
                    Validity.Assert(staticCore != null);

                    ProtoCore.DSASM.ClassTable classTable = staticCore.ClassTable;
                    int ci = classTable.IndexOf(ClassName);

                    if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        classNode = classTable.ClassNodes[ci];
                    }
                }

                ProcedureTable procedureTable = classNode.vtable;
                List<ProcedureNode> procList = procedureTable.procList;
                            
                return StaticMirror.FindMethod(methodName, argumentTypes, procList);
            }
        }

        /// <summary>
        /// Reflects upon a Function to retrieve its arguments
        /// </summary>
        public class MethodMirror : StaticMirror
        {
            public string MethodName { get; private set; }
            public bool IsConstructor { get; private set; }
            public bool IsStatic { get; private set; }
            //public bool IsAutoGenerated { get; private set; }
            //public bool IsAutoGeneratedThisProc { get; private set; }
            public ProtoCore.Type? ReturnType 
            { 
                get 
                {
                    if (procNode != null)
                        return procNode.returntype;
                    else
                        return null;
                } 
            }

            private ProcedureNode procNode;

            internal MethodMirror(ProcedureNode procNode)
            {
                MethodName = procNode.name;
                IsConstructor = procNode.isConstructor;
                IsStatic = procNode.isStatic;
                //IsAutoGenerated = procNode.isAutoGenerated;
                //IsAutoGeneratedThisProc = procNode.isAutoGeneratedThisProc;

                this.procNode = procNode;
            }

            //private bool IsMethodProperty()
            //{
            //    return false;
            //}

            public List<string> GetArgumentNames()
            {
                List<string> argNames = new List<string>();
                if (procNode != null)
                {
                    List<ArgumentInfo> argList = procNode.argInfoList;
                    foreach (var arg in argList)
                    {
                        argNames.Add(arg.Name);
                    }
                }
                return argNames;
            }

            public List<ProtoCore.Type> GetArgumentTypes()
            {
                List<ProtoCore.Type> argTypes = new List<ProtoCore.Type>();
                if (procNode != null)
                {
                    foreach (var arg in procNode.argTypeList)
                    {
                        argTypes.Add(arg);
                    }
                }
                return argTypes;
            }

            
        }

        public class PropertyMirror : StaticMirror
        {
            private ProcedureNode procNode = null;
            public ProtoCore.Type? Type
            {
                get
                {
                    if(procNode != null)
                    {
                        if(isSetter)
                            return procNode.argTypeList[0];
                        else
                            return procNode.returntype;
                    }
                    return null;
                }
                set{}
            }


            public string PropertyName { get;private set; }

            private bool isSetter = false;
            public bool IsSetter
            {
                get
                {
                    return isSetter;
                }
            }

            public bool IsStatic 
            { 
                get 
                { 
                    return procNode.isStatic; 
                } 
            }

            //private PropertyMirror(string name)
            //{
            //    PropertyName = name;
            //}

            internal PropertyMirror(ProcedureNode procNode, bool isSetter = false)
            {
                this.procNode = procNode;

                string getterPrefix = ProtoCore.DSASM.Constants.kGetterPrefix;
                int prefixLength = getterPrefix.Length;                
                PropertyName = procNode.name.Substring(prefixLength); 

                this.isSetter = isSetter;
            }
        }

        /// <summary>
        /// The LibraryMirror reflects upon an assembly or DS file to return assembly specific information
        /// such as imported classes, global methods, etc.
        /// </summary>
        public class LibraryMirror : StaticMirror
        {
            public string LibraryName { get; set; }

            private List<ClassMirror> classMirrors = null;
            private List<MethodMirror> globalMethods = null;
            
            //public LibraryMirror()
            //{ }

            public LibraryMirror(string libName, ProtoCore.Core core)
                : base(core)
            {
                LibraryName = libName;
            }

            //public LibraryMirror(string libName)
            //{
            //    LibraryName = libName;
            //}

            public LibraryMirror(ProtoCore.Core core, string libName, IList<ProtoCore.DSASM.ClassNode> classNodes) : base(core)
            {
                LibraryName = libName;

                classMirrors = new List<ClassMirror>();
                foreach (ProtoCore.DSASM.ClassNode cnode in classNodes)
                {
                    ClassMirror classMirror = new ClassMirror(core, cnode, this);
                    classMirrors.Add(classMirror);
                }
            }

            /// <summary>
            /// Returns list of classes imported from a given assembly
            /// </summary>
            /// <returns></returns>
            public List<ClassMirror> GetClasses()
            {
                return classMirrors;
            }

            /// <summary>
            /// Returns list of global methods defined in an imported DS file
            /// </summary>
            /// <returns></returns>
            public List<MethodMirror> GetGlobalMethods()
            {
                if (globalMethods == null)
                {
                    List<MethodMirror> methods = new List<MethodMirror>();

                    Validity.Assert(staticCore != null);

                    Validity.Assert(staticCore.CodeBlockList.Count > 0);

                    List<ProcedureNode> procNodes = staticCore.CodeBlockList[0].procedureTable.procList;

                    int numNewMethods = procNodes.Count - numBuiltInMethods;
                    Validity.Assert(numNewMethods >= 0);

                    for (int i = numBuiltInMethods; i < procNodes.Count; ++i)
                    {
                        MethodMirror method = new MethodMirror(procNodes[i]);
                        methods.Add(method);
                    }

                    numBuiltInMethods = procNodes.Count;

                    globalMethods = methods;
                }
                
                return globalMethods;
            }

            public List<MethodMirror> GetOverloads(string methodName)
            {
                List<MethodMirror> globalMethods = GetGlobalMethods();
                List<MethodMirror> overloads = new List<MethodMirror>();

                if (globalMethods == null)
                    return null;

                foreach (var method in globalMethods)
                {
                    if (method.MethodName == methodName)
                    {
                        overloads.Add(method);
                    }
                }

                return overloads;
            }

            public MethodMirror GetDeclaredMethod(string className, string methodName, List<ProtoCore.Type> argumentTypes)
            {
                // Check global methods if classname is empty or null
                if (string.IsNullOrEmpty(className))
                {
                    List<MethodMirror> methods = null;
                    methods = GetGlobalMethods();
                    foreach (var method in methods)
                    {
                        if (method.MethodName == methodName)
                        {
                            List<ProtoCore.Type> argTypes = method.GetArgumentTypes();
                            if (argTypes.Count == argumentTypes.Count)
                            {
                                bool isEqual = true;
                                for (int i = 0; i < argumentTypes.Count; ++i)
                                {
                                    if (!argumentTypes[i].Equals(argTypes[i]))
                                    {
                                        isEqual = false;
                                        break;
                                    }
                                }
                                if (isEqual)
                                    return method;
                            }
                        }
                    }
                }
                else // find method in Class
                {

                    Validity.Assert(staticCore != null);

                    ClassNode classNode = null;
                    ProtoCore.DSASM.ClassTable classTable = staticCore.ClassTable;
                    int ci = classTable.IndexOf(className);

                    if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        classNode = classTable.ClassNodes[ci];
                    }
                

                    ProcedureTable procedureTable = classNode.vtable;
                    List<ProcedureNode> procList = procedureTable.procList;

                    return StaticMirror.FindMethod(methodName, argumentTypes, procList);
                }

                return null;
            }

            public enum LibraryType
            {
                kDSfile = 0,
                kDLL,
                kEXE
            }

            public LibraryType GetLibraryType()
            {
                return LibraryType.kDLL;
            }
        }
    }
}
