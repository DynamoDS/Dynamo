using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Nodes;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using ProtoFFI;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// A helper class to get some information from DesignScript core.
    /// </summary>
    public class DSLibrary
    {
        public static class BuiltInCategories
        {
            public const string BuiltIns = "Builtin Functions";
            public const string Operators = "Opertors";
        }

        public static DSLibrary Instance = new DSLibrary();

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

        public List<DSFunctionItem> this[string category]
        {
            get
            {
                if (string.IsNullOrEmpty(category))
                {
                    return null;
                }

                List<DSFunctionItem> functions = null;;
                importedFunctions.TryGetValue(category, out functions);
                return functions;
            }
        }

        private List<DSFunctionItem> TryGetImportedFunctions(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                throw new ArgumentException("Invalid category");
            }

            List<DSFunctionItem> functions = null;
            if (!importedFunctions.TryGetValue(category, out functions))
            {
                functions = new List<DSFunctionItem>();
                importedFunctions[category] = functions;
            }
            return functions;
        }

        private Dictionary<string, List<DSFunctionItem>> importedFunctions;

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
                    CoreUtils.TryGetPropertyName(proc.name, out displayName);
                }
                else
                {
                    type = DSLibraryItemType.StaticMethod;
                }
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

            string library = classNode.ExternLib;
            List<DSFunctionItem> functions = TryGetImportedFunctions(library);

            string category = System.IO.Path.GetFileNameWithoutExtension(library) + "." + classNode.name;
            foreach (var proc in classNode.vtable.procList)
            {
                var function = ImportProcedure(library, category, classNode.name, proc);
                functions.Add(function);
            }
        }
  
        private void ImportLibrary(string assemblyPath)
        {
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());

            var importedClasses = GraphToDSCompiler.GraphUtilities.GetClassesForAssembly(assemblyPath);
            if (GraphToDSCompiler.GraphUtilities.BuildStatus.ErrorCount > 0)
            {
                return;
            }

            foreach (ProtoCore.DSASM.ClassNode classNode in importedClasses)
            {
                ImportClass(classNode);
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

        private DSLibrary()
        {
            GraphToDSCompiler.GraphUtilities.PreloadAssembly(new List<string> { "Math.dll", "ProtoGeometry.dll"});

            importedFunctions = new Dictionary<string, List<DSFunctionItem>>();
            PopulateBuiltIns();
            PopulateOperators();
            PopulatePreLoadedLibaries();
        }
    }
}
