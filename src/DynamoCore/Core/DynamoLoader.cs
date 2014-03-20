#define __NO_SAMPLES_MENU

using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using Dynamo.Controls;
using System.Reflection;
using System.IO;
using System.Windows.Controls;
using System.Windows;
using Autodesk.DesignScript.Runtime;
using String = System.String;

namespace Dynamo.Utilities
{
    /// <summary>
    ///     Handles loading various types of elements into Dynamo at startup
    /// </summary>
    internal class DynamoLoader
    {
        private static string _dynamoDirectory = "";
        public static string GetDynamoDirectory()
        {
            if (String.IsNullOrEmpty(_dynamoDirectory))
            {
                var dynamoAssembly = Assembly.GetExecutingAssembly();
                _dynamoDirectory = Path.GetDirectoryName(dynamoAssembly.Location);
                
            }
            return _dynamoDirectory;
        }

        public static HashSet<string> LoadedAssemblyNames = new HashSet<string>();
        public static Dictionary<string, List<Type>> AssemblyPathToTypesLoaded = new Dictionary<string, List<Type>>();

        internal static void LoadPackages()
        {
            dynSettings.PackageLoader.LoadPackages();
        }

        /// <summary>
        ///     Enumerate local library assemblies and add them to DynamoController's
        ///     dictionaries and search.  
        /// </summary>
        /// <param name="searchViewModel">The searchViewModel to which the nodes will be added</param>
        /// <param name="controller">The DynamoController, whose dictionaries will be modified</param>
        internal static void LoadBuiltinTypes()
        {
            string location = GetDynamoDirectory();

            #region determine assemblies to load

            var allLoadedAssembliesByPath = new Dictionary<string, Assembly>();
            var allLoadedAssemblies = new Dictionary<string, Assembly>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    allLoadedAssembliesByPath[assembly.Location] = assembly;
                    allLoadedAssemblies[assembly.FullName] = assembly;
                }
                catch { }
            }

            IEnumerable<string> allDynamoAssemblyPaths = 
                SearchPaths.Select(path => Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly))
                           .Aggregate(
                                Directory.GetFiles(location, "*.dll") as IEnumerable<string>, 
                                Enumerable.Concat);

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

                if (LoadedAssemblyNames.Contains(fn))
                    continue;

                LoadedAssemblyNames.Add(fn);

                if (allLoadedAssembliesByPath.ContainsKey(assemblyPath))
                {
                    LoadNodesFromAssembly(allLoadedAssembliesByPath[assemblyPath]);
                }
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
                    catch(Exception e)
                    {
                        DynamoLogger.Instance.Log(e);
                    }
                }
            }

#if USE_DSENGINE
            dynSettings.Controller.SearchViewModel.Add(dynSettings.Controller.EngineController.GetFunctionGroups());
