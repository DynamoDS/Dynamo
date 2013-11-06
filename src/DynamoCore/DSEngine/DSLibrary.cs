using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Nodes;
using Dynamo.UI.Commands;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using ProtoFFI;

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
        public string Category { get; set; }
        public string ClassName { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public LibraryItemType Type { get; set; }
        public string QualifiedName
        {
            get
            {
                return string.IsNullOrEmpty(ClassName) ? Name : ClassName + "." + Name;
            }
        }

        public LibraryItem(string assembly, string category, string className, string name, string displayName, LibraryItemType type)
        {
            Assembly = assembly;
            Category = category;
            ClassName = className;
            Name = name;
            DisplayName = displayName;
            Type = type;
        }
    }

    /// <summary>
    /// Describe a DesignScript function in a imported library
    /// </summary>
    public class FunctionItem : LibraryItem
    {
        public List<string> Arguments { get; set; }
        public List<string> ReturnKeys { get; set; }

        public FunctionItem(string assembly,
                            string category,
                            string className,
                            string name,
                            string displayName,
                            LibraryItemType type,
                            List<string> arguments,
                            List<string> returnKeys = null) :
            base(assembly, category, className, name, displayName, type)
        {
            this.Arguments = arguments;
            this.ReturnKeys = returnKeys;
        }
    }

    /// <summary>
    /// A helper class to get some information from DesignScript core.
    /// </summary>
    public class LibraryServices
    {
        public static class BuiltInCategories
        {
            public const string BuiltIns = "Builtin Functions";
            public const string Operators = "Opertors";
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
                LibraryPath = libraryPath; ;
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
            GraphToDSCompiler.GraphUtilities.PreloadAssembly(this.BuiltinLibraries);
            libraryFunctionMap = new Dictionary<string, List<FunctionItem>>(new LibraryPathComparer());
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
                if (libraryFunctionMap == null)
                {
                    return null;
                }

                return libraryFunctionMap.Keys.ToList();
            }
        }

        private List<string> builtinLibraries = new List<string> { "Math.dll", "ProtoGeometry.dll"};
        public List<string> BuiltinLibraries
        {
            get
            {
                return builtinLibraries;
            }
        }

        private List<string> importedLibraries = new List<string>();
        public List<string> ImportedLibraries
        {
            get
            {
                return importedLibraries;
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

                List<FunctionItem> functions = null;;
                libraryFunctionMap.TryGetValue(library, out functions);
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
            if (this.libraryFunctionMap.ContainsKey(libraryPath))
            {
                string errorMessage = string.Format("Library {0} has been loaded.", libraryPath);
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(libraryPath, errorMessage));
                return;
            }

            OnLibraryLoading(new LibraryLoadingEventArgs(libraryPath));
            List<FunctionItem> functions = new List<FunctionItem>();

            try
            {
                DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
                var importedClasses = GraphToDSCompiler.GraphUtilities.GetClassesForAssembly(libraryPath);
                if (GraphToDSCompiler.GraphUtilities.BuildStatus.ErrorCount > 0)
                {
                    string errorMessage = string.Format("Build error for library: {0}", libraryPath);
                    DynamoLogger.Instance.LogWarning(errorMessage, WarningLevel.Moderate);
                    foreach (var error in GraphToDSCompiler.GraphUtilities.BuildStatus.Errors)
                    {
                        DynamoLogger.Instance.LogWarning(error.Message, WarningLevel.Moderate);
                        errorMessage += error.Message + "\n";
                    }

                    OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(libraryPath, errorMessage));
                    return;
                }

                foreach (ProtoCore.DSASM.ClassNode classNode in importedClasses)
                {
                    ImportClass(classNode, libraryPath, functions);
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

        private Dictionary<string, List<FunctionItem>> libraryFunctionMap;

        private void AddToLibraryList(string libraryPath, List<FunctionItem> functions)
        {
            if (string.IsNullOrEmpty(libraryPath))
            {
                throw new ArgumentException("Invalid library path");
            }

            if (libraryFunctionMap.ContainsKey(libraryPath))
            {

            }
            else
            {
                libraryFunctionMap[libraryPath] = functions;
            }
        }       

        private void PopulateBuiltIns()
        {
            string category = BuiltInCategories.BuiltIns;
            List<FunctionItem> builtInFunctions = new List<FunctionItem>();
            List<ProcedureNode> builtins = GraphToDSCompiler.GraphUtilities.BuiltInMethods;

            foreach (var method in builtins)
            {
                List<string> arguments = method.argInfoList.Select(x => x.Name).ToList();
                builtInFunctions.Add(new FunctionItem(null, category, null, method.name, method.name, LibraryItemType.GenericFunction, arguments, null));
            }

            AddToLibraryList(category, builtInFunctions);
        }

        private void PopulateOperators()
        {
            string category = BuiltInCategories.Operators;
            List<FunctionItem> opFunctions = new List<FunctionItem>();

            List<string> args = new List<string> { "operand1", "operand2" };
            opFunctions.Add(new FunctionItem(null, category, null, Op.GetOpFunction(Operator.add), Op.GetOpSymbol(Operator.add), LibraryItemType.GenericFunction, args, null));
            opFunctions.Add(new FunctionItem(null, category, null, Op.GetOpFunction(Operator.sub), Op.GetOpSymbol(Operator.sub), LibraryItemType.GenericFunction, args, null));
            opFunctions.Add(new FunctionItem(null, category, null, Op.GetOpFunction(Operator.mul), Op.GetOpSymbol(Operator.mul), LibraryItemType.GenericFunction, args, null));
            opFunctions.Add(new FunctionItem(null, category, null, Op.GetOpFunction(Operator.div), Op.GetOpSymbol(Operator.div), LibraryItemType.GenericFunction, args, null));

            AddToLibraryList(category, opFunctions);
        }

        private string GetCategory(ClassNode classNode)
        {
            string libraryPath = Path.GetFullPath(classNode.ExternLib);
            string category = Path.GetFileNameWithoutExtension(libraryPath) + "." + classNode.name;
            return category;
        }

        private FunctionItem ImportProcedure(string library, string category, string className, ProcedureNode proc)
        {
            bool isGenericFunction = string.IsNullOrEmpty(className);
            if (!isGenericFunction)
            {
                // Skip overloaded member functions whose first parameter is 
                // %thisPtrArg
                if (proc.argInfoList != null && proc.argInfoList.Count >= 1)
                {
                    var firstArgument = proc.argInfoList[0];
                    if (firstArgument.Name.Equals(ProtoCore.DSASM.Constants.kThisPointerArgName))
                    {
                        return null;
                    }
                }
            }

            string displayName = proc.name;
            LibraryItemType type = LibraryItemType.GenericFunction;
            if (CoreUtils.IsGetterSetter(proc.name))
            {
                type = proc.isStatic ? LibraryItemType.StaticProperty : LibraryItemType.InstanceProperty;
                CoreUtils.TryGetPropertyName(proc.name, out displayName);

                // temporary add prefix to distinguish getter/setter until the
                // design is nailed down
                if (CoreUtils.IsGetter(proc.name))
                {
                    displayName = "get" + displayName;
                }
                else
                {
                    displayName = "set" + displayName;
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
            
            if (!string.IsNullOrEmpty(className))
            {
                displayName = className + "." + displayName;
            }

            List<string> arguments = proc.argInfoList.Select(x => x.Name).ToList();
            List<string> returnKeys = null;
            if (proc.MethodAttribute != null && proc.MethodAttribute.MutilReturnMap != null)
            {
                returnKeys = proc.MethodAttribute.MutilReturnMap.Keys.ToList();
            }

            var function = new FunctionItem(library, category, className, proc.name, displayName, type, arguments, returnKeys);
            return function;
        }

        private void ImportClass(ClassNode classNode, string libraryPath, List<FunctionItem> functions)
        {
            string category = GetCategory(classNode);
            foreach (var proc in classNode.vtable.procList)
            {
                var function = ImportProcedure(libraryPath, category, classNode.name, proc);
                if (function != null)
                {
                    functions.Add(function);
                }
            }
        }

        private void PopulateBuiltinLibraries()
        {
            var importedFunctions = new Dictionary<string, List<FunctionItem>>(new LibraryPathComparer());
            foreach (var library in BuiltinLibraries)
            {
                importedFunctions[library] = new List<FunctionItem>();
            }

            foreach (var classNode in GraphToDSCompiler.GraphUtilities.ClassTable.ClassNodes)
            {
                if (classNode.IsImportedClass && !string.IsNullOrEmpty(classNode.ExternLib))
                {
                    string library = Path.GetFileName(classNode.ExternLib);
                    List<FunctionItem> functions = null;

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
            importedLibraries.Add(e.LibraryPath);

            EventHandler<LibraryLoadedEventArgs> handler = LibraryLoaded;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
