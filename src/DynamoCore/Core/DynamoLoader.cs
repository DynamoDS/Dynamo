//#define __NO_SAMPLES_MENU

using System;
using System.Collections.Generic;
using System.Linq;

using Dynamo.Core;
using Dynamo.Models;
using System.Reflection;
using System.IO;
using Autodesk.DesignScript.Runtime;

using Dynamo.PackageManager;

using DynamoUtilities;

namespace Dynamo.Utilities
{
    /// <summary>
    /// The DynamoLoader is responsible for loading custom nodes and
    /// types which derive from NodeModel. For information
    /// about package loading see the PackageLoader. For information
    /// about loading other libraries, see LibraryServices.
    /// </summary>
    public class DynamoLoader
    {
        #region Properties/Fields

        private DynamoModel dynamoModel;
        public PackageLoader PackageLoader { get; private set; }

        private static string _dynamoDirectory = "";
        public static HashSet<string> SearchPaths = new HashSet<string>();
        public static HashSet<string> LoadedAssemblyNames = new HashSet<string>();
        public static Dictionary<string, List<Type>> AssemblyPathToTypesLoaded =
            new Dictionary<string, List<Type>>();

        #endregion

        public DynamoLoader(DynamoModel model)
        {
            this.dynamoModel = model;
            this.PackageLoader = new PackageLoader(this, dynamoModel.Logger);
        }

        #region Methods

        /// <summary>
        /// Load all types which inherit from NodeModel whose assemblies are located in
        /// the bin/nodes directory. Add the types to the searchviewmodel and
        /// the controller's dictionaries.
        /// </summary>
        internal void LoadNodeModels()
        {
            var allLoadedAssembliesByPath = new Dictionary<string, Assembly>();
            var allLoadedAssemblies = new Dictionary<string, Assembly>();

            // cache the loaded assembly information
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                try
                {
                    allLoadedAssembliesByPath[assembly.Location] = assembly;
                    allLoadedAssemblies[assembly.FullName] = assembly;
                }
                catch { }
            }

            // find all the dlls registered in all search paths
            // and concatenate with all dlls in the current directory
            List<string> allDynamoAssemblyPaths =
                DynamoPathManager.Instance.Nodes.SelectMany(path => Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly)).ToList();

            // add the core assembly to get things like code block nodes and watches.
            allDynamoAssemblyPaths.Add(Path.Combine(DynamoPathManager.Instance.MainExecPath, "DynamoCore.dll"));

            var resolver = new ResolveEventHandler(delegate(object sender, ResolveEventArgs args)
            {
                Assembly result;
                allLoadedAssemblies.TryGetValue(args.Name, out result);
                return result;
            });

            AppDomain.CurrentDomain.AssemblyResolve += resolver;

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

                if (allLoadedAssembliesByPath.ContainsKey(assemblyPath))
                    LoadNodesFromAssembly(allLoadedAssembliesByPath[assemblyPath]);
                else
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(assemblyPath);
                        allLoadedAssemblies[assembly.GetName().Name] = assembly;
                        LoadNodesFromAssembly(assembly);
                    }
                    catch (BadImageFormatException)
                    {
                        //swallow these warnings.
                    }
                    catch (Exception e)
                    {
                        dynamoModel.Logger.Log(e);
                    }
                }
            }

            dynamoModel.SearchModel.Add(dynamoModel.EngineController.GetFunctionGroups());
            AppDomain.CurrentDomain.AssemblyResolve -= resolver;
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
        public List<Type> LoadNodesFromAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var searchViewModel = dynamoModel.SearchModel;

            AssemblyPathToTypesLoaded.Add(assembly.Location, new List<Type>());

            try
            {
                var loadedTypes = assembly.GetTypes();

                foreach (var t in loadedTypes)
                {
                    try
                    {
                        //only load types that are in the right namespace, are not abstract
                        //and have the elementname attribute
                        var attribs = t.GetCustomAttributes(typeof(NodeNameAttribute), false);
                        var isDeprecated = t.GetCustomAttributes(typeof(NodeDeprecatedAttribute), true).Any();
                        var isMetaNode = t.GetCustomAttributes(typeof(IsMetaNodeAttribute), false).Any();
                        var isDSCompatible = t.GetCustomAttributes(typeof(IsDesignScriptCompatibleAttribute), true).Any();

                        bool isHidden = false;
                        var attrs = t.GetCustomAttributes(typeof(IsVisibleInDynamoLibraryAttribute), true);
                        if (null != attrs && attrs.Any())
                        {
                            var isVisibleAttr = attrs[0] as IsVisibleInDynamoLibraryAttribute;
                            if (null != isVisibleAttr && isVisibleAttr.Visible == false)
                            {
                                isHidden = true;
                            }
                        }

                        if (!IsNodeSubType(t) && t.Namespace != "Dynamo.Nodes") /*&& attribs.Length > 0*/
                            continue;

                        //if we are running in revit (or any context other than NONE) use the DoNotLoadOnPlatforms attribute, 
                        //if available, to discern whether we should load this type
                        if (!dynamoModel.Context.Equals(Context.NONE))
                        {

                            object[] platformExclusionAttribs = t.GetCustomAttributes(typeof(DoNotLoadOnPlatformsAttribute), false);
                            if (platformExclusionAttribs.Length > 0)
                            {
                                string[] exclusions = (platformExclusionAttribs[0] as DoNotLoadOnPlatformsAttribute).Values;

                                //if the attribute's values contain the context stored on the Model
                                //then skip loading this type.
                                if (exclusions.Reverse().Any(e => e.Contains(dynamoModel.Context)))
                                    continue;
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

                        var data = new TypeLoadData(assembly, t);

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

                }
            }
            catch (Exception e)
            {
                dynamoModel.Logger.Log("Could not load types.");
                dynamoModel.Logger.Log(e);
                if (e is ReflectionTypeLoadException)
                {
                    var typeLoadException = e as ReflectionTypeLoadException;
                    Exception[] loaderExceptions = typeLoadException.LoaderExceptions;
                    dynamoModel.Logger.Log("Dll Load Exception: " + loaderExceptions[0]);
                    dynamoModel.Logger.Log(loaderExceptions[0].ToString());
                    if (loaderExceptions.Count() > 1)
                    {
                        dynamoModel.Logger.Log("Dll Load Exception: " + loaderExceptions[1]);
                        dynamoModel.Logger.Log(loaderExceptions[1].ToString());
                    }
                }
            }

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
        public List<CustomNodeInfo> LoadCustomNodes(string path)
        {
            if (!Directory.Exists(path))
                return new List<CustomNodeInfo>();

            var customNodeLoader = dynamoModel.CustomNodeManager;
            var searchModel = dynamoModel.SearchModel;

            var loadedNodes = customNodeLoader.ScanNodeHeadersInDirectory(path).ToList();
            customNodeLoader.AddDirectoryToSearchPath(path);

            // add nodes to search
            loadedNodes.ForEach(x => searchModel.Add(x));

            // update search view
            searchModel.OnRequestSync();

            return loadedNodes;
        }

        internal void ClearCachedAssemblies()
        {
            LoadedAssemblyNames = new HashSet<string>();
            AssemblyPathToTypesLoaded = new Dictionary<string, List<Type>>();
        }

        #endregion

    }
}
