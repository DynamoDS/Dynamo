using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using Dynamo.Interfaces;
using Dynamo.Library;
using DynamoUtilities;

using ProtoCore.AST.AssociativeAST;
using ProtoCore.BuildData;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using ProtoFFI;

using Operator = ProtoCore.DSASM.Operator;

namespace Dynamo.DSEngine
{
    /// <summary>
    ///     LibraryServices is a singleton class which manages builtin libraries
    ///     as well as imported libraries. It is across different sessions.
    /// </summary>
    public class LibraryServices: IDisposable
    {
        private readonly Dictionary<string, FunctionGroup> builtinFunctionGroups =
            new Dictionary<string, FunctionGroup>();

        private readonly Dictionary<string, Dictionary<string, FunctionGroup>> importedFunctionGroups =
            new Dictionary<string, Dictionary<string, FunctionGroup>>(new LibraryPathComparer());

        private List<string> importedLibraries = new List<string>();

        private readonly ProtoCore.Core libraryManagementCore;

        private Dictionary<string, string> priorNameHints =
            new Dictionary<string, string>();

        public LibraryServices(ProtoCore.Core libraryManagementCore)
        {
            this.libraryManagementCore = libraryManagementCore;

            PreloadLibraries();
            PopulateBuiltIns();
            PopulateOperators();
            PopulatePreloadLibraries();
        }

        public void Dispose()
        {
            builtinFunctionGroups.Clear();
            importedFunctionGroups.Clear();
            importedLibraries.Clear();
        }

        /// <summary>
        ///     Get a list of imported libraries.
        /// </summary>
        public IEnumerable<string> ImportedLibraries
        {
            get { return importedLibraries; }
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

        private void PreloadLibraries()
        {
            importedLibraries.AddRange(DynamoPathManager.Instance.PreloadLibraries);

            foreach (var library in importedLibraries)
            {
                CompilerUtils.TryLoadAssemblyIntoCore(libraryManagementCore, library); 
            }
        }

        public string NicknameFromFunctionSignatureHint(string functionSignature)
        {
            string[] splitted = functionSignature.Split('@');

            if (splitted.Length < 1 || String.IsNullOrEmpty(splitted[0]))
                return null;

            string qualifiedFunction = splitted[0];

            if (!priorNameHints.ContainsKey(qualifiedFunction))
                return null;

            string newName = priorNameHints[qualifiedFunction];

            splitted = newName.Split('.');

            if (splitted.Length < 2)
                return null;

            return splitted[splitted.Length - 2] + "." + splitted[splitted.Length - 1];
        }

        public string FunctionSignatureFromFunctionSignatureHint(string functionSignature)
        {
            string[] splitted = functionSignature.Split('@');

            if (splitted.Length < 2 || String.IsNullOrEmpty(splitted[0]) || String.IsNullOrEmpty(splitted[1]))
                return null;

            string qualifiedFunction = splitted[0];

            if (!priorNameHints.ContainsKey(qualifiedFunction))
                return null;

            string newName = priorNameHints[qualifiedFunction];

            return newName + "@" + splitted[1];
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

        /// <summary>
        /// Checks if a given library is already loaded or not.
        /// Only unique assembly names are allowed to be loaded
        /// </summary>
        /// <param name="library"> can be either the full path or the assembly name </param>
        /// <returns> true even if the same library name is loaded from different paths </returns>
        public bool IsLibraryLoaded(string library)
        {
            return importedFunctionGroups.ContainsKey(library);
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
        public void ImportLibrary(string library, ILogger logger)
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
                DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());

                var functionTable = libraryManagementCore.CodeBlockList[0].procedureTable;
                var classTable = libraryManagementCore.ClassTable;

                int functionNumber = functionTable.procList.Count;
                int classNumber = classTable.ClassNodes.Count;

                CompilerUtils.TryLoadAssemblyIntoCore(libraryManagementCore, library);

                if (libraryManagementCore.BuildStatus.ErrorCount > 0)
                {
                    string errorMessage = string.Format("Build error for library: {0}", library);
                    logger.LogWarning(errorMessage, WarningLevel.Moderate);
                    foreach (ErrorEntry error in libraryManagementCore.BuildStatus.Errors)
                    {
                        logger.LogWarning(error.Message, WarningLevel.Moderate);
                        errorMessage += error.Message + "\n";
                    }

                    OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, errorMessage));
                    return;
                }

                var loadedClasses = classTable.ClassNodes.Skip(classNumber);
                foreach (var classNode in loadedClasses)
                {
                    ImportClass(library, classNode);
                }

