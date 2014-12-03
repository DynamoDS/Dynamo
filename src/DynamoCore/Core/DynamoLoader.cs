using System;
using System.Collections.Generic;
using System.Linq;

using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using System.Reflection;
using System.IO;

using DynamoUtilities;
using System.Text;

namespace Dynamo.Utilities
{
    /// <summary>
    /// The DynamoLoader is responsible for loading custom nodes and
    /// types which derive from NodeModel. For information
    /// about package loading see the PackageLoader. For information
    /// about loading other libraries, see LibraryServices.
    /// </summary>
    public class DynamoLoader : LogSourceBase
    {
        #region Properties/Fields

        public readonly HashSet<string> LoadedAssemblyNames = new HashSet<string>();
        private readonly HashSet<Assembly> loadedAssemblies = new HashSet<Assembly>();

        #endregion

        #region Events

        public delegate void AssemblyLoadedHandler(AssemblyLoadedEventArgs args);

        public class AssemblyLoadedEventArgs
        {
            public Assembly Assembly { get; private set; }

            public AssemblyLoadedEventArgs(Assembly assembly)
            {
                Assembly = assembly;
            }
        }

        public event AssemblyLoadedHandler AssemblyLoaded;

        private void OnAssemblyLoaded(Assembly assem)
        {
            if (AssemblyLoaded != null)
            {
                AssemblyLoaded(new AssemblyLoadedEventArgs(assem));
            }
        }

        #endregion
        
        #region Methods
        /// <summary>
        /// Load all types which inherit from NodeModel whose assemblies are located in
        /// the bin/nodes directory. Add the types to the searchviewmodel and
        /// the controller's dictionaries.
        /// </summary>
        /// <param name="context"></param>
        public List<TypeLoadData> LoadNodeModels(string context)
        {
            var loadedAssembliesByPath = new Dictionary<string, Assembly>();
            var loadedAssembliesByName = new Dictionary<string, Assembly>();

            // cache the loaded assembly information
            foreach (
                var assembly in 
                    AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.IsDynamic))
            {
                try
                {
                    loadedAssembliesByPath[assembly.Location] = assembly;
                    loadedAssembliesByName[assembly.FullName] = assembly;
                }
                catch { }
            }

            // find all the dlls registered in all search paths
            // and concatenate with all dlls in the current directory
            List<string> allDynamoAssemblyPaths =
                DynamoPathManager.Instance.Nodes.SelectMany(
                    path => Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly)).ToList();

            // add the core assembly to get things like code block nodes and watches.
            allDynamoAssemblyPaths.Add(Path.Combine(DynamoPathManager.Instance.MainExecPath, "DynamoCore.dll"));

            ResolveEventHandler resolver = 
                (sender, args) =>
                {
                    Assembly resolvedAssembly;
                    loadedAssembliesByName.TryGetValue(args.Name, out resolvedAssembly);
                    return resolvedAssembly;
                };

            AppDomain.CurrentDomain.AssemblyResolve += resolver;

            var result = new List<TypeLoadData>();

            foreach (var assemblyPath in allDynamoAssemblyPaths)
            {
                var fn = Path.GetFileName(assemblyPath);

                if (fn == null)
                    continue;

                // if the assembly has already been loaded, then
                // skip it, otherwise cache it.
                if (LoadedAssemblyNames.Contains(fn))
                    continue;

                LoadedAssemblyNames.Add(fn);

                try
                {
                    Assembly assembly;
                    if (!loadedAssembliesByPath.TryGetValue(assemblyPath, out assembly))
                    {
                        assembly = Assembly.LoadFrom(assemblyPath);
                        loadedAssembliesByName[assembly.GetName().Name] = assembly;
                        loadedAssembliesByPath[assemblyPath] = assembly;
                    }

                    result.AddRange(LoadNodesFromAssembly(assembly, context));
                    loadedAssemblies.Add(assembly);
                    OnAssemblyLoaded(assembly);
                }
                catch (BadImageFormatException)
                {
                    //swallow these warnings.
                }
                catch (Exception e)
                {
                    Log(e);
                }
            }

<<<<<<< HEAD
=======
            var functionGroups = dynamoModel.EngineController.GetFunctionGroups();
            dynamoModel.SearchModel.Add(functionGroups);

#if DEBUG_LIBRARY
            DumpLibrarySnapshot(functionGroups);
#endif
>>>>>>> 95d40a5d4bb8b49d755245c08c15577407d1b098
            AppDomain.CurrentDomain.AssemblyResolve -= resolver;

