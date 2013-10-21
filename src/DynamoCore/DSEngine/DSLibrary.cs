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

        public class LibraryLoadedEventArgs : EventArgs
        {
            public LibraryLoadedEventArgs(string libraryPath)
            {
                LibraryPath = libraryPath;
            }

            public string LibraryPath { get; private set; }
        }

        public delegate void LibraryLoadedEventHandler(object sender, LibraryLoadedEventArgs e);
        public event LibraryLoadedEventHandler LibraryLoaded;
        public static DSLibraryServices Instance = new DSLibraryServices();

        public List<string> Libraries
        {
            get
            {
                if (importedFunctions == null)
                {
                    return null;
                }

                return importedFunctions.Keys.ToList();
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

        public List<DSFunctionItem> this[string library]
        {
            get
            {
                if (string.IsNullOrEmpty(library))
                {
                    return null;
                }

                List<DSFunctionItem> functions = null;;
                importedFunctions.TryGetValue(library, out functions);
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
                return;
            }

            foreach (ProtoCore.DSASM.ClassNode classNode in importedClasses)
            {
                ImportClass(classNode);
            }

            LibraryLoaded(this, new LibraryLoadedEventArgs(libraryPath));
        }

        private Dictionary<string, List<DSFunctionItem>> importedFunctions;

        private List<DSFunctionItem> TryGetImportedFunctions(string libraryPath)
        {
            if (string.IsNullOrEmpty(libraryPath))
            {
                throw new ArgumentException("Invalid category");
            }

            List<DSFunctionItem> functions = null;
            if (!importedFunctions.TryGetValue(libraryPath, out functions))
            {
                functions = new List<DSFunctionItem>();
                importedFunctions[libraryPath] = functions;
            }
            return functions;
        }       

        private void PopulateBuiltIns()
        {
            string category = BuiltInCategories.BuiltIns;
            List<DSFunctionItem> builtInFunctions = TryGetImportedFunctions(category);

            List<ProcedureNode> builtins = GraphToDSCompiler.GraphUtilities.BuiltInMethods;
            foreach (var method in builtins)
            {
                List<string> arguments = method.argInfoList.Select(x => x.Name).ToList();
                builtInFunctions.Add(new DSFunctionItem(null, category, null, method.name, method.name, DSLibraryItemType.GenericFunction, arguments, null));
            }
        }

        private void PopulateOperators()
        {
            string category = BuiltInCategories.Operators;
            List<DSFunctionItem> opFunctions = TryGetImportedFunctions(category);

            List<string> args = new List<string> { "operand1", "operand2" };
            opFunctions.Add(new DSFunctionItem(null, category, null, Op.GetOpFunction(Operator.add), Op.GetOpSymbol(Operator.add), DSLibraryItemType.GenericFunction, args, null));
            opFunctions.Add(new DSFunctionItem(null, category, null, Op.GetOpFunction(Operator.sub), Op.GetOpSymbol(Operator.sub), DSLibraryItemType.GenericFunction, args, null));
            opFunctions.Add(new DSFunctionItem(null, category, null, Op.GetOpFunction(Operator.mul), Op.GetOpSymbol(Operator.mul), DSLibraryItemType.GenericFunction, args, null));
            opFunctions.Add(new DSFunctionItem(null, category, null, Op.GetOpFunction(Operator.div), Op.GetOpSymbol(Operator.div), DSLibraryItemType.GenericFunction, args, null));
        }

        private string GetCategory(ClassNode classNode)
        {
            string libraryPath = Path.GetFullPath(classNode.ExternLib);
            string category = Path.GetFileNameWithoutExtension(libraryPath) + "." + classNode.name;
            return category;
        }

        private DSFunctionItem ImportProcedure(string library, string category, string className, ProcedureNode proc)
        {
            DSLibraryItemType type = DSLibraryItemType.GenericFunction;
            string displayName = proc.name;

            if (proc.isConstructor)
            {
                type = DSLibraryItemType.Constructor;
            }
            else if (proc.isStatic)
            {
                if (CoreUtils.IsGetterSetter(proc.name))
                {
                    type = DSLibraryItemType.StaticProperty;
                }
                else
                {
                    type = DSLibraryItemType.StaticMethod;
                }
            }

            if (CoreUtils.IsGetterSetter(proc.name))
            {            
                CoreUtils.TryGetPropertyName(proc.name, out displayName);
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

        private void ImportClass(ClassNode classNode)
        {
            if (!classNode.IsImportedClass || string.IsNullOrEmpty(classNode.ExternLib))
            {
                return;
            }

            string libraryPath = Path.GetFullPath(classNode.ExternLib);
            List<DSFunctionItem> functions = TryGetImportedFunctions(libraryPath);

            string category = GetCategory(classNode);
            foreach (var proc in classNode.vtable.procList)
            {
                var function = ImportProcedure(libraryPath, category, classNode.name, proc);
                functions.Add(function);
            }
        }

        private void PopulatePreLoadedLibaries()
        {
            foreach (var classNode in GraphToDSCompiler.GraphUtilities.ClassTable.ClassNodes)
            {
                if (classNode.IsImportedClass && !string.IsNullOrEmpty(classNode.ExternLib))
                {
                    ImportClass(classNode);
                }
            }
        }

        private DSLibraryServices()
        {
            GraphToDSCompiler.GraphUtilities.PreloadAssembly(this.PreLoadedLibraries);

            importedFunctions = new Dictionary<string, List<DSFunctionItem>>();
            PopulateBuiltIns();
            PopulateOperators();
            PopulatePreLoadedLibaries();
        }
    }
}
