using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Dynamo.Commands;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Search;
using Dynamo.Utilities;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Dynamo
{
    public class DynamoController : INotifyPropertyChanged
    {
        private readonly SortedDictionary<string, TypeLoadData> builtinTypesByNickname =
            new SortedDictionary<string, TypeLoadData>();

        private readonly Dictionary<string, TypeLoadData> builtinTypesByTypeName =
            new Dictionary<string, TypeLoadData>();

        private readonly Queue<Tuple<object, object>> commandQueue = new Queue<Tuple<object, object>>();
        private string UnlockLoadPath;
        private dynWorkspace _cspace;
        private bool isProcessingCommandQueue = false;

        public SearchViewModel SearchViewModel { get; internal set; }
        public PackageManagerLoginViewModel PackageManagerLoginViewModel { get; internal set; }
        public PackageManagerPublishViewModel PackageManagerPublishViewModel { get; internal set; }
        public PackageManagerClient PackageManagerClient { get; internal set; }

        private bool runEnabled = true;
        public bool RunEnabled
        {
            get { return runEnabled; }
            set
            {
                runEnabled = value;
                NotifyPropertyChanged("RunEnabled");
            }
        }

        List<UIElement> clipBoard = new List<UIElement>();
        public List<UIElement> ClipBoard
        {
            get { return clipBoard; }
            set { clipBoard = value; }
        }

        public bool IsProcessingCommandQueue
        {
            get { return isProcessingCommandQueue; }
        }

        public Queue<Tuple<object, object>> CommandQueue
        {
            get { return commandQueue; }
        }

        public dynBench Bench { get; private set; }

        public IEnumerable<dynNode> AllNodes
        {
            get
            {
                return HomeSpace.Nodes.Concat(
                    dynSettings.FunctionDict.Values.Aggregate(
                        (IEnumerable<dynNode>) new List<dynNode>(),
                        (a, x) => a.Concat(x.Workspace.Nodes)
                        )
                    );
            }
        }

        public SortedDictionary<string, TypeLoadData> BuiltInTypesByNickname
        {
            get { return builtinTypesByNickname; }
        }

        public DynamoSplash SplashScreen { get; set; }

        public dynWorkspace CurrentSpace
        {
            get { return _cspace; }
            internal set
            {
                _cspace = value;
                //Bench.CurrentX = _cspace.PositionX;
                //Bench.CurrentY = _cspace.PositionY;
                if (Bench != null)
                    Bench.CurrentOffset = new Point(_cspace.PositionX, _cspace.PositionY);

                //TODO: Also set the name here.
            }
        }

        public dynWorkspace HomeSpace { get; protected set; }

        public ExecutionEnvironment FSchemeEnvironment { get; private set; }

        public List<dynNode> Nodes
        {
            get { return CurrentSpace.Nodes; }
        }

        public bool ViewingHomespace
        {
            get { return CurrentSpace == HomeSpace; }
        }

        #region Constructor and Initialization

        //public DynamoController(SplashScreen splash)
        private bool _activated;

        public DynamoController()
        {
            this.RunEnabled = true;
            this.CanRunDynamically = true;
            
            Bench = new dynBench(this);

            SearchViewModel = new SearchViewModel();
            PackageManagerClient = new PackageManagerClient(this);
            PackageManagerLoginViewModel = new PackageManagerLoginViewModel(PackageManagerClient);
            PackageManagerPublishViewModel = new PackageManagerPublishViewModel(PackageManagerClient);

            HomeSpace = CurrentSpace = new HomeWorkspace();

            //Bench.CurrentX = dynBench.CANVAS_OFFSET_X;
            //Bench.CurrentY = dynBench.CANVAS_OFFSET_Y;
            if (Bench != null) {
                Bench.CurrentOffset = new Point(dynBench.CANVAS_OFFSET_X, dynBench.CANVAS_OFFSET_Y);

                Bench.InitializeComponent();
                Bench.Log(String.Format(
                    "Dynamo -- Build {0}.",
                    Assembly.GetExecutingAssembly().GetName().Version));

                dynSettings.Bench = Bench;

                //WTF
                Bench.settings_curves.IsChecked = true;
                Bench.settings_curves.IsChecked = false;

                Bench.LockUI();

                Bench.Activated += Bench_Activated;
                dynSettings.Workbench = Bench.WorkBench;

                //run tests
                if (FScheme.RunTests(Bench.Log))
                {
                    if (Bench != null)
                        Bench.Log("All Tests Passed. Core library loaded OK.");
                }

                if (DynamoCommands.ShowSplashScreenCmd.CanExecute(null))
                {
                    DynamoCommands.ShowSplashScreenCmd.Execute(null);
                }
            }

            dynSettings.Controller = this;

            FSchemeEnvironment = new ExecutionEnvironment();

            LoadBuiltinTypes();
            PopulateSamplesMenu();

            

            //Dispatcher.CurrentDispatcher.Hooks.DispatcherInactive += new EventHandler(Hooks_DispatcherInactive);
        }

        private void Hooks_DispatcherInactive(object sender, EventArgs e)
        {
            ProcessCommandQueue();
        }

        public void ProcessCommandQueue()
        {
            while (commandQueue.Count > 0)
            {
                var cmdData = commandQueue.Dequeue();
                var cmd = cmdData.Item1 as ICommand;
                if (cmd != null)
                {
                    if (cmd.CanExecute(cmdData.Item2))
                    {
                        cmd.Execute(cmdData.Item2);
                    }
                }
            }
            commandQueue.Clear();

            if (Bench != null)
            {
                dynSettings.Writer.WriteLine(string.Format("Bench Thread : {0}",
                                                       Bench.Dispatcher.Thread.ManagedThreadId.ToString()));
            }
        }

        private void Bench_Activated(object sender, EventArgs e)
        {
            if (!_activated)
            {
                _activated = true;

                LoadUserTypes();
                Bench.Log("Welcome to Dynamo!");

                if (UnlockLoadPath != null && !OpenWorkbench(UnlockLoadPath))
                {
                    //MessageBox.Show("Workbench could not be opened.");
                    Bench.Log("Workbench could not be opened.");

                    //dynSettings.Writer.WriteLine("Workbench could not be opened.");
                    //dynSettings.Writer.WriteLine(UnlockLoadPath);

                    if (DynamoCommands.WriteToLogCmd.CanExecute(null))
                    {
                        DynamoCommands.WriteToLogCmd.Execute("Workbench could not be opened.");
                        DynamoCommands.WriteToLogCmd.Execute(UnlockLoadPath);
                    }
                }

                UnlockLoadPath = null;

                Bench.UnlockUI();
                Bench.WorkBench.Visibility = Visibility.Visible;

                if (DynamoCommands.CloseSplashScreenCmd.CanExecute(null))
                {
                    DynamoCommands.CloseSplashScreenCmd.Execute(null);
                }

                DynamoCommands.ShowSearchCmd.Execute(null);

                HomeSpace.OnDisplayed();
            }
        }

        #endregion

        #region Loading

        internal void QueueLoad(string path)
        {
            UnlockLoadPath = path;
        }

        /// <summary>
        ///     Setup the "Add" menu with all available dynElement types.
        /// </summary>
        private void LoadBuiltinTypes()
        {
            //setup the menu with all the types by reflecting
            //the DynamoElements.dll
            Assembly dynamoAssembly = Assembly.GetExecutingAssembly();

            string location = Path.GetDirectoryName(dynamoAssembly.Location);

            //try getting the element types via reflection. 
            // MDJ - I wrapped this in a try-catch as we were having problems with an 
            // external dll (MIConvexHullPlugin.dll) not loading correctly from \dynamo\packages 
            // because the dll did not have a strong name by default and was not loaded into the GAC.
            // The exceptions are now caught but if there is an exception no built-in types are loaded.
            // TODO - move the try catch inside the for loop if possible to not fail all loads. this could slow down load times though.

            //var loadedAssemblies = new List<Assembly>();
            //var assembliesToLoad = new List<string>();

            #region determine assemblies to load

            var allLoadedAssembliesByPath = new Dictionary<string, Assembly>(
                AppDomain.CurrentDomain.GetAssemblies().ToDictionary(x => x.Location));

            var allLoadedAssemblies = new Dictionary<string, Assembly>(
                AppDomain.CurrentDomain.GetAssemblies().ToDictionary(x => x.FullName));

            //var tempDomain = AppDomain.CreateDomain("TemporaryAppDomain");

            string path = Path.Combine(location, "Packages");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            IEnumerable<string> allDynamoAssemblyPaths =
                Directory.GetFiles(location, "*.dll")
                         .Concat(Directory.GetFiles(
                             path,
                             "*.dll",
                             SearchOption.AllDirectories));

            var resolver = new ResolveEventHandler(delegate(object sender, ResolveEventArgs args)
                {
                    Assembly result;
                    allLoadedAssemblies.TryGetValue(args.Name, out result);
                    return result;
                });

            AppDomain.CurrentDomain.AssemblyResolve += resolver;

            foreach (string assemblyPath in allDynamoAssemblyPaths)
            {
                if (allLoadedAssembliesByPath.ContainsKey(assemblyPath))
                    loadNodesFromAssembly(allLoadedAssembliesByPath[assemblyPath]);
                    //loadedAssemblies.Add(allLoadedAssemblies[assemblyPath]);
                else
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(assemblyPath);
                        allLoadedAssemblies[assembly.GetName().Name] = assembly;
                        loadNodesFromAssembly(assembly);
                    }
                    catch
                    {
                    }
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve -= resolver;

            //AppDomain.Unload(tempDomain);

            #endregion

            //foreach (var assembly in loadedAssemblies.Concat(assembliesToLoad.Select(Assembly.LoadFile)))
            //    loadNodesFromAssembly(assembly);

            ProcessThreadCollection threads = Process.GetCurrentProcess().Threads;
                // trying to understand why processor pegs after loading.

            //string pluginsPath = Path.Combine(location, "definitions");

            //if (Directory.Exists(pluginsPath))
            //{
            //    loadUserAssemblies(pluginsPath);
            //}

            #region PopulateUI

            var sortedExpanders = new SortedDictionary<string, Tuple<Expander, SortedList<string, dynNodeUI>>>();

            foreach (var kvp in builtinTypesByNickname)
            {
                //if (!kvp.Value.t.Equals(typeof(dynSymbol)))
                //{
                //   System.Windows.Controls.MenuItem mi = new System.Windows.Controls.MenuItem();
                //   mi.Header = kvp.Key;
                //   mi.Click += new RoutedEventHandler(AddElement_Click);
                //   AddMenu.Items.Add(mi);
                //}

                //---------------------//

                object[] catAtts = kvp.Value.Type.GetCustomAttributes(typeof (NodeCategoryAttribute), false);
                string categoryName;
                if (catAtts.Length > 0)
                {
                    categoryName = ((NodeCategoryAttribute) catAtts[0]).ElementCategory;
                }
                else
                {
                    if (Bench != null)
                        Bench.Log("No category specified for \"" + kvp.Key + "\"");
                    continue;
                }

                dynNode newNode = null;

                try
                {
                    object obj = Activator.CreateInstance(kvp.Value.Type);
                    newNode = (dynNode) obj; //.Unwrap();
                    SearchViewModel.Add(newNode);
                }
                catch (Exception e) //TODO: Narrow down
                {
                    if (Bench != null)
                    {
                        Bench.Log("Error loading \"" + kvp.Key);
                        Bench.Log(e.InnerException);
                    }
                    
                    continue;
                }

                try
                {
                    dynNodeUI nodeUI = newNode.NodeUI;

                    nodeUI.DisableInteraction();

                    string name = kvp.Key;

                    //newEl.MouseDoubleClick += delegate { AddElement(name); };

                    nodeUI.MouseDown += delegate
                        {
                            Bench.BeginDragElement(nodeUI, nodeUI.NodeLogic.GetType().ToString(),
                                                   Mouse.GetPosition(nodeUI));
                            nodeUI.Visibility = Visibility.Hidden;
                        };

                    nodeUI.GUID = Guid.NewGuid();
                    nodeUI.Margin = new Thickness(5, 30, 5, 5);

                    double target = Bench.sidebarGrid.Width - 30;
                    double width = nodeUI.ActualWidth != 0 ? nodeUI.ActualWidth : nodeUI.Width;
                    double scale = Math.Min(target/width, .8);

                    nodeUI.LayoutTransform = new ScaleTransform(scale, scale);

                    Tuple<Expander, SortedList<string, dynNodeUI>> expander;

                    if (sortedExpanders.ContainsKey(categoryName))
                    {
                        expander = sortedExpanders[categoryName];
                    }
                    else
                    {
                        var e = new Expander
                            {
                                Header = categoryName,
                                Height = double.NaN,
                                Margin = new Thickness(0, 5, 0, 0),
                                Content = new WrapPanel
                                    {
                                        Height = double.NaN,
                                        Width = double.NaN
                                    },
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200))
                            };

                        Bench.addMenuCategoryDict[categoryName] = e;

                        expander = new Tuple<Expander, SortedList<string, dynNodeUI>>(e,
                                                                                      new SortedList<string, dynNodeUI>());

                        sortedExpanders[categoryName] = expander;
                    }

                    SortedList<string, dynNodeUI> sortedElements = expander.Item2;
                    sortedElements.Add(kvp.Key, nodeUI);

                    Bench.addMenuItemsDictNew[kvp.Key] = nodeUI;

                    //--------------//

                    object[] tagAtts = kvp.Value.Type.GetCustomAttributes(typeof (NodeSearchTagsAttribute), false);

                    List<string> tags = null;

                    if (tagAtts.Length > 0)
                    {
                        tags = ((NodeSearchTagsAttribute) tagAtts[0]).Tags;
                    }

                    if (tags != null)
                    {
                        searchDict.Add(nodeUI, tags.Where(x => x.Length > 0));
                    }

                    searchDict.Add(nodeUI, kvp.Key);
                }
                catch (Exception e)
                {
                    if (Bench != null)
                    {
                        Bench.Log("Error loading \"" + kvp.Key);
                        Bench.Log(e);
                    } 
                }
            }

            //Add everything to the menu here
            foreach (var kvp in sortedExpanders)
            {
                Tuple<Expander, SortedList<string, dynNodeUI>> expander = kvp.Value;
                Bench.SideStackPanel.Children.Add(expander.Item1);
                var wp = (WrapPanel) expander.Item1.Content;
                foreach (dynNodeUI e in expander.Item2.Values)
                {
                    wp.Children.Add(e);
                }
            }

            #endregion
        }


        private bool isNodeSubType(Type t)
        {
            return t.Namespace == "Dynamo.Nodes" &&
                   !t.IsAbstract &&
                   t.IsSubclassOf(typeof (dynNode));
        }

        private void loadNodesFromAssembly(Assembly assembly)
        {
            try
            {
                Type[] loadedTypes = assembly.GetTypes();

                foreach (Type t in loadedTypes)
                {
                    //only load types that are in the right namespace, are not abstract
                    //and have the elementname attribute
                    object[] attribs = t.GetCustomAttributes(typeof (NodeNameAttribute), false);

                    if (isNodeSubType(t) && attribs.Length > 0)
                    {
                        string typeName = (attribs[0] as NodeNameAttribute).Name;
                        var data = new TypeLoadData(assembly, t);
                        builtinTypesByNickname.Add(typeName, data);
                        builtinTypesByTypeName.Add(t.FullName, data);
                    }
                }
            }
            catch (Exception e)
            {
                Bench.Log("Could not load types.");
                Bench.Log(e);
                if (e is ReflectionTypeLoadException)
                {
                    var typeLoadException = e as ReflectionTypeLoadException;
                    Exception[] loaderExceptions = typeLoadException.LoaderExceptions;
                    Bench.Log("Dll Load Exception: " + loaderExceptions[0]);
                    Bench.Log(loaderExceptions[0].ToString());
                    if (loaderExceptions.Count() > 1)
                    {
                        Bench.Log("Dll Load Exception: " + loaderExceptions[1]);
                        Bench.Log(loaderExceptions[1].ToString());
                    }
                }
            }
        }

        /// <summary>
        ///     Setup the "Samples" sub-menu with contents of samples directory.
        /// </summary>
        private void PopulateSamplesMenu()
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
                        item.Click += sample_Click;
                        Bench.SamplesMenu.Items.Add(item);
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
                                item.Click += sample_Click;
                                dirItem.Items.Add(item);
                            }
                        }
                        Bench.SamplesMenu.Items.Add(dirItem);
                    }
                    return;
                }
            }
            //this.fileMenu.Items.Remove(this.samplesMenu);
        }

        private void sample_Click(object sender, RoutedEventArgs e)
        {
            var path = (string) ((MenuItem) sender).Tag;

            if (Bench.UILocked)
                QueueLoad(path);
            else
            {
                if (!ViewingHomespace)
                    ViewHomeWorkspace();

                OpenWorkbench(path);
            }
        }

        /// <summary>
        ///     Setup the "Add" menu with all available user-defined types.
        /// </summary>
        public void LoadUserTypes()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            if (Directory.Exists(pluginsPath))
            {
                Bench.Log("Autoloading definitions...");
                loadUserWorkspaces(pluginsPath);
            }
        }

        private void loadUserWorkspaces(string directory)
        {
            var parentBuffer = new Dictionary<Guid, HashSet<Guid>>();
            var childrenBuffer = new Dictionary<Guid, HashSet<FunctionDefinition>>();
            string[] filePaths = Directory.GetFiles(directory, "*.dyf");
            foreach (string filePath in filePaths)
            {
                OpenDefinition(filePath, childrenBuffer, parentBuffer);
            }
            foreach (dynNode e in AllNodes)
            {
                e.EnableReporting();
            }
        }

        #endregion

        #region Node Initialization

        /// <summary>
        ///     This method adds dynElements when opening from a file
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="nickName"></param>
        /// <param name="guid"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public dynNode AddDynElement(
            Type elementType, string nickName, Guid guid,
            double x, double y, dynWorkspace ws,
            Visibility vis = Visibility.Visible)
        {
            try
            {
                //create a new object from a type
                //that is passed in
                //dynElement el = (dynElement)Activator.CreateInstance(elementType, new object[] { nickName });
                var node = (dynNode) Activator.CreateInstance(elementType);

                dynNodeUI nodeUI = node.NodeUI;

                if (!string.IsNullOrEmpty(nickName))
                {
                    nodeUI.NickName = nickName;
                }
                else
                {
                    var elNameAttrib =
                        node.GetType().GetCustomAttributes(typeof (NodeNameAttribute), true)[0] as NodeNameAttribute;
                    if (elNameAttrib != null)
                    {
                        nodeUI.NickName = elNameAttrib.Name;
                    }
                }

                nodeUI.GUID = guid;

                string name = nodeUI.NickName;

                //store the element in the elements list
                ws.Nodes.Add(node);
                node.WorkSpace = ws;

                nodeUI.Visibility = vis;

                Bench.WorkBench.Children.Add(nodeUI);

                Canvas.SetLeft(nodeUI, x);
                Canvas.SetTop(nodeUI, y);

                //create an event on the element itself
                //to update the elements ports and connectors
                //nodeUI.PreviewMouseRightButtonDown += new MouseButtonEventHandler(UpdateElement);

                return node;
            }
            catch (Exception e)
            {
                Bench.Log("Could not create an instance of the selected type: " + elementType);
                Bench.Log(e);
                return null;
            }
        }

        internal FunctionDefinition NewFunction(Guid id, string name, string category, bool display)
        {
            //Add an entry to the funcdict
            var workSpace = new FuncWorkspace(
                name, category, dynBench.CANVAS_OFFSET_X, dynBench.CANVAS_OFFSET_Y);

            List<dynNode> newElements = workSpace.Nodes;
            List<dynConnector> newConnectors = workSpace.Connectors;

            var functionDefinition = new FunctionDefinition(id)
                {
                    Workspace = workSpace
                };

            dynSettings.FunctionDict[functionDefinition.FunctionId] = functionDefinition;

            // add the element to search
            SearchViewModel.Add(workSpace);

            dynFunction newEl = CreateFunction(
                workSpace.Nodes.Where(el => el is dynSymbol)
                         .Select(s => ((dynSymbol) s).Symbol),
                new List<String> {"out"},
                functionDefinition);

            newEl.NodeUI.NickName = name;

            newEl.NodeUI.DisableInteraction();
            newEl.NodeUI.MouseDown += delegate
                {
                    Bench.BeginDragElement(newEl.NodeUI, newEl.Definition.FunctionId.ToString(),
                                           Mouse.GetPosition(newEl.NodeUI));

                    newEl.NodeUI.Visibility = Visibility.Hidden;
                };
            newEl.NodeUI.GUID = Guid.NewGuid();
            newEl.NodeUI.Margin = new Thickness(5, 30, 5, 5);
            newEl.NodeUI.LayoutTransform = new ScaleTransform(.8, .8);
            newEl.NodeUI.State = ElementState.DEAD;

            Expander expander;

            if (Bench.addMenuCategoryDict.ContainsKey(category))
            {
                expander = Bench.addMenuCategoryDict[category];
            }
            else
            {
                expander = new Expander
                    {
                        Header = category,
                        Height = double.NaN,
                        Margin = new Thickness(0, 5, 0, 0),
                        Content = new WrapPanel
                            {
                                Height = double.NaN,
                                Width = 240
                            },
                        HorizontalAlignment = HorizontalAlignment.Left,
                        //FontWeight = FontWeights.Bold
                        Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200))
                    };

                Bench.addMenuCategoryDict[category] = expander;

                var sortedExpanders = new SortedList<string, Expander>();
                foreach (Expander child in Bench.SideStackPanel.Children)
                {
                    sortedExpanders.Add((string) child.Header, child);
                }
                sortedExpanders.Add(category, expander);

                Bench.SideStackPanel.Children.Clear();

                foreach (Expander child in sortedExpanders.Values)
                {
                    Bench.SideStackPanel.Children.Add(child);
                }
            }

            var wp = (WrapPanel) expander.Content;

            var sortedElements = new SortedList<string, dynNodeUI>();
            foreach (dynNodeUI child in wp.Children)
            {
                sortedElements.Add(child.NickName, child);
            }
            sortedElements.Add(name, newEl.NodeUI);

            wp.Children.Clear();

            foreach (dynNodeUI child in sortedElements.Values)
            {
                wp.Children.Add(child);
            }

            Bench.addMenuItemsDictNew[name] = newEl.NodeUI;

            searchDict.Add(newEl.NodeUI, name);

            if (display)
            {
                //Store old workspace
                //var ws = new dynWorkspace(this.elements, this.connectors, this.CurrentX, this.CurrentY);

                if (!ViewingHomespace)
                {
                    //Step 2: Store function workspace in the function dictionary
                    //this.FunctionDict[this.CurrentSpace.Name] = this.CurrentSpace;

                    //Step 3: Save function
                    SaveFunction(dynSettings.FunctionDict.Values.First(x => x.Workspace == CurrentSpace));
                }

                //Make old workspace invisible
                foreach (dynNode dynE in Nodes)
                {
                    dynE.NodeUI.Visibility = Visibility.Collapsed;
                }
                foreach (dynConnector dynC in CurrentSpace.Connectors)
                {
                    dynC.Visible = false;
                }
                foreach (dynNote note in CurrentSpace.Notes)
                {
                    note.Visibility = Visibility.Hidden;
                }

                //this.currentFunctionName = name;

                ////Clear the bench for the new function
                //this.elements = newElements;
                //this.connectors = newConnectors;
                //this.CurrentX = CANVAS_OFFSET_X;
                //this.CurrentY = CANVAS_OFFSET_Y;
                CurrentSpace = workSpace;

                //this.saveFuncItem.IsEnabled = true;
                Bench.homeButton.IsEnabled = true;
                //this.varItem.IsEnabled = true;

                Bench.workspaceLabel.Content = CurrentSpace.Name;
                Bench.editNameButton.Visibility = Visibility.Visible;
                Bench.editNameButton.IsHitTestVisible = true;
                Bench.setFunctionBackground();
            }

            return functionDefinition;
        }

        protected virtual dynFunction CreateFunction(IEnumerable<string> inputs, IEnumerable<string> outputs,
                                                     FunctionDefinition functionDefinition)
        {
            return new dynFunction(inputs, outputs, functionDefinition);
        }

        internal dynNode CreateDragNode(string name)
        {
            dynNode result;

            if (builtinTypesByTypeName.ContainsKey(name))
            {
                TypeLoadData tld = builtinTypesByTypeName[name];

                ObjectHandle obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
                var newEl = (dynNode) obj.Unwrap();
                newEl.NodeUI.DisableInteraction();
                result = newEl;
            }
            else if (builtinTypesByNickname.ContainsKey(name))
            {
                TypeLoadData tld = builtinTypesByNickname[name];

                ObjectHandle obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
                var newEl = (dynNode) obj.Unwrap();
                newEl.NodeUI.DisableInteraction();
                result = newEl;
            }
            else
            {
                FunctionDefinition def;
                dynSettings.FunctionDict.TryGetValue(Guid.Parse(name), out def);
                if (def == null)
                {
                    Bench.Log("Failed to find FunctionDefinition.");
                    return null;
                }

                dynWorkspace ws = def.Workspace;

                //TODO: Update to base off of Definition
                IEnumerable<string> inputs =
                    ws.Nodes.Where(e => e is dynSymbol)
                      .Select(s => (s as dynSymbol).Symbol);

                IEnumerable<string> outputs =
                    ws.Nodes.Where(e => e is dynOutput)
                      .Select(o => (o as dynOutput).Symbol);

                if (!outputs.Any())
                {
                    var topMost = new List<Tuple<int, dynNode>>();

                    IEnumerable<dynNode> topMostNodes = ws.GetTopMostNodes();

                    foreach (dynNode topNode in topMostNodes)
                    {
                        foreach (int output in Enumerable.Range(0, topNode.OutPortData.Count))
                        {
                            if (!topNode.HasOutput(output))
                                topMost.Add(Tuple.Create(output, topNode));
                        }
                    }

                    outputs = topMost.Select(x => x.Item2.OutPortData[x.Item1].NickName);
                }

                result = new dynFunction(inputs, outputs, def);
                result.NodeUI.NickName = ws.Name;
            }

            //if (result is dynDouble)
            //    (result as dynDouble).Value = this.storedSearchNum;
            //else if (result is dynStringInput)
            //    (result as dynStringInput).Value = this.storedSearchStr;
            //else if (result is dynBool)
            //    (result as dynBool).Value = this.storedSearchBool;

            return result;
        }

        #endregion

        #region Saving and Opening Workspaces

        /// <summary>
        ///     Attempts to save the current workspace
        ///     to a given path.  If successful, the CurrentSpace.FilePath
        ///     field is updated as a side effect
        /// </summary>
        internal void SaveAs(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (!SaveWorkspace(path, CurrentSpace))
                {
                    Bench.Log("Workbench could not be saved.");
                }
                else
                {
                    CurrentSpace.FilePath = path;
                }
            }
        }

        /// <summary>
        ///     Attempts to save an element, assuming that the CurrentSpace.FilePath 
        ///     field is already  populated with a path has a filename associated with it. 
        /// </summary>
        internal void Save()
        {
            if (!string.IsNullOrEmpty(CurrentSpace.FilePath))
                SaveAs(CurrentSpace.FilePath);
        }

        public XmlDocument GetXmlDocFromWorkspace(dynWorkspace workSpace)
        {
            try
            {
                //create the xml document
                var xmlDoc = new XmlDocument();
                xmlDoc.CreateXmlDeclaration("1.0", null, null);

                XmlElement root = xmlDoc.CreateElement("dynWorkspace"); //write the root element
                root.SetAttribute("X", workSpace.PositionX.ToString());
                root.SetAttribute("Y", workSpace.PositionY.ToString());

                if (workSpace != HomeSpace) //If we are not saving the home space
                {
                    root.SetAttribute("Name", workSpace.Name);
                    root.SetAttribute("Category", ((FuncWorkspace) workSpace).Category);
                    root.SetAttribute(
                        "ID",
                        dynSettings.FunctionDict.Values
                                   .First(x => x.Workspace == workSpace).FunctionId.ToString());
                }

                xmlDoc.AppendChild(root);

                XmlElement elementList = xmlDoc.CreateElement("dynElements"); //write the root element
                root.AppendChild(elementList);

                foreach (dynNode el in workSpace.Nodes)
                {
                    XmlElement dynEl = xmlDoc.CreateElement(el.GetType().ToString());
                    elementList.AppendChild(dynEl);

                    //set the type attribute
                    dynEl.SetAttribute("type", el.GetType().ToString());
                    dynEl.SetAttribute("guid", el.NodeUI.GUID.ToString());
                    dynEl.SetAttribute("nickname", el.NodeUI.NickName);
                    dynEl.SetAttribute("x", Canvas.GetLeft(el.NodeUI).ToString());
                    dynEl.SetAttribute("y", Canvas.GetTop(el.NodeUI).ToString());

                    el.SaveElement(xmlDoc, dynEl);
                }

                //write only the output connectors
                XmlElement connectorList = xmlDoc.CreateElement("dynConnectors"); //write the root element
                root.AppendChild(connectorList);

                foreach (dynNode el in workSpace.Nodes)
                {
                    foreach (dynPort port in el.NodeUI.OutPorts)
                    {
                        foreach (dynConnector c in port.Connectors.Where(c => c.Start != null && c.End != null))
                        {
                            XmlElement connector = xmlDoc.CreateElement(c.GetType().ToString());
                            connectorList.AppendChild(connector);
                            connector.SetAttribute("start", c.Start.Owner.GUID.ToString());
                            connector.SetAttribute("start_index", c.Start.Index.ToString());
                            connector.SetAttribute("end", c.End.Owner.GUID.ToString());
                            connector.SetAttribute("end_index", c.End.Index.ToString());

                            if (c.End.PortType == PortType.INPUT)
                                connector.SetAttribute("portType", "0");
                        }
                    }
                }

                //save the notes
                XmlElement noteList = xmlDoc.CreateElement("dynNotes"); //write the root element
                root.AppendChild(noteList);
                foreach (dynNote n in workSpace.Notes)
                {
                    XmlElement note = xmlDoc.CreateElement(n.GetType().ToString());
                    noteList.AppendChild(note);
                    note.SetAttribute("text", n.noteText.Text);
                    note.SetAttribute("x", Canvas.GetLeft(n).ToString());
                    note.SetAttribute("y", Canvas.GetTop(n).ToString());
                }

                return xmlDoc;
            }
            catch (Exception ex)
            {
                Bench.Log(ex);
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                return null;
            }
        }

        private bool SaveWorkspace(string xmlPath, dynWorkspace workSpace)
        {
            Bench.Log("Saving " + xmlPath + "...");
            try
            {
                var xmlDoc = GetXmlDocFromWorkspace(workSpace);
                xmlDoc.Save(xmlPath);

                //cache the file path for future save operations
                workSpace.FilePath = xmlPath;
            }
            catch (Exception ex)
            {
                Bench.Log(ex);
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                return false;
            }

            return true;
        }

        public void SaveFunction(FunctionDefinition definition, bool writeDefinition = true)
        {
            if (definition == null)
                return;

            dynWorkspace functionWorkspace = definition.Workspace;

            // must create a guid for the definition, save it to xml, 
            //Generate xml, and save it in a fixed place
            if (writeDefinition)
            {
                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pluginsPath = Path.Combine(directory, "definitions");

                try
                {
                    if (!Directory.Exists(pluginsPath))
                        Directory.CreateDirectory(pluginsPath);

                    string path = Path.Combine(pluginsPath, FormatFileName(functionWorkspace.Name) + ".dyf");
                    SaveWorkspace(path, functionWorkspace);
                    SearchViewModel.Add(definition.Workspace);
                }
                catch (Exception e)
                {
                    Bench.Log("Error saving:" + e.GetType());
                    Bench.Log(e);
                }
            }

            try
            {
                IEnumerable<dynNode> outputs = functionWorkspace.Nodes.Where(x => x is dynOutput);

                var topMost = new List<Tuple<int, dynNode>>();

                IEnumerable<string> outputNames;

                if (outputs.Any())
                {
                    topMost.AddRange(
                        outputs.Where(x => x.HasInput(0)).Select(x => x.Inputs[0]));

                    outputNames = outputs.Select(x => (x as dynOutput).Symbol);
                }
                else
                {
                    IEnumerable<dynNode> topMostNodes = functionWorkspace.GetTopMostNodes();

                    var outNames = new List<string>();

                    foreach (dynNode topNode in topMostNodes)
                    {
                        foreach (int output in Enumerable.Range(0, topNode.OutPortData.Count))
                        {
                            if (!topNode.HasOutput(output))
                            {
                                topMost.Add(Tuple.Create(output, topNode));
                                outNames.Add(topNode.OutPortData[output].NickName);
                            }
                        }
                    }

                    outputNames = outNames;
                }

                foreach (var ele in topMost)
                {
                    ele.Item2.NodeUI.ValidateConnections();
                }

                //Find function entry point, and then compile the function and add it to our environment
                //dynNode top = topMost.FirstOrDefault();

                IEnumerable<dynNode> variables = functionWorkspace.Nodes.Where(x => x is dynSymbol);
                IEnumerable<string> inputNames = variables.Select(x => (x as dynSymbol).Symbol);

                INode top;
                var buildDict = new Dictionary<dynNode, Dictionary<int, INode>>();

                if (topMost.Count > 1)
                {
                    InputNode node = new ExternalFunctionNode(
                        FScheme.Value.NewList,
                        Enumerable.Range(0, topMost.Count).Select(x => x.ToString()));

                    int i = 0;
                    foreach (var topNode in topMost)
                    {
                        string inputName = i.ToString();
                        node.ConnectInput(inputName, topNode.Item2.Build(buildDict, topNode.Item1));
                        i++;
                    }

                    top = node;
                }
                else
                    top = topMost[0].Item2.BuildExpression(buildDict);

                if (outputs.Any())
                {
                    var beginNode = new BeginNode();
                    List<dynNode> hangingNodes = functionWorkspace.GetTopMostNodes().ToList();
                    foreach (var tNode in hangingNodes.Select((x, index) => new {Index = index, Node = x}))
                    {
                        beginNode.AddInput(tNode.Index.ToString());
                        beginNode.ConnectInput(tNode.Index.ToString(), tNode.Node.Build(buildDict, 0));
                    }
                    beginNode.AddInput(hangingNodes.Count.ToString());
                    beginNode.ConnectInput(hangingNodes.Count.ToString(), top);

                    top = beginNode;
                }

                FScheme.Expression expression = Utils.MakeAnon(variables.Select(x => x.NodeUI.GUID.ToString()),
                                                               top.Compile());

                FSchemeEnvironment.DefineSymbol(definition.FunctionId.ToString(), expression);

                //Update existing function nodes which point to this function to match its changes
                foreach (dynNode el in AllNodes)
                {
                    if (el is dynFunction)
                    {
                        var node = (dynFunction) el;

                        if (node.Definition != definition)
                            continue;

                        node.SetInputs(inputNames);
                        node.SetOutputs(outputNames);
                        el.NodeUI.RegisterAllPorts();
                    }
                }

                //Call OnSave for all saved elements
                foreach (dynNode el in functionWorkspace.Nodes)
                    el.onSave();

                //Update new add menu
                var addItem = (dynFunction) Bench.addMenuItemsDictNew[functionWorkspace.Name].NodeLogic;
                addItem.SetInputs(inputNames);
                addItem.SetOutputs(outputNames);
                addItem.NodeUI.RegisterAllPorts();
                addItem.NodeUI.State = ElementState.DEAD;
            }
            catch (Exception ex)
            {
                Bench.Log(ex.GetType() + ": " + ex.Message);
            }
        }

        private static string FormatFileName(string filename)
        {
            return RemoveChars(
                filename,
                new[] {"\\", "/", ":", "*", "?", "\"", "<", ">", "|"}
                );
        }

        internal static string RemoveChars(string s, IEnumerable<string> chars)
        {
            foreach (string c in chars)
                s = s.Replace(c, "");
            return s;
        }

        internal bool OpenDefinition(string xmlPath)
        {
            return OpenDefinition(
                xmlPath,
                new Dictionary<Guid, HashSet<FunctionDefinition>>(),
                new Dictionary<Guid, HashSet<Guid>>());
        }

        private bool OpenDefinition(
            string xmlPath,
            Dictionary<Guid, HashSet<FunctionDefinition>> children,
            Dictionary<Guid, HashSet<Guid>> parents)
        {
            try
            {
                #region read xml file

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                string funName = null;
                string category = "";
                double cx = dynBench.CANVAS_OFFSET_X;
                double cy = dynBench.CANVAS_OFFSET_Y;
                string id = "";

                foreach (XmlNode node in xmlDoc.GetElementsByTagName("dynWorkspace"))
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("X"))
                            cx = Convert.ToDouble(att.Value);
                        else if (att.Name.Equals("Y"))
                            cy = Convert.ToDouble(att.Value);
                        else if (att.Name.Equals("Name"))
                            funName = att.Value;
                        else if (att.Name.Equals("Category"))
                            category = att.Value;
                        else if (att.Name.Equals("ID"))
                        {
                            id = att.Value;
                            if (string.IsNullOrEmpty(id) && node.Name == "Dynamo.Nodes.dynFunction")
                            {
                                // assign a legacy dyf with no guid
                                id = Guid.NewGuid().ToString();
                            }
                        }
                            
                    }
                }

 

                //If there is no function name, then we are opening a home definition
                if (funName == null)
                {
                    //View the home workspace, then open the bench file
                    if (!ViewingHomespace)
                        ViewHomeWorkspace(); //TODO: Refactor
                    return OpenWorkbench(xmlPath);
                }
                else if (dynSettings.FunctionDict.Values.Any(x => x.Workspace.Name == funName))
                {
                    Bench.Log("ERROR: Could not load definition for \"" + funName +
                              "\", a node with this name already exists.");
                    return false;
                }

                Bench.Log("Loading node definition for \"" + funName + "\" from: " + xmlPath);

                //TODO: refactor to include x,y
                FunctionDefinition def = NewFunction(
                    Guid.Parse(id),
                    funName,
                    category.Length > 0
                        ? category
                        : BuiltinNodeCategories.MISC,
                    false
                    );

                dynWorkspace ws = def.Workspace;

                ws.PositionX = cx;
                ws.PositionY = cy;

                //this.Log("Opening definition " + xmlPath + "...");

                XmlNodeList elNodes = xmlDoc.GetElementsByTagName("dynElements");
                XmlNodeList cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
                XmlNodeList nNodes = xmlDoc.GetElementsByTagName("dynNotes");

                XmlNode elNodesList = elNodes[0];
                XmlNode cNodesList = cNodes[0];
                XmlNode nNodesList = nNodes[0];

                var dependencies = new Stack<Guid>();

                #region instantiate nodes

                foreach (XmlNode elNode in elNodesList.ChildNodes)
                {
                    XmlAttribute typeAttrib = elNode.Attributes[0];
                    XmlAttribute guidAttrib = elNode.Attributes[1];
                    XmlAttribute nicknameAttrib = elNode.Attributes[2];
                    XmlAttribute xAttrib = elNode.Attributes[3];
                    XmlAttribute yAttrib = elNode.Attributes[4];

                    string typeName = typeAttrib.Value;

                    string oldNamespace = "Dynamo.Elements.";
                    if (typeName.StartsWith(oldNamespace))
                        typeName = "Dynamo.Nodes." + typeName.Remove(0, oldNamespace.Length);

                    //test the GUID to confirm that it is non-zero
                    //if it is zero, then we have to fix it
                    //this will break the connectors, but it won't keep
                    //propagating bad GUIDs
                    var guid = new Guid(guidAttrib.Value);
                    if (guid == Guid.Empty)
                    {
                        guid = Guid.NewGuid();
                    }

                    string nickname = nicknameAttrib.Value;

                    double x = Convert.ToDouble(xAttrib.Value);
                    double y = Convert.ToDouble(yAttrib.Value);

                    //Type t = Type.GetType(typeName);
                    TypeLoadData tData;
                    Type t;

                    if (!builtinTypesByTypeName.TryGetValue(typeName, out tData))
                    {
                        t = Type.GetType(typeName);
                        if (t == null)
                        {
                            Bench.Log("Error loading definition. Could not load node of type: " + typeName);
                            return false;
                        }
                    }
                    else
                        t = tData.Type;

                    dynNode el = AddDynElement(t, nickname, guid, x, y, ws, Visibility.Hidden);

                    if (el == null)
                        return false;

                    el.DisableReporting();
                    el.LoadElement(elNode);

                    if (el is dynFunction)
                    {
                        var fun = el as dynFunction;

                        Guid funId = Guid.Parse(fun.Symbol);
                        if (fun.Symbol == null || funId == Guid.Empty)
                        {
                            funId = Guid.NewGuid();
                        }

                        FunctionDefinition funcDef;
                        if (dynSettings.FunctionDict.TryGetValue(funId, out funcDef))
                            fun.Definition = funcDef;
                        else
                            dependencies.Push(funId);
                    }
                }

                #endregion

                Bench.WorkBench.UpdateLayout();

                #region instantiate connectors

                foreach (XmlNode connector in cNodesList.ChildNodes)
                {
                    XmlAttribute guidStartAttrib = connector.Attributes[0];
                    XmlAttribute intStartAttrib = connector.Attributes[1];
                    XmlAttribute guidEndAttrib = connector.Attributes[2];
                    XmlAttribute intEndAttrib = connector.Attributes[3];
                    XmlAttribute portTypeAttrib = connector.Attributes[4];

                    var guidStart = new Guid(guidStartAttrib.Value);
                    var guidEnd = new Guid(guidEndAttrib.Value);
                    int startIndex = Convert.ToInt16(intStartAttrib.Value);
                    int endIndex = Convert.ToInt16(intEndAttrib.Value);
                    int portType = Convert.ToInt16(portTypeAttrib.Value);

                    //find the elements to connect
                    dynNode start = null;
                    dynNode end = null;

                    foreach (dynNode e in ws.Nodes)
                    {
                        if (e.NodeUI.GUID == guidStart)
                        {
                            start = e;
                        }
                        else if (e.NodeUI.GUID == guidEnd)
                        {
                            end = e;
                        }
                        if (start != null && end != null)
                        {
                            break;
                        }
                    }

                    //don't connect if the end element is an instance map
                    //those have a morphing set of inputs
                    //dynInstanceParameterMap endTest = end as dynInstanceParameterMap;

                    //if (endTest != null)
                    //{
                    //    continue;
                    //}

                    try
                    {
                        if (start != null && end != null && start != end)
                        {
                            var newConnector = new dynConnector(
                                start.NodeUI, end.NodeUI,
                                startIndex, endIndex,
                                portType, false
                                );

                            ws.Connectors.Add(newConnector);
                        }
                    }
                    catch
                    {
                        Bench.Log(string.Format("ERROR : Could not create connector between {0} and {1}.", start.NodeUI.GUID, end.NodeUI.GUID));
                    }
                }

                #endregion

                #region instantiate notes

                if (nNodesList != null)
                {
                    foreach (XmlNode note in nNodesList.ChildNodes)
                    {
                        XmlAttribute textAttrib = note.Attributes[0];
                        XmlAttribute xAttrib = note.Attributes[1];
                        XmlAttribute yAttrib = note.Attributes[2];

                        string text = textAttrib.Value;
                        double x = Convert.ToDouble(xAttrib.Value);
                        double y = Convert.ToDouble(yAttrib.Value);

                        //dynNote n = Bench.AddNote(text, x, y, ws);
                        //Bench.AddNote(text, x, y, ws);

                        var paramDict = new Dictionary<string, object>();
                        paramDict.Add("x", x);
                        paramDict.Add("y", y);
                        paramDict.Add("text", text);
                        paramDict.Add("workspace", ws);
                        DynamoCommands.AddNoteCmd.Execute(paramDict);
                    }
                }

                #endregion

                foreach (dynNode e in ws.Nodes)
                    e.EnableReporting();

                hideWorkspace(ws);

                #endregion

                ws.FilePath = xmlPath;

                bool canLoad = true;

                //For each node this workspace depends on...
                foreach (Guid dep in dependencies)
                {
                    canLoad = false;
                    //Dep -> Ws
                    if (children.ContainsKey(dep))
                        children[dep].Add(def);
                    else
                        children[dep] = new HashSet<FunctionDefinition> {def};

                    //Ws -> Deps
                    if (parents.ContainsKey(def.FunctionId))
                        parents[def.FunctionId].Add(dep);
                    else
                        parents[def.FunctionId] = new HashSet<Guid> {dep};
                }

                if (canLoad)
                    SaveFunction(def, false);

                PackageManagerClient.LoadPackageHeader(def, funName);

                nodeWorkspaceWasLoaded(def, children, parents);
            }
            catch (Exception ex)
            {
                Bench.Log("There was an error opening the workbench.");
                Bench.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
                CleanWorkbench();
                return false;
            }

            return true;
        }


        private void nodeWorkspaceWasLoaded(
            FunctionDefinition def,
            Dictionary<Guid, HashSet<FunctionDefinition>> children,
            Dictionary<Guid, HashSet<Guid>> parents)
        {
            //If there were some workspaces that depended on this node...
            if (children.ContainsKey(def.FunctionId))
            {
                //For each workspace...
                foreach (FunctionDefinition child in children[def.FunctionId])
                {
                    //Nodes the workspace depends on
                    HashSet<Guid> allParents = parents[child.FunctionId];
                    //Remove this workspace, since it's now loaded.
                    allParents.Remove(def.FunctionId);
                    //If everything the node depends on has been loaded...
                    if (!allParents.Any())
                    {
                        SaveFunction(child, false);
                        nodeWorkspaceWasLoaded(child, children, parents);
                    }
                }
            }
        }

        private void hideWorkspace(dynWorkspace ws)
        {
            foreach (dynNode e in ws.Nodes)
                e.NodeUI.Visibility = Visibility.Collapsed;
            foreach (dynConnector c in ws.Connectors)
                c.Visible = false;
            foreach (dynNote n in ws.Notes)
                n.Visibility = Visibility.Hidden;
        }

        private bool OpenWorkbench(string xmlPath)
        {
            Bench.Log("Opening home workspace " + xmlPath + "...");
            CleanWorkbench();

            try
            {
                #region read xml file

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                foreach (XmlNode node in xmlDoc.GetElementsByTagName("dynWorkspace"))
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("X"))
                        {
                            //Bench.CurrentX = Convert.ToDouble(att.Value);
                            Bench.CurrentOffset = new Point(Convert.ToDouble(att.Value), Bench.CurrentOffset.Y);
                        }
                        else if (att.Name.Equals("Y"))
                        {
                            //Bench.CurrentY = Convert.ToDouble(att.Value);
                            Bench.CurrentOffset = new Point(Bench.CurrentOffset.X, Convert.ToDouble(att.Value));
                        }
                    }
                }

                XmlNodeList elNodes = xmlDoc.GetElementsByTagName("dynElements");
                XmlNodeList cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
                XmlNodeList nNodes = xmlDoc.GetElementsByTagName("dynNotes");

                XmlNode elNodesList = elNodes[0];
                XmlNode cNodesList = cNodes[0];
                XmlNode nNodesList = nNodes[0];

                foreach (XmlNode elNode in elNodesList.ChildNodes)
                {
                    XmlAttribute typeAttrib = elNode.Attributes[0];
                    XmlAttribute guidAttrib = elNode.Attributes[1];
                    XmlAttribute nicknameAttrib = elNode.Attributes[2];
                    XmlAttribute xAttrib = elNode.Attributes[3];
                    XmlAttribute yAttrib = elNode.Attributes[4];

                    string typeName = typeAttrib.Value;

                    //test the GUID to confirm that it is non-zero
                    //if it is zero, then we have to fix it
                    //this will break the connectors, but it won't keep
                    //propagating bad GUIDs
                    var guid = new Guid(guidAttrib.Value);
                    if (guid == Guid.Empty)
                    {
                        guid = Guid.NewGuid();
                    }

                    string nickname = nicknameAttrib.Value;

                    double x = Convert.ToDouble(xAttrib.Value);
                    double y = Convert.ToDouble(yAttrib.Value);

                    if (typeName.StartsWith("Dynamo.Elements."))
                        typeName = "Dynamo.Nodes." + typeName.Remove(0, 16);

                    TypeLoadData tData;
                    Type t;

                    if (!builtinTypesByTypeName.TryGetValue(typeName, out tData))
                    {
                        t = Type.GetType(typeName);
                        if (t == null)
                        {
                            Bench.Log("Error loading workspace. Could not load node of type: " + typeName);
                            return false;
                        }
                    }
                    else
                        t = tData.Type;

                    dynNode el = AddDynElement(
                        t, nickname, guid, x, y,
                        CurrentSpace
                        );

                    el.DisableReporting();

                    el.LoadElement(elNode);

                    if (ViewingHomespace)
                        el.SaveResult = true;

                    if (el is dynFunction)
                    {
                        var fun = el as dynFunction;
                        Guid funId = Guid.Parse(fun.Symbol);

                        FunctionDefinition funcDef;
                        if (dynSettings.FunctionDict.TryGetValue(funId, out funcDef))
                            fun.Definition = funcDef;
                        else
                            fun.NodeUI.Error("No definition found.");
                    }

                    //read the sub elements
                    //set any numeric values 
                    //foreach (XmlNode subNode in elNode.ChildNodes)
                    //{
                    //   if (subNode.Name == "System.Double")
                    //   {
                    //      double val = Convert.ToDouble(subNode.Attributes[0].Value);
                    //      el.OutPortData[0].Object = val;
                    //      el.Update();
                    //   }
                    //   else if (subNode.Name == "System.Int32")
                    //   {
                    //      int val = Convert.ToInt32(subNode.Attributes[0].Value);
                    //      el.OutPortData[0].Object = val;
                    //      el.Update();
                    //   }
                    //}
                }

                dynSettings.Workbench.UpdateLayout();

                foreach (XmlNode connector in cNodesList.ChildNodes)
                {
                    XmlAttribute guidStartAttrib = connector.Attributes[0];
                    XmlAttribute intStartAttrib = connector.Attributes[1];
                    XmlAttribute guidEndAttrib = connector.Attributes[2];
                    XmlAttribute intEndAttrib = connector.Attributes[3];
                    XmlAttribute portTypeAttrib = connector.Attributes[4];

                    var guidStart = new Guid(guidStartAttrib.Value);
                    var guidEnd = new Guid(guidEndAttrib.Value);
                    int startIndex = Convert.ToInt16(intStartAttrib.Value);
                    int endIndex = Convert.ToInt16(intEndAttrib.Value);
                    int portType = Convert.ToInt16(portTypeAttrib.Value);

                    //find the elements to connect
                    dynNode start = null;
                    dynNode end = null;

                    foreach (dynNode e in Nodes)
                    {
                        if (e.NodeUI.GUID == guidStart)
                        {
                            start = e;
                        }
                        else if (e.NodeUI.GUID == guidEnd)
                        {
                            end = e;
                        }
                        if (start != null && end != null)
                        {
                            break;
                        }
                    }

                    //don't connect if the end element is an instance map
                    //those have a morphing set of inputs
                    //dynInstanceParameterMap endTest = end as dynInstanceParameterMap;

                    //if (endTest != null)
                    //{
                    //    continue;
                    //}

                    if (start != null && end != null && start != end)
                    {
                        var newConnector = new dynConnector(start.NodeUI, end.NodeUI,
                                                            startIndex, endIndex, portType);

                        CurrentSpace.Connectors.Add(newConnector);
                    }
                }

                #region instantiate notes

                if (nNodesList != null)
                {
                    foreach (XmlNode note in nNodesList.ChildNodes)
                    {
                        XmlAttribute textAttrib = note.Attributes[0];
                        XmlAttribute xAttrib = note.Attributes[1];
                        XmlAttribute yAttrib = note.Attributes[2];

                        string text = textAttrib.Value;
                        double x = Convert.ToDouble(xAttrib.Value);
                        double y = Convert.ToDouble(yAttrib.Value);

                        //dynNote n = Bench.AddNote(text, x, y, this.CurrentSpace);
                        //Bench.AddNote(text, x, y, this.CurrentSpace);

                        var paramDict = new Dictionary<string, object>();
                        paramDict.Add("x", x);
                        paramDict.Add("y", y);
                        paramDict.Add("text", text);
                        paramDict.Add("workspace", CurrentSpace);
                        DynamoCommands.AddNoteCmd.Execute(paramDict);
                    }
                }

                #endregion

                foreach (dynNode e in CurrentSpace.Nodes)
                    e.EnableReporting();

                #endregion

                HomeSpace.FilePath = xmlPath;
            }
            catch (Exception ex)
            {
                Bench.Log("There was an error opening the workbench.");
                Bench.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
                CleanWorkbench();
                return false;
            }
            return true;
        }

        internal void CleanWorkbench()
        {
            Bench.Log("Clearing workflow...");

            //Copy locally
            List<dynNode> elements = Nodes.ToList();

            foreach (dynNode el in elements)
            {
                el.DisableReporting();
                try
                {
                    el.Destroy();
                }
                catch
                {
                }
            }

            foreach (dynNode el in elements)
            {
                foreach (dynPort p in el.NodeUI.InPorts)
                {
                    for (int i = p.Connectors.Count - 1; i >= 0; i--)
                        p.Connectors[i].Kill();
                }
                foreach (dynPort port in el.NodeUI.OutPorts)
                {
                    for (int i = port.Connectors.Count - 1; i >= 0; i--)
                        port.Connectors[i].Kill();
                }

                dynSettings.Workbench.Children.Remove(el.NodeUI);
            }

            foreach (dynNote n in CurrentSpace.Notes)
            {
                dynSettings.Workbench.Children.Remove(n);
            }

            CurrentSpace.Nodes.Clear();
            CurrentSpace.Connectors.Clear();
            CurrentSpace.Notes.Clear();
            CurrentSpace.Modified();
        }

        #endregion

        #region Running

        //protected bool _debug;
        private bool _showErrors;

        protected bool canRunDynamically = true;
        protected bool debug = false;

        protected bool dynamicRun = false;
        private bool runAgain;
        public bool Running { get; protected set; }

        public bool RunCancelled { get; protected internal set; }

        public virtual bool CanRunDynamically
        {
            get
            {
                //we don't want to be able to run
                //dynamically if we're in debug mode
                return !debug;
            }
            set
            {
                canRunDynamically = value;
                NotifyPropertyChanged("CanRunDynamically");
            }
        }

        public virtual bool DynamicRunEnabled
        {
            get { return dynamicRun; //selecting debug now toggles this on/off
            }
            set
            {
                dynamicRun = value;
                NotifyPropertyChanged("DynamicRunEnabled");
            }
        }

        public virtual bool RunInDebug
        {
            get { return debug; }
            set
            {
                debug = value;

                //toggle off dynamic run
                CanRunDynamically = !debug;

                if (debug)
                    DynamicRunEnabled = false;

                NotifyPropertyChanged("RunInDebug");
            }
        }

        internal void QueueRun()
        {
            RunCancelled = true;
            runAgain = true;
        }

        public void RunExpression(bool showErrors = true)
        {
            //If we're already running, do nothing.
            if (Running)
                return;

            _showErrors = showErrors;

            //TODO: Hack. Might cause things to break later on...
            //Reset Cancel and Rerun flags
            RunCancelled = false;
            runAgain = false;

            //We are now considered running
            Running = true;

            //Set run auto flag
            //this.DynamicRunEnabled = !showErrors;

            //Setup background worker
            var worker = new BackgroundWorker();
            worker.DoWork += EvaluationThread;

            //Disable Run Button

            //Bench.Dispatcher.Invoke(new Action(
            //   delegate { Bench.RunButton.IsEnabled = false; }
            //));

            this.RunEnabled = false;

            //Let's start
            worker.RunWorkerAsync();
        }

        protected virtual void EvaluationThread(object s, DoWorkEventArgs args)
        {
            /* Execution Thread */

            //Get our entry points (elements with nothing connected to output)
            IEnumerable<dynNode> topElements = HomeSpace.GetTopMostNodes();

            //Mark the topmost as dirty/clean
            foreach (dynNode topMost in topElements)
                topMost.MarkDirty();

            //TODO: Flesh out error handling
            try
            {
                var topNode = new BeginNode(new List<string>());
                int i = 0;
                var buildDict = new Dictionary<dynNode, Dictionary<int, INode>>();
                foreach (dynNode topMost in topElements)
                {
                    string inputName = i.ToString();
                    topNode.AddInput(inputName);
                    topNode.ConnectInput(inputName, topMost.BuildExpression(buildDict));
                    i++;
                }

                FScheme.Expression runningExpression = topNode.Compile();

                Run(topElements, runningExpression);
            }
            catch (CancelEvaluationException ex)
            {
                /* Evaluation was cancelled */

                OnRunCancelled(false);
                //this.CancelRun = false; //Reset cancel flag
                RunCancelled = true;

                //If we are forcing this, then make sure we don't run again either.
                if (ex.Force)
                    runAgain = false;
            }
            catch (Exception ex)
            {
                /* Evaluation has an error */

                //Catch unhandled exception
                if (ex.Message.Length > 0)
                {
                    Bench.Dispatcher.Invoke(new Action(
                                                delegate { Bench.Log(ex); }
                                                ));
                }

                OnRunCancelled(true);

                //Reset the flags
                runAgain = false;
                RunCancelled = true;
            }
            finally
            {
                /* Post-evaluation cleanup */

                //Re-enable run button
                //Bench.Dispatcher.Invoke(new Action(
                //   delegate
                //   {
                //       Bench.RunButton.IsEnabled = true;
                //   }
                //));

                this.RunEnabled = true;

                //No longer running
                Running = false;

                foreach (FunctionDefinition def in dynSettings.FunctionWasEvaluated)
                    def.RequiresRecalc = false;

                //If we should run again...
                if (runAgain)
                {
                    //Reset flag
                    runAgain = false;

                    //Run this method again from the main thread
                    Bench.Dispatcher.BeginInvoke(new Action(
                                                     delegate { RunExpression(_showErrors); }
                                                     ));
                }
            }
        }

        protected internal virtual void Run(IEnumerable<dynNode> topElements, FScheme.Expression runningExpression)
        {
            //Print some stuff if we're in debug mode
            if (debug)
            {
                //string exp = FScheme.print(runningExpression);
                Bench.Dispatcher.Invoke(new Action(
                                            delegate
                                                {
                                                    foreach (dynNode node in topElements)
                                                    {
                                                        string exp = node.PrintExpression();
                                                        Bench.Log("> " + exp);
                                                    }
                                                }
                                            ));
            }

            try
            {
                //Evaluate the expression
                FScheme.Value expr = FSchemeEnvironment.Evaluate(runningExpression);

                //Print some more stuff if we're in debug mode
                if (debug && expr != null)
                {
                    Bench.Dispatcher.Invoke(new Action(
                                                () => Bench.Log(FScheme.print(expr))
                                                ));
                }
            }
            catch (CancelEvaluationException ex)
            {
                /* Evaluation was cancelled */

                OnRunCancelled(false);
                //this.RunCancelled = false;
                if (ex.Force)
                    runAgain = false;
            }
            catch (Exception ex)
            {
                /* Evaluation failed due to error */

                //Print unhandled exception
                if (ex.Message.Length > 0)
                {
                    Bench.Dispatcher.Invoke(new Action(
                                                delegate { Bench.Log(ex); }
                                                ));
                }
                OnRunCancelled(true);
                RunCancelled = true;
                runAgain = false;
            }

            OnEvaluationCompleted();
        }

        protected virtual void OnRunCancelled(bool error)
        {
            if (error)
                dynSettings.FunctionWasEvaluated.Clear();
        }

        protected virtual void OnEvaluationCompleted()
        {
        }

        internal void ShowElement(dynNode e)
        {
            if (dynamicRun)
                return;

            if (!Nodes.Contains(e))
            {
                if (HomeSpace != null && HomeSpace.Nodes.Contains(e))
                {
                    //Show the homespace
                    ViewHomeWorkspace();
                }
                else
                {
                    foreach (FunctionDefinition funcDef in dynSettings.FunctionDict.Values)
                    {
                        if (funcDef.Workspace.Nodes.Contains(e))
                        {
                            DisplayFunction(funcDef);
                            break;
                        }
                    }
                }
            }

            Bench.CenterViewOnElement(e.NodeUI);
        }

        #endregion

        #region Changing Workspace Views

        internal void ViewHomeWorkspace()
        {
            //Step 1: Make function workspace invisible
            foreach (dynNode ele in Nodes)
            {
                ele.NodeUI.Visibility = Visibility.Collapsed;
            }
            foreach (dynConnector con in CurrentSpace.Connectors)
            {
                con.Visible = false;
            }
            foreach (dynNote note in CurrentSpace.Notes)
            {
                note.Visibility = Visibility.Hidden;
            }
            //var ws = new dynWorkspace(this.elements, this.connectors, this.CurrentX, this.CurrentY);

            //Step 2: Store function workspace in the function dictionary
            //this.FunctionDict[this.CurrentSpace.Name] = this.CurrentSpace;

            //Step 3: Save function
            SaveFunction( dynSettings.FunctionDict.Values.FirstOrDefault(x => x.Workspace == CurrentSpace) );

            //Step 4: Make home workspace visible
            //this.elements = this.homeSpace.elements;
            //this.connectors = this.homeSpace.connectors;
            //this.CurrentX = this.homeSpace.savedX;
            //this.CurrentY = this.homeSpace.savedY;
            CurrentSpace = HomeSpace;

            foreach (dynNode ele in Nodes)
            {
                ele.NodeUI.Visibility = Visibility.Visible;
            }
            foreach (dynConnector con in CurrentSpace.Connectors)
            {
                con.Visible = true;
            }
            foreach (dynNote note in CurrentSpace.Notes)
            {
                note.Visibility = Visibility.Visible;
            }

            //this.saveFuncItem.IsEnabled = false;
            Bench.homeButton.IsEnabled = false;
            //this.varItem.IsEnabled = false;

            PackageManagerClient.HidePackageControlInformation();

            Bench.workspaceLabel.Content = "Home";
            Bench.editNameButton.Visibility = Visibility.Collapsed;
            Bench.editNameButton.IsHitTestVisible = false;

            Bench.setHomeBackground();

            CurrentSpace.OnDisplayed();
        }

        internal void DisplayFunction(FunctionDefinition symbol)
        {
            if (symbol == null || CurrentSpace.Name.Equals(symbol.Workspace.Name))
                return;

            dynWorkspace newWs = symbol.Workspace;

            //Make sure we aren't dragging
            Bench.WorkBench.isDragInProgress = false;
            Bench.WorkBench.ignoreClick = true;

            //Step 1: Make function workspace invisible
            foreach (dynNode ele in Nodes)
            {
                ele.NodeUI.Visibility = Visibility.Collapsed;
            }
            foreach (dynConnector con in CurrentSpace.Connectors)
            {
                con.Visible = false;
            }
            foreach (dynNote note in CurrentSpace.Notes)
            {
                note.Visibility = Visibility.Hidden;
            }
            //var ws = new dynWorkspace(this.elements, this.connectors, this.CurrentX, this.CurrentY);

            if (!ViewingHomespace)
            {
                //Step 2: Store function workspace in the function dictionary
                //this.FunctionDict[this.CurrentSpace.Name] = this.CurrentSpace;

                //Step 3: Save function
                SaveFunction(dynSettings.FunctionDict.Values.First(x => x.Workspace == CurrentSpace));
            }

            //Step 4: Make home workspace visible
            //this.elements = newWs.elements;
            //this.connectors = newWs.connectors;
            //this.CurrentX = newWs.savedX;
            //this.CurrentY = newWs.savedY;
            CurrentSpace = newWs;

            foreach (dynNode ele in Nodes)
            {
                ele.NodeUI.Visibility = Visibility.Visible;
            }
            foreach (dynConnector con in CurrentSpace.Connectors)
            {
                con.Visible = true;
            }

            foreach (dynNote note in CurrentSpace.Notes)
            {
                note.Visibility = Visibility.Visible;
            }

            //this.saveFuncItem.IsEnabled = true;
            Bench.homeButton.IsEnabled = true;
            //this.varItem.IsEnabled = true;

            Bench.workspaceLabel.Content = symbol.Workspace.Name;

            Bench.editNameButton.Visibility = Visibility.Visible;
            Bench.editNameButton.IsHitTestVisible = true;

            Bench.setFunctionBackground();

            PackageManagerClient.ShowPackageControlInformation();

            CurrentSpace.OnDisplayed();
        }

        #endregion

        #region Updating Nodes

        internal void SaveNameEdit()
        {
            string newName = Bench.editNameBox.Text;

            if (dynSettings.FunctionDict.Values.Any(x => x.Workspace.Name == newName))
            {
                Bench.Log("ERROR: Cannot rename to \"" + newName + "\", node with same name already exists.");
                return;
            }

            Bench.workspaceLabel.Content = Bench.editNameBox.Text;

            SearchViewModel.Refactor(CurrentSpace, newName);

            var newAddItem = (dynFunction) Bench.addMenuItemsDictNew[CurrentSpace.Name].NodeLogic;
            if (newAddItem.NodeUI.NickName.Equals(CurrentSpace.Name))
                newAddItem.NodeUI.NickName = newName;
            //newAddItem.Symbol = newName;
            Bench.addMenuItemsDictNew.Remove(CurrentSpace.Name);
            Bench.addMenuItemsDictNew[newName] = newAddItem.NodeUI;

            //Sort the menu after a rename
            Expander unsorted = Bench.addMenuCategoryDict.Values.FirstOrDefault(
                ex => ((WrapPanel) ex.Content).Children.Contains(newAddItem.NodeUI)
                );

            var wp = (WrapPanel) unsorted.Content;

            var sortedElements = new SortedList<string, dynNodeUI>();
            foreach (dynNodeUI child in wp.Children)
            {
                sortedElements.Add(child.NickName, child);
            }

            wp.Children.Clear();

            foreach (dynNodeUI child in sortedElements.Values)
            {
                wp.Children.Add(child);
            }


            //Update search dictionary after a rename
            IEnumerable<string> oldTags = CurrentSpace.Name.Split(' ').Where(x => x.Length > 0);
            searchDict.Remove(newAddItem.NodeUI, oldTags);
            searchDict.Add(newAddItem.NodeUI, CurrentSpace.Name);

            IEnumerable<string> newTags = newName.Split(' ').Where(x => x.Length > 0);
            searchDict.Add(newAddItem.NodeUI, newTags);
            searchDict.Add(newAddItem.NodeUI, newName);

            //------------------//

            //Update existing function nodes
            foreach (dynNode el in AllNodes)
            {
                if (el is dynFunction)
                {
                    var node = (dynFunction) el;

                    if (!node.Definition.Workspace.Name.Equals(CurrentSpace.Name))
                        continue;

                    //node.Symbol = newName;

                    //Rename nickname only if it's still referring to the old name
                    if (node.NodeUI.NickName.Equals(CurrentSpace.Name))
                        node.NodeUI.NickName = newName;
                }
            }

            FSchemeEnvironment.RemoveSymbol(CurrentSpace.Name);

            //TODO: Delete old stored definition
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            if (Directory.Exists(pluginsPath))
            {
                string oldpath = Path.Combine(pluginsPath, CurrentSpace.Name + ".dyf");
                if (File.Exists(oldpath))
                {
                    string newpath = FormatFileName(
                        Path.Combine(pluginsPath, newName + ".dyf")
                        );

                    File.Move(oldpath, newpath);
                }
            }

            //Update function dictionary
            //var tmp = this.FunctionDict[this.CurrentSpace.Name];
            //this.FunctionDict.Remove(this.CurrentSpace.Name);
            //this.FunctionDict[newName] = tmp;

            (CurrentSpace).Name = newName;

            SaveFunction(dynSettings.FunctionDict.Values.First(x => x.Workspace == CurrentSpace));
        }

        #endregion

        #region Filtering

        private static readonly Regex searchBarNumRegex = new Regex(@"^-?\d+(\.\d*)?$");
        private static readonly Regex searchBarStrRegex = new Regex("^\"([^\"]*)\"?$");
        private readonly SearchDictionary<dynNodeUI> searchDict = new SearchDictionary<dynNodeUI>();
        private bool storedSearchBool;
        private double storedSearchNum;
        private string storedSearchStr = "";

        internal void filterCategory(HashSet<dynNodeUI> elements, Expander ex)
        {
            var content = (WrapPanel) ex.Content;

            bool filterWholeCategory = true;

            foreach (dynNodeUI ele in content.Children)
            {
                if (!elements.Contains(ele))
                {
                    ele.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ele.Visibility = Visibility.Visible;
                    filterWholeCategory = false;
                }
            }

            if (filterWholeCategory)
            {
                ex.Visibility = Visibility.Collapsed;
            }
            else
            {
                ex.Visibility = Visibility.Visible;

                //if (filter.Length > 0)
                //   ex.IsExpanded = true;
            }
        }

        internal void UpdateSearch(string search)
        {
            Match m;

            if (searchBarNumRegex.IsMatch(search))
            {
                storedSearchNum = Convert.ToDouble(search);
                Bench.FilterAddMenu(
                    new HashSet<dynNodeUI>
                        {
                            Bench.addMenuItemsDictNew["Number"],
                            Bench.addMenuItemsDictNew["Number Slider"]
                        }
                    );
            }
            else if ((m = searchBarStrRegex.Match(search)).Success) //(search.StartsWith("\""))
            {
                storedSearchStr = m.Groups[1].Captures[0].Value;
                Bench.FilterAddMenu(
                    new HashSet<dynNodeUI>
                        {
                            Bench.addMenuItemsDictNew["String"]
                        }
                    );
            }
            else if (search.Equals("true") || search.Equals("false"))
            {
                storedSearchBool = Convert.ToBoolean(search);
                Bench.FilterAddMenu(
                    new HashSet<dynNodeUI>
                        {
                            Bench.addMenuItemsDictNew["Boolean"]
                        }
                    );
            }
            else
            {
                storedSearchNum = 0;
                storedSearchStr = "";
                storedSearchBool = false;

                HashSet<dynNodeUI> filter = search.Length == 0
                                                ? new HashSet<dynNodeUI>(Bench.addMenuItemsDictNew.Values)
                                                : searchDict.Filter(search.ToLower());

                Bench.FilterAddMenu(filter);
            }
        }

        #endregion

        #region Refactor

        internal void NodeFromSelection(IEnumerable<dynNode> selectedNodes)
        {
            var selectedNodeSet = new HashSet<dynNode>(selectedNodes);

            #region Prompt

            //First, prompt the user to enter a name
            string newNodeName, newNodeCategory;
            string error = "";

            do
            {
                var dialog = new FunctionNamePrompt(Bench.addMenuCategoryDict.Keys, error);
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                newNodeName = dialog.Text;
                newNodeCategory = dialog.Category;

                if (dynSettings.FunctionDict.Values.Any(x => x.Workspace.Name == newNodeName))
                {
                    error = "A function with this name already exists.";
                }
                else if (newNodeCategory.Equals(""))
                {
                    error = "Please enter a valid category.";
                }
                else
                {
                    error = "";
                }
            } while (!error.Equals(""));

            FunctionDefinition newNodeDefinition = NewFunction(Guid.NewGuid(), newNodeName, newNodeCategory, false);
            dynWorkspace newNodeWorkspace = newNodeDefinition.Workspace;

            #endregion

            CurrentSpace.DisableReporting();

            #region UI Positioning Calculations

            double avgX = selectedNodeSet.Average(node => Canvas.GetLeft(node.NodeUI));
            double avgY = selectedNodeSet.Average(node => Canvas.GetTop(node.NodeUI));

            double leftMost = selectedNodeSet.Min(node => Canvas.GetLeft(node.NodeUI));
            double topMost = selectedNodeSet.Min(node => Canvas.GetTop(node.NodeUI));
            double rightMost = selectedNodeSet.Max(node => Canvas.GetLeft(node.NodeUI) + node.NodeUI.Width);

            #endregion

            #region Determine Inputs and Outputs

            //Step 1: determine which nodes will be inputs to the new node
            var inputs = new HashSet<Tuple<dynNode, int, Tuple<int, dynNode>>>(
                selectedNodeSet.SelectMany(
                    node => Enumerable.Range(0, node.InPortData.Count).Where(node.HasInput).Select(
                        data => Tuple.Create(node, data, node.Inputs[data])).Where(
                            input => !selectedNodeSet.Contains(input.Item3.Item2))));

            var outputs = new HashSet<Tuple<dynNode, int, Tuple<int, dynNode>>>(
                selectedNodeSet.SelectMany(
                    node => Enumerable.Range(0, node.OutPortData.Count).Where(node.HasOutput).SelectMany(
                        data => node.Outputs[data]
                                    .Where(output => !selectedNodeSet.Contains(output.Item2))
                                    .Select(output => Tuple.Create(node, data, output)))));

            #endregion

            #region Detect 1-node holes (higher-order function extraction)

            var curriedNodeArgs =
                new HashSet<dynNode>(
                    inputs
                        .Select(x => x.Item3.Item2)
                        .Intersect(outputs.Select(x => x.Item3.Item2)))
                    .Select(
                        outerNode =>
                            {
                                var node = new dynApply1();

                                dynNodeUI nodeUI = node.NodeUI;

                                var elNameAttrib =
                                    node.GetType().GetCustomAttributes(typeof (NodeNameAttribute), true)[0] as
                                    NodeNameAttribute;
                                if (elNameAttrib != null)
                                {
                                    nodeUI.NickName = elNameAttrib.Name;
                                }

                                nodeUI.GUID = Guid.NewGuid();

                                //store the element in the elements list
                                newNodeWorkspace.Nodes.Add(node);
                                node.WorkSpace = newNodeWorkspace;

                                node.DisableReporting();

                                Bench.WorkBench.Children.Add(nodeUI);

                                //Place it in an appropriate spot
                                Canvas.SetLeft(nodeUI, Canvas.GetLeft(outerNode.NodeUI));
                                Canvas.SetTop(nodeUI, Canvas.GetTop(outerNode.NodeUI));

                                //Fetch all input ports
                                // in order
                                // that have inputs
                                // and whose input comes from an inner node
                                List<int> inPortsConnected = Enumerable.Range(0, outerNode.InPortData.Count)
                                                                       .Where(
                                                                           x =>
                                                                           outerNode.HasInput(x) &&
                                                                           selectedNodeSet.Contains(
                                                                               outerNode.Inputs[x].Item2))
                                                                       .ToList();

                                var nodeInputs = outputs
                                    .Where(output => output.Item3.Item2 == outerNode)
                                    .Select(
                                        output =>
                                        new
                                            {
                                                InnerNodeInputSender = output.Item1,
                                                OuterNodeInPortData = output.Item3.Item1
                                            }).ToList();

                                nodeInputs.ForEach(_ => node.AddInput());

                                node.NodeUI.RegisterAllPorts();

                                Bench.WorkBench.UpdateLayout();

                                return new
                                    {
                                        OuterNode = outerNode,
                                        InnerNode = node,
                                        Outputs = inputs.Where(input => input.Item3.Item2 == outerNode)
                                                        .Select(input => input.Item3.Item1),
                                        Inputs = nodeInputs,
                                        OuterNodePortDataList = inPortsConnected
                                    };
                            }).ToList();

            #endregion

            #region Move selection to new workspace

            var connectors = new HashSet<dynConnector>(
                CurrentSpace.Connectors.Where(
                    conn => selectedNodeSet.Contains(conn.Start.Owner.NodeLogic)
                            && selectedNodeSet.Contains(conn.End.Owner.NodeLogic)));

            //Step 2: move all nodes to new workspace
            //  remove from old
            CurrentSpace.Nodes.RemoveAll(selectedNodeSet.Contains);
            CurrentSpace.Connectors.RemoveAll(connectors.Contains);
            //  add to new
            newNodeWorkspace.Nodes.AddRange(selectedNodeSet);
            newNodeWorkspace.Connectors.AddRange(connectors);

            double leftShift = leftMost - 250;
            foreach (dynNodeUI node in newNodeWorkspace.Nodes.Select(x => x.NodeUI))
            {
                Canvas.SetLeft(node, Canvas.GetLeft(node) - leftShift);
                Canvas.SetTop(node, Canvas.GetTop(node) - topMost);
            }

            #endregion

            #region Insert new node replacement into the current workspace

            //Step 5: insert new node into original workspace
            var collapsedNode = new dynFunction(
                inputs.Select(x => x.Item1.InPortData[x.Item2].NickName),
                outputs
                    .Where(x => !curriedNodeArgs.Any(y => y.OuterNode == x.Item3.Item2))
                    .Select(x => x.Item1.OutPortData[x.Item2].NickName),
                newNodeDefinition);

            collapsedNode.NodeUI.GUID = Guid.NewGuid();

            CurrentSpace.Nodes.Add(collapsedNode);
            collapsedNode.WorkSpace = CurrentSpace;

            Bench.WorkBench.Children.Add(collapsedNode.NodeUI);

            Canvas.SetLeft(collapsedNode.NodeUI, avgX);
            Canvas.SetTop(collapsedNode.NodeUI, avgY);

            Bench.WorkBench.UpdateLayout();

            #endregion

            #region Destroy all hanging connectors

            //Step 6: connect inputs and outputs
            foreach (dynConnector connector in CurrentSpace.Connectors
                                                           .Where(
                                                               c =>
                                                               selectedNodeSet.Contains(c.Start.Owner.NodeLogic) &&
                                                               !selectedNodeSet.Contains(c.End.Owner.NodeLogic))
                                                           .ToList())
            {
                connector.Kill();
            }

            foreach (dynConnector connector in CurrentSpace.Connectors
                                                           .Where(
                                                               c =>
                                                               !selectedNodeSet.Contains(c.Start.Owner.NodeLogic) &&
                                                               selectedNodeSet.Contains(c.End.Owner.NodeLogic)).ToList()
                )
            {
                connector.Kill();
            }

            #endregion

            newNodeWorkspace.Nodes.ForEach(x => x.DisableReporting());

            #region Process inputs

            //Step 3: insert variables (reference step 1)
            foreach (var input in Enumerable.Range(0, inputs.Count).Zip(inputs, Tuple.Create))
            {
                int inputIndex = input.Item1;

                dynNode inputReceiverNode = input.Item2.Item1;
                int inputReceiverData = input.Item2.Item2;

                dynNode inputNode = input.Item2.Item3.Item2;
                int inputData = input.Item2.Item3.Item1;

                //Connect outside input to the node
                CurrentSpace.Connectors.Add(
                    new dynConnector(
                        inputNode.NodeUI,
                        collapsedNode.NodeUI,
                        inputData,
                        inputIndex,
                        0,
                        true));

                //Create Symbol Node
                var node = new dynSymbol
                    {
                        Symbol = inputReceiverNode.InPortData[inputReceiverData].NickName
                    };

                dynNodeUI nodeUI = node.NodeUI;

                var elNameAttrib =
                    node.GetType().GetCustomAttributes(typeof (NodeNameAttribute), true)[0] as NodeNameAttribute;
                if (elNameAttrib != null)
                {
                    nodeUI.NickName = elNameAttrib.Name;
                }

                nodeUI.GUID = Guid.NewGuid();

                //store the element in the elements list
                newNodeWorkspace.Nodes.Add(node);
                node.WorkSpace = newNodeWorkspace;

                node.DisableReporting();

                Bench.WorkBench.Children.Add(nodeUI);

                //Place it in an appropriate spot
                Canvas.SetLeft(nodeUI, 0);
                Canvas.SetTop(nodeUI, inputIndex*(50 + node.NodeUI.Height));

                Bench.WorkBench.UpdateLayout();

                var curriedNode = curriedNodeArgs.FirstOrDefault(
                    x => x.OuterNode == inputNode);

                if (curriedNode == null)
                {
                    //Connect it (new dynConnector)
                    newNodeWorkspace.Connectors.Add(new dynConnector(
                                                        nodeUI,
                                                        inputReceiverNode.NodeUI,
                                                        0,
                                                        inputReceiverData,
                                                        0,
                                                        false));
                }
                else
                {
                    //Connect it to the applier
                    newNodeWorkspace.Connectors.Add(new dynConnector(
                                                        nodeUI,
                                                        curriedNode.InnerNode.NodeUI,
                                                        0,
                                                        0,
                                                        0,
                                                        false));

                    //Connect applier to the inner input receiver
                    newNodeWorkspace.Connectors.Add(new dynConnector(
                                                        curriedNode.InnerNode.NodeUI,
                                                        inputReceiverNode.NodeUI,
                                                        0,
                                                        inputReceiverData,
                                                        0,
                                                        false));
                }
            }

            #endregion

            #region Process outputs

            //List of all inner nodes to connect an output. Unique.
            var outportList = new List<Tuple<dynNode, int>>();

            int i = 0;
            foreach (var output in outputs)
            {
                if (outportList.All(x => !(x.Item1 == output.Item1 && x.Item2 == output.Item2)))
                {
                    dynNode outputSenderNode = output.Item1;
                    int outputSenderData = output.Item2;
                    dynNode outputReceiverNode = output.Item3.Item2;

                    if (curriedNodeArgs.Any(x => x.OuterNode == outputReceiverNode))
                        continue;

                    outportList.Add(Tuple.Create(outputSenderNode, outputSenderData));

                    //Create Symbol Node
                    var node = new dynOutput
                        {
                            Symbol = outputSenderNode.OutPortData[outputSenderData].NickName
                        };

                    dynNodeUI nodeUI = node.NodeUI;

                    var elNameAttrib =
                        node.GetType().GetCustomAttributes(typeof (NodeNameAttribute), false)[0] as NodeNameAttribute;
                    if (elNameAttrib != null)
                    {
                        nodeUI.NickName = elNameAttrib.Name;
                    }

                    nodeUI.GUID = Guid.NewGuid();

                    //store the element in the elements list
                    newNodeWorkspace.Nodes.Add(node);
                    node.WorkSpace = newNodeWorkspace;

                    node.DisableReporting();

                    Bench.WorkBench.Children.Add(nodeUI);

                    //Place it in an appropriate spot
                    Canvas.SetLeft(nodeUI, rightMost + 75 - leftShift);
                    Canvas.SetTop(nodeUI, i*(50 + node.NodeUI.Height));

                    Bench.WorkBench.UpdateLayout();

                    newNodeWorkspace.Connectors.Add(new dynConnector(
                                                        outputSenderNode.NodeUI,
                                                        nodeUI,
                                                        outputSenderData,
                                                        0,
                                                        0,
                                                        false));

                    i++;
                }
            }

            //Connect outputs to new node
            foreach (var output in outputs)
            {
                //Node to be connected to in CurrentSpace
                dynNode outputSenderNode = output.Item1;

                //Port to be connected to on outPutNode_outer
                int outputSenderData = output.Item2;

                int outputReceiverData = output.Item3.Item1;
                dynNode outputReceiverNode = output.Item3.Item2;

                var curriedNode = curriedNodeArgs.FirstOrDefault(
                    x => x.OuterNode == outputReceiverNode);

                if (curriedNode == null)
                {
                    CurrentSpace.Connectors.Add(
                        new dynConnector(
                            collapsedNode.NodeUI,
                            outputReceiverNode.NodeUI,
                            outportList.FindIndex(x => x.Item1 == outputSenderNode && x.Item2 == outputSenderData),
                            outputReceiverData,
                            0,
                            true));
                }
                else
                {
                    int targetPort = curriedNode.Inputs
                                                .First(
                                                    x => x.InnerNodeInputSender == outputSenderNode)
                                                .OuterNodeInPortData;

                    int targetPortIndex = curriedNode.OuterNodePortDataList.IndexOf(targetPort);

                    //Connect it (new dynConnector)
                    newNodeWorkspace.Connectors.Add(new dynConnector(
                                                        outputSenderNode.NodeUI,
                                                        curriedNode.InnerNode.NodeUI,
                                                        outputSenderData,
                                                        targetPortIndex + 1,
                                                        0));
                }
            }

            #endregion

            #region Make new workspace invisible

            //Step 4: make nodes invisible
            // and update positions
            foreach (dynNodeUI node in newNodeWorkspace.Nodes.Select(x => x.NodeUI))
                node.Visibility = Visibility.Hidden;

            foreach (dynConnector connector in newNodeWorkspace.Connectors)
                connector.Visible = false;

            #endregion

            newNodeWorkspace.Nodes.ForEach(x =>
                {
                    x.EnableReporting();
                    x.NodeUI.UpdateConnections();
                });

            collapsedNode.EnableReporting();
            collapsedNode.NodeUI.UpdateConnections();
            
            //set the name on the node
            collapsedNode.NodeUI.NickName = newNodeName;

            CurrentSpace.EnableReporting();

            SaveFunction(newNodeDefinition, true);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Used by various properties to notify observers that a property has changed.
        /// </summary>
        /// <param name="info">What changed.</param>
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
