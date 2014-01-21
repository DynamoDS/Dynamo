﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GraphToDSCompiler;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using ProtoFFI;
using Constants = ProtoCore.DSASM.Constants;
using Operator = ProtoCore.DSASM.Operator;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// The type of a function.
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
    /// A tuple of parameter and its type. 
    /// </summary>
    public class TypedParameter
    {
        public string Parameter { get; private set; }
        public string Type { get; private set; }

        public TypedParameter(string parameter, string type)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                throw new ArgumentException();
            }
            Parameter = parameter;

            if (null == type)
            {
                throw new ArgumentNullException("Type cannot be null.");
            }
            Type = type;
        }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Type))
            {
                return Parameter;
            }
            else
            {
                return Parameter + ": " + Type;
            }
        }
    }

    /// <summary>
    /// Describe a DesignScript function in a imported library
    /// </summary>
    public class FunctionDescriptor
    {
        /// <summary>
        /// Full path to the assembly the defined this function
        /// </summary>
        public string Assembly
        {
            get;
            private set;
        }

        /// <summary>
        /// Class name of this function. If the functino is global function,
        /// return String.Empty.
        /// </summary>
        public string ClassName
        {
            get;
            private set;
        }

        /// <summary>
        /// Function name.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Function parameters.
        /// </summary>
        public IEnumerable<TypedParameter> Parameters
        {
            get;
            private set;
        }

        /// <summary>
        /// Function return type.
        /// </summary>
        public string ReturnType
        {
            get;
            private set;
        }

        /// <summary>
        /// If the function returns a dictionary, ReturnKeys is the key collection
        /// used in returned dictionary.
        /// </summary>
        public IEnumerable<string> ReturnKeys
        {
            get;
            private set;
        }

        /// <summary>
        /// Function type.
        /// </summary>
        public FunctionType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// The category of this function.
        /// </summary>
        public string Category
        {
            get
            {
                StringBuilder categoryBuf = new StringBuilder();
                categoryBuf.Append(GetRootCategory());
                switch (Type)
                {
                    case FunctionType.Constructor:
                        categoryBuf.Append("." + ClassName + "." + LibraryServices.Categories.Constructors);
                        break;

                    case FunctionType.StaticMethod:
                    case FunctionType.StaticProperty:
                        categoryBuf.Append("." + ClassName + "." + LibraryServices.Categories.StaticMembers);
                        break;

                    case FunctionType.InstanceMethod:
                    case FunctionType.InstanceProperty:
                        categoryBuf.Append("." + ClassName + "." + LibraryServices.Categories.Members);
                        break;
                }
                return categoryBuf.ToString();
            }
        }

        /// <summary>
        /// The string that is used to search for this function. 
        /// </summary>
        public string QualifiedName
        {
            get
            {
                return FunctionType.GenericFunction == Type ? UserFriendlyName : ClassName + "." + Name;
            }
        }

        /// <summary>
        /// A unique name to identify a function. It is necessary when a 
        /// function is overloaded.
        /// </summary>
        public string MangledName
        {
            get
            {
                if (Parameters != null && Parameters.Any())
                {
                    return QualifiedName + "@" + string.Join(",", Parameters.Select(p => p.Type));
                }
                else
                {
                    return QualifiedName;
                }
            }
        }

        /// <summary>
        /// The full signature of the function. 
        /// </summary>
        public string Signature
        {
            get
            {
                StringBuilder descBuf = new StringBuilder();
                descBuf.Append(QualifiedName);

                if (!string.IsNullOrEmpty(ReturnType))
                    descBuf.Append(": " + ReturnType);

                if (Parameters != null && Parameters.Any())
                {
                    string signature = string.Join(", ", Parameters.Select(p => p.ToString()));

                    descBuf.Append(" (");
                    descBuf.Append(signature);
                    descBuf.Append(")");
                }
                else if (FunctionType.InstanceProperty != Type && FunctionType.StaticProperty != Type)
                {
                    descBuf.Append(" ( )");
                }

                return descBuf.ToString();
            }
        }

        /// <summary>
        /// Return a user friendly name. E.g., for operator '+' it will return 
        /// 'Add'
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
                    {
                        name = Op.GetOpSymbol(op);
                    }
                    return name;
                }
                return Name;
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
                if (CoreUtils.IsInternalMethod(Name))
                {
                    return LibraryServices.Categories.Operators;
                }
                return LibraryServices.Categories.BuiltIns;
            }
            else
            {
                return Path.GetFileNameWithoutExtension(Assembly);
            }
        }

        public FunctionDescriptor(string assembly,
                            string className,
                            string name,
                            IEnumerable<TypedParameter> parameters,
                            string returnType,
                            FunctionType type,
                            IEnumerable<string> returnKeys = null)
        {
            Assembly = assembly;
            ClassName = className;
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
            Type = type;
            ReturnKeys = returnKeys;
        }
    }

    /// <summary>
    /// A group of overloaded functions
    /// </summary>
    public class FunctionGroup
    {
        public string QualifiedName 
        { 
            get; 
            private set; 
        }

        public IEnumerable<FunctionDescriptor> Functions
        {
            get
            {
                return _functions;
            }
        }

        private List<FunctionDescriptor> _functions;

        public FunctionGroup(string qualifiedName)
        {
            _functions = new List<FunctionDescriptor>();
            QualifiedName = qualifiedName;
        }

        /// <summary>
        /// Add a function descriptor to the group
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public bool AddFunctionDescriptor(FunctionDescriptor function)
        {
            if (!QualifiedName.Equals(function.QualifiedName) ||
                _functions.Contains(function))
                return false;

            _functions.Add(function);
            return true;
        }

        /// <summary>
        /// Get function descriptor from mangled function name
        /// </summary>
        /// <param name="managledName"></param>
        /// <returns></returns>
        public FunctionDescriptor GetFunctionDescriptor(string managledName)
        {
            if (null == managledName)
                throw new ArgumentNullException();

            if (_functions.Count == 0)
                return null;

            var func = _functions.FirstOrDefault(f => f.MangledName.Equals(managledName));
            return null == func ? _functions.First() : func;
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
    /// LibraryServices is a singleton class which manages builtin libraries
    /// as well as imported libraries. It is across different sessions. 
    /// </summary>
    internal class LibraryServices
    {
        public static class Categories
        {
            public const string BuiltIns = "Builtin Functions";
            public const string Operators = "Operators";
            public const string Constructors = "Constructor";
            public const string StaticMembers = "Static Members";
            public const string Members = "Members";
        }

        public class LibraryLoadedEventArgs : EventArgs
        {
            public LibraryLoadedEventArgs(string libraryPath)
            {
                LibraryPath = libraryPath;
            }

            public string LibraryPath { get; private set; }
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

        public class LibraryLoadingEventArgs : EventArgs
        {
            public LibraryLoadingEventArgs(string libraryPath)
            {
                LibraryPath = libraryPath;
            }

            public string LibraryPath { get; private set; }
        }

        public event EventHandler<LibraryLoadingEventArgs> LibraryLoading;
        public event EventHandler<LibraryLoadFailedEventArgs> LibraryLoadFailed;
        public event EventHandler<LibraryLoadedEventArgs> LibraryLoaded;

        public static LibraryServices GetInstance()
        {
            return _libraryServices;
        }

        private static LibraryServices _libraryServices = new LibraryServices();
        private LibraryServices()
        {
            GraphUtilities.PreloadAssembly(_libraries);

            PopulateBuiltIns();
            PopulateOperators();
            PopulatePreloadLibraries();
        }

        /// <summary>
        /// Reset the whole library services. Note it should only be used in 
        /// testing. 
        /// </summary>
        public void Reset()
        {
            _importedFunctionGroups.Clear();
            _builtinFunctionGroups.Clear();
            _libraries = new List<string> { "Math.dll", "ProtoGeometry.dll" };

            GraphUtilities.Reset();
            GraphUtilities.PreloadAssembly(_libraries);
            PopulateBuiltIns();
            PopulateOperators();
            PopulatePreloadLibraries();
        }

        /// <summary>
        /// Get a list of imported libraries.
        /// </summary>
        public IEnumerable<string> Libraries
        {
            get 
            {
                return _libraries;
            }
        }

        private List<string> _libraries = new List<string> { "Math.dll", "ProtoGeometry.dll" } ;
        private readonly Dictionary<string, Dictionary<string, FunctionGroup>> _importedFunctionGroups = new Dictionary<string, Dictionary<string, FunctionGroup>>(new LibraryPathComparer());
        private Dictionary<string, FunctionGroup> _builtinFunctionGroups = new Dictionary<string, FunctionGroup>();

        /// <summary>
        /// Get function groups from an imported library.
        /// </summary>
        /// <param name="library">Library path</param>
        /// <returns></returns>
        public IEnumerable<FunctionGroup> GetFunctionGroups(string library)
        {
            if (null == library)
            {
                throw new ArgumentNullException();
            }

            Dictionary<string, FunctionGroup> functionGroups;
            if (_importedFunctionGroups.TryGetValue(library, out functionGroups))
            {
                return functionGroups.Values;
            }

            return null;
        }

        /// <summary>
        /// Get builtin function groups.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FunctionGroup> BuiltinFunctionGroups
        {
            get
            {
                return _builtinFunctionGroups.Values;
            }
        }

        /// <summary>
        /// Get all imported function groups.
        /// </summary>
        public IEnumerable<FunctionGroup> ImportedFunctionGroups
        {
            get
            {
                return _importedFunctionGroups.SelectMany(d => d.Value).Select(p => p.Value);
            }
        }

        /// <summary>
        /// Get function descriptor from the managled function name.
        /// name.
        /// </summary>
        /// <param name="library">Library path</param>
        /// <param name="mangledName">Mangled function name</param>
        /// <returns></returns>
        public FunctionDescriptor GetFunctionDescriptor(string library, string mangledName)
        {
            if (null == library || null == mangledName)
            {
                throw new ArgumentNullException();
            }

            Dictionary<string, FunctionGroup> groups;
            if (_importedFunctionGroups.TryGetValue(library, out groups))
            {
                FunctionGroup functionGroup;
                string qualifiedName = mangledName.Split(new char[] { '@' })[0];
                if (groups.TryGetValue(qualifiedName, out functionGroup))
                {
                    return functionGroup.GetFunctionDescriptor(mangledName);
                }
            }
            return null;
        }

        /// <summary>
        /// Get function descriptor from the managed function name.
        /// </summary>
        /// <param name="managedName"></param>
        /// <returns></returns>
        public FunctionDescriptor GetFunctionDescriptor(string managledName)
        {
            if (string.IsNullOrEmpty(managledName))
            {
                throw new ArgumentException("Invalid arguments");
            }

            string qualifiedName = managledName.Split(new char[] { '@' })[0];
            FunctionGroup functionGroup;

            if (_builtinFunctionGroups.TryGetValue(qualifiedName, out functionGroup))
            {
                return functionGroup.GetFunctionDescriptor(managledName);
            }
            else
            {
                foreach (var groupMap in _importedFunctionGroups.Values)
                {
                   if (groupMap.TryGetValue(qualifiedName, out functionGroup))
                   {
                       return functionGroup.GetFunctionDescriptor(managledName);
                   }
                }
            }

            return null;
        }

        /// <summary>
        /// Import a library (if it hasn't been imported yet).
        /// </summary>
        /// <param name="library"></param>
        public void ImportLibrary(string library)
        {
            if (null == library)
            {
                throw new ArgumentNullException();
            }

            if (!library.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) &&
                !library.EndsWith(".ds", StringComparison.InvariantCultureIgnoreCase))
            {
                string errorMessage = "Invalid library format.";
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, errorMessage));
                return;
            }

            if (_importedFunctionGroups.ContainsKey(library))
            {
                string errorMessage = string.Format("Library {0} has been loaded.", library);
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, errorMessage));
                return;
            }

            // Give add-in folder a higher priority and look alongside "DynamoCore.dll".
            string assemblyName = Path.GetFileName(library); // Strip out possible directory.
            string currAsmLocation = System.Reflection.Assembly.GetCallingAssembly().Location;
            library = Path.Combine(Path.GetDirectoryName(currAsmLocation), assemblyName);

            if (!File.Exists(library)) // Not found under add-in folder...
            {
                library = Path.GetFullPath(library);
                if (!File.Exists(library))
                {
                    string errorMessage = string.Format("Cannot find library path: {0}.", library);
                    OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, errorMessage));
                    return;
                }
            }

            OnLibraryLoading(new LibraryLoadingEventArgs(library));

            try
            {
                int globalFunctionNumber = GraphUtilities.GetGlobalMethods(string.Empty).Count;

                DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
                var importedClasses = GraphUtilities.GetClassesForAssembly(library);
                if (GraphUtilities.BuildStatus.ErrorCount > 0)
                {
                    string errorMessage = string.Format("Build error for library: {0}", library);
                    DynamoLogger.Instance.LogWarning(errorMessage, WarningLevel.Moderate);
                    foreach (var error in GraphUtilities.BuildStatus.Errors)
                    {
                        DynamoLogger.Instance.LogWarning(error.Message, WarningLevel.Moderate);
                        errorMessage += error.Message + "\n";
                    }

                    OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, errorMessage));
                    return;
                }

                foreach (ClassNode classNode in importedClasses)
                {
                    ImportClass(library, classNode);
                }

                // GraphUtilities.GetGlobalMethods() ignores input and just 
                // return all global functions. The workaround is to get 
                // new global functions after importing this assembly.
                var globalFunctions = GraphUtilities.GetGlobalMethods(string.Empty);
                for (int i = globalFunctionNumber; i < globalFunctions.Count; ++i)
                {
                    ImportProcedure(library, null, globalFunctions[i]);
                }
            }
            catch (Exception e)
            {
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, e.Message));
                return;
            }

            OnLibraryLoaded(new LibraryLoadedEventArgs(library));
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

        private void AddImportedFunctions(string library, IEnumerable<FunctionDescriptor> functions)
        {
            if (null == library || null == functions)
            {
                throw new ArgumentNullException();
            }

            Dictionary<string, FunctionGroup> fptrs;
            if (!_importedFunctionGroups.TryGetValue(library, out fptrs))
            {
                fptrs = new Dictionary<string, FunctionGroup>();
                _importedFunctionGroups[library] = fptrs;
            }

            foreach (var function in functions)
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
            {
                throw new ArgumentNullException();
            }

            foreach (var function in functions)
            {
                string qualifiedName = function.QualifiedName;
                FunctionGroup functionGroup;
                if (!_builtinFunctionGroups.TryGetValue(qualifiedName, out functionGroup))
                {
                    functionGroup = new FunctionGroup(qualifiedName);
                    _builtinFunctionGroups[qualifiedName] = functionGroup;
                }
                functionGroup.AddFunctionDescriptor(function);
            }
        }

        /// <summary>
        /// Add DesignScript builtin functions to the library.
        /// </summary>
        private void PopulateBuiltIns()
        {
            var functions = from method in GraphUtilities.BuiltInMethods
                            let arguments = method.argInfoList.Zip(method.argTypeList, (arg, argType) => new TypedParameter(arg.Name, argType.ToString()))
                            select new FunctionDescriptor(null, null, method.name, arguments, method.returntype.ToString(), FunctionType.GenericFunction);

            AddBuiltinFunctions(functions);
        }

        /// <summary>
        /// Add operators to the library.
        /// </summary>
        private void PopulateOperators()
        {
            var args = new List<TypedParameter> 
            {
                new TypedParameter("x", string.Empty),
                new TypedParameter("y", string.Empty),
            };

            var functions = new List<FunctionDescriptor>()
            {
                new FunctionDescriptor(null, null, Op.GetOpFunction(Operator.add), args, null, FunctionType.GenericFunction),
                new FunctionDescriptor(null, null, Op.GetOpFunction(Operator.sub), args, null, FunctionType.GenericFunction),
                new FunctionDescriptor(null, null, Op.GetOpFunction(Operator.mul), args, null, FunctionType.GenericFunction),
                new FunctionDescriptor(null, null, Op.GetOpFunction(Operator.div), args, null, FunctionType.GenericFunction),
            };

            AddBuiltinFunctions(functions);
        }

        /// <summary>
        /// Polulate preloaded libraries.
        /// </summary>
        private void PopulatePreloadLibraries()
        {
            foreach (var classNode in GraphUtilities.ClassTable.ClassNodes)
            {
                if (classNode.IsImportedClass && !string.IsNullOrEmpty(classNode.ExternLib))
                {
                    string library = Path.GetFileName(classNode.ExternLib);
                    ImportClass(library, classNode);
                }
            }
        }

        private void ImportProcedure(string library, string className, ProcedureNode proc)
        {
            if (proc.isAutoGeneratedThisProc)
            {
                return;
            }

            string procName = proc.name;
            if (CoreUtils.IsSetter(procName) || 
                procName.Equals(ProtoCore.DSDefinitions.Keyword.Dispose))
            {
                return;
            }

            FunctionType type;
            if (CoreUtils.IsGetter(procName))
            {
                type = proc.isStatic ? FunctionType.StaticProperty : FunctionType.InstanceProperty;

                string property;
                if (CoreUtils.TryGetPropertyName(procName, out property))
                {
                    procName = property;
                }
            }
            else
            {
                if (proc.isConstructor)
                {
                    type = FunctionType.Constructor;
                }
                else if (proc.isStatic)
                {
                    type = FunctionType.StaticMethod;
                }
                else if (!string.IsNullOrEmpty(className))
                {
                    type = FunctionType.InstanceMethod;
                }
                else
                {
                    type = FunctionType.GenericFunction;
                }
            }

            var arguments = proc.argInfoList.Zip(proc.argTypeList, (arg, argType) => new TypedParameter(arg.Name, argType.ToString()));
            IEnumerable<string> returnKeys = null;
            if (proc.MethodAttribute != null && proc.MethodAttribute.MutilReturnMap != null)
            {
                returnKeys = proc.MethodAttribute.MutilReturnMap.Keys;
            }

            var function = new FunctionDescriptor(library, className, procName, arguments, proc.returntype.ToString(), type, returnKeys);
            AddImportedFunctions(library, new FunctionDescriptor[] { function });
        }

        private void ImportClass(string library, ClassNode classNode)
        {
            foreach (var proc in classNode.vtable.procList)
            {
                ImportProcedure(library, classNode.name, proc); 
            }
        }

        private void OnLibraryLoading(LibraryLoadingEventArgs e)
        {
            EventHandler<LibraryLoadingEventArgs> handler = LibraryLoading;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnLibraryLoadFailed(LibraryLoadFailedEventArgs e)
        {
            EventHandler<LibraryLoadFailedEventArgs> handler = LibraryLoadFailed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnLibraryLoaded(LibraryLoadedEventArgs e)
        {
            _libraries.Add(e.LibraryPath);

            EventHandler<LibraryLoadedEventArgs> handler = LibraryLoaded;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
