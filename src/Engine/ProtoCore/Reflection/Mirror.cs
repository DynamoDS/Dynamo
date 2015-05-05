
using System;
using System.Collections.Generic;
using System.Linq;
using ProtoCore.Utils;
using ProtoCore.DSASM;
using ProtoCore.Lang;
using ProtoCore.AST.AssociativeAST;
using System.Text;

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
            protected ProtoCore.RuntimeCore runtimeCore = null;
            protected static ProtoCore.Core staticCore = null;

            protected MirrorObject() { }

            //protected MirrorObject(ProtoCore.Core core)
            //{
            //    this.core = core;
            //}

            protected MirrorObject(ProtoCore.RuntimeCore runtimeCore, ProtoCore.Core staticCore = null)
            {
                this.runtimeCore = runtimeCore;
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
            public RuntimeMirror(MirrorData mirrorData, ProtoCore.RuntimeCore runtimeCoreReflect, ProtoCore.Core staticCore = null)
                : base(runtimeCoreReflect, staticCore)
            {
                Validity.Assert(this.runtimeCore != null);
                TargetExecutive = runtimeCoreReflect.CurrentExecutive.CurrentDSASMExec;
                deprecateThisMirror = new DSASM.Mirror.ExecutionMirror(TargetExecutive, runtimeCoreReflect);
                this.mirrorData = mirrorData;
            }

            public RuntimeMirror(string varname, int blockDecl, ProtoCore.RuntimeCore runtimeCore, ProtoCore.Core staticCore = null)
                : base(runtimeCore, staticCore)
            {
                TargetExecutive = runtimeCore.CurrentExecutive.CurrentDSASMExec;
                deprecateThisMirror = new DSASM.Mirror.ExecutionMirror(TargetExecutive, runtimeCore);

                Validity.Assert(this.runtimeCore != null);

                variableName = varname;
                blockDeclaration = blockDecl;
                StackValue svData = deprecateThisMirror.GetValue(variableName, blockDeclaration).DsasmValue;

                mirrorData = new MirrorData(staticCore, this.runtimeCore, svData);
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
                Validity.Assert(this.runtimeCore != null);
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
                if (sv.IsObject())
                {
                    ClassNode classNode = core.ClassTable.ClassNodes[(int)sv.metaData.type];
                    assemblyName = classNode.ExternLib;
                    
                }
                else if (sv.IsArray)
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

                Validity.Assert(svData.IsArray);

                HeapElement hs = runtimeCore.RuntimeMemory.Heap.GetHeapElement(svData);
                foreach (var sv in hs.VisibleItems)
                {
                    if (sv.IsArray)
                    {
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
                            }
                        }
                    }
                    else
                    {
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
                if (sv.IsPointer)
                {
                    ClassNode classNode = runtimeCore.DSExecutable.classTable.ClassNodes[sv.metaData.type];
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

            public IEnumerable<String> GetMembers()
            {
                List<string> members = new List<string>();
                Validity.Assert(mirrorData != null);

                ClassMirror type = mirrorData.Class;
                if (type == null)
                    return members;

                IEnumerable<ClassMirror> baseClasses = type.GetClassHierarchy();
                foreach (var baseClass in baseClasses)
                {
                    foreach (var method in baseClass.GetFunctions())
                    {
                        if (!members.Contains(method.MethodName))
                            members.Add(method.MethodName);
                    }

                    foreach (var property in baseClass.GetProperties())
                    {
                        if (!members.Contains(property.PropertyName))
                            members.Add(property.PropertyName);
                    }
                }

                foreach (var method in type.GetFunctions())
                {
                    if (!members.Contains(method.MethodName))
                        members.Add(method.MethodName);
                }
                foreach (var property in type.GetProperties())
                {
                    if (!members.Contains(property.PropertyName))
                        members.Add(property.PropertyName);
                }
                return members;
            }


        }

        /// <summary>
        /// StaticMirror is a base class representing all Mirror classes that 
        /// perform static (build time) reflection on types
        /// Static reflection can be done without executing the code
        /// </summary>
        public abstract class StaticMirror : MirrorObject
        {
            protected static int numBuiltInMethods = 0;

            /// <summary>
            /// Name of the Mirror object - In the case of:
            /// ClassMirror: class name
            /// MethodMirror: method name
            /// PropertyMirror: property name
            /// </summary>
            public string Name { get; protected set; }

            protected StaticMirror() { }

            protected StaticMirror(ProtoCore.Core core, string name = "")
            {
                if (core == null)
                    throw new ArgumentNullException("core");

                MirrorObject.staticCore = core;
                Name = name;
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

            /// <summary>
            /// Returns list of overloads (one or more) for a given built-in method
            /// </summary>
            /// <param name="core"></param>
            /// <param name="methodName"></param>
            /// <returns></returns>
            public static IEnumerable<MethodMirror> GetOverloadsOnBuiltIns(Core core, string methodName)
            {
                if (core == null || string.IsNullOrEmpty(methodName))
                    throw new ArgumentNullException();

                return GetBuiltInMethods(core).Where(x => x.MethodName == methodName);
            }

            public static IEnumerable<ClassMirror> GetClasses(Core core)
            {
                return core.ClassTable.ClassNodes.Skip((int)PrimitiveType.kMaxPrimitives).
                    Where(x => !CoreUtils.StartsWithSingleUnderscore(x.name)).
                    Select(x => new ClassMirror(core, x));
            }

            public static IEnumerable<ClassMirror> GetAllTypes(Core core)
            {
                // TODO: Get rid of keyword "PointerReserved" and PrimitiveType.kTypePointer
                // if not used in the language: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-6752
                return core.ClassTable.ClassNodes.
                    Where(x => !CoreUtils.StartsWithSingleUnderscore(x.name)
                    && x.name != DSDefinitions.Keyword.PointerReserved).
                    Select(x => new ClassMirror(core, x));
            }

            public static IEnumerable<StaticMirror> GetGlobals(Core core)
            {
                List<StaticMirror> members = new List<StaticMirror>();
                members.AddRange(GetBuiltInMethods(core));

                var classes = GetClasses(core);
                members.AddRange(classes.SelectMany(x => x.GetConstructors()));
                members.AddRange(classes.SelectMany(x => x.GetFunctions()));
                members.AddRange(classes.SelectMany(x => x.GetProperties()));

                return members;
            }

            /// <summary>
            /// List of built-in methods that are preloaded by default
            /// </summary>
            public static IEnumerable<MethodMirror> GetBuiltInMethods(Core core)
            {
                if (core == null)
                    throw new ArgumentNullException();

                Validity.Assert(core.CodeBlockList.Count > 0);

                List<ProcedureNode> procNodes = core.CodeBlockList[0].procedureTable.procList;
                numBuiltInMethods = procNodes.Count;

                return procNodes.Where(
                    x =>
                    {
                        bool hidden = x.MethodAttribute == null ? false : x.MethodAttribute.HiddenInLibrary;
                        return !x.isAssocOperator
                            && !x.isAutoGenerated
                            && !hidden
                            && !x.name.StartsWith(ProtoCore.DSASM.Constants.kInternalNamePrefix)
                            && !x.name.Equals("Break")
                            && !CoreUtils.StartsWithDoubleUnderscores(x.name);

                    }
                    ).Select(y => new MethodMirror(y));
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

            public override string ToString()
            {
                return this.Name;
            }
        }

        /// <summary>
        ///  A ClassMirror object reflects upon the type of a single designscript variable
        ///  The information here is populated during the code generation phase
        /// </summary>
        public class ClassMirror : StaticMirror
        {
            /// <summary>
            /// Fully qualified class name
            /// </summary>
            public string ClassName { get; private set; }

            private string alias = null;
            /// <summary>
            /// Class name
            /// </summary>
            public string Alias
            {
                get
                {
                    return alias ?? (alias = ClassName.Split('.').Last());
                }
                set { alias = value; }
            }

            private LibraryMirror libraryMirror = null;

            private ClassNode classNode = null;
            private ClassNode ClassNode
            {
                get
                {
                    if (classNode == null)
                    {
                        ProtoCore.DSASM.ClassTable classTable = staticCore.ClassTable;
                        var ci = classTable.IndexOf(ClassName);

                        if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                        {
                            classNode = classTable.ClassNodes[ci];
                        }
                    }
                    return classNode;
                }
            }

            public bool IsHiddenInLibrary
            {
                get
                {
                    return ClassNode.ClassAttributes == null ? false : ClassNode.ClassAttributes.HiddenInLibrary;
                }
            }

            public ClassMirror(ProtoCore.Type type, ProtoCore.Core core)
                : base(core, type.Name)
            {
                ClassName = type.Name;
                if (classNode == null)
                {
                    ProtoCore.DSASM.ClassTable classTable = core.ClassTable;
                    classNode = classTable.ClassNodes[type.UID];
                }
                libraryMirror = new LibraryMirror(classNode.ExternLib, core);
            }

            public ClassMirror(string className, ProtoCore.Core core)
                : base(core, className)
            {
                ClassName = className;

                if (classNode == null)
                {

                    ProtoCore.DSASM.ClassTable classTable = core.ClassTable;
                    int ci = classTable.IndexOf(ClassName);

                    if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        classNode = classTable.ClassNodes[ci];
                    }
                    else
                        throw new Exception(String.Format("Class {0} not defined", className));
                }
                libraryMirror = new LibraryMirror(classNode.ExternLib, core);
            }

            public ClassMirror(int classIndex, ProtoCore.Core core)
            {
                if (classIndex == Constants.kInvalidIndex)
                {
                    throw new ArgumentException("classIndex is invalid");
                }

                ProtoCore.DSASM.ClassTable classTable = core.ClassTable;
                classNode = classTable.ClassNodes[classIndex];
                libraryMirror = new LibraryMirror(classNode.ExternLib, core);
                ClassName = classNode.name;
            }

            internal ClassMirror(ProtoCore.Core core, ProtoCore.DSASM.ClassNode classNode,
                LibraryMirror libraryMirror = null)
                : base(core, classNode.name)
            {
                ClassName = classNode.name;
                if (libraryMirror == null)
                    this.libraryMirror = new LibraryMirror(classNode.ExternLib, core);
                else
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
                Validity.Assert(svData.IsPointer);
                Validity.Assert(null != core.DSExecutable.classTable);

                IList<ClassNode> classNodes = core.DSExecutable.classTable.ClassNodes;
                Validity.Assert(classNodes != null && classNodes.Count > 0);

                this.classNode = classNodes[svData.metaData.type];
                this.ClassName = this.classNode.name;
                this.Name = this.ClassName;
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
            /// Returns the constructors and static methods and properties 
            /// belonging to the type and its base types
            /// </summary>
            /// <returns></returns>
            public IEnumerable<StaticMirror> GetMembers()
            {
                // TODO: Factor out reflection functionality for both LibraryServices and Mirrors
                List<StaticMirror> members = new List<StaticMirror>();

                IEnumerable<ClassMirror> baseClasses = this.GetClassHierarchy();
                foreach (var baseClass in baseClasses)
                {
                    members.AddRange(baseClass.GetFunctions().Where(m => m.IsStatic).GroupBy(x => x.Name).Select(y => y.First()));
                    members.AddRange(baseClass.GetProperties().Where(m => m.IsStatic).GroupBy(x => x.Name).Select(y => y.First()));
                }

                members.AddRange(this.GetConstructors().GroupBy(x => x.Name).Select(y => y.First()));
                members.AddRange(this.GetFunctions().Where(m => m.IsStatic).GroupBy(x => x.Name).Select(y => y.First()));
                members.AddRange(this.GetProperties().Where(m => m.IsStatic).GroupBy(x => x.Name).Select(y => y.First()));
                return members;
            }


            /// <summary>
            ///  Get the super class of this class
            /// </summary>
            /// <returns></returns>
            public ClassMirror GetSuperClass()
            {
                Validity.Assert(!string.IsNullOrEmpty(ClassName));
                Validity.Assert(staticCore != null);

                int ci = ClassNode.baseList[0];
                Validity.Assert(ci != ProtoCore.DSASM.Constants.kInvalidIndex);

                return new ClassMirror(staticCore, staticCore.ClassTable.ClassNodes[ci], this.libraryMirror);

            }

            /// <summary>
            /// Returns the base class hierarchy for the given class
            /// </summary>
            /// <returns></returns>
            public IEnumerable<ClassMirror> GetClassHierarchy()
            {
                List<ClassMirror> baseClasses = new List<ClassMirror>();

                Validity.Assert(!string.IsNullOrEmpty(ClassName));
                Validity.Assert(staticCore != null);

                ClassNode cNode = ClassNode;
                while (cNode.baseList.Count > 0)
                {

                    int ci = cNode.baseList[0];
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
            public IEnumerable<PropertyMirror> GetProperties()
            {
                List<PropertyMirror> properties = new List<PropertyMirror>();

                string name = string.Empty;

                ProcedureTable procedureTable = ClassNode.vtable;
                List<ProcedureNode> procList = procedureTable.procList;
                string getterPrefix = ProtoCore.DSASM.Constants.kGetterPrefix;
                int prefixLength = getterPrefix.Length;

                foreach (ProcedureNode pNode in procList)
                {
                    name = pNode.name;
                    bool hidden = pNode.MethodAttribute == null ? false : pNode.MethodAttribute.HiddenInLibrary;
                    if (!hidden && name.Contains(getterPrefix) && pNode.argInfoList.Count == 0)
                    {
                        properties.Add(new PropertyMirror(pNode));
                    }
                }
                
                return properties;
            }

            /// <summary>
            /// Returns the list of constructors defined for the given class
            /// </summary>
            /// <returns></returns>
            public IEnumerable<MethodMirror> GetConstructors()
            {
                return ClassNode.vtable.procList.Where(
                    x =>
                    {
                        bool hidden = x.MethodAttribute == null ? false : x.MethodAttribute.HiddenInLibrary;
                        return x.isConstructor && !hidden;
                    }
                    ).Select(y => new MethodMirror(y));
            }

            /// <summary>
            ///  Returns the list of functions of the class only
            /// </summary>
            /// <returns> function nodes </returns>
            public IEnumerable<MethodMirror> GetFunctions()
            {
                List<MethodMirror> methods = new List<MethodMirror>();

                ProcedureTable procedureTable = ClassNode.vtable;
                List<ProcedureNode> procList = procedureTable.procList;

                foreach (ProcedureNode pNode in procList)
                {
                    bool hidden = pNode.MethodAttribute == null ? false : pNode.MethodAttribute.HiddenInLibrary;
                    if (!pNode.isAssocOperator
                        && !pNode.isAutoGenerated
                        && !pNode.isAutoGeneratedThisProc
                        && !pNode.isConstructor
                        && !hidden
                        && !CoreUtils.IsGetter(pNode.name) && !CoreUtils.IsSetter(pNode.name)
                        && !CoreUtils.StartsWithDoubleUnderscores(pNode.name)
                        && !CoreUtils.IsGetTypeMethod(pNode.name))
                    {
                        methods.Add(new MethodMirror(pNode));
                    }
                }

                return methods;
            }

            /// <summary>
            /// Given the method name, return the list of all matching 
            /// constructors and member functions of this type only
            /// </summary>
            /// <param name="methodName"></param>
            /// <returns></returns>
            public IEnumerable<MethodMirror> GetOverloads(string methodName)
            {
                return ClassNode.vtable.procList.Where(x => x.name == methodName)
                    .Select(y => new MethodMirror(y));
            }

            /// <summary>
            /// Given a method name, return the matching list of 
            /// constructors or static methods on this type and its base types
            /// </summary>
            /// <param name="methodName"></param>
            /// <returns></returns>
            public IEnumerable<MethodMirror> GetOverloadsOnType(string methodName)
            {
                List<MethodMirror> members = new List<MethodMirror>();
                IEnumerable<ClassMirror> baseClasses = this.GetClassHierarchy();
                foreach (var baseClass in baseClasses)
                {
                    members.AddRange(baseClass.GetFunctions().Where(x => x.IsStatic && x.MethodName == methodName));
                }

                members.AddRange(this.GetConstructors().Where(x => x.MethodName == methodName));
                members.AddRange(this.GetFunctions().Where(x => x.IsStatic && x.MethodName == methodName));
                return members;
            }

            /// <summary>
            /// Given a method name, return the matching list of 
            /// instance methods on this type and its base types
            /// </summary>
            /// <param name="methodName"></param>
            /// <returns></returns>
            public IEnumerable<MethodMirror> GetOverloadsOnInstance(string methodName)
            {
                List<MethodMirror> members = new List<MethodMirror>();
                IEnumerable<ClassMirror> baseClasses = this.GetClassHierarchy();
                foreach (var baseClass in baseClasses)
                {
                    members.AddRange(baseClass.GetFunctions().Where(x => !x.IsStatic && x.MethodName == methodName));
                }

                members.AddRange(this.GetFunctions().Where(x => !x.IsStatic && x.MethodName == methodName));
                return members;
            }

            public IEnumerable<StaticMirror> GetInstanceMembers()
            {
                // TODO: Factor out reflection functionality for both LibraryServices and Mirrors
                List<StaticMirror> members = new List<StaticMirror>();

                IEnumerable<ClassMirror> baseClasses = this.GetClassHierarchy();
                foreach (var baseClass in baseClasses)
                {
                    members.AddRange(baseClass.GetFunctions().Where(m => !m.IsStatic).GroupBy(x => x.Name).Select(y => y.First()));
                    members.AddRange(baseClass.GetProperties().Where(m => !m.IsStatic).GroupBy(x => x.Name).Select(y => y.First()));
                }

                members.AddRange(this.GetFunctions().Where(m => !m.IsStatic).GroupBy(x => x.Name).Select(y => y.First()));
                members.AddRange(this.GetProperties().Where(m => !m.IsStatic).GroupBy(x => x.Name).Select(y => y.First()));
                return members;
            }

            public MethodMirror GetDeclaredMethod(string methodName, List<ProtoCore.Type> argumentTypes)
            {
                ProcedureTable procedureTable = ClassNode.vtable;
                List<ProcedureNode> procList = procedureTable.procList;

                return StaticMirror.FindMethod(methodName, argumentTypes, procList);
            }

            public ClassAttributes GetClassAttributes()
            {
                return classNode == null ? null : classNode.ClassAttributes;
            }
        }

        /// <summary>
        /// Reflects upon a Function to retrieve its arguments
        /// </summary>
        public class MethodMirror : StaticMirror
        {
            private ProcedureNode procNode;

            public string MethodName { get; private set; }
            public bool IsConstructor { get; private set; }
            public bool IsStatic { get; private set; }

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

            private Dictionary<string, string> argumentList = null;
            public IEnumerable<KeyValuePair<string, string>> ArgumentList
            {
                get
                {
                    if (argumentList == null)
                    {
                        argumentList = new Dictionary<string, string>();
                        for (int i = 0; i < procNode.argInfoList.Count; ++i)
                        {

                            argumentList.Add(procNode.argInfoList[i].Name,
                                procNode.argTypeList[i].ToString().Split('.').Last());
                        }
                    }
                    return argumentList;
                }
            }

            internal MethodMirror(ProcedureNode procNode)
            {
                MethodName = procNode.name;
                this.Name = MethodName;
                IsConstructor = procNode.isConstructor;
                IsStatic = procNode.isStatic;
                this.procNode = procNode;
            }

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

            public MethodAttributes GetMethodAttributes()
            {
                return procNode == null ? null : procNode.MethodAttribute;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                // TODO: Dropping access specifier and static from function signature
                // until it is required to be displayed to users later
                //Func<Compiler.AccessModifier, string> func =
                //    (x) =>
                //    {
                //        switch (x)
                //        {
                //            case Compiler.AccessModifier.kPrivate:
                //                return "private ";
                //            case Compiler.AccessModifier.kProtected:
                //                return "protected ";
                //            case Compiler.AccessModifier.kPublic:
                //                return "public ";
                //            default:
                //                return string.Empty;
                //        }
                //    };
                //var access = func(procNode.access);
                //var isStatic = this.IsStatic == true ? "static " : "";

                var returnType = string.Empty;
                if (!this.IsConstructor)
                    returnType = " : " + this.ReturnType.ToString().Split('.').Last();
                var methodName = this.MethodName;
                var argList = this.ArgumentList.Select(x => x.Key + " : " + x.Value);
                sb.AppendLine(methodName + returnType + " (" +
                    string.Join(", ", argList.Select(p => p.ToString())) + ')');

                return sb.ToString().Trim();
            }
        }

        public class PropertyMirror : StaticMirror
        {
            private ProcedureNode procNode = null;
            public ProtoCore.Type? Type
            {
                get
                {
                    if (procNode != null)
                    {
                        if (isSetter)
                            return procNode.argTypeList[0];
                        else
                            return procNode.returntype;
                    }
                    return null;
                }
                set { }
            }


            public string PropertyName { get; private set; }

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

            internal PropertyMirror(ProcedureNode procNode, bool isSetter = false)
            {
                this.procNode = procNode;

                string getterPrefix = ProtoCore.DSASM.Constants.kGetterPrefix;
                int prefixLength = getterPrefix.Length;
                PropertyName = procNode.name.Substring(prefixLength);
                this.Name = PropertyName;
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

            public LibraryMirror(string libName, ProtoCore.Core core)
                : base(core, libName)
            {
                LibraryName = libName;
            }

            public LibraryMirror(ProtoCore.Core core, string libName, IList<ProtoCore.DSASM.ClassNode> classNodes)
                : base(core, libName)
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
