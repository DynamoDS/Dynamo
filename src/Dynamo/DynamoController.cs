using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Dynamo.Nodes;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.FSchemeInterop;
using System.Windows.Controls;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using Dynamo.Connectors;

using Expression = Dynamo.FScheme.Expression;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Controls;
using Dynamo.Commands;

namespace Dynamo
{
    public class DynamoController
    {
        //TODO: Remove this?
        public Dictionary<string, dynWorkspace> FunctionDict = new Dictionary<string, dynWorkspace>();

        public dynBench Bench { get; private set; }

        public IEnumerable<dynNode> AllNodes
        {
            get
            {
                return this.homeSpace.Nodes.Concat(
                   this.FunctionDict.Values.Aggregate(
                      (IEnumerable<dynNode>)new List<dynNode>(),
                      (a, x) => a.Concat(x.Nodes)
                   )
                );
            }
        }

        SortedDictionary<string, TypeLoadData> builtinTypesByNickname = new SortedDictionary<string, TypeLoadData>();
        public SortedDictionary<string, TypeLoadData> BuiltInTypesByNickname
        {
            get { return builtinTypesByNickname; }
        }

        Dictionary<string, TypeLoadData> builtinTypesByTypeName = new Dictionary<string, TypeLoadData>();

        DynamoSplash splashScreen;
        public DynamoSplash SplashScreen
        {
            get { return splashScreen; }
            set { splashScreen = value; }
        }

        dynWorkspace _cspace;
        internal dynWorkspace CurrentSpace
        {
            get { return _cspace; }
            set
            {
                _cspace = value;
                Bench.CurrentX = _cspace.PositionX;
                Bench.CurrentY = _cspace.PositionY;
                //TODO: Also set the name here.
            }
        }

        protected dynWorkspace homeSpace;

        private string UnlockLoadPath;

        public ExecutionEnvironment FSchemeEnvironment
        {
            get;
            private set;
        }

        public List<dynNode> Nodes
        {
            get { return CurrentSpace.Nodes; }
        }

        public bool ViewingHomespace
        {
            get { return CurrentSpace == homeSpace; }
        }

        #region Constructor and Initialization
        //public DynamoController(SplashScreen splash)
        public DynamoController()
        {
            Bench = new dynBench(this);

            homeSpace = CurrentSpace = new HomeWorkspace();

            Bench.CurrentX = dynBench.CANVAS_OFFSET_X;
            Bench.CurrentY = dynBench.CANVAS_OFFSET_Y;

            Bench.InitializeComponent();
            Bench.Log(String.Format(
                "Dynamo -- Build {0}.",
                Assembly.GetExecutingAssembly().GetName().Version.ToString()));

            dynSettings.Bench = Bench;
            dynSettings.Controller = this;
            dynSettings.Workbench = Bench.WorkBench;

            if (DynamoCommands.ShowSplashScreenCmd.CanExecute(null))
            {
                DynamoCommands.ShowSplashScreenCmd.Execute(null);
            }

            //WTF
            Bench.settings_curves.IsChecked = true;
            Bench.settings_curves.IsChecked = false;

            Bench.LockUI();

            //run tests
            if (FScheme.RunTests(Bench.Log))
                Bench.Log("All Tests Passed. Core library loaded OK.");

            FSchemeEnvironment = new ExecutionEnvironment();

            LoadBuiltinTypes();
            PopulateSamplesMenu();

            Bench.Activated += Bench_Activated;
        }

        private bool _activated = false;
        void Bench_Activated(object sender, EventArgs e)
        {
            if (!this._activated)
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
                Bench.WorkBench.Visibility = System.Windows.Visibility.Visible;

                if (DynamoCommands.CloseSplashScreenCmd.CanExecute(null))
                {
                    DynamoCommands.CloseSplashScreenCmd.Execute(null);
                }

                homeSpace.OnDisplayed();
            }
        }

        
        #endregion

        #region Loading
        internal void QueueLoad(string path)
        {
            UnlockLoadPath = path;
        }

