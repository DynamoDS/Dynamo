#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DynamoUtilities;

using GraphToDSCompiler;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.BuildData;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using ProtoFFI;
using Constants = ProtoCore.DSASM.Constants;
using Operator = ProtoCore.DSASM.Operator;
using Dynamo.Utilities;

#endregion

namespace Dynamo.DSEngine
{
    /// <summary>
    ///     The type of a function.
    /// </summary>
    public enum FunctionType
    {
        GenericFunction,
        Constructor,
        StaticMethod,
        InstanceMethod,
        StaticProperty,
        InstanceProperty
    }

    /// <summary>
    ///     A tuple of parameter and its type.
    /// </summary>
    public class TypedParameter
    {
        private string summary;

        public TypedParameter(string parameter, string type, object defaultValue = null)
            : this(null, parameter, type, defaultValue) { }

        public TypedParameter(
            FunctionDescriptor function, string name, string type, object defaultValue = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException();
            Name = name;

            if (null == type)
                throw new ArgumentNullException("type", @"Type cannot be null.");
            Type = type;
            DefaultValue = defaultValue;
            Function = function;
        }

        public FunctionDescriptor Function { get; set; }
        public string Name { get; private set; }
        public string Type { get; private set; }
        public object DefaultValue { get; private set; }

        public string Summary
        {
            get { return summary ?? (summary = this.GetXmlDocumentation()); }
        }

        public string Description
        {
            get
            {
                return !String.IsNullOrEmpty(Summary)
                    ? Summary + " (" + (string.IsNullOrEmpty(Type) ? "var" : DisplayTypeName) + ")"
                    : (string.IsNullOrEmpty(Type) ? "var" : DisplayTypeName);
            }
        }

        public string DisplayTypeName
        {
            get { return Type.Split('.').Last(); }
        }

        public override string ToString()
        {
            string str = Name;

            if (!String.IsNullOrEmpty(Type))
                str = Name + ": " + Type.Split('.').Last();

            if (DefaultValue != null)
                str = str + " = " + DefaultValue;

            return str;
        }
    }

    /// <summary>
    ///     Describe a DesignScript function in a imported library
    /// </summary>
    public class FunctionDescriptor
    {
        /// <summary>
        ///     A comment describing the Function
        /// </summary>
        private string summary;

        public FunctionDescriptor(string name, IEnumerable<TypedParameter> parameters, FunctionType type)
            : this(null, null, name, parameters, null, type) 
        { }

        public FunctionDescriptor(
            string assembly, string className, string name, IEnumerable<TypedParameter> parameters,
            string returnType, FunctionType type, bool isVisibleInLibrary = true,
            IEnumerable<string> returnKeys = null, bool isVarArg = false)
            : this(
                assembly,
                className,
                name,
                null,
                parameters,
                returnType,
                type,
                isVisibleInLibrary,
                returnKeys,
                isVarArg) { }

        public FunctionDescriptor(
            string assembly, string className, string name, string summary,
            IEnumerable<TypedParameter> parameters, string returnType, FunctionType type,
            bool isVisibleInLibrary = true, IEnumerable<string> returnKeys = null, bool isVarArg = false)
        {
            this.summary = summary;
            Assembly = assembly;
            ClassName = className;
            Name = name;

            if (parameters == null)
                Parameters = new List<TypedParameter>();
            else
            {
                Parameters = parameters.Select(
                    x =>
                    {
                        x.Function = this;
                        return x;
                    });
            }

            ReturnType = returnType ?? "var[]..[]";
            Type = type;
            ReturnKeys = returnKeys ?? new List<string>();
            IsVarArg = isVarArg;
            IsVisibleInLibrary = isVisibleInLibrary;
        }

        /// <summary>
        ///     Full path to the assembly the defined this function
        /// </summary>
        public string Assembly { get; private set; }

        /// <summary>
        ///     Class name of this function. If the functino is global function,
        ///     return String.Empty.
        /// </summary>
        public string ClassName { get; private set; }

        /// <summary>
        ///     Function name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Function parameters.
        /// </summary>
        public IEnumerable<TypedParameter> Parameters { get; private set; }

