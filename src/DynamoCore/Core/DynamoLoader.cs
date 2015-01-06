using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using DynamoUtilities;
<<<<<<< HEAD
=======
using Dynamo.Search;
using Dynamo.DSEngine;
using System.Text;
>>>>>>> Sitrus2

namespace Dynamo.Utilities
{
    /// <summary>
    /// The DynamoLoader is responsible for loading types which derive
    /// from NodeModel. For information about package loading see the
    /// PackageLoader. For information about loading other libraries, 
    /// see LibraryServices.
    /// </summary>
    public class DynamoLoader : LogSourceBase
    {
        #region Properties/Fields

        public readonly HashSet<string> LoadedAssemblyNames = new HashSet<string>();
        private readonly HashSet<Assembly> loadedAssemblies = new HashSet<Assembly>();

        /// <summary>
        ///     All assemblies that have been loaded into Dynamo.
        /// </summary>
        public IEnumerable<Assembly> LoadedAssemblies
        {
            get { return loadedAssemblies; }
        }

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
        /// <param name="modelTypes"></param>
        /// <param name="migrationTypes"></param>
        public void LoadNodeModelsAndMigrations(string context, out List<TypeLoadData> modelTypes, out List<TypeLoadData> migrationTypes)
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
            var allDynamoAssemblyPaths =
                DynamoPathManager.Instance.Nodes.SelectMany(
                    path => Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly));

            // add the core assembly to get things like code block nodes and watches.
            //allDynamoAssemblyPaths.Add(Path.Combine(DynamoPathManager.Instance.MainExecPath, "DynamoCore.dll"));

            ResolveEventHandler resolver = 
                (sender, args) =>
                {
                    Assembly resolvedAssembly;
                    loadedAssembliesByName.TryGetValue(args.Name, out resolvedAssembly);
                    return resolvedAssembly;
                };

            AppDomain.CurrentDomain.AssemblyResolve += resolver;

            var result = new List<TypeLoadData>();
            var result2 = new List<TypeLoadData>();

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

                    LoadNodesFromAssembly(assembly, context, result, result2);
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

            AppDomain.CurrentDomain.AssemblyResolve -= resolver;

            modelTypes = result;
            migrationTypes = result2;
        }

        /// <summary>
        ///     Determine if a Type is a node.  Used by LoadNodesFromAssembly to figure
        ///     out what nodes to load from other libraries (.dlls).
        /// </summary>
        /// <parameter>The type</parameter>
        /// <returns>True if the type is node.</returns>
        public static bool IsNodeSubType(Type t)
        {
            return //t.Namespace == "Dynamo.Nodes" &&
                !t.IsAbstract &&
                    t.IsSubclassOf(typeof(NodeModel))
                    && t.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool IsMigration(Type t)
        {
            return
                t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .SelectMany(method => method.GetCustomAttributes<NodeMigrationAttribute>(false))
                    .Any();
        }

        internal bool ContainsNodeModelSubType(Assembly assem)
        {
            return assem.GetTypes().Any(IsNodeSubType);
        }

        /// <summary>
        ///     Enumerate the types in an assembly and add them to DynamoController's
        ///     dictionaries and the search view model.  Internally catches exceptions and sends the error 
        ///     to the console.
        /// </summary>
        /// <Returns>The list of node types loaded from this assembly</Returns>
<<<<<<< HEAD
        public void LoadNodesFromAssembly(
            Assembly assembly, string context, List<TypeLoadData> nodeModels,
            List<TypeLoadData> migrationTypes)
=======
        public List<Type> LoadNodesFromAssembly(Assembly assembly, SearchModel.ElementType nodesType = SearchModel.ElementType.Regular)
>>>>>>> Sitrus2
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
                    Log("Dll Load Exception:");
                    Log(ex.ToString());
                }
            }
            catch (Exception e)
            {
                Log("Could not load types.");
                Log(e);
            }

            foreach (var t in (loadedTypes ?? Enumerable.Empty<Type>()))
            {
                try
                {
                    //only load types that are in the right namespace, are not abstract
                    //and have the elementname attribute
                    if (IsNodeSubType(t))
                    {
                        //if we are running in revit (or any context other than NONE) use the DoNotLoadOnPlatforms attribute, 
                        //if available, to discern whether we should load this type
                        if (context.Equals(Context.NONE)
                            || !t.GetCustomAttributes<DoNotLoadOnPlatformsAttribute>(false)
                                .SelectMany(attr => attr.Values)
                                .Any(e => e.Contains(context)))
                        {
                            nodeModels.Add(new TypeLoadData(t));
                        }
<<<<<<< HEAD
=======

                        string typeName;

                        if (attribs.Length > 0 && !isDeprecated && !isMetaNode && isDSCompatible && !isHidden)
                        {
                            searchViewModel.Add(t, nodesType);
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

                        
>>>>>>> Sitrus2
                    }

                    if (IsMigration(t))
                    {
                        migrationTypes.Add(new TypeLoadData(t));
                    }
                }
                catch (Exception e)
                {
                    Log("Failed to load type from " + assembly.FullName);
                    Log("The type was " + t.FullName);
                    Log(e);
                }
            }
<<<<<<< HEAD
=======

            

            return AssemblyPathToTypesLoaded[assembly.Location];
        }

        /// <summary>
        ///     Load Custom Nodes from the default directory - the "definitions"
        ///     directory where the executing assembly is located..
        /// </summary>
        public IEnumerable<CustomNodeInfo> LoadCustomNodes()
        {
            var customNodeLoader = dynamoModel.CustomNodeManager;
            var searchModel = dynamoModel.SearchModel;
            var loadedNodes = customNodeLoader.UpdateSearchPath();

            // add nodes to search
            loadedNodes.ForEach(x => searchModel.Add(x));

            // update search view
            searchModel.OnRequestSync();

            return loadedNodes;
        }

        /// <summary>
        ///     Load Custom Nodes from the CustomNodeLoader search path and update search
        /// </summary>
        public List<CustomNodeInfo> LoadCustomNodes(string path, SearchModel.ElementType elementType)
        {
            if (!Directory.Exists(path))
                return new List<CustomNodeInfo>();

            var customNodeLoader = dynamoModel.CustomNodeManager;
            var searchModel = dynamoModel.SearchModel;

            var loadedNodes = customNodeLoader.ScanNodeHeadersInDirectory(path).ToList();
            
            // add nodes to search
            loadedNodes.ForEach(x =>
            {
                x.ElementType = elementType;
                searchModel.Add(x);
            });

            // update search view
            searchModel.OnRequestSync();

            return loadedNodes;
        }

        internal void ClearCachedAssemblies()
        {
            LoadedAssemblyNames = new HashSet<string>();
            AssemblyPathToTypesLoaded = new Dictionary<string, List<Type>>();
>>>>>>> Sitrus2
        }

        #endregion
    }
}
