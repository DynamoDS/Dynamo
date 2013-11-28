using System;
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
    public enum LibraryItemType
    {
        GenericFunction,
        Constructor,
        StaticMethod,
        InstanceMethod,
        StaticProperty,
        InstanceProperty
    }

    /// <summary>
    /// Describe a DesignScript library item.
    /// </summary>
    public abstract class LibraryItem
    {
        public string Assembly { get; set; }
        public string ClassName { get; set; }
        public string Name { get; set; }
        public LibraryItemType Type { get; set; }
        public string QualifiedName
        {
            get
            {
                return string.IsNullOrEmpty(ClassName) ? Name : ClassName + "." + Name;
            }
        }

        protected LibraryItem(string assembly, string className, string name, LibraryItemType type)
        {
            Assembly = assembly;
            ClassName = className;
            Name = name;
            Type = type;
        }
    }

    /// <summary>
    /// Describe a DesignScript function in a imported library
    /// </summary>
    public class FunctionItem : LibraryItem
    {
        public List<Tuple<string, string>> Parameters { get; private set; }
        public List<string> ReturnKeys { get; private set; }
        public string ReturnType { get; private set; }

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
                    case LibraryItemType.Constructor:
                        categoryBuf.Append("." + ClassName + "." + LibraryServices.Categories.Constructors);
                        break;

                    case LibraryItemType.StaticMethod:
                    case LibraryItemType.StaticProperty:
                        categoryBuf.Append("." + ClassName + "." + LibraryServices.Categories.StaticMembers);
                        break;

                    case LibraryItemType.InstanceMethod:
                    case LibraryItemType.InstanceProperty:
                        categoryBuf.Append("." + ClassName + "." + LibraryServices.Categories.Members);
                        break;
                }
                return categoryBuf.ToString();
            }
        }

        /// <summary>
        /// The string that is used to search for this function. 
        /// </summary>
        public string SearchName
        {
            get
            {
                return LibraryItemType.GenericFunction == Type ? UserFriendlyName : ClassName + "." + Name;
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
                if (Parameters != null && Parameters.Count > 0)
                {
                    return SearchName + "@" + string.Join(",", Parameters.Select(p => string.IsNullOrEmpty(p.Item2) ? string.Empty : p.Item2));
                }
                else
                {
                    return SearchName;
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
                descBuf.Append(SearchName);

                if (!string.IsNullOrEmpty(ReturnType))
                    descBuf.Append(": " + ReturnType);

                if (Parameters != null && Parameters.Count > 0)
                {
                    string signature = string.Join(", ", Parameters.Select(p => p.Item1 + (string.IsNullOrEmpty(p.Item2) ? string.Empty : ": " + p.Item2)));

                    descBuf.Append(" (");
                    descBuf.Append(signature);
                    descBuf.Append(")");
                }

                return descBuf.ToString();
            }
        }

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

            return MangledName.Equals(obj as FunctionItem);
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

        public FunctionItem(string assembly,
                            string className,
                            string name,
                            LibraryItemType type,
                            List<Tuple<string, string>> parameters,
                            string returnType,
                            List<string> returnKeys = null) :
            base(assembly, className, name, type)
        {
            Parameters = parameters;
            ReturnKeys = returnKeys;
            ReturnType = returnType;
        }
    }

    /// <summary>
    /// A helper class to get some information from DesignScript core.
    /// </summary>
    public class LibraryServices
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

        public LibraryServices()
        {
            GraphUtilities.PreloadAssembly(BuiltinLibraries);
            _libraryFunctionMap = new Dictionary<string, List<FunctionItem>>(new LibraryPathComparer());
            PopulateBuiltIns();
            PopulateOperators();
            PopulateBuiltinLibraries();
        }

        /// <summary>
        /// Get a list of imported libraries.
        /// </summary>
        public List<string> Libraries
        {
            get
            {
                if (_libraryFunctionMap == null)
                {
                    return null;
                }

                return _libraryFunctionMap.Keys.ToList();
            }
        }

        private readonly List<string> _builtinLibraries = new List<string>
        {
            "Math.dll", "ProtoGeometry.dll"
        };
        public List<string> BuiltinLibraries
        {
            get
            {
                return _builtinLibraries;
            }
        }

        private readonly List<string> _importedLibraries = new List<string>();
        public List<string> ImportedLibraries
        {
            get
            {
                return _importedLibraries;
            }
        }

        /// <summary>
        /// Get a list of imported functions.
        /// </summary>
        /// <param name="library"></param>
        /// <returns></returns>
        public List<FunctionItem> this[string library]
        {
            get
            {
                if (string.IsNullOrEmpty(library))
                {
                    return null;
                }

                List<FunctionItem> functions;
                _libraryFunctionMap.TryGetValue(library, out functions);
                return functions;
            }
        }

        /// <summary>
        /// Import a library (if it hasn't been imported yet).
        /// </summary>
        /// <param name="libraryPath"></param>
        public void ImportLibrary(string libraryPath)
        {
            if (!File.Exists(libraryPath))
            {
                string errorMessage = string.Format("Cannot find library path: {0}.", libraryPath);
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(libraryPath, errorMessage));
                return;
            }

            libraryPath = Path.GetFullPath(libraryPath);
            if (_libraryFunctionMap.ContainsKey(libraryPath))
            {
                string errorMessage = string.Format("Library {0} has been loaded.", libraryPath);
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(libraryPath, errorMessage));
                return;
            }

            OnLibraryLoading(new LibraryLoadingEventArgs(libraryPath));
            var functions = new List<FunctionItem>();

            try
            {
                int globalFunctionNumber = GraphUtilities.GetGlobalMethods(string.Empty).Count;

                DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
                var importedClasses = GraphUtilities.GetClassesForAssembly(libraryPath);
                if (GraphUtilities.BuildStatus.ErrorCount > 0)
                {
                    string errorMessage = string.Format("Build error for library: {0}", libraryPath);
                    DynamoLogger.Instance.LogWarning(errorMessage, WarningLevel.Moderate);
                    foreach (var error in GraphUtilities.BuildStatus.Errors)
                    {
                        DynamoLogger.Instance.LogWarning(error.Message, WarningLevel.Moderate);
                        errorMessage += error.Message + "\n";
                    }

                    OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(libraryPath, errorMessage));
                    return;
                }

                foreach (ClassNode classNode in importedClasses)
                {
                    ImportClass(classNode, libraryPath, functions);
                }

                // GraphUtilities.GetGlobalMethods() ignores input and just 
                // return all global functions. The workaround is to get 
                // new global functions after importing this assembly.
                var globalFunctions = GraphUtilities.GetGlobalMethods(string.Empty);
                for (int i = globalFunctionNumber; i < globalFunctions.Count; ++i)
                {
                    var functionItem = ImportProcedure(libraryPath, null, globalFunctions[i]);
                    if (functionItem != null)
                        functions.Add(functionItem);
                }
            }
            catch (Exception e)
            {
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(libraryPath, e.Message));
                return;
            }

            AddToLibraryList(libraryPath, functions);
            OnLibraryLoaded(new LibraryLoadedEventArgs(libraryPath));
        }

        private class LibraryPathComparer: IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase) == 0;
            }

            public int GetHashCode(string obj)
            {
                return obj.ToUpper().GetHashCode();
            }
        }

        private readonly Dictionary<string, List<FunctionItem>> _libraryFunctionMap;

        private void AddToLibraryList(string libraryPath, List<FunctionItem> functions)
        {
            if (string.IsNullOrEmpty(libraryPath))
            {
                throw new ArgumentException("Invalid library path");
            }

            if (_libraryFunctionMap.ContainsKey(libraryPath))
            {

            }
            else
            {
                _libraryFunctionMap[libraryPath] = functions;
            }
        }       

        private void PopulateBuiltIns()
        {
            const string category = Categories.BuiltIns;
            List<ProcedureNode> builtins = GraphUtilities.BuiltInMethods;

            var builtInFunctions = from method in builtins
                                   let arguments = method.argInfoList.Zip(method.argTypeList, (arg, argType) => Tuple.Create(arg.Name, argType.ToString())).ToList()
                                   select new FunctionItem( null, null, method.name, LibraryItemType.GenericFunction, arguments, method.returntype.ToString());

            AddToLibraryList(category, builtInFunctions.ToList());
        }

        private void PopulateOperators()
        {
            const string category = Categories.Operators;
            var opFunctions = new List<FunctionItem>();
            var args = new List<Tuple<string, string>> 
            {
                Tuple.Create("x", string.Empty),
                Tuple.Create("y", string.Empty),
            };

            opFunctions.Add(new FunctionItem(null, null, Op.GetOpFunction(Operator.add), LibraryItemType.GenericFunction, args, null));
            opFunctions.Add(new FunctionItem(null, null, Op.GetOpFunction(Operator.sub), LibraryItemType.GenericFunction, args, null));
            opFunctions.Add(new FunctionItem(null, null, Op.GetOpFunction(Operator.mul), LibraryItemType.GenericFunction, args, null));
            opFunctions.Add(new FunctionItem(null, null, Op.GetOpFunction(Operator.div), LibraryItemType.GenericFunction, args, null));

            AddToLibraryList(category, opFunctions);
        }

        private FunctionItem ImportProcedure(string library, string className, ProcedureNode proc)
        {
            if (proc.isAutoGeneratedThisProc)
                return null;

            string procName = proc.name;
            if (CoreUtils.IsSetter(procName) || procName.Equals(ProtoCore.DSDefinitions.Keyword.Dispose))
                return null;

            LibraryItemType type;
            if (CoreUtils.IsGetter(procName))
            {
                type = proc.isStatic ? LibraryItemType.StaticProperty : LibraryItemType.InstanceProperty;

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
                    type = LibraryItemType.Constructor;
                }
                else if (proc.isStatic)
                {
                    type = LibraryItemType.StaticMethod;
                }
                else if (!string.IsNullOrEmpty(className))
                {
                    type = LibraryItemType.InstanceMethod;
                }
                else
                {
                    type = LibraryItemType.GenericFunction;
                }
            }
            
            List<Tuple<string, string>> arguments = proc.argInfoList.Zip(proc.argTypeList, (arg, argType) => Tuple.Create(arg.Name, argType.ToString())).ToList();
            List<string> returnKeys = null;
            if (proc.MethodAttribute != null && proc.MethodAttribute.MutilReturnMap != null)
            {
                returnKeys = proc.MethodAttribute.MutilReturnMap.Keys.ToList();
            }

            var function = new FunctionItem(library, className, procName, type, arguments, proc.returntype.ToString(), returnKeys);
            return function;
        }

        private void ImportClass(ClassNode classNode, string libraryPath, List<FunctionItem> functions)
        {
            functions.AddRange(
                classNode.vtable.procList.Select(
                    proc =>
                        ImportProcedure(libraryPath, classNode.name, proc))
                         .Where(function => function != null));
        }

        private void PopulateBuiltinLibraries()
        {
            var importedFunctions = new Dictionary<string, List<FunctionItem>>(new LibraryPathComparer());
            foreach (var library in BuiltinLibraries)
            {
                importedFunctions[library] = new List<FunctionItem>();
            }

            foreach (var classNode in GraphUtilities.ClassTable.ClassNodes)
            {
                if (classNode.IsImportedClass && !string.IsNullOrEmpty(classNode.ExternLib))
                {
                    string library = Path.GetFileName(classNode.ExternLib);

                    List<FunctionItem> functions;
                    if (importedFunctions.TryGetValue(library, out functions))
                    {
                        ImportClass(classNode, library, functions);
                    }
                }
            }

            foreach (var keyValue in importedFunctions)
            {
                AddToLibraryList(keyValue.Key, keyValue.Value); 
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
            _importedLibraries.Add(e.LibraryPath);

            EventHandler<LibraryLoadedEventArgs> handler = LibraryLoaded;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