        /// <summary>
        ///     Function return type.
        /// </summary>
        public string ReturnType { get; private set; }

        /// <summary>
        ///     If the function returns a dictionary, ReturnKeys is the key collection
        ///     used in returned dictionary.
        /// </summary>
        public IEnumerable<string> ReturnKeys { get; private set; }

        /// <summary>
        ///     Does the function accept a variable number of arguments?
        /// </summary>
        public bool IsVarArg { get; private set; }

        /// <summary>
        ///     Function type.
        /// </summary>
        public FunctionType Type { get; private set; }

        public string Summary
        {
            get { return summary ?? (summary = this.GetXmlDocumentation()); }
        }

        /// <summary>
        ///     A comment describing the function along with the signature
        /// </summary>
        public string Description
        {
            get { return !String.IsNullOrEmpty(Summary) ? Summary + "\n\n" + Signature : Signature; }
        }

        /// <summary>
        ///     The category of this function.
        /// </summary>
        public string Category
        {
            get
            {
                var categoryBuf = new StringBuilder();
                categoryBuf.Append(GetRootCategory());
                switch (Type)
                {
                    case FunctionType.Constructor:
                        categoryBuf.Append(
                            "." + UnqualifedClassName + "." + LibraryServices.Categories.Constructors);
                        break;

                    case FunctionType.StaticMethod:
                    case FunctionType.InstanceMethod:
                        categoryBuf.Append(
                            "." + UnqualifedClassName + "." + LibraryServices.Categories.MemberFunctions);
                        break;

                    case FunctionType.StaticProperty:
                    case FunctionType.InstanceProperty:
                        categoryBuf.Append(
                            "." + UnqualifedClassName + "." + LibraryServices.Categories.Properties);
                        break;
                }
                return categoryBuf.ToString();
            }
        }

        /// <summary>
        ///     The string that is used to search for this function.
        /// </summary>
        public string QualifiedName
        {
            get
            {
                return FunctionType.GenericFunction == Type
                    ? UserFriendlyName
                    : ClassName + "." + UserFriendlyName;
            }
        }

        /// <summary>
        ///     A unique name to identify a function. It is necessary when a
        ///     function is overloaded.
        /// </summary>
        public string MangledName
        {
            get
            {
                return Parameters != null && Parameters.Any()
                    ? QualifiedName + "@" + string.Join(",", Parameters.Select(p => p.Type))
                    : QualifiedName;
            }
        }

        /// <summary>
        ///     The full signature of the function.
        /// </summary>
        public string Signature
        {
            get
            {
                var descBuf = new StringBuilder();
                descBuf.Append(DisplayName);
                
                if (Parameters != null && Parameters.Any())
                {
                    string signature = string.Join(", ", Parameters.Select(p => p.ToString()));
                    descBuf.Append(" (");
                    descBuf.Append(signature);
                    descBuf.Append(")");
                }
                else if (FunctionType.InstanceProperty != Type && FunctionType.StaticProperty != Type)
                    descBuf.Append(" ( )");

                if (!string.IsNullOrEmpty(ReturnType))
                    descBuf.Append(": " + ReturnType);

                return descBuf.ToString();
            }
        }

        /// <summary>
        ///     Return a user friendly name. E.g., for operator '+' it will return
        ///     'Add'
        /// </summary>
        public string UserFriendlyName
        {
            get
            {
                if (Name.StartsWith(Constants.kInternalNamePrefix))
                {
                    string name = Name.Substring(Constants.kInternalNamePrefix.Length);

                    Operator op;
                    if (Enum.TryParse(name, out op))
                        name = Op.GetOpSymbol(op);

                    return name;
                }
                return Name;
            }
        }

        /// <summary>
        ///     QualifiedName with leading namespaces removed.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (FunctionType.GenericFunction == Type)
                    return UserFriendlyName;