#endif
            AppDomain.CurrentDomain.AssemblyResolve -= resolver;

            #endregion

        }

        /// <summary>
        /// Load all types which inherit from NodeModel whose assemblies are located in
        /// the bin/nodes directory. Add the types to the searchviewmodel and
        /// the controller's dictionaries.
        /// </summary>
        internal static void LoadNodeModels()
        {
            string location = Path.Combine(GetDynamoDirectory(), "nodes");

            var allLoadedAssembliesByPath = new Dictionary<string, Assembly>();
            var allLoadedAssemblies = new Dictionary<string, Assembly>();

            // cache the loaded assembly information
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
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
                SearchPaths.Select(path => Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly))
                           .Aggregate(
                                Directory.GetFiles(location, "*.dll") as IEnumerable<string>,
                                Enumerable.Concat).ToList();

            // add the core assembly to get things like code block nodes and watches.
            allDynamoAssemblyPaths.Add(Path.Combine(GetDynamoDirectory(), "DynamoCore.dll"));

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
                {
                    LoadNodesFromAssembly(allLoadedAssembliesByPath[assemblyPath]);
                }
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
                        DynamoLogger.Instance.Log(e);
                    }
                }
            }

            dynSettings.Controller.SearchViewModel.Add(dynSettings.Controller.EngineController.GetFunctionGroups());
            AppDomain.CurrentDomain.AssemblyResolve -= resolver;
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
                   t.IsSubclassOf(typeof(NodeModel));
        }

        /// <summary>
        ///     Enumerate the types in an assembly and add them to DynamoController's
        ///     dictionaries and the search view model.  Internally catches exceptions and sends the error 
        ///     to the console.
        /// </summary>
        /// <param name="searchViewModel">The searchViewModel to which the nodes will be added</param>
        /// <param name="controller">The DynamoController, whose dictionaries will be modified</param>
        /// <param name="bench">The bench where logging errors will be sent</param>
        /// <Returns>The list of node types loaded from this assembly</Returns>
        public static List<Type> LoadNodesFromAssembly(Assembly assembly)
        {
            var controller = dynSettings.Controller;
            var searchViewModel = dynSettings.Controller.SearchViewModel;

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
                        var attribs = t.GetCustomAttributes(typeof (NodeNameAttribute), false);
                        var isDeprecated = t.GetCustomAttributes(typeof (NodeDeprecatedAttribute), true).Any();
                        var isMetaNode = t.GetCustomAttributes(typeof(IsMetaNodeAttribute), false).Any();
                        var isDSCompatible = t.GetCustomAttributes(typeof(IsDesignScriptCompatibleAttribute), true).Any();

                        bool isHidden = false;
                        var attrs = t.GetCustomAttributes(typeof(IsVisibleInDynamoLibraryAttribute), true);
                        if (null != attrs && attrs.Count() > 0)
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
                        if (!controller.Context.Equals(Context.NONE))
                        {

                            object[] platformExclusionAttribs = t.GetCustomAttributes(typeof(DoNotLoadOnPlatformsAttribute), false);
                            if (platformExclusionAttribs.Length > 0)
                            {
                                string[] exclusions = (platformExclusionAttribs[0] as DoNotLoadOnPlatformsAttribute).Values;
                                int iExclusion = exclusions.Length - 1;
                                for (; iExclusion > -1; iExclusion--)
                                {
                                    if (exclusions[iExclusion].Contains(controller.Context))
                                        //if the attribute's values contain the context stored on the controller
                                        //then skip loading this type.
                                        break;
                                }
                                if (iExclusion > -1)
                                    continue;

                                //utility was late for Vasari release, but could be available with after-post RevitAPI.dll
                                if (t.Name.Equals("dynSkinCurveLoops"))
                                {
                                    MethodInfo[] specialTypeStaticMethods = t.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                                    String nameOfMethodCreate = "noSkinSolidMethod";
                                    bool exclude = true;
                                    foreach (MethodInfo m in specialTypeStaticMethods)
                                    {
                                        if (m.Name == nameOfMethodCreate)
                                        {
                                            object[] argsM = new object[0];
                                            exclude = (bool)m.Invoke(null, argsM);
                                            break;
                                        }
                                    }
                                    if (exclude)
                                        continue;
                                }
                            }
                        }

                        string typeName;

#if USE_DSENGINE
                        if (attribs.Length > 0 && !isDeprecated && !isMetaNode && isDSCompatible && !isHidden)
#else
                        if (attribs.Length > 0 && !isDeprecated && !isMetaNode && !isHidden)
#endif
                        {
                            searchViewModel.Add(t);
                            typeName = (attribs[0] as NodeNameAttribute).Name;
  
                        }
                        else
                        {
                            typeName = t.Name;
                        }

                        AssemblyPathToTypesLoaded[assembly.Location].Add(t);

                        var data = new TypeLoadData(assembly, t);

                        if (!controller.BuiltInTypesByNickname.ContainsKey(typeName))
                        {
                            controller.BuiltInTypesByNickname.Add(typeName, data);
                        }
                        else
                        {
                            DynamoLogger.Instance.Log("Duplicate type encountered: " + typeName);
                        }

                        if (!controller.BuiltInTypesByName.ContainsKey(t.FullName))
                        {
                            controller.BuiltInTypesByName.Add(t.FullName, data);
                        }
                        else
                        {
                            DynamoLogger.Instance.Log("Duplicate type encountered: " + typeName);
                        }

                    }
                    catch (Exception e)
                    {
                        DynamoLogger.Instance.Log("Failed to load type from " + assembly.FullName);
                        DynamoLogger.Instance.Log("The type was " + t.FullName);
                        DynamoLogger.Instance.Log(e);
                    }

                }
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("Could not load types.");
                DynamoLogger.Instance.Log(e);
                if (e is ReflectionTypeLoadException)
                {
                    var typeLoadException = e as ReflectionTypeLoadException;
                    Exception[] loaderExceptions = typeLoadException.LoaderExceptions;
                    DynamoLogger.Instance.Log("Dll Load Exception: " + loaderExceptions[0]);
                    DynamoLogger.Instance.Log(loaderExceptions[0].ToString());
                    if (loaderExceptions.Count() > 1)
                    {
                        DynamoLogger.Instance.Log("Dll Load Exception: " + loaderExceptions[1]);
                        DynamoLogger.Instance.Log(loaderExceptions[1].ToString());
                    }
                }
            }

            return AssemblyPathToTypesLoaded[assembly.Location];
        }