            return result;
        }

        /// <summary>
        ///     Determine if a Type is a node.  Used by LoadNodesFromAssembly to figure
        ///     out what nodes to load from other libraries (.dlls).
        /// </summary>
        /// <parameter>The type</parameter>
        /// <returns>True if the type is node.</returns>
        public bool IsNodeSubType(Type t)
        {
            return //t.Namespace == "Dynamo.Nodes" &&
                   !t.IsAbstract &&
                   t.IsSubclassOf(typeof(NodeModel));
        }

        private void DumpLibrarySnapshot(IEnumerable<DSEngine.FunctionGroup> functionGroups)
        {
            if (null == functionGroups)
                return;

            StringBuilder sb = new StringBuilder();
            foreach (var functionGroup in functionGroups)
            {
                var functions = functionGroup.Functions.ToList();
                if (!functions.Any())
                    continue;

                foreach (var function in functions)
                {
                    //Don't add the functions that are not visible in library.
                    if (!function.IsVisibleInLibrary)
                        continue;

                    var displayString = function.UserFriendlyName;
                
                    // do not add GetType method names to search
                    if (displayString.Contains("GetType"))
                    {
                        continue;
                    }

                    var nameSpace = string.IsNullOrEmpty(function.Namespace) ? "" : function.Namespace + ".";
                    var description = nameSpace + function.Signature + "\n";

                    sb.Append(description + "\n");
                }
            }
            dynamoModel.Logger.Log(sb.ToString(), LogLevel.File);
        }

        internal bool ContainsNodeModelSubType(Assembly assem)
        {
            return assem.GetTypes().Any(Nodes.Utilities.IsNodeSubType);
        }

        /// <summary>
        ///     Enumerate the types in an assembly and add them to DynamoController's
        ///     dictionaries and the search view model.  Internally catches exceptions and sends the error 
        ///     to the console.
        /// </summary>
        /// <Returns>The list of node types loaded from this assembly</Returns>
        public IEnumerable<TypeLoadData> LoadNodesFromAssembly(Assembly assembly, string context)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            Type[] loadedTypes = null;

            try
            {
                loadedTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Log("Could not load types.");
                Log(e);
                foreach (var ex in e.LoaderExceptions)
                {
<<<<<<< HEAD
                    Log("Dll Load Exception:");
                    Log(ex.ToString());
=======
                    try
                    {
                        var data = Nodes.Utilities.GetDataForType(dynamoModel, t);

                        if (data == null)
                            continue;

                        var attribs = t.GetCustomAttributes(typeof(NodeNameAttribute), false);
                        var isDeprecated = t.GetCustomAttributes(typeof(NodeDeprecatedAttribute), true).Any();
                        var isMetaNode = t.GetCustomAttributes(typeof(IsMetaNodeAttribute), false).Any();
                        var isDSCompatible = t.GetCustomAttributes(typeof(IsDesignScriptCompatibleAttribute), true).Any();

                        bool isHidden = false;
                        if (data.IsObsolete)
                            isHidden = true;
                        else
                        {
                            var attrs = t.GetCustomAttributes(typeof(IsVisibleInDynamoLibraryAttribute), true);
                            if (null != attrs && attrs.Any())
                            {
                                var isVisibleAttr = attrs[0] as IsVisibleInDynamoLibraryAttribute;
                                if (null != isVisibleAttr && isVisibleAttr.Visible == false)
                                {
                                    isHidden = true;
                                }
                            }
                        }

                        string typeName;

                        if (attribs.Length > 0 && !isDeprecated && !isMetaNode && isDSCompatible && !isHidden)
                        {
                            searchViewModel.Add(t);
                            typeName = (attribs[0] as NodeNameAttribute).Name;
                        }
                        else
                            typeName = t.Name;

                        AssemblyPathToTypesLoaded[assembly.Location].Add(t);

                        if (!dynamoModel.BuiltInTypesByNickname.ContainsKey(typeName))
                            dynamoModel.BuiltInTypesByNickname.Add(typeName, data);
                        else
                            dynamoModel.Logger.Log("Duplicate type encountered: " + typeName);

                        if (!dynamoModel.BuiltInTypesByName.ContainsKey(t.FullName))
                            dynamoModel.BuiltInTypesByName.Add(t.FullName, data);
                        else
                            dynamoModel.Logger.Log("Duplicate type encountered: " + typeName);

                        
                    }
                    catch (Exception e)
                    {
                        dynamoModel.Logger.Log("Failed to load type from " + assembly.FullName);
                        dynamoModel.Logger.Log("The type was " + t.FullName);
                        dynamoModel.Logger.Log(e);
                    }

                    OnAssemblyLoaded(assembly);
>>>>>>> 95d40a5d4bb8b49d755245c08c15577407d1b098
                }
            }
            catch (Exception e)
            {
                Log("Could not load types.");
                Log(e);
            }

            foreach (var t in (loadedTypes ?? Enumerable.Empty<Type>()))
            {
                TypeLoadData data;
                try
                {
                    //var attribs = t.GetCustomAttributes<NodeNameAttribute>(false);
                    //var isDeprecated = t.GetCustomAttributes<NodeDeprecatedAttribute>(true).Any();
                    //var isMetaNode = t.GetCustomAttributes<IsMetaNodeAttribute>(false).Any();
                    //var isDSCompatible =
                    //    t.GetCustomAttributes<IsDesignScriptCompatibleAttribute>(true).Any();

                    //bool isHidden =
                    //    t.GetCustomAttributes<IsVisibleInDynamoLibraryAttribute>(true)
                    //        .Any(attr => attr.Visible);

                    //only load types that are in the right namespace, are not abstract
                    //and have the elementname attribute
                    if (!IsNodeSubType(t) && t.Namespace != "Dynamo.Nodes")
                        continue;

                    //if we are running in revit (or any context other than NONE) use the DoNotLoadOnPlatforms attribute, 
                    //if available, to discern whether we should load this type
                    if (!context.Equals(Context.NONE)
                        && t.GetCustomAttributes<DoNotLoadOnPlatformsAttribute>(false)
                            .SelectMany(attr => attr.Values)
                            .Any(e => e.Contains(context)))
                    {
                        continue;
                    }

                    //string typeName;

<<<<<<< HEAD
                    //if (attribs.Any() && !isDeprecated && !isMetaNode && isDSCompatible && !isHidden)
                    //{
                    //    typeName = attribs.First().Name;
                    //}
                    //else
                    //    typeName = t.Name;
=======
            return loadedNodes;
        }

        /// <summary>
        ///     Load Custom Nodes from the CustomNodeLoader search path and update search
        /// </summary>
        public List<CustomNodeInfo> LoadCustomNodes(string path)
        {
            if (!Directory.Exists(path))
                return new List<CustomNodeInfo>();

            var customNodeLoader = dynamoModel.CustomNodeManager;
            var searchModel = dynamoModel.SearchModel;

            var loadedNodes = customNodeLoader.ScanNodeHeadersInDirectory(path).ToList();
            
            // add nodes to search
            loadedNodes.ForEach(x => searchModel.Add(x));
>>>>>>> 95d40a5d4bb8b49d755245c08c15577407d1b098

                    data = new TypeLoadData(t);
                }
                catch (Exception e)
                {
                    Log("Failed to load type from " + assembly.FullName);
                    Log("The type was " + t.FullName);
                    Log(e);
                    data = null;
                }

                if (data != null)
                    yield return data;
            }
        }

        #endregion
    }
}
