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
    public enum DSLibraryItemType
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
    public abstract class DSLibraryItem
    {
        public string Assembly { get; set; }
        public string Category { get; set; }
        public string ClassName { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public DSLibraryItemType Type { get; set; }
        public string QualifiedName
        {
            get
            {
                return string.IsNullOrEmpty(ClassName) ? Name : ClassName + "." + Name;
            }
        }

        public DSLibraryItem(string assembly, string category, string className, string name, string displayName, DSLibraryItemType type)
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
    public class DSFunctionItem : DSLibraryItem
    {
        public List<string> Arguments { get; set; }
        public List<string> ReturnKeys { get; set; }

        public DSFunctionItem(string assembly,
                                    string category,
                                    string className,
                                    string name,
                                    string displayName,
                                    DSLibraryItemType type,
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
    public class DSLibraryServices
    {
        public static class BuiltInCategories
        {
            public const string BuiltIns = "Builtin Functions";
            public const string Operators = "Opertors";
        }

        public enum LibraryLoadStatus
        {
            Ok,
            Failed
        }

        public class LibraryLoadedEventArgs : EventArgs
        {
            public LibraryLoadedEventArgs(string libraryPath, LibraryLoadStatus status, string message)
            {
                LibraryPath = libraryPath;
                Status = status;
                Message = message;
            }

            public string LibraryPath { get; private set; }
            public LibraryLoadStatus Status { get; private set; }
            public string Message { get; private set;}
        }

        public class LibraryLoadingEventArgs : EventArgs
        {
            public LibraryLoadingEventArgs(string libraryPath)
            {
                LibraryPath = libraryPath;
            }

            public string LibraryPath { get; private set; }
        }

        public delegate void LibraryLoadingEventHandler(object sender, LibraryLoadingEventArgs e);
        public event LibraryLoadingEventHandler LibraryLoading = delegate {};

        public delegate void LibraryLoadedEventHandler(object sender, LibraryLoadedEventArgs e);
        public event LibraryLoadedEventHandler LibraryLoaded = delegate {};

        public static DSLibraryServices Instance = new DSLibraryServices();

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

        private List<string> preLoadedLibraries;
        public List<string> PreLoadedLibraries
        {
            get
            {
                if (preLoadedLibraries == null)
                {
                    preLoadedLibraries = new List<string> { "Math.dll", "ProtoGeometry.dll" };
                }
                return preLoadedLibraries;
            }
        }

        /// <summary>
        /// Get a list of imported functions.
        /// </summary>
        /// <param name="library"></param>
        /// <returns></returns>
        public List<DSFunctionItem> this[string library]
        {
            get
            {
                if (string.IsNullOrEmpty(library))
                {
                    return null;
                }

                List<DSFunctionItem> functions = null;;
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
                throw new FileNotFoundException(errorMessage);
            }

            libraryPath = Path.GetFullPath(libraryPath);
            if (this.Libraries.Any(path => string.Compare(libraryPath, path, true) == 0))
            {
                return;
            }

            LibraryLoading(this, new LibraryLoadingEventArgs(libraryPath));
            List<DSFunctionItem> functions = new List<DSFunctionItem>();

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
                    }

                    LibraryLoaded(this, new LibraryLoadedEventArgs(libraryPath, LibraryLoadStatus.Failed, errorMessage));
                    return;
                }

                foreach (ProtoCore.DSASM.ClassNode classNode in importedClasses)
                {
                    ImportClass(classNode, libraryPath, functions);
                }
            }
            catch (Exception e)
            {
                LibraryLoaded(this, new LibraryLoadedEventArgs(libraryPath, LibraryLoadStatus.Failed, e.Message));
            }

            AddToLibraryList(libraryPath, functions);
            LibraryLoaded(this, new LibraryLoadedEventArgs(libraryPath, LibraryLoadStatus.Ok, null));
        }

        private Dictionary<string, List<DSFunctionItem>> libraryFunctionMap;

        private void AddToLibraryList(string libraryPath, List<DSFunctionItem> functions)
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
            List<DSFunctionItem> builtInFunctions = new List<DSFunctionItem>();
            List<ProcedureNode> builtins = GraphToDSCompiler.GraphUtilities.BuiltInMethods;

            foreach (var method in builtins)
            {
                List<string> arguments = method.argInfoList.Select(x => x.Name).ToList();
                builtInFunctions.Add(new DSFunctionItem(null, category, null, method.name, method.name, DSLibraryItemType.GenericFunction, arguments, null));
            }

            AddToLibraryList(category, builtInFunctions);
        }

        private void PopulateOperators()
        {
            string category = BuiltInCategories.Operators;
            List<DSFunctionItem> opFunctions = new List<DSFunctionItem>();

            List<string> args = new List<string> { "operand1", "operand2" };
            opFunctions.Add(new DSFunctionItem(null, category, null, Op.GetOpFunction(Operator.add), Op.GetOpSymbol(Operator.add), DSLibraryItemType.GenericFunction, args, null));
            opFunctions.Add(new DSFunctionItem(null, category, null, Op.GetOpFunction(Operator.sub), Op.GetOpSymbol(Operator.sub), DSLibraryItemType.GenericFunction, args, null));
            opFunctions.Add(new DSFunctionItem(null, category, null, Op.GetOpFunction(Operator.mul), Op.GetOpSymbol(Operator.mul), DSLibraryItemType.GenericFunction, args, null));
            opFunctions.Add(new DSFunctionItem(null, category, null, Op.GetOpFunction(Operator.div), Op.GetOpSymbol(Operator.div), DSLibraryItemType.GenericFunction, args, null));

            AddToLibraryList(category, opFunctions);
        }

        private string GetCategory(ClassNode classNode)
        {
            string libraryPath = Path.GetFullPath(classNode.ExternLib);
            string category = Path.GetFileNameWithoutExtension(libraryPath) + "." + classNode.name;
            return category;
        }

        private DSFunctionItem ImportProcedure(string library, string category, string className, ProcedureNode proc)
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
            DSLibraryItemType type = DSLibraryItemType.GenericFunction;
            if (CoreUtils.IsGetterSetter(proc.name))
            {
                type = proc.isStatic ? DSLibraryItemType.StaticProperty : DSLibraryItemType.InstanceProperty;
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
                    type = DSLibraryItemType.Constructor;
                }
                else if (proc.isStatic)
                {
                    type = DSLibraryItemType.StaticMethod;
                }
                else if (!string.IsNullOrEmpty(className))
                {
                    type = DSLibraryItemType.InstanceMethod;
                }
                else
                {
                    type = DSLibraryItemType.GenericFunction;
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

            var function = new DSFunctionItem(library, category, className, proc.name, displayName, type, arguments, returnKeys);
            return function;
        }

        private void ImportClass(ClassNode classNode, string libraryPath, List<DSFunctionItem> functions)
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

        private void PopulatePreLoadedLibaries()
        {
            Dictionary<string, List<DSFunctionItem>> importedFunctions = new Dictionary<string, List<DSFunctionItem>>();
            foreach (var library in PreLoadedLibraries)
            {
                string libraryPath = Path.GetFullPath(library);
                importedFunctions[libraryPath] = new List<DSFunctionItem>();
            }

            foreach (var classNode in GraphToDSCompiler.GraphUtilities.ClassTable.ClassNodes)
            {
                if (classNode.IsImportedClass && !string.IsNullOrEmpty(classNode.ExternLib))
                {
                    string libraryPath = Path.GetFullPath(classNode.ExternLib);
                    List<DSFunctionItem> functions = null;

                    if (importedFunctions.TryGetValue(libraryPath, out functions))
                    {
                        ImportClass(classNode, libraryPath, functions);
                    }
                }
            }

            foreach (var keyValue in importedFunctions)
            {
                AddToLibraryList(keyValue.Key, keyValue.Value); 
            }
        }

        private DSLibraryServices()
        {
            GraphToDSCompiler.GraphUtilities.PreloadAssembly(this.PreLoadedLibraries);

            libraryFunctionMap = new Dictionary<string, List<DSFunctionItem>>();
            PopulateBuiltIns();
            PopulateOperators();
            PopulatePreLoadedLibaries();
        }
    }
}