#if !__NO_SAMPLES_MENU
        /// <summary>
        ///     Setup the "Samples" sub-menu with contents of samples directory.
        /// </summary>
        /// <param name="bench">The bench where the UI will be loaded</param>
        public static void LoadSamplesMenu(DynamoView bench)
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string samplesPath = Path.Combine(directory, "samples");

            if (Directory.Exists(samplesPath))
            {
                string[] dirPaths = Directory.GetDirectories(samplesPath);
                string[] filePaths = Directory.GetFiles(samplesPath, "*.dyn");

                // handle top-level files
                if (filePaths.Any())
                {
                    foreach (string path in filePaths)
                    {
                        var item = new MenuItem
                        {
                            Header = Path.GetFileNameWithoutExtension(path),
                            Tag = path
                        };
                        item.Click += OpenSample_Click;
                        bench.SamplesMenu.Items.Add(item);
                    }
                }

                // handle top-level dirs, TODO - factor out to a seperate function, make recusive
                if (dirPaths.Any())
                {
                    foreach (string dirPath in dirPaths)
                    {
                        var dirItem = new MenuItem
                        {
                            Header = Path.GetFileName(dirPath),
                            Tag = Path.GetFileName(dirPath)
                        };

                        filePaths = Directory.GetFiles(dirPath, "*.dyn");
                        if (filePaths.Any())
                        {
                            foreach (string path in filePaths)
                            {
                                var item = new MenuItem
                                {
                                    Header = Path.GetFileNameWithoutExtension(path),
                                    Tag = path
                                };
                                item.Click += OpenSample_Click;
                                dirItem.Items.Add(item);
                            }
                        }
                        bench.SamplesMenu.Items.Add(dirItem);
                    }
                    return;
                }
            }
            //this.fileMenu.Items.Remove(this.samplesMenu);
        }
#endif

        /// <summary>
        ///     Callback for opening a sample.
        /// </summary>
        private static void OpenSample_Click(object sender, RoutedEventArgs e)
        {
            var path = (string)((MenuItem)sender).Tag;

            if (dynSettings.Controller.DynamoViewModel.IsUILocked)
                dynSettings.Controller.DynamoViewModel.QueueLoad(path);
            else
            {
                if (!dynSettings.Controller.DynamoViewModel.ViewingHomespace)
                    dynSettings.Controller.DynamoModel.ViewHomeWorkspace();

                dynSettings.Controller.DynamoModel.OpenWorkspace(path);
            }
        }

        /// <summary>
        ///     Load Custom Nodes from the default directory - the "definitions"
        ///     directory where the executing assembly is located..
        /// </summary>
        public static IEnumerable<CustomNodeInfo> LoadCustomNodes()
        {

            var customNodeLoader = dynSettings.CustomNodeManager;
            var searchViewModel = dynSettings.Controller.SearchViewModel;
            var loadedNodes = customNodeLoader.UpdateSearchPath();

            // add nodes to search
            loadedNodes.ForEach(x => searchViewModel.Add(x) );
            
            // update search view
            searchViewModel.SearchAndUpdateResultsSync(searchViewModel.SearchText);

            return loadedNodes;

        }

        /// <summary>
        ///     Load Custom Nodes from the CustomNodeLoader search path and update search
        /// </summary>
        public static List<CustomNodeInfo> LoadCustomNodes(string path)
        {
            if (!Directory.Exists(path))
                return new List<CustomNodeInfo>();

            var customNodeLoader = dynSettings.CustomNodeManager;
            var searchViewModel = dynSettings.Controller.SearchViewModel;

            var loadedNodes = customNodeLoader.ScanNodeHeadersInDirectory(path).ToList();
            customNodeLoader.AddDirectoryToSearchPath(path);

            // add nodes to search
            loadedNodes.ForEach( x => searchViewModel.Add(x) );

            // update search view
            searchViewModel.SearchAndUpdateResultsSync(searchViewModel.SearchText);

            return loadedNodes;

        }

        public static HashSet<string> SearchPaths = new HashSet<string>();

        internal static void AddBinarySearchPath(string p)
        {
            SearchPaths.Add(p);
        }

        internal static void ClearCachedAssemblies()
        {
            LoadedAssemblyNames = new HashSet<string>();
            AssemblyPathToTypesLoaded = new Dictionary<string, List<Type>>();
        }
    }
}