        /// <summary>
        /// Setup the "Add" menu with all available dynElement types.
        /// </summary>
        private void LoadBuiltinTypes()
        {
            //setup the menu with all the types by reflecting
            //the DynamoElements.dll
            Assembly dynamoAssembly = Assembly.GetExecutingAssembly();

            var location = Path.GetDirectoryName(dynamoAssembly.Location);

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

            var path = Path.Combine(location, "Packages");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var allDynamoAssemblyPaths =
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

            foreach (var assemblyPath in allDynamoAssemblyPaths)
            {
                if (allLoadedAssembliesByPath.ContainsKey(assemblyPath))
                    loadNodesFromAssembly(allLoadedAssembliesByPath[assemblyPath]);
                //loadedAssemblies.Add(allLoadedAssemblies[assemblyPath]);
                else
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(assemblyPath);
                        allLoadedAssemblies[assembly.GetName().Name] = assembly;
                        loadNodesFromAssembly(assembly);
                    }
                    catch { }
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve -= resolver;

            //AppDomain.Unload(tempDomain);
            #endregion

            //foreach (var assembly in loadedAssemblies.Concat(assembliesToLoad.Select(Assembly.LoadFile)))
            //    loadNodesFromAssembly(assembly);

            var threads = Process.GetCurrentProcess().Threads; // trying to understand why processor pegs after loading.

            //string pluginsPath = Path.Combine(location, "definitions");

            //if (Directory.Exists(pluginsPath))
            //{
            //    loadUserAssemblies(pluginsPath);
            //}

            #region PopulateUI

            var sortedExpanders = new SortedDictionary<string, Tuple<Expander, SortedList<string, dynNodeUI>>>();

            foreach (KeyValuePair<string, TypeLoadData> kvp in builtinTypesByNickname)
            {
                //if (!kvp.Value.t.Equals(typeof(dynSymbol)))
                //{
                //   System.Windows.Controls.MenuItem mi = new System.Windows.Controls.MenuItem();
                //   mi.Header = kvp.Key;
                //   mi.Click += new RoutedEventHandler(AddElement_Click);
                //   AddMenu.Items.Add(mi);
                //}

                //---------------------//

                var catAtts = kvp.Value.Type.GetCustomAttributes(typeof(NodeCategoryAttribute), false);
                string categoryName;
                if (catAtts.Length > 0)
                {
                    categoryName = ((NodeCategoryAttribute)catAtts[0]).ElementCategory;
                }
                else
                {
                    Bench.Log("No category specified for \"" + kvp.Key + "\"");
                    continue;
                }

                dynNode newNode = null;

                try
                {
                    var obj = Activator.CreateInstance(kvp.Value.Type);
                    //var obj = Activator.CreateInstanceFrom(kvp.Value.assembly.Location, kvp.Value.t.FullName);
                    newNode = (dynNode)obj;//.Unwrap();
                }
                catch (Exception e) //TODO: Narrow down
                {
                    Bench.Log("Error loading \"" + kvp.Key);
                    Bench.Log(e.InnerException);
                    continue;
                }

                try
                {
                    var nodeUI = newNode.NodeUI;

                    nodeUI.DisableInteraction();

                    string name = kvp.Key;

                    //newEl.MouseDoubleClick += delegate { AddElement(name); };

                    nodeUI.MouseDown += delegate
                    {
                        Bench.BeginDragElement(nodeUI, name, Mouse.GetPosition(nodeUI));
                        nodeUI.Visibility = System.Windows.Visibility.Hidden;
                    };

                    nodeUI.GUID = new Guid();
                    nodeUI.Margin = new Thickness(5, 30, 5, 5);

                    var target = Bench.sidebarGrid.Width - 30;
                    var width = nodeUI.ActualWidth != 0 ? nodeUI.ActualWidth : nodeUI.Width;
                    var scale = Math.Min(target / width, .8);

                    nodeUI.LayoutTransform = new ScaleTransform(scale, scale);
                    nodeUI.nickNameBlock.FontSize *= .8 / scale;

                    Tuple<Expander, SortedList<string, dynNodeUI>> expander;

                    if (sortedExpanders.ContainsKey(categoryName))
                    {
                        expander = sortedExpanders[categoryName];
                    }
                    else
                    {
                        var e = new Expander()
                        {
                            Header = categoryName,
                            Height = double.NaN,
                            Margin = new Thickness(0, 5, 0, 0),
                            Content = new WrapPanel()
                            {
                                Height = double.NaN,
                                Width = double.NaN
                            },
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                            //FontWeight = FontWeights.Bold
                        };

                        Bench.addMenuCategoryDict[categoryName] = e;

                        expander = new Tuple<Expander, SortedList<string, dynNodeUI>>(e, new SortedList<string, dynNodeUI>());

                        sortedExpanders[categoryName] = expander;
                    }

                    var sortedElements = expander.Item2;
                    sortedElements.Add(kvp.Key, nodeUI);

                    Bench.addMenuItemsDictNew[kvp.Key] = nodeUI;

                    //--------------//

                    var tagAtts = kvp.Value.Type.GetCustomAttributes(typeof(NodeSearchTagsAttribute), false);
                    List<string> tags = null;
                    if (tagAtts.Length > 0)
                    {
                        tags = ((NodeSearchTagsAttribute)tagAtts[0]).Tags;
                    }

                    if (tags != null)
                    {
                        searchDict.Add(nodeUI, tags.Where(x => x.Length > 0));
                    }

                    searchDict.Add(nodeUI, kvp.Key.Split(' ').Where(x => x.Length > 0));
                    searchDict.Add(nodeUI, kvp.Key);
                }
                catch (Exception e)
                {
                    Bench.Log("Error loading \"" + kvp.Key);
                    Bench.Log(e);
                }
            }

            //Add everything to the menu here
            foreach (var kvp in sortedExpanders)
            {
                var expander = kvp.Value;
                Bench.SideStackPanel.Children.Add(expander.Item1);
                var wp = (WrapPanel)expander.Item1.Content;
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
                t.IsSubclassOf(typeof(dynNode));
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
                    object[] attribs = t.GetCustomAttributes(typeof(NodeNameAttribute), false);

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
                if (e is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = e as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    Bench.Log("Dll Load Exception: " + loaderExceptions[0].ToString());
                    Bench.Log(loaderExceptions[0].ToString());
                    Bench.Log("Dll Load Exception: " + loaderExceptions[1].ToString());
                    Bench.Log(loaderExceptions[1].ToString());
                }
            }
        }

        /// <summary>
        /// Setup the "Samples" sub-menu with contents of samples directory.
        /// </summary>
        void PopulateSamplesMenu()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string samplesPath = Path.Combine(directory, "samples");