                int idx = ClassName.LastIndexOf('.');
                return idx < 0
                    ? QualifiedName
                    : string.Format("{0}.{1}", ClassName.Substring(idx + 1), UserFriendlyName);
            }
        }

        /// <summary>
        ///     This attribute sets, if this function is shown in library or not.
        /// </summary>
        public bool IsVisibleInLibrary { get; private set; }

        public string UnqualifedClassName
        {
            get
            {
                if (string.IsNullOrEmpty(ClassName))
                    return string.Empty;

                int idx = ClassName.LastIndexOf('.');
                return idx < 0 ? String.Empty : ClassName.Substring(idx + 1);
            }
        }

        public string Namespace
        {
            get
            {
                if (string.IsNullOrEmpty(ClassName))
                    return string.Empty;

                int idx = ClassName.LastIndexOf('.');
                return idx < 0 ? String.Empty : ClassName.Substring(0, idx);
            }
        }

        public override bool Equals(object obj)
        {
            if (null == obj || GetType() != obj.GetType())
                return false;

            return MangledName.Equals(obj as FunctionDescriptor);
        }

        public override int GetHashCode()
        {
            return MangledName.GetHashCode();
        }

        private string GetRootCategory()
        {
            if (string.IsNullOrEmpty(Assembly))
            {
                return CoreUtils.IsInternalMethod(Name)
                    ? LibraryServices.Categories.Operators
                    : LibraryServices.Categories.BuiltIns;
            }

            LibraryCustomization cust = LibraryCustomizationServices.GetForAssembly(Assembly);

            if (cust != null)
            {
                string f = cust.GetNamespaceCategory(Namespace);
                if (!String.IsNullOrEmpty(f))
                    return f;
            }

            string filename = Path.GetFileNameWithoutExtension(Assembly);

            return string.IsNullOrEmpty(Namespace) ? filename : filename + "." + Namespace;
        }
    }

    /// <summary>
    ///     A group of overloaded functions
    /// </summary>
    public class FunctionGroup
    {
        private readonly List<FunctionDescriptor> functions;

        public FunctionGroup(string qualifiedName)
        {
            functions = new List<FunctionDescriptor>();
            QualifiedName = qualifiedName;
        }

        public string QualifiedName { get; private set; }

        public IEnumerable<FunctionDescriptor> Functions
        {
            get { return functions; }
        }

        /// <summary>
        ///     Add a function descriptor to the group
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public bool AddFunctionDescriptor(FunctionDescriptor function)
        {
            if (!QualifiedName.Equals(function.QualifiedName) || functions.Contains(function))
                return false;

            functions.Add(function);
            return true;
        }

        /// <summary>
        ///     Get function descriptor from mangled function name
        /// </summary>
        /// <param name="managledName"></param>
        /// <returns></returns>
        public FunctionDescriptor GetFunctionDescriptor(string managledName)
        {
            if (null == managledName)
                throw new ArgumentNullException();

            if (functions.Count == 0)
                return null;

            FunctionDescriptor func = functions.FirstOrDefault(f => f.MangledName.EndsWith(managledName));
            return func ?? functions.First();
        }

        public override bool Equals(object obj)
        {
            if (null == obj || GetType() != obj.GetType())
                return false;

            return QualifiedName.Equals((obj as FunctionGroup).QualifiedName);
        }

        public override int GetHashCode()
        {
            return QualifiedName.GetHashCode();
        }
    }

    /// <summary>
    ///     LibraryServices is a singleton class which manages builtin libraries
    ///     as well as imported libraries. It is across different sessions.
    /// </summary>
    internal class LibraryServices
    {
        /// <summary>
        ///     lock object to prevent races on establishing the singleton
        /// </summary>
        private static readonly Object singletonMutex = new object();

        private static LibraryServices _libraryServices; // new LibraryServices();

        private readonly Dictionary<string, FunctionGroup> builtinFunctionGroups =
            new Dictionary<string, FunctionGroup>();

        private readonly Dictionary<string, Dictionary<string, FunctionGroup>> importedFunctionGroups =
            new Dictionary<string, Dictionary<string, FunctionGroup>>(new LibraryPathComparer());

        private List<string> libraries;

        private LibraryServices()
        {
            PreloadLibraries();

            PopulateBuiltIns();
            PopulateOperators();
            PopulatePreloadLibraries();
        }

        /// <summary>
        ///     Get a list of imported libraries.
        /// </summary>
        public IEnumerable<string> Libraries
        {
            get { return libraries; }
        }

        /// <summary>
        ///     Get builtin function groups.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FunctionGroup> BuiltinFunctionGroups
        {
            get { return builtinFunctionGroups.Values; }
        }

        /// <summary>
        ///     Get all imported function groups.
        /// </summary>
        public IEnumerable<FunctionGroup> ImportedFunctionGroups
        {
            get { return importedFunctionGroups.SelectMany(d => d.Value).Select(p => p.Value); }
        }

        public event EventHandler<LibraryLoadingEventArgs> LibraryLoading;
        public event EventHandler<LibraryLoadFailedEventArgs> LibraryLoadFailed;
        public event EventHandler<LibraryLoadedEventArgs> LibraryLoaded;

        public static LibraryServices GetInstance()
        {
            lock (singletonMutex)
            {
                return _libraryServices ?? (_libraryServices = new LibraryServices());
            }
        }

        public static void DestroyInstance()
        {
            lock (singletonMutex)
            {
                _libraryServices = null;
            }
        }

        /// <summary>
        ///     Reset the whole library services. Note it should only be used in
        ///     testing.
        /// </summary>
        public void Reset()
        {
            importedFunctionGroups.Clear();
            builtinFunctionGroups.Clear();

            PreloadLibraries();

            PopulateBuiltIns();
            PopulateOperators();
            PopulatePreloadLibraries();
        }

        private void PreloadLibraries()
        {
            GraphUtilities.Reset();
            libraries = DynamoPathManager.Instance.PreloadLibraries;
            GraphUtilities.PreloadAssembly(libraries);
        }

        /// <summary>
        ///     Get function groups from an imported library.
        /// </summary>
        /// <param name="library">Library path</param>
        /// <returns></returns>
        public IEnumerable<FunctionGroup> GetFunctionGroups(string library)
        {
            if (null == library)
                throw new ArgumentNullException();

            Dictionary<string, FunctionGroup> functionGroups;
            if (importedFunctionGroups.TryGetValue(library, out functionGroups))
                return functionGroups.Values;

            // Return an empty list instead of 'null' as some of the caller may
            // not have the opportunity to check against 'null' enumerator (for
            // example, an inner iterator in a nested LINQ statement).
            return new List<FunctionGroup>();
        }

        /// <summary>
        ///     Get function descriptor from the managled function name.
        ///     name.
        /// </summary>
        /// <param name="library">Library path</param>
        /// <param name="mangledName">Mangled function name</param>
        /// <returns></returns>
        public FunctionDescriptor GetFunctionDescriptor(string library, string mangledName)
        {
            if (null == library || null == mangledName)
                throw new ArgumentNullException();

            Dictionary<string, FunctionGroup> groups;
            if (importedFunctionGroups.TryGetValue(library, out groups))
            {
                FunctionGroup functionGroup;
                string qualifiedName = mangledName.Split(new[] { '@' })[0];

                if (TryGetFunctionGroup(groups, qualifiedName, out functionGroup))
                    return functionGroup.GetFunctionDescriptor(mangledName);
            }
            return null;
        }

        /// <summary>
        ///     Get function descriptor from the managed function name.
        /// </summary>
        /// <param name="managledName"></param>
        /// <returns></returns>
        public FunctionDescriptor GetFunctionDescriptor(string managledName)
        {
            if (string.IsNullOrEmpty(managledName))
                throw new ArgumentException("Invalid arguments");

            string qualifiedName = managledName.Split(new[] { '@' })[0];
            FunctionGroup functionGroup;

            if (builtinFunctionGroups.TryGetValue(qualifiedName, out functionGroup))
                return functionGroup.GetFunctionDescriptor(managledName);

            return
                importedFunctionGroups.Values.Any(
                    groupMap => TryGetFunctionGroup(groupMap, qualifiedName, out functionGroup))
                    ? functionGroup.GetFunctionDescriptor(managledName)
                    : null;
        }

        private static bool CanbeResolvedTo(ICollection<string> partialName, ICollection<string> fullName)
        {
            return null != partialName && null != fullName && partialName.Count <= fullName.Count
                && fullName.Reverse().Take(partialName.Count).SequenceEqual(partialName.Reverse());
        }

        private static bool TryGetFunctionGroup(
            Dictionary<string, FunctionGroup> funcGroupMap, string qualifiedName, out FunctionGroup funcGroup)
        {
            if (funcGroupMap.TryGetValue(qualifiedName, out funcGroup))
                return true;

            string[] partialName = qualifiedName.Split('.');
            string key = funcGroupMap.Keys.FirstOrDefault(k => CanbeResolvedTo(partialName, k.Split('.')));

            if (key != null)
            {
                funcGroup = funcGroupMap[key];
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Import a library (if it hasn't been imported yet).
        /// </summary>
        /// <param name="library"></param>
        public void ImportLibrary(string library)
        {
            if (null == library)
                throw new ArgumentNullException();

            if (!library.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)
                && !library.EndsWith(".ds", StringComparison.InvariantCultureIgnoreCase))
            {
                const string errorMessage = "Invalid library format.";
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, errorMessage));
                return;
            }

            if (importedFunctionGroups.ContainsKey(library))
            {
                string errorMessage = string.Format("Library {0} has been loaded.", library);
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, errorMessage));
                return;
            }

            if (!DynamoPathManager.Instance.ResolveLibraryPath(ref library))
            {
                string errorMessage = string.Format("Cannot find library path: {0}.", library);
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, errorMessage));
                return;
            }

            OnLibraryLoading(new LibraryLoadingEventArgs(library));

            try
            {
                int globalFunctionNumber = GraphUtilities.GetGlobalMethods(string.Empty).Count;

                DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
                IList<ClassNode> importedClasses = GraphUtilities.GetClassesForAssembly(library);

                if (GraphUtilities.BuildStatus.ErrorCount > 0)
                {
                    string errorMessage = string.Format("Build error for library: {0}", library);
                    dynSettings.DynamoLogger.LogWarning(errorMessage, WarningLevel.Moderate);
                    foreach (ErrorEntry error in GraphUtilities.BuildStatus.Errors)
                    {
                        dynSettings.DynamoLogger.LogWarning(error.Message, WarningLevel.Moderate);
                        errorMessage += error.Message + "\n";
                    }

                    OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, errorMessage));
                    return;
                }

                foreach (ClassNode classNode in importedClasses)
                    ImportClass(library, classNode);

                // GraphUtilities.GetGlobalMethods() ignores input and just 
                // return all global functions. The workaround is to get 
                // new global functions after importing this assembly.
                List<ProcedureNode> globalFunctions = GraphUtilities.GetGlobalMethods(string.Empty);
                for (int i = globalFunctionNumber; i < globalFunctions.Count; ++i)
                    ImportProcedure(library, null, globalFunctions[i]);
            }
            catch (Exception e)
            {
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, e.Message));
                return;
            }

            OnLibraryLoaded(new LibraryLoadedEventArgs(library));
        }

        private void AddImportedFunctions(string library, IEnumerable<FunctionDescriptor> functions)
        {
            if (null == library || null == functions)
                throw new ArgumentNullException();

            Dictionary<string, FunctionGroup> fptrs;
            if (!importedFunctionGroups.TryGetValue(library, out fptrs))
            {
                fptrs = new Dictionary<string, FunctionGroup>();
                importedFunctionGroups[library] = fptrs;
            }

            foreach (FunctionDescriptor function in functions)
            {
                string qualifiedName = function.QualifiedName;
                FunctionGroup functionGroup;
                if (!fptrs.TryGetValue(qualifiedName, out functionGroup))
                {
                    functionGroup = new FunctionGroup(qualifiedName);
                    fptrs[qualifiedName] = functionGroup;
                }
                functionGroup.AddFunctionDescriptor(function);
            }
        }

        private void AddBuiltinFunctions(IEnumerable<FunctionDescriptor> functions)
        {
            if (null == functions)
                throw new ArgumentNullException();

            foreach (FunctionDescriptor function in functions)
            {
                string qualifiedName = function.QualifiedName;

                if (CoreUtils.StartsWithDoubleUnderscores(qualifiedName))
                    continue;

                FunctionGroup functionGroup;
                if (!builtinFunctionGroups.TryGetValue(qualifiedName, out functionGroup))
                {
                    functionGroup = new FunctionGroup(qualifiedName);
                    builtinFunctionGroups[qualifiedName] = functionGroup;
                }
                functionGroup.AddFunctionDescriptor(function);
            }
        }

        /// <summary>
        ///     Add DesignScript builtin functions to the library.
        /// </summary>
        private void PopulateBuiltIns()
        {
            IEnumerable<FunctionDescriptor> functions = from method in GraphUtilities.BuiltInMethods
                                                        let arguments =
                                                            method.argInfoList.Zip(
                                                                method.argTypeList,
                                                                (arg, argType) =>
                                                                    new TypedParameter(
                                                                    arg.Name,
                                                                    argType.ToString()))
                                                        let visibleInLibrary =
                                                            (method.MethodAttribute == null
                                                                || !method.MethodAttribute.HiddenInLibrary)
                                                        select
                                                            new FunctionDescriptor(
                                                                null,
                                                                null,
                                                                method.name,
                                                                arguments,
                                                                method.returntype.ToString(),
                                                                FunctionType.GenericFunction,
                                                                visibleInLibrary);

            AddBuiltinFunctions(functions);
        }

        private static List<TypedParameter> GetBinaryFuncArgs()
        {
            return new List<TypedParameter>
            {
                new TypedParameter(null, "x", string.Empty),
                new TypedParameter(null, "y", string.Empty),
            };
        }

        private static IEnumerable<TypedParameter> GetUnaryFuncArgs()
        {
            return new List<TypedParameter> { new TypedParameter(null, "x", string.Empty), };
        }

        /// <summary>
        ///     Add operators to the library.
        /// </summary>
        private void PopulateOperators()
        {
            var args = GetBinaryFuncArgs();

            var functions = new List<FunctionDescriptor>
            {
                new FunctionDescriptor(Op.GetOpFunction(Operator.add), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.sub), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.mul), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.div), args, FunctionType.GenericFunction),

                //add new operators
                new FunctionDescriptor(Op.GetOpFunction(Operator.eq), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.ge), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.gt), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.mod), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.le), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.lt), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.and), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.or), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.nq), args, FunctionType.GenericFunction),
                /*
                new FunctionDescriptor(Op.GetOpFunction(Operator.assign), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.bitwiseand), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.bitwiseor), args, FunctionType.GenericFunction),
                new FunctionDescriptor(Op.GetOpFunction(Operator.bitwisexor), args, FunctionType.GenericFunction),
                */

                new FunctionDescriptor(
                    Op.GetUnaryOpFunction(UnaryOperator.Not),
                    GetUnaryFuncArgs(),
                    FunctionType.GenericFunction),
            };

            AddBuiltinFunctions(functions);
        }

        /// <summary>
        ///     Polulate preloaded libraries.
        /// </summary>
        private void PopulatePreloadLibraries()
        {
            foreach (ClassNode classNode in GraphUtilities.ClassTable.ClassNodes)
            {
                if (classNode.IsImportedClass && !string.IsNullOrEmpty(classNode.ExternLib))
                {
                    string library = Path.GetFileName(classNode.ExternLib);
                    ImportClass(library, classNode);
                }
            }
        }

        private void ImportProcedure(string library, ClassNode classNode, ProcedureNode proc)
        {
            if (proc.isAutoGeneratedThisProc)
                return;

            bool isVisibleInLibrary = !(null != proc.MethodAttribute && proc.MethodAttribute.HiddenInLibrary);

            //If the class is Hidden all methods are hidden. 
            if (null != classNode.ClassAttributes && classNode.ClassAttributes.HiddenInLibrary)
            {
                isVisibleInLibrary = null != proc.MethodAttribute && !proc.MethodAttribute.HiddenInLibrary;
                //But if a particular method is not hidden, then this method is visible
            }

            string procName = proc.name;
            if (CoreUtils.IsSetter(procName) || CoreUtils.IsDisposeMethod(procName)
                || CoreUtils.StartsWithDoubleUnderscores(procName))
                return;

            FunctionType type;
            if (CoreUtils.IsGetter(procName))
            {
                type = proc.isStatic ? FunctionType.StaticProperty : FunctionType.InstanceProperty;

                string property;
                if (CoreUtils.TryGetPropertyName(procName, out property))
                    procName = property;
            }
            else
            {
                if (proc.isConstructor)
                    type = FunctionType.Constructor;
                else if (proc.isStatic)
                    type = FunctionType.StaticMethod;
                else
                    type = FunctionType.InstanceMethod;
            }

            IEnumerable<TypedParameter> arguments = proc.argInfoList.Zip(
                proc.argTypeList,
                (arg, argType) =>
                {
                    object defaultValue = null;
                    if (arg.IsDefault)
                    {
                        var binaryExpr = arg.DefaultExpression as BinaryExpressionNode;
                        if (binaryExpr != null)
                        {
                            AssociativeNode vnode = binaryExpr.RightNode;
                            if (vnode is IntNode)
                                defaultValue = (vnode as IntNode).Value;
                            else if (vnode is DoubleNode)
                                defaultValue = (vnode as DoubleNode).Value;
                            else if (vnode is BooleanNode)
                                defaultValue = (vnode as BooleanNode).Value;
                            else if (vnode is StringNode)
                                defaultValue = (vnode as StringNode).value;
                        }
                    }

                    return new TypedParameter(arg.Name, argType.ToString(), defaultValue);
                });

            IEnumerable<string> returnKeys = null;
            if (proc.MethodAttribute != null && proc.MethodAttribute.ReturnKeys != null)
                returnKeys = proc.MethodAttribute.ReturnKeys;

            var function = new FunctionDescriptor(
                library,
                classNode.name,
                procName,
                arguments,
                proc.returntype.ToString(),
                type,
                isVisibleInLibrary,
                returnKeys,
                proc.isVarArg);

            AddImportedFunctions(library, new[] { function });
        }

        private void ImportClass(string library, ClassNode classNode)
        {
            foreach (ProcedureNode proc in classNode.vtable.procList)
                ImportProcedure(library, classNode, proc);
        }

        private void OnLibraryLoading(LibraryLoadingEventArgs e)
        {
            EventHandler<LibraryLoadingEventArgs> handler = LibraryLoading;
            if (handler != null)
                handler(this, e);
        }

        private void OnLibraryLoadFailed(LibraryLoadFailedEventArgs e)
        {
            EventHandler<LibraryLoadFailedEventArgs> handler = LibraryLoadFailed;
            if (handler != null)
                handler(this, e);
        }

        private void OnLibraryLoaded(LibraryLoadedEventArgs e)
        {
            libraries.Add(e.LibraryPath);

            EventHandler<LibraryLoadedEventArgs> handler = LibraryLoaded;
            if (handler != null)
                handler(this, e);
        }

        public static class Categories
        {
            public const string BuiltIns = "Builtin Functions";
            public const string Operators = "Operators";
            public const string Constructors = "Create";
            public const string MemberFunctions = "Actions";
            public const string Properties = "Query";
        }

        public class LibraryLoadFailedEventArgs : EventArgs
        {
            public LibraryLoadFailedEventArgs(string libraryPath, string reason)
            {
                LibraryPath = libraryPath;
                Reason = reason;
            }

            public string LibraryPath { get; private set; }
            public string Reason { get; private set; }
        }

        public class LibraryLoadedEventArgs : EventArgs
        {
            public LibraryLoadedEventArgs(string libraryPath)
            {
                LibraryPath = libraryPath;
            }

            public string LibraryPath { get; private set; }
        }

        public class LibraryLoadingEventArgs : EventArgs
        {
            public LibraryLoadingEventArgs(string libraryPath)
            {
                LibraryPath = libraryPath;
            }

            public string LibraryPath { get; private set; }
        }

        private class LibraryPathComparer : IEqualityComparer<string>
        {
            public bool Equals(string path1, string path2)
            {
                string file1 = Path.GetFileName(path1);
                string file2 = Path.GetFileName(path2);
                return string.Compare(file1, file2, StringComparison.InvariantCultureIgnoreCase) == 0;
            }

            public int GetHashCode(string path)
            {
                string file = Path.GetFileName(path);
                return file.ToUpper().GetHashCode();
            }
        }
    }
}
