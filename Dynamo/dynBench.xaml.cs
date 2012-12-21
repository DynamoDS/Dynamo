//Copyright 2012 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using Dynamo.Connectors;
using Dynamo.Elements;
using Dynamo.FSchemeInterop;
using Dynamo.Utilities;
using FailureHandlingOptions = Autodesk.Revit.DB.FailureHandlingOptions;
using Transaction = Autodesk.Revit.DB.Transaction;
using TransactionStatus = Autodesk.Revit.DB.TransactionStatus;
using Path = System.IO.Path;
using Expression = Dynamo.FScheme.Expression;
using System.Text.RegularExpressions;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for DynamoForm.xaml
    /// </summary>
    public partial class dynBench : Window, INotifyPropertyChanged
    {
        private const int CANVAS_OFFSET_Y = 55;
        private const int CANVAS_OFFSET_X = 10;

        double zoom = 1.0;
        double newX = 0.0;
        double newY = 0.0;
        double oldY = 0.0;
        double oldX = 0.0;

        dynSelection selectedElements;
        bool isConnecting = false;
        dynConnector activeConnector;
        List<DependencyObject> hitResultsList = new List<DependencyObject>();
        bool isPanning = false;
        StringWriter sw;
        string logText;

        dynWorkspace _cspace;
        internal dynWorkspace CurrentSpace
        {
            get { return _cspace; }
            set
            {
                _cspace = value;
                this.CurrentX = _cspace.PositionX;
                this.CurrentY = _cspace.PositionY;
                //TODO: Also set the name here.
            }
        }

        dynWorkspace homeSpace;
        public Dictionary<string, dynWorkspace> dynFunctionDict = new Dictionary<string, dynWorkspace>();

        public dynToolFinder toolFinder;
        public event PropertyChangedEventHandler PropertyChanged;

        SortedDictionary<string, TypeLoadData> builtinTypes = new SortedDictionary<string, TypeLoadData>();

        SplashScreen splashScreen;

        public dynBench(DynamoUpdater updater, SplashScreen splash)
        {
            this.splashScreen = splash;

            this.Updater = updater;

            this.homeSpace = this.CurrentSpace = new HomeWorkspace();

            InitializeComponent();

            LockUI();

            sw = new StringWriter();
            Log(String.Format("Dynamo -- Build {0}.", Assembly.GetExecutingAssembly().GetName().Version.ToString()));

            dynElementSettings.SharedInstance.Workbench = workBench;
            dynElementSettings.SharedInstance.Bench = this;

            //run tests, also load core library
            bool wasError = false;
            FScheme.test(
               delegate(string s)
               {
                   wasError = true;
                   Log(s);
               }
            );
            if (!wasError)
                Log("All Tests Passed. Core library loaded OK.");

            this.Environment = new ExecutionEnvironment();

            selectedElements = new dynSelection();

            this.CurrentX = CANVAS_OFFSET_X;
            this.CurrentY = CANVAS_OFFSET_Y;

            LoadBuiltinTypes();
            PopulateSamplesMenu();
            //LoadUserTypes();
        }

        private bool _activated = false;
        protected override void OnActivated(EventArgs e)
        {
            if (!this._activated)
            {
                this._activated = true;

                LoadUserTypes();
                Log("Welcome to Dynamo!");

                if (this.UnlockLoadPath != null && !this.OpenWorkbench(this.UnlockLoadPath))
                {
                    //MessageBox.Show("Workbench could not be opened.");
                    Log("Workbench could not be opened.");

                    dynElementSettings.SharedInstance.Writer.WriteLine("Workbench could not be opened.");
                    dynElementSettings.SharedInstance.Writer.WriteLine(this.UnlockLoadPath);
                }

                this.UnlockLoadPath = null;

                UnlockUI();

                this.workBench.Visibility = System.Windows.Visibility.Visible;

                this.splashScreen.Close(TimeSpan.FromMilliseconds(100));
            }
        }

        void LockUI()
        {
            this.uiLocked = true;
            this.saveButton.IsEnabled = false;
            this.clearButton.IsEnabled = false;

            this.overlayCanvas.IsHitTestVisible = true;
            this.overlayCanvas.Cursor = System.Windows.Input.Cursors.AppStarting;
            this.overlayCanvas.ForceCursor = true;

            //this.workBench.Visibility = System.Windows.Visibility.Hidden;
        }

        void UnlockUI()
        {
            this.uiLocked = false;
            this.saveButton.IsEnabled = true;
            this.clearButton.IsEnabled = true;

            this.overlayCanvas.IsHitTestVisible = false;
            this.overlayCanvas.Cursor = null;
            this.overlayCanvas.ForceCursor = false;

            //this.workBench.Visibility = System.Windows.Visibility.Visible;
        }

        public IEnumerable<dynNode> AllElements
        {
            get
            {
                return this.homeSpace.Elements.Concat(
                   this.dynFunctionDict.Values.Aggregate(
                      (IEnumerable<dynNode>)new List<dynNode>(),
                      (a, x) => a.Concat(x.Elements)
                   )
                );
            }
        }

        /// <summary>
        /// Used by various properties to notify observers that a property has changed.
        /// </summary>
        /// <param name="info">What changed.</param>
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public string LogText
        {
            get { return logText; }
            set
            {
                logText = value;
                NotifyPropertyChanged("LogText");
            }
        }

        public double Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;
                NotifyPropertyChanged("Zoom");
            }
        }

        public double CurrentX
        {
            get { return this.CurrentSpace.PositionX; }
            set
            {
                this.CurrentSpace.PositionX = Math.Min(CANVAS_OFFSET_X, value);
                NotifyPropertyChanged("CurrentX");
            }
        }

        public double CurrentY
        {
            get { return this.CurrentSpace.PositionY; }
            set
            {
                this.CurrentSpace.PositionY = Math.Min(CANVAS_OFFSET_Y, value);
                NotifyPropertyChanged("CurrentY");
            }
        }

        //public double ZoomCenterX
        //{
        //   get { return (this.CurrentX * -1) + ((this.outerCanvas.ActualWidth / 2) / this.Zoom); }
        //}

        //public double ZoomCenterY
        //{
        //   get { return (this.CurrentY * -1) + ((this.outerCanvas.ActualHeight / 2) / this.Zoom); }
        //}

        public List<dynNode> Elements
        {
            get { return this.CurrentSpace.Elements; }
        }

        public dynSelection SelectedElements
        {
            get { return selectedElements; }
            set { selectedElements = value; }
        }

        public bool ViewingHomespace
        {
            get { return this.CurrentSpace == this.homeSpace; }
        }

        dynNode draggedElement;
        Point dragOffset;

        /// <summary>
        /// Setup the "Add" menu with all available dynElement types.
        /// </summary>
        private void LoadBuiltinTypes()
        {
            //setup the menu with all the types by reflecting
            //the DynamoElements.dll
            Assembly elementsAssembly = Assembly.GetExecutingAssembly();


            //try getting the element types via reflection. 
            // MDJ - I wrapped this in a try-catch as we were having problems with an 
            // external dll (MIConvexHullPlugin.dll) not loading correctly from \dynamo\packages 
            // because the dll did not have a strong name by default and was not loaded into the GAC.
            // The exceptions are now caught but if there is an exception no built-in types are loaded.
            // TODO - move the try catch inside the for loop if possible to not fail all loads. this could slow down load times though.

            try
            {
                Type[] loadedTypes = elementsAssembly.GetTypes();

                foreach (Type t in loadedTypes)
                {
                    //only load types that are in the right namespace, are not abstract
                    //and have the elementname attribute
                    object[] attribs = t.GetCustomAttributes(typeof(ElementNameAttribute), false);

                    if (t.Namespace == "Dynamo.Elements" &&
                        !t.IsAbstract &&
                        attribs.Length > 0 &&
                        t.IsSubclassOf(typeof(dynNode)))
                    {
                        string typeName = (attribs[0] as ElementNameAttribute).ElementName;
                        builtinTypes.Add(typeName, new TypeLoadData(elementsAssembly, t));
                    }
                }
            }
            catch (Exception e)
            {
                dynElementSettings.SharedInstance.Bench.Log( "Could not load types. " + e.ToString());
                Log(e);
                if (e is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = e as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    dynElementSettings.SharedInstance.Bench.Log("Dll Load Exception: " + loaderExceptions[0].ToString());
                    Log(loaderExceptions[0].ToString());
                    dynElementSettings.SharedInstance.Bench.Log("Dll Load Exception: " + loaderExceptions[1].ToString());
                    Log(loaderExceptions[1].ToString());
                }

            }

            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            if (Directory.Exists(pluginsPath))
            {
                loadUserAssemblies(pluginsPath);
            }

            #region PopulateUI

            var sortedExpanders = new SortedDictionary<string, Tuple<Expander, SortedList<string, dynNode>>>();

            foreach (KeyValuePair<string, TypeLoadData> kvp in builtinTypes)
            {
                //if (!kvp.Value.t.Equals(typeof(dynSymbol)))
                //{
                //   System.Windows.Controls.MenuItem mi = new System.Windows.Controls.MenuItem();
                //   mi.Header = kvp.Key;
                //   mi.Click += new RoutedEventHandler(AddElement_Click);
                //   AddMenu.Items.Add(mi);
                //}

                //---------------------//

                var catAtts = kvp.Value.t.GetCustomAttributes(typeof(ElementCategoryAttribute), false);
                string categoryName;
                if (catAtts.Length > 0)
                {
                    categoryName = ((ElementCategoryAttribute)catAtts[0]).ElementCategory;
                }
                else
                {
                    Log("No category specified for \"" + kvp.Key + "\"");
                    continue;
                }

                dynNode newEl = null;

                try
                {
                    var obj = Activator.CreateInstance(kvp.Value.t);
                    //var obj = Activator.CreateInstanceFrom(kvp.Value.assembly.Location, kvp.Value.t.FullName);
                    newEl = (dynNode)obj;//.Unwrap();
                }
                catch (Exception e) //TODO: Narrow down
                {
                    Log("Error loading \"" + kvp.Key);
                    Log(e.InnerException);
                    continue;
                }

                try
                {
                    newEl.DisableInteraction();

                    string name = kvp.Key;

                    //newEl.MouseDoubleClick += delegate { AddElement(name); };

                    newEl.MouseDown += delegate
                    {
                        draggedElementMenuItem = newEl;
                        BeginDragElement(name, Mouse.GetPosition(newEl));
                        newEl.Visibility = System.Windows.Visibility.Hidden;
                    };

                    newEl.GUID = new Guid();
                    newEl.Margin = new Thickness(5, 30, 5, 5);

                    var target = this.sidebarGrid.Width - 30;
                    var width = newEl.ActualWidth != 0 ? newEl.ActualWidth : newEl.Width;
                    var scale = Math.Min(target / width, .8);

                    newEl.LayoutTransform = new ScaleTransform(scale, scale);
                    newEl.nickNameBlock.FontSize *= .8 / scale;

                    Tuple<Expander, SortedList<string, dynNode>> expander;

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

                        addMenuCategoryDict[categoryName] = e;

                        expander = new Tuple<Expander, SortedList<string, dynNode>>(e, new SortedList<string, dynNode>());

                        sortedExpanders[categoryName] = expander;
                    }

                    var sortedElements = expander.Item2;
                    sortedElements.Add(kvp.Key, newEl);

                    addMenuItemsDictNew[kvp.Key] = newEl;

                    //--------------//

                    var tagAtts = kvp.Value.t.GetCustomAttributes(typeof(ElementSearchTagsAttribute), false);
                    List<string> tags = null;
                    if (tagAtts.Length > 0)
                    {
                        tags = ((ElementSearchTagsAttribute)tagAtts[0]).Tags;
                    }

                    if (tags != null)
                    {
                        searchDict.Add(newEl, tags.Where(x => x.Length > 0));
                    }

                    searchDict.Add(newEl, kvp.Key.Split(' ').Where(x => x.Length > 0));
                    searchDict.Add(newEl, kvp.Key);
                }
                catch (Exception e)
                {
                    Log("Error loading \"" + kvp.Key);
                    Log(e);
                }
            }

            //Add everything to the menu here
            foreach (var kvp in sortedExpanders)
            {
                var expander = kvp.Value;
                this.stackPanel1.Children.Add(expander.Item1);
                var wp = (WrapPanel)expander.Item1.Content;
                foreach (dynNode e in expander.Item2.Values)
                {
                    wp.Children.Add(e);
                }
            }

            #endregion
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
                        samplesMenu.Items.Add(item);
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
                        int menuItemCount = samplesMenu.Items.Count;

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
                        samplesMenu.Items.Add(dirItem);
                        
                    }
                    return;
                    
                }
            } 
            //this.fileMenu.Items.Remove(this.samplesMenu);
        }

        void sample_Click(object sender, RoutedEventArgs e)
        {
            var path = (string)((System.Windows.Controls.MenuItem)sender).Tag;

            if (this.uiLocked)
                this.QueueLoad(path);
            else
            {
                if (!this.ViewingHomespace)
                    this.Home_Click(null, null);

                this.OpenWorkbench(path);
            }
        }

        private void QueueLoad(string path)
        {
            this.UnlockLoadPath = path;
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
                Log("Autoloading definitions...");
                loadUserWorkspaces(pluginsPath);
            }
        }

        private void loadUserWorkspaces(string directory)
        {
            string[] filePaths = Directory.GetFiles(directory, "*.dyf");
            foreach (string filePath in filePaths)
            {
                this.OpenDefinition(filePath);
            }
            foreach (var e in this.AllElements)
            {
                e.EnableReporting();
            }
        }

        private void loadUserAssemblies(string directory)
        {
            string[] filePaths = Directory.GetFiles(directory, "*.dll");
            foreach (string filePath in filePaths)
            {
                Assembly currAss = Assembly.LoadFrom(filePath);
                Type[] loadedTypes = currAss.GetTypes();
                foreach (Type t in loadedTypes)
                {
                    //only load types that are in the right namespace, are not abstract
                    //and have the elementname attribute
                    object[] attribs = t.GetCustomAttributes(typeof(ElementNameAttribute), false);

                    if (t.Namespace == "Dynamo.Elements" &&
                        !t.IsAbstract &&
                        attribs.Length > 0 &&
                        t.IsSubclassOf(typeof(dynNode)))
                    {
                        string typeName = (attribs[0] as ElementNameAttribute).ElementName;
                        //System.Windows.Controls.MenuItem mi = new System.Windows.Controls.MenuItem();
                        //mi.Header = typeName;
                        //mi.Click += new RoutedEventHandler(AddElement_Click);
                        //AddMenu.Items.Add(mi);

                        builtinTypes.Add(typeName, new TypeLoadData(currAss, t)); //TODO: this was once usertypes
                    }
                }
            }


        }

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
                dynNode el = (dynNode)Activator.CreateInstance(elementType);

                if (!string.IsNullOrEmpty(nickName))
                {
                    el.NickName = nickName;
                }
                else
                {
                    ElementNameAttribute elNameAttrib = el.GetType().GetCustomAttributes(typeof(ElementNameAttribute), true)[0] as ElementNameAttribute;
                    if (elNameAttrib != null)
                    {
                        el.NickName = elNameAttrib.ElementName;
                    }
                }

                el.GUID = guid;

                string name = el.NickName;

                //store the element in the elements list
                ws.Elements.Add(el);
                el.WorkSpace = ws;

                el.Visibility = vis;

                this.workBench.Children.Add(el);

                Canvas.SetLeft(el, x);
                Canvas.SetTop(el, y);

                //create an event on the element itself
                //to update the elements ports and connectors
                el.PreviewMouseRightButtonDown += new MouseButtonEventHandler(UpdateElement);

                return el;
            }
            catch (Exception e)
            {
                dynElementSettings.SharedInstance.Bench.Log(
                    "Could not create an instance of the selected type: " + elementType
                );
                Log(e);
                return null;
            }
        }

        public dynNote AddNote(string noteText, double x, double y, dynWorkspace workspace)
        {
            dynNote n = new dynNote();
            Canvas.SetLeft(n, x);
            Canvas.SetTop(n, y);
            n.noteText.Text = noteText;

            workspace.Notes.Add(n);
            this.workBench.Children.Add(n);

            return n;
        }

        /// <summary>
        /// Adds the given element to the selection.
        /// </summary>
        /// <param name="sel">The element to select.</param>
        public void SelectElement(System.Windows.Controls.UserControl sel)
        {
            if (!selectedElements.Contains(sel))
            {
                //set all other items to the unselected state
                ClearSelection();
                selectedElements.Add(sel);

                if(sel is dynNode)
                    (sel as dynNode).Select();
            }
        }

        /// <summary>
        /// Deselects all selected elements.
        /// </summary>
        public void ClearSelection()
        {
            //set all other items to the unselected state
            foreach (System.Windows.Controls.UserControl el in selectedElements.ToList())
            {
                if(el is dynNode)
                    (el as dynNode).Deselect();
            }
            selectedElements.Clear();
        }


        /// <summary>
        /// Called when the MouseWheel has been scrolled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scale = .001;
            double newValue = Convert.ToDouble(e.Delta) * scale;

            if (Zoom + newValue <= 1 && Zoom + newValue >= .001)
            {
                Zoom += newValue;
            }

            //if(this.zoomSlider.Value + newValue <= zoomSlider.Maximum &&
            //    this.zoomSlider.Value + newValue >= zoomSlider.Minimum)

            //this.zoomSlider.Value += newValue;
        }

        static bool HasParentType(Type t, Type testType)
        {
            while (t != typeof(object))
            {
                t = t.BaseType;
                if (t.Equals(testType))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Updates an element and all its ports.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void UpdateElement(object sender, MouseButtonEventArgs e)
        {
            dynNode el = sender as dynNode;
            foreach (dynPort p in el.InPorts)
            {
                p.Update();
            }
            el.OutPort.Update();
        }

        /// <summary>
        /// Find the user control of type 'testType' by traversing the tree.
        /// </summary>
        /// <returns></returns>
        public UIElement ElementClicked(DependencyObject depObj, Type testType)
        {
            UIElement foundElement = null;

            //IInputElement el = Mouse.DirectlyOver;
            //FrameworkElement fe = el as FrameworkElement;
            //DependencyObject depObj = fe.Parent;

            //walk up the tree to see whether the element is part of a port
            //then get the port's parent object
            while (depObj != null)
            {
                // If the current object is a UIElement which is a child of the
                // Canvas, exit the loop and return it.
                UIElement elem = depObj as UIElement;

                if (elem != null)
                {
                    Type t = elem.GetType();

                    if (HasParentType(t, testType))
                    {
                        foundElement = elem;
                        return foundElement;
                    }

                    if (elem != null && t.Equals(testType))
                    {
                        foundElement = elem;
                        return foundElement;
                    }
                }

                // VisualTreeHelper works with objects of type Visual or Visual3D.
                // If the current object is not derived from Visual or Visual3D,
                // then use the LogicalTreeHelper to find the parent element.
                if (depObj is Visual)
                    depObj = VisualTreeHelper.GetParent(depObj);
                else
                    depObj = LogicalTreeHelper.GetParent(depObj);
            }

            return foundElement;
        }


        //Performs a hit test on the given point in the UI.
        void TestClick(System.Windows.Point pt)
        {
            // Set up a callback to receive the hit test result enumeration.
            VisualTreeHelper.HitTest(workBench, null,
                new HitTestResultCallback(MyHitTestResult),
                new PointHitTestParameters(pt));

        }

        // Return the result of the hit test to the callback.
        public HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            // Add the hit test result to the list that will be processed after the enumeration.
            hitResultsList.Add(result.VisualHit);

            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }

        void DrawGrid()
        {
            //clear the canvas's children
            workBench.Children.Clear();
            double gridSpacing = 100.0;

            for (double i = 0.0; i < workBench.Width; i += gridSpacing)
            {
                Line xLine = new Line();
                xLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                xLine.X1 = i;
                xLine.Y1 = 0;
                xLine.X2 = i;
                xLine.Y2 = workBench.Height;
                xLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                xLine.VerticalAlignment = VerticalAlignment.Center;
                xLine.StrokeThickness = 1;
                workBench.Children.Add(xLine);
                Dynamo.Controls.DragCanvas.SetCanBeDragged(xLine, false);
            }
            for (double i = 0.0; i < workBench.Height; i += gridSpacing)
            {
                Line yLine = new Line();
                yLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                yLine.X1 = 0;
                yLine.Y1 = i;
                yLine.X2 = workBench.Width;
                yLine.Y2 = i;
                yLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                yLine.VerticalAlignment = VerticalAlignment.Center;
                yLine.StrokeThickness = 1;
                workBench.Children.Add(yLine);
                Dynamo.Controls.DragCanvas.SetCanBeDragged(yLine, false);
            }
        }


        /// <summary>
        /// Called when the mouse has been moved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //If we are currently connecting and there is an active connector,
            //redraw it to match the new mouse coordinates.
            if (isConnecting && activeConnector != null)
            {
                activeConnector.Redraw(e.GetPosition(workBench));
            }

            //If we are currently dragging an element, redraw the element to
            //match the new mouse coordinates.
            if (workBench.isDragInProgress)
            {
                dynNode el = workBench.elementBeingDragged as dynNode;
                if (el != null)
                {
                    foreach (dynPort p in el.InPorts)
                    {
                        p.Update();
                    }
                    el.OutPort.Update();
                    //foreach (dynPort p in el.StatePorts)
                    //{
                    //   p.Update();
                    //}
                }
            }

            //If we are panning the workspace, update the coordinate offset for the
            //next time we are redrawn.
            if (isPanning)
            {
                if (e.MiddleButton == MouseButtonState.Released)
                {
                    isPanning = false;

                    oldX = 0.0;
                    oldY = 0.0;
                    newX = 0.0;
                    newY = 0.0;

                    return;
                }

                if (oldX == 0.0)
                {
                    oldX = e.GetPosition(border).X;
                    oldY = e.GetPosition(border).Y;
                }
                else
                {
                    newX = e.GetPosition(border).X;
                    newY = e.GetPosition(border).Y;
                    this.CurrentX += newX - oldX;
                    this.CurrentY += newY - oldY;
                    oldX = newX;
                    oldY = newY;
                }
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs(string.Empty);
        }

        private void SaveAs(string xmlPath)
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

                
                
                System.Windows.Forms.SaveFileDialog saveDialog = new SaveFileDialog()
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
                    Log("Workbench could not be saved.");
                }
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            //save the active file
            SaveAs(CurrentSpace.FilePath);
        }

        /// <summary>
        /// Called when a mouse button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Pan with middle-click
            if (e.ChangedButton == MouseButton.Middle)
            {
                isPanning = true;
            }

            //close the tool finder if the user
            //has clicked anywhere else on the workbench
            if (toolFinder != null)
            {
                workBench.Children.Remove(toolFinder);
                toolFinder = null;
            }
        }


        /// <summary>
        /// Called when a mouse button is released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Stop panning if we have released the middle mouse button.
            if (e.ChangedButton == MouseButton.Middle)
            {
                isPanning = false;

                oldX = 0.0;
                oldY = 0.0;
                newX = 0.0;
                newY = 0.0;
            }

            if (e.ChangedButton == MouseButton.Left)
                this.beginNameEditClick = false;
        }


        public void Log(Exception e)
        {
            Log(e.GetType().ToString() + ":");
            Log(e.Message);
            Log(e.StackTrace);
        }


        private void BeginDragElement(string name, Point eleOffset)
        {
            if (this.uiLocked)
                return;

            var pos = Mouse.GetPosition(overlayCanvas);

            double x = pos.X;
            double y = pos.Y;

            this.dragOffset = eleOffset;

            dynNode newEl;

            if (this.dynFunctionDict.ContainsKey(name))
            {
                dynWorkspace ws = this.dynFunctionDict[name];

                newEl = new dynFunction(
                   ws.Elements.Where(e => e is dynSymbol)
                      .Select(s => ((dynSymbol)s).Symbol),
                   "out",
                   name
                );
            }
            else
            {
                TypeLoadData tld = builtinTypes[name];

                try
                {
                    var obj = Activator.CreateInstanceFrom(tld.assembly.Location, tld.t.FullName);
                    newEl = (dynNode)obj.Unwrap();

                    if (newEl is dynDouble)
                        (newEl as dynDouble).Value = this.storedSearchNum;
                    else if (newEl is dynStringInput)
                        (newEl as dynStringInput).Value = this.storedSearchStr;
                    else if (newEl is dynBool)
                        (newEl as dynBool).Value = this.storedSearchBool;

                    newEl.DisableInteraction();
                }
                catch (Exception e)
                {
                    Log(e);
                    return;
                }
            }

            newEl.GUID = Guid.NewGuid();

            //Add the element to the workbench
            overlayCanvas.Children.Add(newEl);

            newEl.Opacity = 0.7;

            x -= eleOffset.X;
            y -= eleOffset.Y;

            //Set its initial position
            Canvas.SetLeft(newEl, x);
            Canvas.SetTop(newEl, y);

            this.draggedElement = newEl;

            this.overlayCanvas.IsHitTestVisible = true;
        }

        bool SaveWorkspace(string xmlPath, dynWorkspace workSpace)
        {
            Log("Saving " + xmlPath + "...");
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

                foreach (dynNode el in workSpace.Elements)
                {
                    Point relPoint = el.TransformToAncestor(workBench).Transform(new Point(0, 0));

                    XmlElement dynEl = xmlDoc.CreateElement(el.GetType().ToString());
                    elementList.AppendChild(dynEl);

                    //set the type attribute
                    dynEl.SetAttribute("type", el.GetType().ToString());
                    dynEl.SetAttribute("guid", el.GUID.ToString());
                    dynEl.SetAttribute("nickname", el.NickName);
                    dynEl.SetAttribute("x", Canvas.GetLeft(el).ToString());
                    dynEl.SetAttribute("y", Canvas.GetTop(el).ToString());

                    el.SaveElement(xmlDoc, dynEl);
                }

                //write only the output connectors
                XmlElement connectorList = xmlDoc.CreateElement("dynConnectors");  //write the root element
                root.AppendChild(connectorList);

                foreach (dynNode el in workSpace.Elements)
                {
                    foreach (dynConnector c in el.OutPort.Connectors)
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
                Log(ex);
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                return false;
            }

            return true;
        }

        bool OpenDefinition(string xmlPath)
        {
            try
            {
                #region read xml file

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                string funName = null;
                string category = "";
                double cx = CANVAS_OFFSET_X;
                double cy = CANVAS_OFFSET_Y;

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
                        this.Home_Click(null, null); //TODO: Refactor
                    return this.OpenWorkbench(xmlPath);
                }
                else if (this.dynFunctionDict.ContainsKey(funName))
                {
                    Log("ERROR: Could not load definition for \"" + funName + "\", a node with this name already exists.");
                    return false;
                }

                Log("Loading node definition for \"" + funName + "\" from: " + xmlPath);

                //TODO: refactor to include x,y
                var ws = this.newFunction(
                   funName,
                   category.Length > 0
                      ? category
                      : BuiltinElementCategories.MISC,
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

                #region instantiate nodes
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

                    Type t = Type.GetType(typeName);

                    if (t == null)
                    {
                        Log("Error loading defintion. Could not load node of type: " + typeName);
                        return false;
                    }

                    dynNode el = AddDynElement(t, nickname, guid, x, y, ws, System.Windows.Visibility.Hidden);

                    if (el == null)
                        return false;

                    el.DisableReporting();
                    el.LoadElement(elNode);
                }
                #endregion

                this.workBench.UpdateLayout();

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

                    foreach (dynNode e in ws.Elements)
                    {
                        if (e.GUID == guidStart)
                        {
                            start = e;
                        }
                        else if (e.GUID == guidEnd)
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
                           start, end, startIndex, endIndex, portType, false
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

                        dynNote n = AddNote(text, x, y, ws);
                    }
                }
                #endregion

                foreach (var e in ws.Elements)
                    e.EnableReporting();

                this.hideWorkspace(ws);
                this.SaveFunction(ws, false);

                #endregion

                ws.FilePath = xmlPath;
            }
            catch (Exception ex)
            {
                Log("There was an error opening the workbench.");
                Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
                CleanWorkbench();
                return false;
            }
            return true;
        }

        void hideWorkspace(dynWorkspace ws)
        {
            foreach (var e in ws.Elements)
                e.Visibility = System.Windows.Visibility.Collapsed;
            foreach (var c in ws.Connectors)
                c.Visible = false;
            foreach (var n in ws.Notes)
                n.Visibility = System.Windows.Visibility.Hidden;
        }

        bool OpenWorkbench(string xmlPath)
        {
            Log("Opening home workspace " + xmlPath + "...");
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
                            this.CurrentX = Convert.ToDouble(att.Value);
                        else if (att.Name.Equals("Y"))
                            this.CurrentY = Convert.ToDouble(att.Value);
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

                    Type t = Type.GetType(typeName);

                    if (t == null)
                        throw new Exception("Could not find node of type: " + typeName);

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

                dynElementSettings.SharedInstance.Workbench.UpdateLayout();

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

                    foreach (dynNode e in dynElementSettings.SharedInstance.Bench.Elements)
                    {
                        if (e.GUID == guidStart)
                        {
                            start = e;
                        }
                        else if (e.GUID == guidEnd)
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
                        dynConnector newConnector = new dynConnector(start, end, startIndex,
                            endIndex, portType);

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

                        dynNote n = AddNote(text, x, y, this.CurrentSpace);

                    }
                }
                #endregion

                foreach (var e in this.CurrentSpace.Elements)
                    e.EnableReporting();

                #endregion

                homeSpace.FilePath = xmlPath;
            }
            catch (Exception ex)
            {
                Log("There was an error opening the workbench.");
                Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
                CleanWorkbench();
                return false;
            }
            return true;
        }

        private void CleanWorkbench()
        {
            Log("Clearing workflow...");

            //Copy locally
            var elements = this.Elements.ToList();

            IdlePromise.ExecuteOnIdle(
               delegate
               {
                   InitTransaction();

                   foreach (dynNode el in elements)
                   {
                       el.DisableReporting();
                       try
                       {
                           el.Destroy();
                       }
                       catch { }
                   }

                   EndTransaction();
               },
               true
            );

            foreach (dynNode el in elements)
            {
                foreach (dynPort p in el.InPorts)
                {
                    for (int i = p.Connectors.Count - 1; i >= 0; i--)
                        p.Connectors[i].Kill();
                }
                for (int i = el.OutPort.Connectors.Count - 1; i >= 0; i--)
                    el.OutPort.Connectors[i].Kill();

                dynElementSettings.SharedInstance.Workbench.Children.Remove(el);
            }

            foreach (dynNote n in this.CurrentSpace.Notes)
            {
                dynElementSettings.SharedInstance.Workbench.Children.Remove(n);
            }

            this.CurrentSpace.Elements.Clear();
            this.CurrentSpace.Connectors.Clear();
            this.CurrentSpace.Notes.Clear();
            this.CurrentSpace.Modified();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            //string xmlPath = "C:\\test\\myWorkbench.xml";
            string xmlPath = "";

            System.Windows.Forms.OpenFileDialog openDialog = new OpenFileDialog()
            {
                Filter = "Dynamo Definitions (*.dyn; *.dyf)|*.dyn;*.dyf|All files (*.*)|*.*"
            };

            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                xmlPath = openDialog.FileName;
            }

            if (!string.IsNullOrEmpty(xmlPath))
            {
                if (this.uiLocked)
                {
                    this.QueueLoad(xmlPath);
                    return;
                }

                LockUI();
                if (!OpenDefinition(xmlPath))
                {
                    //MessageBox.Show("Workbench could not be opened.");
                    Log("Workbench could not be opened.");

                    dynElementSettings.SharedInstance.Writer.WriteLine("Workbench could not be opened.");
                    dynElementSettings.SharedInstance.Writer.WriteLine(xmlPath);
                }
                UnlockUI();
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            if (sw != null)
            {
                sw.Close();
                dynElementSettings.SharedInstance.Writer.WriteLine("Dynamo ended " + System.DateTime.Now.ToString());
                dynElementSettings.SharedInstance.Writer.Close();
            }

            //end the transaction 
            //dynElementSettings.SharedInstance.MainTransaction.Commit();
        }

        public void Log(string message)
        {
            sw.WriteLine(message);
            LogText = sw.ToString();
            //LogScroller.ScrollToEnd();

            dynElementSettings.SharedInstance.Writer.WriteLine(message);

            LogScroller.ScrollToBottom();
        }

        void OnPreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //Keyboard.Focus(this);

            hitResultsList.Clear();
            TestClick(e.GetPosition(workBench));

            dynPort p = null;
            DragCanvas dc = null;
            dynNode element = null;

            bool hit = false;

            //figure out which element is hit
            //HACK: put the tests with break in highest to
            //lowest z order 
            if (hitResultsList.Count > 0)
            {
                foreach (DependencyObject depObj in hitResultsList)
                {
                    //traverse the tree through all the
                    //hit elements to see if you get a port
                    p = ElementClicked(depObj, typeof(dynPort)) as dynPort;
                    if (p != null && p.Owner.IsVisible)
                    {
                        hit = true;
                        break;
                    }

                    //traverse the tree through all the
                    //hit elements to see if you get an element
                    element = ElementClicked(depObj, typeof(dynNode)) as dynNode;
                    if (element != null && element.IsVisible)
                    {
                        hit = true;
                        break;
                    }
                }

                if (!hit)
                {
                    //traverse the tree through all the
                    //hit elements to see if you get the canvas
                    dc = ElementClicked(hitResultsList[0], typeof(DragCanvas)) as DragCanvas;
                }
            }

            #region test for a port
            if (p != null)
            {
                Debug.WriteLine("Port clicked");

                if (!isConnecting)
                {
                    //test if port already has a connection if so grab it
                    //and begin connecting to somewhere else
                    //don't allow the grabbing of the start connector
                    if (p.Connectors.Count > 0 && p.Connectors[0].Start != p)
                    {
                        activeConnector = p.Connectors[0];
                        activeConnector.Disconnect(p);
                        isConnecting = true;
                        workBench.isConnecting = true;
                        this.CurrentSpace.Connectors.Remove(activeConnector);
                    }
                    else
                    {
                        try
                        {
                            //you've begun creating a connector
                            dynConnector c = new dynConnector(p, workBench, e.GetPosition(workBench));
                            activeConnector = c;
                            isConnecting = true;
                            workBench.isConnecting = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
                else
                {
                    //attempt a connection between the port
                    //and the connector
                    if (!activeConnector.Connect(p))
                    {
                        activeConnector.Kill();
                        isConnecting = false;
                        workBench.isConnecting = false;
                        activeConnector = null;
                    }
                    else
                    {
                        //you've already started connecting
                        //now you're going to stop
                        this.CurrentSpace.Connectors.Add(activeConnector);
                        isConnecting = false;
                        workBench.isConnecting = false;
                        activeConnector = null;
                    }
                }

                //set the handled flag so that the element doesn't get dragged
                e.Handled = true;
            }
            else
            {
                //if you click on the canvas and you're connecting
                //then drop the connector, otherwise do nothing
                if (activeConnector != null)
                {
                    activeConnector.Kill();
                    isConnecting = false;
                    workBench.isConnecting = false;
                    activeConnector = null;
                }

                if (editingName && !hoveringEditBox)
                {
                    DisableEditNameBox();
                }

                //this.Focus();
            }
            #endregion

            if (element != null)
            {
                Debug.WriteLine("Element clicked");
                SelectElement(element);
            }

            if (dc != null)
            {
                Debug.WriteLine("Canvas clicked");
                ClearSelection();
            }
        }

        //void OnMouseRightButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    hitResultsList.Clear();
        //    TestClick(e.GetPosition(workBench));

        //    dynElement dynEl = null;
        //    if (hitResultsList.Count > 0)
        //    {
        //        foreach (DependencyObject depObj in hitResultsList)
        //        {
        //            //traverse the tree through all the
        //            //hit elements to see if you get a port
        //            dynEl = ElementClicked(depObj, typeof(dynElement)) as dynElement;
        //            if (dynEl != null)
        //            {
        //                break;
        //            }
        //        }
        //    }

        //    //start dragging the element
        //    if (dynEl != null)
        //    {
        //        //this.statusText.Text = "DynElement selected...";
        //        //hold off on setting the isDragInProcess
        //        workBench.isDragInProgress = true;
        //        workBench.elementBeingDragged = dynEl;
        //        workBench.DragElement();
        //    }

        //}

        //bubbling
        //from element up to root
        private void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {


        }

        //tunneling
        //from root down to element
        private void OnPreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //handle key presses for the bench in the bubbling event
            //if no other element has already handled this event it will 
            //start at the bench and move up to root, not raising the event
            //on any other elements

            //if the key down is 'b' open the build window
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.B))
            {
                //get the mouse position

                toolFinder = new dynToolFinder();
                dynElementSettings.SharedInstance.Workbench.Children.Add(toolFinder);
                toolFinder.ToolFinderFinished += new ToolFinderFinishedHandler(toolFinder_ToolFinderFinished);

                Canvas.SetLeft(toolFinder, 100);
                Canvas.SetTop(toolFinder, 100);
                e.Handled = true;
            }
            //changed the delete key combination so as not to interfere with
            //keyboard events
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Back) ||
                Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Delete))
            {

                for (int i = selectedElements.Count - 1; i >= 0; i--)
                {
                    DeleteElement(selectedElements[i]);
                }

                e.Handled = true;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.N))
            {
                dynNote note = new dynNote();
                dynElementSettings.SharedInstance.Workbench.Children.Add(note);

                //convert the current position of the mouse into canvas coordinates
                Point p = Mouse.GetPosition(dynElementSettings.SharedInstance.Workbench);

                Canvas.SetLeft(note, p.X);
                Canvas.SetTop(note, p.Y);

                CurrentSpace.Notes.Add(note);
                if (!ViewingHomespace)
                    CurrentSpace.Modified(); //tell the workspace to save

                e.Handled = true;
            }

            IInputElement focusElement = FocusManager.GetFocusedElement(this);

            if (focusElement != null && focusElement.GetType() != typeof(System.Windows.Controls.TextBox) )
            {
                if (Keyboard.IsKeyDown(Key.Left))
                {
                    this.CurrentX += 20;
                    e.Handled = true;
                }
                if (Keyboard.IsKeyDown(Key.Right))
                {
                    this.CurrentX -= 20;
                    e.Handled = true;
                }
                if (Keyboard.IsKeyDown(Key.Up))
                {
                    this.CurrentY += 20;
                    e.Handled = true;
                }
                if (Keyboard.IsKeyDown(Key.Down))
                {
                    this.CurrentY -= 20;
                    e.Handled = true;
                }
            }

            if (editingName)
            {
                if (Keyboard.IsKeyDown(Key.Enter))
                {
                    SaveNameEdit();
                    DisableEditNameBox();
                    e.Handled = true;
                }
                else if (Keyboard.IsKeyDown(Key.Escape))
                {
                    DisableEditNameBox();
                    e.Handled = true;
                }
            }

            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.F))
            {
                this.searchBox.Focus();
            }
        }

        internal void DeleteElement(System.Windows.Controls.UserControl el)
        {
            dynNote note = el as dynNote;
            dynNode node = el as dynNode;
            if (node != null)
            {
                for (int i = node.OutPort.Connectors.Count - 1; i >= 0; i--)
                {
                    node.OutPort.Connectors[i].Kill();
                }
                foreach (dynPort p in node.InPorts)
                {
                    for (int i = p.Connectors.Count - 1; i >= 0; i--)
                    {
                        p.Connectors[i].Kill();
                    }
                }

                this.Elements.Remove(node);
            }
            else if (note != null)
            {
                this.CurrentSpace.Notes.Remove(note);
            }

            //remove the item from the selection set
            selectedElements.Remove(el);
            dynElementSettings.SharedInstance.Workbench.Children.Remove(el);
            el = null;
        }

        void toolFinder_ToolFinderFinished(object sender, EventArgs e)
        {
            dynElementSettings.SharedInstance.Workbench.Children.Remove(toolFinder);
            toolFinder = null;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            LockUI();
            CleanWorkbench();

            //don't save the file path
            CurrentSpace.FilePath = "";

            UnlockUI();
        }

        public ExecutionEnvironment Environment
        {
            get;
            private set;
        }

        public bool RunInDebug { get { return this.TransMode == TransactionMode.Debug; } }

        public bool InIdleThread;

        public TransactionMode TransMode;

        private List<Autodesk.Revit.DB.ElementId> _transElements = new List<Autodesk.Revit.DB.ElementId>();

        private Dictionary<Autodesk.Revit.DB.ElementId, DynElementUpdateDelegate> _transDelElements
           = new Dictionary<Autodesk.Revit.DB.ElementId, DynElementUpdateDelegate>();

        internal void RegisterSuccessfulDeleteHook(Autodesk.Revit.DB.ElementId id, DynElementUpdateDelegate d)
        {
            this._transDelElements[id] = d;
        }

        private void CommitDeletions()
        {
            var delDict = new Dictionary<DynElementUpdateDelegate, List<Autodesk.Revit.DB.ElementId>>();
            foreach (var kvp in this._transDelElements)
            {
                if (!delDict.ContainsKey(kvp.Value))
                {
                    delDict[kvp.Value] = new List<Autodesk.Revit.DB.ElementId>();
                }
                delDict[kvp.Value].Add(kvp.Key);
            }

            foreach (var kvp in delDict)
                kvp.Key(kvp.Value);
        }

        internal void RegisterDeleteHook(Autodesk.Revit.DB.ElementId id, DynElementUpdateDelegate d)
        {
            DynElementUpdateDelegate del = delegate(List<Autodesk.Revit.DB.ElementId> deleted)
            {
                var valid = new List<Autodesk.Revit.DB.ElementId>();
                var invalid = new List<Autodesk.Revit.DB.ElementId>();
                foreach (var delId in deleted)
                {
                    try
                    {
                        Autodesk.Revit.DB.Element e = dynElementSettings.SharedInstance.Doc.Document.GetElement(delId);
                        if (e != null)
                        {
                            valid.Add(e.Id);
                        }
                        else
                            invalid.Add(delId);
                    }
                    catch
                    {
                        invalid.Add(delId);
                    }
                }
                valid.Clear();
                d(invalid);
                foreach (var invId in invalid)
                {
                    this.Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Modify);
                    this.Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Add);
                    this.Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Delete);
                }
            };

            DynElementUpdateDelegate mod = delegate(List<Autodesk.Revit.DB.ElementId> modded)
            {
                _transElements.RemoveAll(x => modded.Contains(x));

                foreach (var mid in modded)
                {
                    this.Updater.UnRegisterChangeHook(mid, ChangeTypeEnum.Modify);
                    this.Updater.UnRegisterChangeHook(mid, ChangeTypeEnum.Add);
                }
            };

            this.Updater.RegisterChangeHook(
               id, ChangeTypeEnum.Delete, del
            );
            this.Updater.RegisterChangeHook(
               id, ChangeTypeEnum.Modify, mod
            );
            this.Updater.RegisterChangeHook(
               id, ChangeTypeEnum.Add, mod
            );
            this._transElements.Add(id);
        }

        private Transaction _trans;
        public void InitTransaction()
        {
            if (_trans == null || _trans.GetStatus() != TransactionStatus.Started)
            {
                _trans = new Transaction(
                   dynElementSettings.SharedInstance.Doc.Document,
                   "Dynamo Script"
                );
                _trans.Start();

                FailureHandlingOptions failOpt = _trans.GetFailureHandlingOptions();
                failOpt.SetFailuresPreprocessor(new DynamoWarningPrinter(this));
                _trans.SetFailureHandlingOptions(failOpt);
            }
        }

        public Transaction Transaction { get { return this._trans; } }

        public void EndTransaction()
        {
            if (_trans != null)
            {
                if (_trans.GetStatus() == TransactionStatus.Started)
                {
                    _trans.Commit();
                    _transElements.Clear();
                    CommitDeletions();
                    _transDelElements.Clear();
                }
                _trans = null;
            }
        }

        public void CancelTransaction()
        {
            if (_trans != null)
            {
                _trans.RollBack();
                _trans = null;
                this.Updater.RollBack(this._transElements);
                this._transElements.Clear();
                this._transDelElements.Clear();
            }
        }

        public bool IsTransactionActive()
        {
            return _trans != null;
        }

        public bool Running = false;

        public void RunExpression(bool debug, bool showErrors = true)
        {
            //If we're already running, do nothing.
            if (this.Running)
                return;

            //TODO: Hack. Might cause things to break later on...
            //Reset Cancel and Rerun flags
            this.RunCancelled = false;
            this.runAgain = false;

            //We are now considered running
            this.Running = true;

            //Set run auto flag
            this.dynamicRun = !showErrors;

            //Setup background worker
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                /* Execution Thread */

                //Get our entry points (elements with nothing connected to output)
                var topElements = this.homeSpace.Elements.Where(
                   x => !x.OutPort.Connectors.Any()
                );

                //Mark the topmost as dirty/clean
                foreach (var topMost in topElements)
                    topMost.MarkDirty();

                //TODO: Flesh out error handling
                try
                {
                    //Run Delegate
                    Action run = delegate
                    {
                        //For each entry point...
                        foreach (dynNode topMost in topElements)
                        {
                            //Build the expression from the entry point.
                            Expression runningExpression = topMost.Build().Compile();

                            //Print some stuff if we're in debug mode
                            if (debug)
                            {
                                //string exp = FScheme.print(runningExpression);
                                this.Dispatcher.Invoke(new Action(
                                   delegate
                                   {
                                       string exp = topMost.PrintExpression();
                                       Log("> " + exp);
                                   }
                                ));
                            }

                            try
                            {
                                //Evaluate the expression
                                var expr = this.Environment.Evaluate(runningExpression);

                                //Print some more stuff if we're in debug mode
                                if (debug && expr != null)
                                {
                                    this.Dispatcher.Invoke(new Action(
                                       () => Log(FScheme.print(expr))
                                    ));
                                }
                            }
                            catch (CancelEvaluationException ex)
                            {
                                /* Evaluation was cancelled */

                                this.CancelTransaction();
                                //this.RunCancelled = false;
                                if (ex.Force)
                                    this.runAgain = false;

                                //Stop evaluation of other entry points.
                                break;
                            }
                            catch (Exception ex)
                            {
                                /* Evaluation failed due to error */

                                //Print unhandled exception
                                if (ex.Message.Length > 0)
                                {
                                    this.Dispatcher.Invoke(new Action(
                                       delegate
                                       {
                                           Log("ERROR!");
                                           Log(ex);
                                       }
                                    ));
                                }
                                this.CancelTransaction();
                                this.RunCancelled = true;
                                this.runAgain = false;

                                //Stop evaluation of other entry points.
                                break;
                            }
                        }

                        //Cleanup Delegate
                        Action cleanup = delegate
                        {
                            this.InitTransaction(); //Initialize a transaction (if one hasn't been aleady)

                            //Reset all elements
                            foreach (var element in this.AllElements)
                                element.ResetRuns();

                            //////
                            /* FOR NON-DEBUG RUNS, THIS IS THE ACTUAL END POINT FOR DYNAMO TRANSACTION */
                            //////

                            this.EndTransaction(); //Close global transaction.
                        };

                        //If we're in a debug run or not already in the idle thread, then run the Cleanup Delegate
                        //from the idle thread. Otherwise, just run it in this thread.
                        if (debug || !this.InIdleThread)
                            IdlePromise.ExecuteOnIdle(cleanup, false);
                        else
                            cleanup();
                    };

                    //If we are not running in debug...
                    if (!debug)
                    {
                        //Do we need manual transaction control?
                        bool manualTrans = topElements.Any(x => x.RequiresManualTransaction());

                        //Can we avoid running everything in the Revit Idle thread?
                        bool noIdleThread = manualTrans || topElements.All(x => !x.RequiresTransaction());

                        //If we don't need to be in the idle thread...
                        if (noIdleThread)
                        {
                            this.TransMode = TransactionMode.Manual; //Manual transaction control
                            this.InIdleThread = false; //Not in idle thread at the moment
                            run(); //Just run the Run Delegate
                        }
                        else //otherwise...
                        {
                            this.TransMode = TransactionMode.Automatic; //Automatic transaction control
                            this.InIdleThread = true; //Now in the idle thread.
                            IdlePromise.ExecuteOnIdle(run, false); //Execute the Run Delegate in the Idle thread.
                        }
                    }
                    else //If we are in debug mode...
                    {
                        this.TransMode = TransactionMode.Debug; //Debug transaction control
                        this.InIdleThread = true; //Everything will be evaluated in the idle thread.

                        this.Dispatcher.Invoke(new Action(
                           () => Log("Running expression in debug.")
                        ));

                        //Execute the Run Delegate.
                        run();
                    }
                }
                catch (CancelEvaluationException ex)
                {
                    /* Evaluation was cancelled */

                    this.CancelTransaction();
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
                        this.Dispatcher.Invoke(new Action(
                            delegate
                            {
                                Log("ERROR!");
                                Log(ex);
                            }
                        ));
                    }

                    this.CancelTransaction();

                    //Reset the flags
                    this.runAgain = false;
                    this.RunCancelled = true;
                }
                finally
                {
                    /* Post-evaluation cleanup */

                    //Re-enable run button
                    this.runButton.Dispatcher.Invoke(new Action(
                       delegate
                       {
                           this.runButton.IsEnabled = true;
                       }
                    ));

                    //No longer running
                    this.Running = false;

                    //If we should run again...
                    if (this.runAgain)
                    {
                        //Reset flag
                        this.runAgain = false;
                        //this.RunCancelled = false;

                        //Run this method again from the main thread
                        this.Dispatcher.BeginInvoke(new Action(
                           delegate
                           {
                               this.RunExpression(debug, showErrors);
                           }
                        ));
                    }
                }
            };

            //Disable Run Button
            this.runButton.Dispatcher.Invoke(new Action(
               delegate { this.runButton.IsEnabled = false; }
            ));

            //Let's start
            worker.RunWorkerAsync();
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            RunExpression(this.debugCheckBox.IsChecked == true);
        }

        //private void SaveFunction_Click(object sender, RoutedEventArgs e)
        //{
        //   SaveFunction(this.CurrentSpace);
        //}

        //private Dictionary<string, System.Windows.Controls.MenuItem> addMenuItemsDict
        //   = new Dictionary<string, System.Windows.Controls.MenuItem>();

        private Dictionary<string, System.Windows.Controls.MenuItem> viewMenuItemsDict
           = new Dictionary<string, System.Windows.Controls.MenuItem>();

        private Dictionary<string, Expander> addMenuCategoryDict
           = new Dictionary<string, Expander>();

        private Dictionary<string, dynNode> addMenuItemsDictNew
           = new Dictionary<string, dynNode>();

        private void NewFunction_Click(object sender, RoutedEventArgs e)
        {
            //First, prompt the user to enter a name
            string name, category;
            string error = "";

            do
            {
                var dialog = new FunctionNamePrompt(this.addMenuCategoryDict.Keys, error);
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                name = dialog.Text;
                category = dialog.Category;

                if (this.dynFunctionDict.ContainsKey(name))
                {
                    error = "A function with this name already exists.";
                }
                else if (category.Equals(""))
                {
                    error = "Please enter a valid category.";
                }
                else
                {
                    error = "";
                }
            }
            while (!error.Equals(""));

            this.newFunction(name, category, true);
        }

        private dynWorkspace newFunction(string name, string category, bool display)
        {
            //Add an entry to the funcdict
            var workSpace = new FuncWorkspace(name, category, CANVAS_OFFSET_X, CANVAS_OFFSET_Y);

            var newElements = workSpace.Elements;
            var newConnectors = workSpace.Connectors;

            this.dynFunctionDict[name] = workSpace;

            //Add an entry to the View menu
            System.Windows.Controls.MenuItem i = new System.Windows.Controls.MenuItem();
            i.Header = name;
            i.Click += new RoutedEventHandler(ChangeView_Click);
            this.viewMenu.Items.Add(i);
            this.viewMenuItemsDict[name] = i;

            //Add an entry to the Add menu
            //System.Windows.Controls.MenuItem mi = new System.Windows.Controls.MenuItem();
            //mi.Header = name;
            //mi.Click += new RoutedEventHandler(AddElement_Click);
            //AddMenu.Items.Add(mi);
            //this.addMenuItemsDict[name] = mi;

            dynFunction newEl = new dynFunction(
               workSpace.Elements.Where(el => el is dynSymbol)
                  .Select(s => ((dynSymbol)s).Symbol),
               "out",
               name
            );
            newEl.DisableInteraction();
            newEl.MouseDown += delegate
            {
                draggedElementMenuItem = newEl;

                BeginDragElement(newEl.Symbol, Mouse.GetPosition(newEl));

                newEl.Visibility = System.Windows.Visibility.Hidden;
            };
            newEl.GUID = Guid.NewGuid();
            newEl.Margin = new Thickness(5, 30, 5, 5);
            newEl.LayoutTransform = new ScaleTransform(.8, .8);
            newEl.State = ElementState.DEAD;

            Expander expander;

            if (addMenuCategoryDict.ContainsKey(category))
            {
                expander = addMenuCategoryDict[category];
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

                addMenuCategoryDict[category] = expander;

                var sortedExpanders = new SortedList<string, Expander>();
                foreach (Expander child in this.stackPanel1.Children)
                {
                    sortedExpanders.Add((string)child.Header, child);
                }
                sortedExpanders.Add(category, expander);

                this.stackPanel1.Children.Clear();

                foreach (Expander child in sortedExpanders.Values)
                {
                    this.stackPanel1.Children.Add(child);
                }
            }

            var wp = (WrapPanel)expander.Content;

            var sortedElements = new SortedList<string, dynNode>();
            foreach (dynNode child in wp.Children)
            {
                sortedElements.Add(child.NickName, child);
            }
            sortedElements.Add(name, newEl);

            wp.Children.Clear();

            foreach (dynNode child in sortedElements.Values)
            {
                wp.Children.Add(child);
            }

            addMenuItemsDictNew[name] = newEl;
            searchDict.Add(newEl, name.Split(' ').Where(x => x.Length > 0));

            if (display)
            {
                //Store old workspace
                //var ws = new dynWorkspace(this.elements, this.connectors, this.CurrentX, this.CurrentY);

                if (!this.ViewingHomespace)
                {
                    //Step 2: Store function workspace in the function dictionary
                    this.dynFunctionDict[this.CurrentSpace.Name] = this.CurrentSpace;

                    //Step 3: Save function
                    this.SaveFunction(this.CurrentSpace);
                }

                //Make old workspace invisible
                foreach (dynNode dynE in this.Elements)
                {
                    dynE.Visibility = System.Windows.Visibility.Collapsed;
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
                this.homeButton.IsEnabled = true;
                //this.varItem.IsEnabled = true;

                this.workspaceLabel.Content = this.CurrentSpace.Name;
                this.editNameButton.Visibility = System.Windows.Visibility.Visible;
                this.editNameButton.IsHitTestVisible = true;
                this.setFunctionBackground();
            }

            return workSpace;
        }

        private void ChangeView_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem item = sender as System.Windows.Controls.MenuItem;

            DisplayFunction(item.Header.ToString());
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            //Step 1: Make function workspace invisible
            foreach (var ele in this.Elements)
            {
                ele.Visibility = System.Windows.Visibility.Collapsed;
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
            this.dynFunctionDict[this.CurrentSpace.Name] = this.CurrentSpace;

            //Step 3: Save function
            this.SaveFunction(this.CurrentSpace);

            //Step 4: Make home workspace visible
            //this.elements = this.homeSpace.elements;
            //this.connectors = this.homeSpace.connectors;
            //this.CurrentX = this.homeSpace.savedX;
            //this.CurrentY = this.homeSpace.savedY;
            this.CurrentSpace = this.homeSpace;

            foreach (var ele in this.Elements)
            {
                ele.Visibility = System.Windows.Visibility.Visible;
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
            this.homeButton.IsEnabled = false;
            //this.varItem.IsEnabled = false;

            this.workspaceLabel.Content = "Home";
            this.editNameButton.Visibility = System.Windows.Visibility.Collapsed;
            this.editNameButton.IsHitTestVisible = false;

            this.setHomeBackground();
        }

        public void SaveFunction(dynWorkspace funcWorkspace, bool writeDefinition = true)
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

                    string path = Path.Combine(pluginsPath, FormatFileName(funcWorkspace.Name) + ".dyf");
                    SaveWorkspace(path, funcWorkspace);
                }
                catch (Exception e)
                {
                    Log("Error saving:" + e.GetType());
                    Log(e);
                }
            }

            //Find compile errors
            var topMost = funcWorkspace.Elements.Where(x => !x.OutPort.Connectors.Any()).ToList();
            if (topMost.Count > 1)
            {
                foreach (var ele in topMost)
                {
                    ele.Error("Nodes can have only one output.");
                }
            }
            else
            {
                foreach (var ele in topMost)
                {
                    ele.ValidateConnections();
                }
            }

            //Find function entry point, and then compile the function and add it to our environment
            dynNode top = topMost.FirstOrDefault();

            var variables = funcWorkspace.Elements.Where(x => x is dynSymbol);
            var variableNames = variables.Select(x => ((dynSymbol)x).Symbol);

            try
            {
                if (top != default(dynNode))
                {
                    Expression expression = Utils.MakeAnon(
                       variableNames,
                       top.Build().Compile()
                    );

                    this.Environment.DefineSymbol(funcWorkspace.Name, expression);
                }
            }
            catch
            {
                //TODO: flesh out error handling (build-loops?)
            }

            //Update existing function nodes which point to this function to match its changes
            foreach (var el in this.AllElements)
            {
                if (el is dynFunction)
                {
                    var node = (dynFunction)el;

                    if (!node.Symbol.Equals(funcWorkspace.Name))
                        continue;

                    node.SetInputs(variableNames);
                    el.ReregisterInputs();
                    //el.IsDirty = true;
                }
            }

            //Call OnSave for all saved elements
            foreach (var el in funcWorkspace.Elements)
                el.onSave();

            //Update new add menu
            var addItem = (dynFunction)this.addMenuItemsDictNew[funcWorkspace.Name];
            addItem.SetInputs(variableNames);
            addItem.ReregisterInputs();
            addItem.State = ElementState.DEAD;
        }

        internal void DisplayFunction(string symbol)
        {
            if (!this.dynFunctionDict.ContainsKey(symbol) || this.CurrentSpace.Name.Equals(symbol))
                return;

            var newWs = this.dynFunctionDict[symbol];

            //Make sure we aren't dragging
            workBench.isDragInProgress = false;
            workBench.ignoreClick = true;

            //Step 1: Make function workspace invisible
            foreach (var ele in this.Elements)
            {
                ele.Visibility = System.Windows.Visibility.Collapsed;
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
                this.dynFunctionDict[this.CurrentSpace.Name] = this.CurrentSpace;

                //Step 3: Save function
                this.SaveFunction(this.CurrentSpace);
            }

            //Step 4: Make home workspace visible
            //this.elements = newWs.elements;
            //this.connectors = newWs.connectors;
            //this.CurrentX = newWs.savedX;
            //this.CurrentY = newWs.savedY;
            this.CurrentSpace = newWs;

            foreach (var ele in this.Elements)
            {
                ele.Visibility = System.Windows.Visibility.Visible;
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
            this.homeButton.IsEnabled = true;
            //this.varItem.IsEnabled = true;

            this.workspaceLabel.Content = symbol;
            this.editNameButton.Visibility = System.Windows.Visibility.Visible;
            this.editNameButton.IsHitTestVisible = true;

            this.setFunctionBackground();
        }


        private void setFunctionBackground()
        {
            var bgBrush = (LinearGradientBrush)this.outerCanvas.Background;
            bgBrush.GradientStops[0].Color = Color.FromArgb(0xFF, 0x6B, 0x6B, 0x6B); //Dark
            bgBrush.GradientStops[1].Color = Color.FromArgb(0xFF, 0xBA, 0xBA, 0xBA); //Light

            //var sbBrush = (LinearGradientBrush)this.sidebarGrid.Background;
            //sbBrush.GradientStops[0].Color = Color.FromArgb(0xFF, 0x6B, 0x6B, 0x6B); //Dark
            //sbBrush.GradientStops[1].Color = Color.FromArgb(0xFF, 0xBA, 0xBA, 0xBA); //Light
        }


        private void setHomeBackground()
        {
            var bgBrush = (LinearGradientBrush)this.outerCanvas.Background;
            bgBrush.GradientStops[0].Color = Color.FromArgb(0xFF, 0x4B, 0x4B, 0x4B); //Dark
            bgBrush.GradientStops[1].Color = Color.FromArgb(0xFF, 0x7A, 0x7A, 0x7A); //Light

            //var sbBrush = (LinearGradientBrush)this.sidebarGrid.Background;
            //sbBrush.GradientStops[0].Color = Color.FromArgb(0xFF, 0x4B, 0x4B, 0x4B); //Dark
            //sbBrush.GradientStops[1].Color = Color.FromArgb(0xFF, 0x9A, 0x9A, 0x9A); //Light
        }


        private void Print_Click(object sender, RoutedEventArgs e)
        {
            foreach (dynNode el in this.Elements)
            {
                dynNode topMost = null;
                if (!el.OutPort.Connectors.Any())
                {
                    topMost = el;

                    Expression runningExpression = topMost.Build().Compile();

                    //TODO: Flesh out error handling
                    try
                    {
                        string exp = FScheme.print(runningExpression);
                        Log("> " + exp);
                    }
                    catch (Exception ex)
                    {
                        Log(ex);
                    }
                }
            }
        }

        internal void RemoveConnector(dynConnector c)
        {
            this.CurrentSpace.Connectors.Remove(c);
        }

        internal void ShowElement(dynNode e)
        {
            if (dynamicRun)
                return;

            if (!this.Elements.Contains(e))
            {
                if (this.homeSpace != null && this.homeSpace.Elements.Contains(e))
                {
                    //Show the homespace
                    Home_Click(null, null);
                }
                else
                {
                    foreach (var funcPair in this.dynFunctionDict)
                    {
                        if (funcPair.Value.Elements.Contains(e))
                        {
                            DisplayFunction(funcPair.Key);
                            break;
                        }
                    }
                }
            }

            CenterViewOnElement(e);
        }

        private void CenterViewOnElement(dynNode e)
        {
            var left = Canvas.GetLeft(e);
            var top = Canvas.GetTop(e);

            var x = left + e.Width / 2 - this.outerCanvas.ActualWidth / 2;
            var y = top + e.Height / 2 - (this.outerCanvas.ActualHeight / 2 - this.LogScroller.ActualHeight);

            this.CurrentX = -x;
            this.CurrentY = -y;
        }

        private bool beginNameEditClick;
        private bool editingName;

        private void image1_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //highlight

            if (beginNameEditClick && e.LeftButton == MouseButtonState.Released)
            {
                beginNameEditClick = false;
            }
        }

        private void image1_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //unhighlight
        }

        private void image1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (beginNameEditClick)
            {
                if (editingName)
                {
                    SaveNameEdit();
                    DisableEditNameBox();
                }
                else
                {
                    EnableEditNameBox();
                }
            }
            beginNameEditClick = false;
        }

        private void image1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            beginNameEditClick = true;
        }

        void EnableEditNameBox()
        {
            this.workspaceLabel.Visibility = System.Windows.Visibility.Collapsed;

            this.editNameBox.Visibility = System.Windows.Visibility.Visible;
            this.editNameBox.IsEnabled = true;
            this.editNameBox.IsHitTestVisible = true;
            this.editNameBox.Focusable = true;
            this.editNameBox.Focus();
            this.editNameBox.Text = this.CurrentSpace.Name;
            this.editNameBox.SelectAll();

            editingName = true;
        }

        void SaveNameEdit()
        {
            var newName = this.editNameBox.Text;

            if (this.dynFunctionDict.ContainsKey(newName))
            {
                Log("ERROR: Cannot rename to \"" + newName + "\", node with same name already exists.");
                return;
            }

            this.workspaceLabel.Content = this.editNameBox.Text;

            //Update view menu
            var viewItem = this.viewMenuItemsDict[this.CurrentSpace.Name];
            viewItem.Header = newName;
            this.viewMenuItemsDict.Remove(this.CurrentSpace.Name);
            this.viewMenuItemsDict[newName] = viewItem;

            //Update add menu
            //var addItem = this.addMenuItemsDict[this.currentFunctionName];
            //addItem.Header = newName;
            //this.addMenuItemsDict.Remove(this.currentFunctionName);
            //this.addMenuItemsDict[newName] = addItem;

            //------------------//

            var newAddItem = (dynFunction)this.addMenuItemsDictNew[this.CurrentSpace.Name];
            if (newAddItem.NickName.Equals(this.CurrentSpace.Name))
                newAddItem.NickName = newName;
            newAddItem.Symbol = newName;
            this.addMenuItemsDictNew.Remove(this.CurrentSpace.Name);
            this.addMenuItemsDictNew[newName] = newAddItem;

            //Sort the menu after a rename
            Expander unsorted = this.addMenuCategoryDict.Values.FirstOrDefault(
               ex => ((WrapPanel)ex.Content).Children.Contains(newAddItem)
            );

            var wp = (WrapPanel)unsorted.Content;

            var sortedElements = new SortedList<string, dynNode>();
            foreach (dynNode child in wp.Children)
            {
                sortedElements.Add(child.NickName, child);
            }

            wp.Children.Clear();

            foreach (dynNode child in sortedElements.Values)
            {
                wp.Children.Add(child);
            }

            //Update search dictionary after a rename
            var oldTags = this.CurrentSpace.Name.Split(' ').Where(x => x.Length > 0);
            this.searchDict.Remove(newAddItem, oldTags);

            var newTags = newName.Split(' ').Where(x => x.Length > 0);
            this.searchDict.Add(newAddItem, newTags);

            //------------------//

            //Update existing function nodes
            foreach (var el in this.AllElements)
            {
                if (el is dynFunction)
                {
                    var node = (dynFunction)el;

                    if (!node.Symbol.Equals(this.CurrentSpace.Name))
                        continue;

                    node.Symbol = newName;

                    //Rename nickname only if it's still referring to the old name
                    if (node.NickName.Equals(this.CurrentSpace.Name))
                        node.NickName = newName;
                }
            }

            this.Environment.RemoveSymbol(this.CurrentSpace.Name);

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
            var tmp = this.dynFunctionDict[this.CurrentSpace.Name];
            this.dynFunctionDict.Remove(this.CurrentSpace.Name);
            this.dynFunctionDict[newName] = tmp;

            ((FuncWorkspace)this.CurrentSpace).Name = newName;

            this.SaveFunction(this.CurrentSpace);
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

        void DisableEditNameBox()
        {
            this.editNameBox.Visibility = System.Windows.Visibility.Collapsed;
            this.editNameBox.IsEnabled = false;
            this.editNameBox.IsHitTestVisible = false;
            this.editNameBox.Focusable = false;

            this.workspaceLabel.Visibility = System.Windows.Visibility.Visible;

            editingName = false;
        }

        private bool hoveringEditBox = false;
        private dynNode draggedElementMenuItem;

        private void editNameBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            hoveringEditBox = true;
        }

        private void editNameBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            hoveringEditBox = false;
        }

        private void OverlayCanvas_OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.uiLocked)
                return;

            var el = draggedElement;

            var pos = e.GetPosition(overlayCanvas);

            Canvas.SetLeft(el, pos.X - dragOffset.X);
            Canvas.SetTop(el, pos.Y - dragOffset.Y);
        }

        private void OverlayCanvas_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.uiLocked)
                return;

            var el = draggedElement;

            var pos = e.GetPosition(this.workBench);

            this.overlayCanvas.Children.Clear();
            this.overlayCanvas.IsHitTestVisible = false;

            draggedElementMenuItem.Visibility = System.Windows.Visibility.Visible;
            draggedElementMenuItem = null;

            var outerPos = e.GetPosition(this.outerCanvas);

            if (outerPos.X >= 0 && outerPos.X <= this.overlayCanvas.ActualWidth
                && outerPos.Y >= 0 && outerPos.Y <= this.overlayCanvas.ActualHeight)
            {
                this.workBench.Children.Add(el);

                this.Elements.Add(el);

                el.WorkSpace = this.CurrentSpace;

                el.Opacity = 1;

                Canvas.SetLeft(el, Math.Max(pos.X - dragOffset.X, 0));
                Canvas.SetTop(el, Math.Max(pos.Y - dragOffset.Y, 0));

                el.EnableInteraction();

                if (this.ViewingHomespace)
                    el.SaveResult = true;
            }

            dragOffset = new Point();
        }

        SearchDictionary<dynNode> searchDict = new SearchDictionary<dynNode>();

        private bool dynamicRun = false;
        private bool runAgain = false;
        private bool uiLocked;
        private string UnlockLoadPath;

        void FilterAddMenu(HashSet<dynNode> elements)
        {
            foreach (Expander ex in this.stackPanel1.Children)
            {
                this.filterCategory(elements, ex);
            }
        }

        private void filterCategory(HashSet<dynNode> elements, Expander ex)
        {
            var content = (WrapPanel)ex.Content;

            bool filterWholeCategory = true;

            foreach (dynNode ele in content.Children)
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

        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var search = this.searchBox.Text.Trim();

            Match m;

            if (searchBarNumRegex.IsMatch(search))
            {
                storedSearchNum = Convert.ToDouble(search);
                this.FilterAddMenu(
                   new HashSet<dynNode>() 
               { 
                  this.addMenuItemsDictNew["Number"], 
                  this.addMenuItemsDictNew["Number Slider"] 
               }
                );
            }
            else if ((m = searchBarStrRegex.Match(search)).Success)  //(search.StartsWith("\""))
            {
                storedSearchStr = m.Groups[1].Captures[0].Value;
                this.FilterAddMenu(
                   new HashSet<dynNode>()
               {
                  this.addMenuItemsDictNew["String"]
               }
                );
            }
            else if (search.Equals("true") || search.Equals("false"))
            {
                storedSearchBool = Convert.ToBoolean(search);
                this.FilterAddMenu(
                   new HashSet<dynNode>()
               {
                  this.addMenuItemsDictNew["Boolean"]
               }
                );
            }
            else
            {
                this.storedSearchNum = 0;
                this.storedSearchStr = "";
                this.storedSearchBool = false;

                var filter = search.Length == 0
                   ? new HashSet<dynNode>(this.addMenuItemsDictNew.Values)
                   : searchDict.Search(search.ToLower());

                this.FilterAddMenu(filter);
            }
        }

        private void searchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (this.searchBox.Text.Equals(""))
            //   this.searchBox.Text = "Search";
        }

        public bool RunCancelled
        {
            get;
            private set;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.RunCancelled = true;
        }

        public bool DynamicRunEnabled
        {
            get
            {
                var topElements = this.homeSpace.Elements.Where(
                   x => !x.OutPort.Connectors.Any()
                );

                bool manTran = topElements.Any(x => x.RequiresManualTransaction());

                this.dynamicCheckBox.IsEnabled = !manTran && this.debugCheckBox.IsChecked == false;
                if (manTran)
                    this.dynamicCheckBox.IsChecked = false;

                return !manTran
                   && this.dynamicCheckBox.IsEnabled
                   && this.debugCheckBox.IsChecked == false
                   && this.dynamicCheckBox.IsChecked == true;
            }
        }

        internal void QueueRun()
        {
            this.RunCancelled = true;
            this.runAgain = true;
        }

        public DynamoUpdater Updater { get; private set; }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            this.sw.Flush();
            this.sw.Close();
            this.sw = new StringWriter();
            this.LogText = sw.ToString();
        }

        private void debugCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.dynamicCheckBox.IsChecked = false;
            this.dynamicCheckBox.IsEnabled = false;
        }

        private void debugCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.dynamicCheckBox.IsEnabled = true;
        }

        

    }

    public class dynSelection : ObservableCollection<System.Windows.Controls.UserControl>
    {
        public dynSelection() : base() { }
    }

    public class TypeLoadData
    {
        public Assembly assembly;
        public Type t;

        public TypeLoadData(Assembly assemblyIn, Type typeIn)
        {
            assembly = assemblyIn;
            t = typeIn;
        }
    }

    public enum TransactionMode
    {
        Automatic,
        Manual,
        Debug
    }

    public class DynamoWarningPrinter : Autodesk.Revit.DB.IFailuresPreprocessor
    {
        dynBench bench;

        public DynamoWarningPrinter(dynBench b)
        {
            this.bench = b;
        }

        public Autodesk.Revit.DB.FailureProcessingResult PreprocessFailures(Autodesk.Revit.DB.FailuresAccessor failuresAccessor)
        {
            var failList = failuresAccessor.GetFailureMessages();
            foreach (var fail in failList)
            {
                var severity = fail.GetSeverity();
                if (severity == Autodesk.Revit.DB.FailureSeverity.Warning)
                {
                    bench.Log(
                       "!! Warning: " + fail.GetDescriptionText()
                    );
                    failuresAccessor.DeleteWarning(fail);
                }
            }

            return Autodesk.Revit.DB.FailureProcessingResult.Continue;
        }
    }

    public class CancelEvaluationException : Exception
    {
        public bool Force;

        public CancelEvaluationException(bool force)
            : base("Run Cancelled")
        {
            this.Force = force;
        }
    }

}