                var loadedFunctions = functionTable.procList.Skip(functionNumber);
                foreach (var globalFunction in loadedFunctions)
                {
                    ImportProcedure(library, globalFunction);
                }
            }
            catch (Exception e)
            {
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, e.Message));
                return;
            }

            OnLibraryLoaded(new LibraryLoadedEventArgs(library));
        }

        private void ParseLibraryMigrations(string library)
        {
            string fullLibraryName = library;

            if (!DynamoPathManager.Instance.ResolveLibraryPath(ref fullLibraryName))
                return;

            string migrationsXMLFile = Path.Combine(Path.GetDirectoryName(fullLibraryName),
                Path.GetFileNameWithoutExtension(fullLibraryName) + ".Migrations.xml");

            if (!File.Exists(migrationsXMLFile))
                return;

            var foundPriorNameHints = new Dictionary<string, string>();

            try
            {
                using (var reader = XmlReader.Create(migrationsXMLFile))
                {
                    while (reader.Read())
                    {
                        reader.ReadToFollowing("priorNameHint");

                        if (!reader.Read())
                            break;

                        reader.ReadToFollowing("oldName");
                        string oldName = reader.ReadElementContentAsString();
                        reader.ReadToFollowing("newName");
                        string newName = reader.ReadElementContentAsString();

                        foundPriorNameHints[oldName] = newName;
                    }
                }
            }
            catch (Exception e)
            {
                return; // if the XML file is badly formatted, return like it doesn't exist
            }

            // if everything parsed correctly, then add these names to the priorNameHints

            foreach (string key in foundPriorNameHints.Keys)
            {
                priorNameHints[key] = foundPriorNameHints[key];
            }
        }

        private void AddImportedFunctions(string library, IEnumerable<FunctionDescriptor> functions)
        {
            if (null == library || null == functions)
                throw new ArgumentNullException();

            ParseLibraryMigrations(library);

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
            var builtins = libraryManagementCore.CodeBlockList[0]
                                                .procedureTable
                                                .procList
                                                .Where(p =>
                    !p.name.StartsWith(Constants.kInternalNamePrefix) &&
                    !p.name.Equals("Break"));

            IEnumerable<FunctionDescriptor> functions = from method in builtins
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
            foreach (ClassNode classNode in libraryManagementCore.ClassTable.ClassNodes)
            {
                if (classNode.IsImportedClass && !string.IsNullOrEmpty(classNode.ExternLib))
                {
                    string library = Path.GetFileName(classNode.ExternLib);
                    ImportClass(library, classNode);
                }
            }
        }

        private void ImportProcedure(string library, ProcedureNode proc)
        {
            string procName = proc.name;
            if (proc.isAutoGeneratedThisProc ||
                CoreUtils.IsSetter(procName) ||
                CoreUtils.IsDisposeMethod(procName) ||
                CoreUtils.StartsWithDoubleUnderscores(procName))
            {
                return;
            }

            string obsoleteMessage = "";
            int classScope = proc.classScope;
            string className = string.Empty;
            MethodAttributes methodAttribute = proc.MethodAttribute;
            ClassAttributes classAttribute = null;

            if (classScope != ProtoCore.DSASM.Constants.kGlobalScope)
            {
                var classNode = libraryManagementCore.ClassTable.ClassNodes[classScope];

                classAttribute = classNode.ClassAttributes;
                className = classNode.name;
            }

            // MethodAttribute's HiddenInLibrary has higher priority than
            // ClassAttribute's HiddenInLibrary
            bool isVisible = true;
            if (methodAttribute != null)
            {
                isVisible = !methodAttribute.HiddenInLibrary;
            }
            else
            {
                if (classAttribute != null)
                    isVisible = !classAttribute.HiddenInLibrary;
            }

            FunctionType type;

            if (classScope == ProtoCore.DSASM.Constants.kGlobalScope)
            {
                type = FunctionType.GenericFunction;
            }
            else
            {
                if (CoreUtils.IsGetter(procName))
                {
                    type = proc.isStatic
                        ? FunctionType.StaticProperty
                        : FunctionType.InstanceProperty;

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
            if (proc.MethodAttribute != null)
            {
                if (proc.MethodAttribute.ReturnKeys != null)
                    returnKeys = proc.MethodAttribute.ReturnKeys;
                if (proc.MethodAttribute.IsObsolete)
                    obsoleteMessage = proc.MethodAttribute.ObsoleteMessage;
            }

            var function = new FunctionDescriptor(
                library,
                className,
                procName,
                arguments,
                proc.returntype.ToString(),
                type,
                isVisible,
                returnKeys,
                proc.isVarArg,
                obsoleteMessage);

            AddImportedFunctions(library, new[] { function });
        }

        private void ImportClass(string library, ClassNode classNode)
        {
            foreach (ProcedureNode proc in classNode.vtable.procList)
                ImportProcedure(library, proc);
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
            importedLibraries.Add(e.LibraryPath);

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