            if (System.IO.Directory.Exists(samplesPath))
            {
                string[] dirPaths = Directory.GetDirectories(samplesPath);
                string[] filePaths = Directory.GetFiles(samplesPath, "*.dyn");

                // handle top-level files
                if (filePaths.Any())
                {
                    foreach (string path in filePaths)
                    {
                        var item = new System.Windows.Controls.MenuItem()
                        {
                            Header = Path.GetFileNameWithoutExtension(path),
                            Tag = path
                        };
                        item.Click += new RoutedEventHandler(sample_Click);
                        Bench.SamplesMenu.Items.Add(item);
                    }

                }

                // handle top-level dirs, TODO - factor out to a seperate function, make recusive
                if (dirPaths.Any())
                {
                    foreach (string dirPath in dirPaths)
                    {
                        var dirItem = new System.Windows.Controls.MenuItem()
                        {
                            Header = Path.GetFileName(dirPath),
                            Tag = Path.GetFileName(dirPath)
                        };
                        //item.Click += new RoutedEventHandler(sample_Click);
                        //samplesMenu.Items.Add(dirItem);
                        int menuItemCount = Bench.SamplesMenu.Items.Count;

                        filePaths = Directory.GetFiles(dirPath, "*.dyn");
                        if (filePaths.Any())
                        {
                            foreach (string path in filePaths)
                            {
                                var item = new System.Windows.Controls.MenuItem()
                                {
                                    Header = Path.GetFileNameWithoutExtension(path),
                                    Tag = path
                                };
                                item.Click += new RoutedEventHandler(sample_Click);
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

        void sample_Click(object sender, RoutedEventArgs e)
        {
            var path = (string)((System.Windows.Controls.MenuItem)sender).Tag;

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
        /// Setup the "Add" menu with all available user-defined types.
        /// </summary>
        public void LoadUserTypes()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            if (System.IO.Directory.Exists(pluginsPath))
            {
                Bench.Log("Autoloading definitions...");
                loadUserWorkspaces(pluginsPath);
            }
        }

        private void loadUserWorkspaces(string directory)
        {
            var parentBuffer = new Dictionary<string, HashSet<string>>();
            var childrenBuffer = new Dictionary<string, HashSet<dynWorkspace>>();
            string[] filePaths = Directory.GetFiles(directory, "*.dyf");
            foreach (string filePath in filePaths)
            {
                this.OpenDefinition(filePath, childrenBuffer, parentBuffer);
            }
            foreach (var e in this.AllNodes)
            {
                e.EnableReporting();
            }
        }
        #endregion

        #region Node Initialization
        /// <summary>
        /// This method adds dynElements when opening from a file
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
            System.Windows.Visibility vis = System.Windows.Visibility.Visible)
        {
            try
            {
                //create a new object from a type
                //that is passed in
                //dynElement el = (dynElement)Activator.CreateInstance(elementType, new object[] { nickName });
                dynNode node = (dynNode)Activator.CreateInstance(elementType);

                var nodeUI = node.NodeUI;

                if (!string.IsNullOrEmpty(nickName))
                {
                    nodeUI.NickName = nickName;
                }
                else
                {
                    NodeNameAttribute elNameAttrib = node.GetType().GetCustomAttributes(typeof(NodeNameAttribute), true)[0] as NodeNameAttribute;
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

        internal dynWorkspace NewFunction(string name, string category, bool display)
        {
            //Add an entry to the funcdict
            var workSpace = new FuncWorkspace(name, category, dynBench.CANVAS_OFFSET_X, dynBench.CANVAS_OFFSET_Y);

            var newElements = workSpace.Nodes;
            var newConnectors = workSpace.Connectors;

            this.FunctionDict[name] = workSpace;

            //Add an entry to the View menu
            System.Windows.Controls.MenuItem i = new System.Windows.Controls.MenuItem();
            i.Header = name;
            i.Click += new RoutedEventHandler(Bench.ChangeView_Click);
            Bench.viewMenu.Items.Add(i);
            Bench.viewMenuItemsDict[name] = i;

            //Add an entry to the Add menu
            //System.Windows.Controls.MenuItem mi = new System.Windows.Controls.MenuItem();
            //mi.Header = name;
            //mi.Click += new RoutedEventHandler(AddElement_Click);
            //AddMenu.Items.Add(mi);
            //this.addMenuItemsDict[name] = mi;

            dynFunction newEl = new dynFunction(
               workSpace.Nodes.Where(el => el is dynSymbol)
                  .Select(s => ((dynSymbol)s).Symbol),
               new List<String>() { "out" },
               name
            );

            newEl.NodeUI.DisableInteraction();
            newEl.NodeUI.MouseDown += delegate
            {
                Bench.BeginDragElement(newEl.NodeUI, name, Mouse.GetPosition(newEl.NodeUI));

                newEl.NodeUI.Visibility = System.Windows.Visibility.Hidden;
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
                expander = new Expander()
                {
                    Header = category,
                    Height = double.NaN,
                    Margin = new Thickness(0, 5, 0, 0),
                    Content = new WrapPanel()
                    {
                        Height = double.NaN,
                        Width = 240
                    },
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    //FontWeight = FontWeights.Bold
                };

                Bench.addMenuCategoryDict[category] = expander;

                var sortedExpanders = new SortedList<string, Expander>();
                foreach (Expander child in Bench.SideStackPanel.Children)
                {
                    sortedExpanders.Add((string)child.Header, child);
                }
                sortedExpanders.Add(category, expander);

                Bench.SideStackPanel.Children.Clear();

                foreach (Expander child in sortedExpanders.Values)
                {
                    Bench.SideStackPanel.Children.Add(child);
                }
            }

            var wp = (WrapPanel)expander.Content;

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
            searchDict.Add(newEl.NodeUI, name.Split(' ').Where(x => x.Length > 0));

            if (display)
            {
                //Store old workspace
                //var ws = new dynWorkspace(this.elements, this.connectors, this.CurrentX, this.CurrentY);

                if (!this.ViewingHomespace)
                {
                    //Step 2: Store function workspace in the function dictionary
                    this.FunctionDict[this.CurrentSpace.Name] = this.CurrentSpace;

                    //Step 3: Save function
                    this.SaveFunction(this.CurrentSpace);
                }

                //Make old workspace invisible
                foreach (dynNode dynE in this.Nodes)
                {
                    dynE.NodeUI.Visibility = System.Windows.Visibility.Collapsed;
                }
                foreach (dynConnector dynC in this.CurrentSpace.Connectors)
                {
                    dynC.Visible = false;
                }
                foreach (dynNote note in this.CurrentSpace.Notes)
                {
                    note.Visibility = System.Windows.Visibility.Hidden;
                }

                //this.currentFunctionName = name;

                ////Clear the bench for the new function
                //this.elements = newElements;
                //this.connectors = newConnectors;
                //this.CurrentX = CANVAS_OFFSET_X;
                //this.CurrentY = CANVAS_OFFSET_Y;
                this.CurrentSpace = workSpace;

                //this.saveFuncItem.IsEnabled = true;
                Bench.homeButton.IsEnabled = true;
                //this.varItem.IsEnabled = true;

                Bench.workspaceLabel.Content = this.CurrentSpace.Name;
                Bench.editNameButton.Visibility = System.Windows.Visibility.Visible;
                Bench.editNameButton.IsHitTestVisible = true;
                Bench.setFunctionBackground();
            }

            return workSpace;
        }

        internal dynNode CreateDragNode(string name)
        {
            dynNode result;
            if (FunctionDict.ContainsKey(name))
            {
                dynWorkspace ws = FunctionDict[name];

                var inputs = 
                    ws.Nodes.Where(e => e is dynSymbol)
                        .Select(s => (s as dynSymbol).Symbol);

                var outputs =
                    ws.Nodes.Where(e => e is dynOutput)
                        .Select(o => (o as dynOutput).Symbol);

                if (!outputs.Any())
                {
                    var topMost = new List<Tuple<int, dynNode>>();

                    var topMostNodes = ws.GetTopMostNodes();

                    foreach (var topNode in topMostNodes)
                    {
                        foreach (var output in Enumerable.Range(0, topNode.OutPortData.Count))
                        {
                            if (!topNode.HasOutput(output))
                                topMost.Add(Tuple.Create(output, topNode));
                        }
                    }

                    outputs = topMost.Select(x => x.Item2.OutPortData[x.Item1].NickName);
                }
                
                result = new dynFunction(inputs, outputs, name);
            }
            else
            {
                TypeLoadData tld = builtinTypesByNickname[name];

                var obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
                var newEl = (dynNode)obj.Unwrap();
                newEl.NodeUI.DisableInteraction();
                result = newEl;
            }

            if (result is dynDouble)
                (result as dynDouble).Value = this.storedSearchNum;
            else if (result is dynStringInput)
                (result as dynStringInput).Value = this.storedSearchStr;
            else if (result is dynBool)
                (result as dynBool).Value = this.storedSearchBool;

            return result;
        }
        #endregion

        #region Saving and Opening Workspaces
        internal void SaveAs()
        {
            save(string.Empty);
        }

        internal void Save()
        {
            save(CurrentSpace.FilePath);
        }

        private void save(string xmlPath)
        {
            //string xmlPath = "C:\\test\\myWorkbench.xml";
            //string xmlPath = "";

            //if the incoming path is empty
            //present the user with save options
            if (string.IsNullOrEmpty(xmlPath))
            {
                string ext, fltr;
                if (this.ViewingHomespace)
                {
                    ext = ".dyn";
                    fltr = "Dynamo Workspace (*.dyn)|*.dyn";
                }
                else
                {
                    ext = ".dyf";
                    fltr = "Dynamo Function (*.dyf)|*.dyf";
                }
                fltr += "|All files (*.*)|*.*";

                SaveFileDialog saveDialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    DefaultExt = ext,
                    Filter = fltr,
                };

                //if the xmlPath is not empty set the default directory
                if (!string.IsNullOrEmpty(xmlPath))
                {
                    FileInfo fi = new FileInfo(xmlPath);
                    saveDialog.InitialDirectory = fi.DirectoryName;
                }
                else if (!string.IsNullOrEmpty(CurrentSpace.FilePath))
                {
                    //if you've got the file location of the current
                    //space cached then use its directory 
                    FileInfo fi = new FileInfo(CurrentSpace.FilePath);
                    saveDialog.InitialDirectory = fi.DirectoryName;
                }

                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    xmlPath = saveDialog.FileName;
                    CurrentSpace.FilePath = xmlPath;
                }
            }

            if (!string.IsNullOrEmpty(xmlPath))
            {
                if (!SaveWorkspace(xmlPath, this.CurrentSpace))
                {
                    //MessageBox.Show("Workbench could not be saved.");
                    Bench.Log("Workbench could not be saved.");
                }
            }
        }

        bool SaveWorkspace(string xmlPath, dynWorkspace workSpace)
        {
            Bench.Log("Saving " + xmlPath + "...");
            try
            {
                //create the xml document
                //create the xml document
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.CreateXmlDeclaration("1.0", null, null);

                XmlElement root = xmlDoc.CreateElement("dynWorkspace");  //write the root element
                root.SetAttribute("X", workSpace.PositionX.ToString());
                root.SetAttribute("Y", workSpace.PositionY.ToString());

                if (workSpace != this.homeSpace) //If we are not saving the home space
                {
                    root.SetAttribute("Name", workSpace.Name);
                    root.SetAttribute("Category", ((FuncWorkspace)workSpace).Category);
                }

                xmlDoc.AppendChild(root);

                XmlElement elementList = xmlDoc.CreateElement("dynElements");  //write the root element
                root.AppendChild(elementList);

                foreach (dynNode el in workSpace.Nodes)
                {
                    Point relPoint = el.NodeUI
                        .TransformToAncestor(Bench.WorkBench)
                        .Transform(new Point(0, 0));

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
                XmlElement connectorList = xmlDoc.CreateElement("dynConnectors");  //write the root element
                root.AppendChild(connectorList);

                foreach (dynNode el in workSpace.Nodes)
                {
                    foreach (var port in el.NodeUI.OutPorts)
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
                XmlElement noteList = xmlDoc.CreateElement("dynNotes");  //write the root element
                root.AppendChild(noteList);
                foreach (dynNote n in workSpace.Notes)
                {
                    XmlElement note = xmlDoc.CreateElement(n.GetType().ToString());
                    noteList.AppendChild(note);
                    note.SetAttribute("text", n.noteText.Text);
                    note.SetAttribute("x", Canvas.GetLeft(n).ToString());
                    note.SetAttribute("y", Canvas.GetTop(n).ToString());
                }

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

        public void SaveFunction(dynWorkspace functionWorkspace, bool writeDefinition = true)
        {
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
                }
                catch (Exception e)
                {
                    Bench.Log("Error saving:" + e.GetType());
                    Bench.Log(e);
                }
            }

            try
            {
                var outputs = functionWorkspace.Nodes.Where(x => x is dynOutput);

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
                    var topMostNodes = functionWorkspace.GetTopMostNodes();

                    var outNames = new List<string>();

                    foreach (var topNode in topMostNodes)
                    {
                        foreach (var output in Enumerable.Range(0, topNode.OutPortData.Count))
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

                var variables = functionWorkspace.Nodes.Where(x => x is dynSymbol);
                var inputNames = variables.Select(x => (x as dynSymbol).Symbol);

                INode top;
                var buildDict = new Dictionary<dynNode, Dictionary<int, INode>>();

                if (topMost.Count > 1)
                {
                    InputNode node = new ExternalFunctionNode(
                        FScheme.Value.NewList,
                        Enumerable.Range(0, topMost.Count).Select(x => x.ToString()));

                    var i = 0;
                    foreach (var topNode in topMost)
                    {
                        var inputName = i.ToString();
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
                    var hangingNodes = functionWorkspace.GetTopMostNodes().ToList();
                    foreach (var tNode in hangingNodes.Select((x, index) => new { Index = index, Node = x }))
                    {
                        beginNode.AddInput(tNode.Index.ToString());
                        beginNode.ConnectInput(tNode.Index.ToString(), tNode.Node.Build(buildDict, 0));
                    }
                    beginNode.AddInput(hangingNodes.Count.ToString());
                    beginNode.ConnectInput(hangingNodes.Count.ToString(), top);

                    top = beginNode;
                }

                Expression expression = Utils.MakeAnon(variables.Select(x => x.NodeUI.GUID.ToString()), top.Compile());

                FSchemeEnvironment.DefineSymbol(functionWorkspace.Name, expression);

                //Update existing function nodes which point to this function to match its changes
                foreach (var el in this.AllNodes)
                {
                    if (el is dynFunction)
                    {
                        var node = (dynFunction)el;

                        if (!node.Symbol.Equals(functionWorkspace.Name))
                            continue;

                        node.SetInputs(inputNames);
                        node.SetOutputs(outputNames);
                        el.NodeUI.RegisterAllPorts();
                    }
                }

                //Call OnSave for all saved elements
                foreach (var el in functionWorkspace.Nodes)
                    el.onSave();

                //Update new add menu
                var addItem = (dynFunction)Bench.addMenuItemsDictNew[functionWorkspace.Name].NodeLogic;
                addItem.SetInputs(inputNames);
                addItem.SetOutputs(outputNames);
                addItem.NodeUI.RegisterAllPorts();
                addItem.NodeUI.State = ElementState.DEAD;
            }
            catch (Exception ex)
            {
                Bench.Log(ex.GetType().ToString() + ": " + ex.Message);
            }
        }

        private static string FormatFileName(string filename)
        {
            return RemoveChars(
               filename,
               new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" }
            );
        }

        internal static string RemoveChars(string s, IEnumerable<string> chars)
        {
            foreach (var c in chars)
                s = s.Replace(c, "");
            return s;
        }

        internal bool OpenDefinition(string xmlPath)
        {
            return OpenDefinition(
                xmlPath,
                new Dictionary<string, HashSet<dynWorkspace>>(),
                new Dictionary<string, HashSet<string>>());
        }

        bool OpenDefinition(
            string xmlPath,
            Dictionary<string, HashSet<dynWorkspace>> children,
            Dictionary<string, HashSet<string>> parents)
        {
            try
            {
                #region read xml file

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                string funName = null;
                string category = "";
                double cx = dynBench.CANVAS_OFFSET_X;
                double cy = dynBench.CANVAS_OFFSET_Y;

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
                    }
                }

                //If there is no function name, then we are opening a home definition
                if (funName == null)
                {
                    //View the home workspace, then open the bench file
                    if (!this.ViewingHomespace)
                        ViewHomeWorkspace(); //TODO: Refactor
                    return this.OpenWorkbench(xmlPath);
                }
                else if (this.FunctionDict.ContainsKey(funName))
                {
                    Bench.Log("ERROR: Could not load definition for \"" + funName + "\", a node with this name already exists.");
                    return false;
                }

                Bench.Log("Loading node definition for \"" + funName + "\" from: " + xmlPath);

                //TODO: refactor to include x,y
                var ws = this.NewFunction(
                   funName,
                   category.Length > 0
                      ? category
                      : BuiltinNodeCategories.MISC,
                   false
                );

                ws.PositionX = cx;
                ws.PositionY = cy;

                //this.Log("Opening definition " + xmlPath + "...");

                XmlNodeList elNodes = xmlDoc.GetElementsByTagName("dynElements");
                XmlNodeList cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
                XmlNodeList nNodes = xmlDoc.GetElementsByTagName("dynNotes");

                XmlNode elNodesList = elNodes[0] as XmlNode;
                XmlNode cNodesList = cNodes[0] as XmlNode;
                XmlNode nNodesList = nNodes[0] as XmlNode;

                var dependencies = new Stack<string>();

                #region instantiate nodes
                foreach (XmlNode elNode in elNodesList.ChildNodes)
                {
                    XmlAttribute typeAttrib = elNode.Attributes[0];
                    XmlAttribute guidAttrib = elNode.Attributes[1];
                    XmlAttribute nicknameAttrib = elNode.Attributes[2];
                    XmlAttribute xAttrib = elNode.Attributes[3];
                    XmlAttribute yAttrib = elNode.Attributes[4];

                    string typeName = typeAttrib.Value.ToString();

                    var oldNamespace = "Dynamo.Elements.";
                    if (typeName.StartsWith(oldNamespace))
                        typeName = "Dynamo.Nodes." + typeName.Remove(0, oldNamespace.Length);

                    Guid guid = new Guid(guidAttrib.Value.ToString());
                    string nickname = nicknameAttrib.Value.ToString();

                    double x = Convert.ToDouble(xAttrib.Value.ToString());
                    double y = Convert.ToDouble(yAttrib.Value.ToString());

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

                    dynNode el = AddDynElement(t, nickname, guid, x, y, ws, System.Windows.Visibility.Hidden);

                    if (el == null)
                        return false;

                    el.DisableReporting();
                    el.LoadElement(elNode);

                    if (el is dynFunction)
                    {
                        var fun = el as dynFunction;
                        if (fun.Symbol != ws.Name)
                            dependencies.Push(fun.Symbol);
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

                    Guid guidStart = new Guid(guidStartAttrib.Value.ToString());
                    Guid guidEnd = new Guid(guidEndAttrib.Value.ToString());
                    int startIndex = Convert.ToInt16(intStartAttrib.Value.ToString());
                    int endIndex = Convert.ToInt16(intEndAttrib.Value.ToString());
                    int portType = Convert.ToInt16(portTypeAttrib.Value.ToString());

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

                    if (start != null && end != null && start != end)
                    {
                        dynConnector newConnector = new dynConnector(
                           start.NodeUI, end.NodeUI,
                           startIndex, endIndex,
                           portType, false
                        );

                        ws.Connectors.Add(newConnector);
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

                        string text = textAttrib.Value.ToString();
                        double x = Convert.ToDouble(xAttrib.Value.ToString());
                        double y = Convert.ToDouble(yAttrib.Value.ToString());

                        //dynNote n = Bench.AddNote(text, x, y, ws);
                        //Bench.AddNote(text, x, y, ws);

                        Dictionary<string, object> paramDict = new Dictionary<string, object>();
                        paramDict.Add("x", x);
                        paramDict.Add("y", y);
                        paramDict.Add("text", text);
                        paramDict.Add("workspace", ws);
                        DynamoCommands.AddNoteCmd.Execute(paramDict);
                    }
                }
                #endregion

                foreach (var e in ws.Nodes)
                    e.EnableReporting();

                this.hideWorkspace(ws);

                #endregion

                ws.FilePath = xmlPath;

                bool canLoad = true;

                //For each node this workspace depends on...
                foreach (var dep in dependencies)
                {
                    //If the node hasn't been loaded...
                    if (!FunctionDict.ContainsKey(dep))
                    {
                        canLoad = false;
                        //Dep -> Ws
                        if (children.ContainsKey(dep))
                            children[dep].Add(ws);
                        else
                            children[dep] = new HashSet<dynWorkspace>() { ws };

                        //Ws -> Deps
                        if (parents.ContainsKey(ws.Name))
                            parents[ws.Name].Add(dep);
                        else
                            parents[ws.Name] = new HashSet<string>() { dep };
                    }
                }

                if (canLoad)
                    SaveFunction(ws, false);

                nodeWorkspaceWasLoaded(ws, children, parents);
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

        void nodeWorkspaceWasLoaded(
            dynWorkspace ws,
            Dictionary<string, HashSet<dynWorkspace>> children,
            Dictionary<string, HashSet<string>> parents)
        {
            //If there were some workspaces that depended on this node...
            if (children.ContainsKey(ws.Name))
            {
                //For each workspace...
                foreach (var child in children[ws.Name])
                {
                    //Nodes the workspace depends on
                    var allParents = parents[child.Name];
                    //Remove this workspace, since it's now loaded.
                    allParents.Remove(ws.Name);
                    //If everything the node depends on has been loaded...
                    if (!allParents.Any())
                    {
                        SaveFunction(child, false);
                        nodeWorkspaceWasLoaded(child, children, parents);
                    }
                }
            }
        }

        void hideWorkspace(dynWorkspace ws)
        {
            foreach (var e in ws.Nodes)
                e.NodeUI.Visibility = System.Windows.Visibility.Collapsed;
            foreach (var c in ws.Connectors)
                c.Visible = false;
            foreach (var n in ws.Notes)
                n.Visibility = System.Windows.Visibility.Hidden;
        }

        bool OpenWorkbench(string xmlPath)
        {
            Bench.Log("Opening home workspace " + xmlPath + "...");
            CleanWorkbench();

            try
            {
                #region read xml file

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                foreach (XmlNode node in xmlDoc.GetElementsByTagName("dynWorkspace"))
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("X"))
                            Bench.CurrentX = Convert.ToDouble(att.Value);
                        else if (att.Name.Equals("Y"))
                            Bench.CurrentY = Convert.ToDouble(att.Value);
                    }
                }

                XmlNodeList elNodes = xmlDoc.GetElementsByTagName("dynElements");
                XmlNodeList cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
                XmlNodeList nNodes = xmlDoc.GetElementsByTagName("dynNotes");

                XmlNode elNodesList = elNodes[0] as XmlNode;
                XmlNode cNodesList = cNodes[0] as XmlNode;
                XmlNode nNodesList = nNodes[0] as XmlNode;

                foreach (XmlNode elNode in elNodesList.ChildNodes)
                {
                    XmlAttribute typeAttrib = elNode.Attributes[0];
                    XmlAttribute guidAttrib = elNode.Attributes[1];
                    XmlAttribute nicknameAttrib = elNode.Attributes[2];
                    XmlAttribute xAttrib = elNode.Attributes[3];
                    XmlAttribute yAttrib = elNode.Attributes[4];

                    string typeName = typeAttrib.Value.ToString();
                    Guid guid = new Guid(guidAttrib.Value.ToString());
                    string nickname = nicknameAttrib.Value.ToString();

                    double x = Convert.ToDouble(xAttrib.Value.ToString());
                    double y = Convert.ToDouble(yAttrib.Value.ToString());

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
                       this.CurrentSpace
                    );

                    el.DisableReporting();

                    el.LoadElement(elNode);

                    if (this.ViewingHomespace)
                        el.SaveResult = true;

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

                    Guid guidStart = new Guid(guidStartAttrib.Value.ToString());
                    Guid guidEnd = new Guid(guidEndAttrib.Value.ToString());
                    int startIndex = Convert.ToInt16(intStartAttrib.Value.ToString());
                    int endIndex = Convert.ToInt16(intEndAttrib.Value.ToString());
                    int portType = Convert.ToInt16(portTypeAttrib.Value.ToString());

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
                        dynConnector newConnector = new dynConnector(start.NodeUI, end.NodeUI,
                            startIndex, endIndex, portType);

                        this.CurrentSpace.Connectors.Add(newConnector);
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

                        string text = textAttrib.Value.ToString();
                        double x = Convert.ToDouble(xAttrib.Value.ToString());
                        double y = Convert.ToDouble(yAttrib.Value.ToString());

                        //dynNote n = Bench.AddNote(text, x, y, this.CurrentSpace);
                        //Bench.AddNote(text, x, y, this.CurrentSpace);

                        Dictionary<string, object> paramDict = new Dictionary<string, object>();
                        paramDict.Add("x", x);
                        paramDict.Add("y", y);
                        paramDict.Add("text", text);
                        paramDict.Add("workspace", this.CurrentSpace);
                        DynamoCommands.AddNoteCmd.Execute(paramDict);
                    }
                }
                #endregion

                foreach (var e in this.CurrentSpace.Nodes)
                    e.EnableReporting();

                #endregion

                homeSpace.FilePath = xmlPath;
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
            var elements = this.Nodes.ToList();

            foreach (dynNode el in elements)
            {
                el.DisableReporting();
                try
                {
                    el.Destroy();
                }
                catch { }
            }

            foreach (dynNode el in elements)
            {
                foreach (dynPort p in el.NodeUI.InPorts)
                {
                    for (int i = p.Connectors.Count - 1; i >= 0; i--)
                        p.Connectors[i].Kill();
                }
                foreach (var port in el.NodeUI.OutPorts)
                {
                    for (int i = port.Connectors.Count - 1; i >= 0; i--)
                        port.Connectors[i].Kill();
                }

                dynSettings.Workbench.Children.Remove(el.NodeUI);
            }

            foreach (dynNote n in this.CurrentSpace.Notes)
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
        public bool Running = false;

        public bool RunCancelled
        {
            get;
            protected internal set;
        }

        private bool runAgain = false;

        private bool dynamicRun = false;

        protected bool _debug;
        private bool _showErrors;

        public virtual bool DynamicRunEnabled
        {
            get
            {
                return Bench.dynamicCheckBox.IsEnabled
                   && Bench.debugCheckBox.IsChecked == false
                   && Bench.dynamicCheckBox.IsChecked == true;
            }
        }

        public virtual bool RunInDebug
        {
            get
            {
                return _debug;
            }
        }

        internal void QueueRun()
        {
            this.RunCancelled = true;
            this.runAgain = true;
        }

        public void RunExpression(bool debug, bool showErrors = true)
        {
            //If we're already running, do nothing.
            if (Running)
                return;

            _debug = debug;
            _showErrors = showErrors;

            //TODO: Hack. Might cause things to break later on...
            //Reset Cancel and Rerun flags
            RunCancelled = false;
            runAgain = false;

            //We are now considered running
            Running = true;

            //Set run auto flag
            dynamicRun = !showErrors;

            //Setup background worker
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(EvaluationThread);

            //Disable Run Button
            Bench.Dispatcher.Invoke(new Action(
               delegate { Bench.RunButton.IsEnabled = false; }
            ));

            //Let's start
            worker.RunWorkerAsync();
        }

        protected virtual void EvaluationThread(object s, DoWorkEventArgs args)
        {
            /* Execution Thread */

            //Get our entry points (elements with nothing connected to output)
            var topElements = homeSpace.GetTopMostNodes();

            //Mark the topmost as dirty/clean
            foreach (var topMost in topElements)
                topMost.MarkDirty();

            //TODO: Flesh out error handling
            try
            {
                var topNode = new BeginNode(new List<string>());
                var i = 0;
                var buildDict = new Dictionary<dynNode, Dictionary<int, INode>>();
                foreach (var topMost in topElements)
                {
                    var inputName = i.ToString();
                    topNode.AddInput(inputName);
                    topNode.ConnectInput(inputName, topMost.BuildExpression(buildDict));
                    i++;
                }

                Expression runningExpression = topNode.Compile();

                Run(topElements, runningExpression);
            }
            catch (CancelEvaluationException ex)
            {
                /* Evaluation was cancelled */

                OnRunCancelled(false);
                //this.CancelRun = false; //Reset cancel flag
                this.RunCancelled = true;

                //If we are forcing this, then make sure we don't run again either.
                if (ex.Force)
                    this.runAgain = false;
            }
            catch (Exception ex)
            {
                /* Evaluation has an error */

                //Catch unhandled exception
                if (ex.Message.Length > 0)
                {
                    Bench.Dispatcher.Invoke(new Action(
                        delegate
                        {
                            Bench.Log(ex);
                        }
                    ));
                }

                OnRunCancelled(true);

                //Reset the flags
                this.runAgain = false;
                this.RunCancelled = true;
            }
            finally
            {
                /* Post-evaluation cleanup */

                //Re-enable run button
                Bench.Dispatcher.Invoke(new Action(
                   delegate
                   {
                       Bench.RunButton.IsEnabled = true;
                   }
                ));

                //No longer running
                this.Running = false;

                //If we should run again...
                if (this.runAgain)
                {
                    //Reset flag
                    this.runAgain = false;

                    //Run this method again from the main thread
                    Bench.Dispatcher.BeginInvoke(new Action(
                       delegate
                       {
                           RunExpression(_debug, _showErrors);
                       }
                    ));
                }
            }
        }

        protected internal virtual void Run(IEnumerable<dynNode> topElements, Expression runningExpression)
        {
            //Print some stuff if we're in debug mode
            if (_debug)
            {
                //string exp = FScheme.print(runningExpression);
                Bench.Dispatcher.Invoke(new Action(
                   delegate
                   {
                       foreach (var node in topElements)
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
                var expr = FSchemeEnvironment.Evaluate(runningExpression);

                //Print some more stuff if we're in debug mode
                if (_debug && expr != null)
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
                    this.runAgain = false;
            }
            catch (Exception ex)
            {
                /* Evaluation failed due to error */

                //Print unhandled exception
                if (ex.Message.Length > 0)
                {
                    Bench.Dispatcher.Invoke(new Action(
                       delegate
                       {
                           Bench.Log(ex);
                       }
                    ));
                }
                OnRunCancelled(true);
                this.RunCancelled = true;
                this.runAgain = false;
            }

            OnEvaluationCompleted();
        }

        protected virtual void OnRunCancelled(bool error)
        {

        }

        protected virtual void OnEvaluationCompleted()
        {

        }

        internal void ShowElement(dynNode e)
        {
            if (dynamicRun)
                return;

            if (!this.Nodes.Contains(e))
            {
                if (this.homeSpace != null && this.homeSpace.Nodes.Contains(e))
                {
                    //Show the homespace
                    ViewHomeWorkspace();
                }
                else
                {
                    foreach (var funcPair in this.FunctionDict)
                    {
                        if (funcPair.Value.Nodes.Contains(e))
                        {
                            DisplayFunction(funcPair.Key);
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
            foreach (var ele in this.Nodes)
            {
                ele.NodeUI.Visibility = System.Windows.Visibility.Collapsed;
            }
            foreach (var con in this.CurrentSpace.Connectors)
            {
                con.Visible = false;
            }
            foreach (var note in this.CurrentSpace.Notes)
            {
                note.Visibility = System.Windows.Visibility.Hidden;
            }
            //var ws = new dynWorkspace(this.elements, this.connectors, this.CurrentX, this.CurrentY);

            //Step 2: Store function workspace in the function dictionary
            this.FunctionDict[this.CurrentSpace.Name] = this.CurrentSpace;

            //Step 3: Save function
            this.SaveFunction(this.CurrentSpace);

            //Step 4: Make home workspace visible
            //this.elements = this.homeSpace.elements;
            //this.connectors = this.homeSpace.connectors;
            //this.CurrentX = this.homeSpace.savedX;
            //this.CurrentY = this.homeSpace.savedY;
            this.CurrentSpace = this.homeSpace;

            foreach (var ele in this.Nodes)
            {
                ele.NodeUI.Visibility = System.Windows.Visibility.Visible;
            }
            foreach (var con in this.CurrentSpace.Connectors)
            {
                con.Visible = true;
            }
            foreach (var note in this.CurrentSpace.Notes)
            {
                note.Visibility = System.Windows.Visibility.Visible;
            }

            //this.saveFuncItem.IsEnabled = false;
            Bench.homeButton.IsEnabled = false;
            //this.varItem.IsEnabled = false;

            Bench.workspaceLabel.Content = "Home";
            Bench.editNameButton.Visibility = System.Windows.Visibility.Collapsed;
            Bench.editNameButton.IsHitTestVisible = false;

            Bench.setHomeBackground();

            CurrentSpace.OnDisplayed();
        }

        internal void DisplayFunction(string symbol)
        {
            if (!this.FunctionDict.ContainsKey(symbol) || this.CurrentSpace.Name.Equals(symbol))
                return;

            var newWs = this.FunctionDict[symbol];

            //Make sure we aren't dragging
            Bench.WorkBench.isDragInProgress = false;
            Bench.WorkBench.ignoreClick = true;

            //Step 1: Make function workspace invisible
            foreach (var ele in this.Nodes)
            {
                ele.NodeUI.Visibility = System.Windows.Visibility.Collapsed;
            }
            foreach (var con in this.CurrentSpace.Connectors)
            {
                con.Visible = false;
            }
            foreach (var note in this.CurrentSpace.Notes)
            {
                note.Visibility = System.Windows.Visibility.Hidden;
            }
            //var ws = new dynWorkspace(this.elements, this.connectors, this.CurrentX, this.CurrentY);

            if (!this.ViewingHomespace)
            {
                //Step 2: Store function workspace in the function dictionary
                this.FunctionDict[this.CurrentSpace.Name] = this.CurrentSpace;

                //Step 3: Save function
                this.SaveFunction(this.CurrentSpace);
            }

            //Step 4: Make home workspace visible
            //this.elements = newWs.elements;
            //this.connectors = newWs.connectors;
            //this.CurrentX = newWs.savedX;
            //this.CurrentY = newWs.savedY;
            this.CurrentSpace = newWs;

            foreach (var ele in this.Nodes)
            {
                ele.NodeUI.Visibility = System.Windows.Visibility.Visible;
            }
            foreach (var con in this.CurrentSpace.Connectors)
            {
                con.Visible = true;
            }

            foreach (var note in this.CurrentSpace.Notes)
            {
                note.Visibility = System.Windows.Visibility.Visible;
            }

            //this.saveFuncItem.IsEnabled = true;
            Bench.homeButton.IsEnabled = true;
            //this.varItem.IsEnabled = true;

            Bench.workspaceLabel.Content = symbol;
            Bench.editNameButton.Visibility = System.Windows.Visibility.Visible;
            Bench.editNameButton.IsHitTestVisible = true;

            Bench.setFunctionBackground();

            CurrentSpace.OnDisplayed();
        }

        #endregion

        #region Updating Nodes
        internal void SaveNameEdit()
        {
            var newName = Bench.editNameBox.Text;

            if (FunctionDict.ContainsKey(newName))
            {
                Bench.Log("ERROR: Cannot rename to \"" + newName + "\", node with same name already exists.");
                return;
            }

            Bench.workspaceLabel.Content = Bench.editNameBox.Text;

            //Update view menu
            var viewItem = Bench.viewMenuItemsDict[CurrentSpace.Name];
            viewItem.Header = newName;
            Bench.viewMenuItemsDict.Remove(CurrentSpace.Name);
            Bench.viewMenuItemsDict[newName] = viewItem;

            //Update add menu
            //var addItem = this.addMenuItemsDict[this.currentFunctionName];
            //addItem.Header = newName;
            //this.addMenuItemsDict.Remove(this.currentFunctionName);
            //this.addMenuItemsDict[newName] = addItem;

            //------------------//

            var newAddItem = (dynFunction)Bench.addMenuItemsDictNew[this.CurrentSpace.Name].NodeLogic;
            if (newAddItem.NodeUI.NickName.Equals(this.CurrentSpace.Name))
                newAddItem.NodeUI.NickName = newName;
            newAddItem.Symbol = newName;
            Bench.addMenuItemsDictNew.Remove(this.CurrentSpace.Name);
            Bench.addMenuItemsDictNew[newName] = newAddItem.NodeUI;

            //Sort the menu after a rename
            Expander unsorted = Bench.addMenuCategoryDict.Values.FirstOrDefault(
               ex => ((WrapPanel)ex.Content).Children.Contains(newAddItem.NodeUI)
            );

            var wp = (WrapPanel)unsorted.Content;

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
            var oldTags = this.CurrentSpace.Name.Split(' ').Where(x => x.Length > 0);
            this.searchDict.Remove(newAddItem.NodeUI, oldTags);

            var newTags = newName.Split(' ').Where(x => x.Length > 0);
            this.searchDict.Add(newAddItem.NodeUI, newTags);

            //------------------//

            //Update existing function nodes
            foreach (var el in this.AllNodes)
            {
                if (el is dynFunction)
                {
                    var node = (dynFunction)el;

                    if (!node.Symbol.Equals(this.CurrentSpace.Name))
                        continue;

                    node.Symbol = newName;

                    //Rename nickname only if it's still referring to the old name
                    if (node.NodeUI.NickName.Equals(this.CurrentSpace.Name))
                        node.NodeUI.NickName = newName;
                }
            }

            FSchemeEnvironment.RemoveSymbol(this.CurrentSpace.Name);

            //TODO: Delete old stored definition
            string directory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = System.IO.Path.Combine(directory, "definitions");

            if (Directory.Exists(pluginsPath))
            {
                string oldpath = System.IO.Path.Combine(pluginsPath, this.CurrentSpace.Name + ".dyf");
                if (File.Exists(oldpath))
                {
                    string newpath = FormatFileName(
                       System.IO.Path.Combine(pluginsPath, newName + ".dyf")
                    );

                    File.Move(oldpath, newpath);
                }
            }

            //Update function dictionary
            var tmp = this.FunctionDict[this.CurrentSpace.Name];
            this.FunctionDict.Remove(this.CurrentSpace.Name);
            this.FunctionDict[newName] = tmp;

            ((FuncWorkspace)this.CurrentSpace).Name = newName;

            this.SaveFunction(this.CurrentSpace);
        }
        #endregion

        #region Searching
        SearchDictionary<dynNodeUI> searchDict = new SearchDictionary<dynNodeUI>();

        internal void filterCategory(HashSet<dynNodeUI> elements, Expander ex)
        {
            var content = (WrapPanel)ex.Content;

            bool filterWholeCategory = true;

            foreach (dynNodeUI ele in content.Children)
            {
                if (!elements.Contains(ele))
                {
                    ele.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    ele.Visibility = System.Windows.Visibility.Visible;
                    filterWholeCategory = false;
                }
            }

            if (filterWholeCategory)
            {
                ex.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ex.Visibility = System.Windows.Visibility.Visible;

                //if (filter.Length > 0)
                //   ex.IsExpanded = true;
            }
        }

        private static Regex searchBarNumRegex = new Regex(@"^-?\d+(\.\d*)?$");
        private static Regex searchBarStrRegex = new Regex("^\"([^\"]*)\"?$");
        private double storedSearchNum = 0;
        private string storedSearchStr = "";
        private bool storedSearchBool = false;

        internal void UpdateSearch(string search)
        {
            Match m;

            if (searchBarNumRegex.IsMatch(search))
            {
                storedSearchNum = Convert.ToDouble(search);
                Bench.FilterAddMenu(
                   new HashSet<dynNodeUI>() 
                   { 
                      Bench.addMenuItemsDictNew["Number"], 
                      Bench.addMenuItemsDictNew["Number Slider"] 
                   }
                );
            }
            else if ((m = searchBarStrRegex.Match(search)).Success)  //(search.StartsWith("\""))
            {
                storedSearchStr = m.Groups[1].Captures[0].Value;
                Bench.FilterAddMenu(
                   new HashSet<dynNodeUI>()
                   {
                      Bench.addMenuItemsDictNew["String"]
                   }
                );
            }
            else if (search.Equals("true") || search.Equals("false"))
            {
                storedSearchBool = Convert.ToBoolean(search);
                Bench.FilterAddMenu(
                   new HashSet<dynNodeUI>()
                   {
                      Bench.addMenuItemsDictNew["Boolean"]
                   }
                );
            }
            else
            {
                this.storedSearchNum = 0;
                this.storedSearchStr = "";
                this.storedSearchBool = false;

                var filter = search.Length == 0
                   ? new HashSet<dynNodeUI>(Bench.addMenuItemsDictNew.Values)
                   : searchDict.Search(search.ToLower());

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

                if (FunctionDict.ContainsKey(newNodeName))
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
            }
            while (!error.Equals(""));

            var newNodeWorkspace = NewFunction(newNodeName, newNodeCategory, false);
            #endregion

            CurrentSpace.DisableReporting();

            #region UI Positioning Calculations
            var avgX = selectedNodeSet.Average(node => Canvas.GetLeft(node.NodeUI));
            var avgY = selectedNodeSet.Average(node => Canvas.GetTop(node.NodeUI));

            var leftMost = selectedNodeSet.Min(node => Canvas.GetLeft(node.NodeUI));
            var topMost = selectedNodeSet.Min(node => Canvas.GetTop(node.NodeUI));
            var rightMost = selectedNodeSet.Max(node => Canvas.GetLeft(node.NodeUI) + node.NodeUI.Width);
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

                        var nodeUI = node.NodeUI;

                        NodeNameAttribute elNameAttrib = node.GetType().GetCustomAttributes(typeof(NodeNameAttribute), true)[0] as NodeNameAttribute;
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
                        var inPortsConnected = Enumerable.Range(0, outerNode.InPortData.Count)
                            .Where(x => outerNode.HasInput(x) && selectedNodeSet.Contains(outerNode.Inputs[x].Item2))
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

            var leftShift = leftMost - 250;
            foreach (var node in newNodeWorkspace.Nodes.Select(x => x.NodeUI))
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
                newNodeName);

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
            foreach (var connector in CurrentSpace.Connectors
                .Where(c => selectedNodeSet.Contains(c.Start.Owner.NodeLogic) && !selectedNodeSet.Contains(c.End.Owner.NodeLogic)).ToList())
            {
                connector.Kill();
            }

            foreach (var connector in CurrentSpace.Connectors
                .Where(c => !selectedNodeSet.Contains(c.Start.Owner.NodeLogic) && selectedNodeSet.Contains(c.End.Owner.NodeLogic)).ToList())
            {
                connector.Kill();
            }
            #endregion

            newNodeWorkspace.Nodes.ForEach(x => x.DisableReporting());

            #region Process inputs
            //Step 3: insert variables (reference step 1)
            foreach (var input in Enumerable.Range(0, inputs.Count).Zip(inputs, Tuple.Create))
            {
                var inputIndex = input.Item1;
                
                var inputReceiverNode = input.Item2.Item1;
                var inputReceiverData = input.Item2.Item2;

                var inputNode = input.Item2.Item3.Item2;
                var inputData = input.Item2.Item3.Item1;
                
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
                dynSymbol node = new dynSymbol()
                {
                    Symbol = inputReceiverNode.InPortData[inputReceiverData].NickName
                };

                var nodeUI = node.NodeUI;

                NodeNameAttribute elNameAttrib = node.GetType().GetCustomAttributes(typeof(NodeNameAttribute), true)[0] as NodeNameAttribute;
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
                Canvas.SetTop(nodeUI, inputIndex * (50 + node.NodeUI.Height));

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
                    var outputSenderNode = output.Item1;
                    var outputSenderData = output.Item2;
                    var outputReceiverNode = output.Item3.Item2;

                    if (curriedNodeArgs.Any(x => x.OuterNode == outputReceiverNode))
                        continue;

                    outportList.Add(Tuple.Create(outputSenderNode, outputSenderData));

                    //Create Symbol Node
                    var node = new dynOutput()
                    {
                        Symbol = outputSenderNode.OutPortData[outputSenderData].NickName
                    };

                    var nodeUI = node.NodeUI;

                    NodeNameAttribute elNameAttrib = node.GetType().GetCustomAttributes(typeof(NodeNameAttribute), false)[0] as NodeNameAttribute;
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
                    Canvas.SetTop(nodeUI, i * (50 + node.NodeUI.Height));

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
                var outputSenderNode = output.Item1;

                //Port to be connected to on outPutNode_outer
                var outputSenderData = output.Item2;

                var outputReceiverData = output.Item3.Item1;
                var outputReceiverNode = output.Item3.Item2;

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
                    var targetPort = curriedNode.Inputs
                        .First(
                            x => x.InnerNodeInputSender == outputSenderNode)
                        .OuterNodeInPortData;

                    var targetPortIndex = curriedNode.OuterNodePortDataList.IndexOf(targetPort);

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
            foreach (var node in newNodeWorkspace.Nodes.Select(x => x.NodeUI))
                node.Visibility = Visibility.Hidden;

            foreach (var connector in newNodeWorkspace.Connectors)
                connector.Visible = false;
            #endregion

            newNodeWorkspace.Nodes.ForEach(x => { x.EnableReporting(); x.NodeUI.UpdateConnections(); });

            collapsedNode.EnableReporting();
            collapsedNode.NodeUI.UpdateConnections();

            CurrentSpace.EnableReporting();

            SaveFunction(newNodeWorkspace, true);
        }
        #endregion
        
    }

}
