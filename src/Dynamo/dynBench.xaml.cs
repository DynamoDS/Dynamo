//Copyright 2013 Ian Keough

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
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.FSchemeInterop;
using Dynamo.Utilities;
using Dynamo.Commands;

using Path = System.IO.Path;
using Expression = Dynamo.FScheme.Expression;
using Value = Dynamo.FScheme.Value;


namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for DynamoForm.xaml
    /// </summary>
    public partial class dynBench : Window, INotifyPropertyChanged
    {
        double newX = 0.0;
        double newY = 0.0;
        double oldY = 0.0;
        double oldX = 0.0;

        private List<DependencyObject> hitResultsList = new List<DependencyObject>();
        private bool isPanning = false;
        private StringWriter sw;
        private string logText;
        private ConnectorType connectorType;
        private bool isWindowSelecting = false;
        private Point mouseDownPos;
        private SortedDictionary<string, TypeLoadData> builtinTypes = new SortedDictionary<string, TypeLoadData>();
        Point dragOffset;

        private dynConnector activeConnector;
        public dynConnector ActiveConnector
        {
            get { return activeConnector; }
            set { activeConnector = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
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

        public ConnectorType ConnectorType
        {
            get { return connectorType; }
        }

        DynamoController controller;
        public DynamoController Controller
        {
            get { return controller; }
            set
            {
                controller = value;
                NotifyPropertyChanged("Controller");
            }
        }

        internal dynBench(DynamoController controller)
        {
            Controller = controller;
            sw = new StringWriter();
            connectorType = ConnectorType.BEZIER;
        }

        public void LockUI()
        {
            this.UILocked = true;
            this.saveButton.IsEnabled = false;
            this.clearButton.IsEnabled = false;

            this.overlayCanvas.IsHitTestVisible = true;
            this.overlayCanvas.Cursor = System.Windows.Input.Cursors.AppStarting;
            this.overlayCanvas.ForceCursor = true;

            //this.workBench.Visibility = System.Windows.Visibility.Hidden;
        }

        public void UnlockUI()
        {
            this.UILocked = false;
            this.saveButton.IsEnabled = true;
            this.clearButton.IsEnabled = true;

            this.overlayCanvas.IsHitTestVisible = false;
            this.overlayCanvas.Cursor = null;
            this.overlayCanvas.ForceCursor = false;

            //this.workBench.Visibility = System.Windows.Visibility.Visible;
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

        double zoom = 1.0;
        public double Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;
                NotifyPropertyChanged("Zoom");
            }
        }

        public const int CANVAS_OFFSET_Y = 55;
        public const int CANVAS_OFFSET_X = 10;
        public double CurrentX
        {
            get { return Controller.CurrentSpace.PositionX; }
            set
            {
                Controller.CurrentSpace.PositionX = Math.Min(CANVAS_OFFSET_X, value);
                NotifyPropertyChanged("CurrentX");
            }
        }

        public double CurrentY
        {
            get { return Controller.CurrentSpace.PositionY; }
            set
            {
                Controller.CurrentSpace.PositionY = Math.Min(CANVAS_OFFSET_Y, value);
                NotifyPropertyChanged("CurrentY");
            }
        }

        dynNodeUI draggedNode;

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
            dynNodeUI el = sender as dynNodeUI;
            foreach (dynPort p in el.InPorts)
            {
                p.Update();
            }
            //el.OutPorts.ForEach(x => x.Update());
            foreach (dynPort p in el.OutPorts)
            {
                p.Update();
            }
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
            VisualTreeHelper.HitTest(WorkBench, null,
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
            WorkBench.Children.Clear();
            double gridSpacing = 100.0;

            for (double i = 0.0; i < WorkBench.Width; i += gridSpacing)
            {
                Line xLine = new Line();
                xLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                xLine.X1 = i;
                xLine.Y1 = 0;
                xLine.X2 = i;
                xLine.Y2 = WorkBench.Height;
                xLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                xLine.VerticalAlignment = VerticalAlignment.Center;
                xLine.StrokeThickness = 1;
                WorkBench.Children.Add(xLine);
                Dynamo.Controls.DragCanvas.SetCanBeDragged(xLine, false);
            }
            for (double i = 0.0; i < WorkBench.Height; i += gridSpacing)
            {
                Line yLine = new Line();
                yLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                yLine.X1 = 0;
                yLine.Y1 = i;
                yLine.X2 = WorkBench.Width;
                yLine.Y2 = i;
                yLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                yLine.VerticalAlignment = VerticalAlignment.Center;
                yLine.StrokeThickness = 1;
                WorkBench.Children.Add(yLine);
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
            //Debug.WriteLine("Mouse move.");

            //If we are currently connecting and there is an active connector,
            //redraw it to match the new mouse coordinates.
            if (WorkBench.IsConnecting && activeConnector != null)
            {
                activeConnector.Redraw(e.GetPosition(WorkBench));
            }

            //If we are currently dragging elements, redraw the element to
            //match the new mouse coordinates.
            if (WorkBench.isDragInProgress)
            {

                var allConnectors = WorkBench.Selection
                    .Where(x => x is dynNodeUI)
                    .Select(x => x as dynNodeUI)
                    .SelectMany(
                        el => el.OutPorts
                            .SelectMany(x => x.Connectors)
                            .Concat(el.InPorts.SelectMany(x => x.Connectors)));

                foreach (var connector in allConnectors)
                {
                    connector.Redraw();
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

            if (isWindowSelecting)
            {
                // When the mouse is held down, reposition the drag selection box.

                Point mousePos = e.GetPosition(WorkBench);

                if (mouseDownPos.X < mousePos.X)
                {
                    Canvas.SetLeft(selectionBox, mouseDownPos.X);
                    selectionBox.Width = mousePos.X - mouseDownPos.X;
                }
                else
                {
                    Canvas.SetLeft(selectionBox, mousePos.X);
                    selectionBox.Width = mouseDownPos.X - mousePos.X;
                }

                if (mouseDownPos.Y < mousePos.Y)
                {
                    Canvas.SetTop(selectionBox, mouseDownPos.Y);
                    selectionBox.Height = mousePos.Y - mouseDownPos.Y;
                }
                else
                {
                    Canvas.SetTop(selectionBox, mousePos.Y);
                    selectionBox.Height = mouseDownPos.Y - mousePos.Y;
                }
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            Controller.SaveAs();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            //save the active file
            Controller.Save();
        }

        /// <summary>
        /// Called when a mouse button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Starting mouse down.");

            mainGrid.Focus();

            //Pan with middle-click
            if (e.ChangedButton == MouseButton.Middle)
            {
                isPanning = true;
            }

            //close the tool finder if the user
            //has clicked anywhere else on the workbench
            if (dynToolFinder.Instance != null)
            {
                WorkBench.Children.Remove(dynToolFinder.Instance);
            }

            if (e.ChangedButton == MouseButton.Left && !WorkBench.IsConnecting)
            {
                #region window selection

                WorkBench.ClearSelection();

                //DEBUG WINDOW SELECTION
                // Capture and track the mouse.
                isWindowSelecting = true;
                mouseDownPos = e.GetPosition(WorkBench);
                //workBench.CaptureMouse();

                // Initial placement of the drag selection box.         
                Canvas.SetLeft(selectionBox, mouseDownPos.X);
                Canvas.SetTop(selectionBox, mouseDownPos.Y);
                selectionBox.Width = 0;
                selectionBox.Height = 0;

                // Make the drag selection box visible.
                selectionBox.Visibility = Visibility.Visible;

                #endregion
            }
        }

        /// <summary>
        /// Called when a mouse button is released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Starting mouse up.");

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
            {
                this.beginNameEditClick = false;

                if (isWindowSelecting)
                {
                    #region release window selection
                    //DEBUG WINDOW SELECTION
                    // Release the mouse capture and stop tracking it.
                    isWindowSelecting = false;
                    //workBench.ReleaseMouseCapture();

                    // Hide the drag selection box.
                    selectionBox.Visibility = Visibility.Collapsed;

                    Point mouseUpPos = e.GetPosition(WorkBench);

                    //clear the selected elements
                    WorkBench.ClearSelection();

                    

                    System.Windows.Rect rect =
                                new System.Windows.Rect(
                                    Canvas.GetLeft(selectionBox),
                                    Canvas.GetTop(selectionBox),
                                    selectionBox.Width,
                                    selectionBox.Height);

                    if (mouseUpPos.X > mouseDownPos.X)
                    {
                        #region contain select
                        foreach (dynNodeUI n in Controller.Nodes.Select(node => node.NodeUI))
                        {
                            //check if the node is within the boundary
                            double x0 = Canvas.GetLeft(n);
                            double y0 = Canvas.GetTop(n);
                            double x1 = x0 + n.Width;
                            double y1 = y0 + n.Height;

                            bool contains = rect.Contains(x0, y0) && rect.Contains(x1, y1);
                            if (contains)
                            {
                                if (!WorkBench.Selection.Contains(n))
                                    WorkBench.Selection.Add(n);
                            }
                        }
                        #endregion
                    }
                    else if (mouseUpPos.X < mouseDownPos.X)
                    {
                        #region crossing select
                        foreach (dynNodeUI n in Controller.Nodes.Select(node => node.NodeUI))
                        {
                            //check if the node is within the boundary
                            double x0 = Canvas.GetLeft(n);
                            double y0 = Canvas.GetTop(n);

                            bool intersects = rect.IntersectsWith(new Rect(x0, y0, n.Width, n.Height));
                            if (intersects)
                            {
                                if (!WorkBench.Selection.Contains(n))
                                    WorkBench.Selection.Add(n);
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
            }
        }

        public void Log(Exception e)
        {
            Log(e.GetType().ToString() + ":");
            Log(e.Message);
            Log(e.StackTrace);
        }

        internal void BeginDragElement(dynNodeUI nodeUI, string name, Point eleOffset)
        {
            if (this.UILocked)
                return;

            draggedElementMenuItem = nodeUI;

            var pos = Mouse.GetPosition(overlayCanvas);

            double x = pos.X;
            double y = pos.Y;

            this.dragOffset = eleOffset;

            dynNode newEl;
            try
            {
                newEl = Controller.CreateDragNode(name);
            }
            catch (Exception e)
            {
                Log(e);
                return;
            }

            newEl.NodeUI.GUID = Guid.NewGuid();

            //Add the element to the workbench
            overlayCanvas.Children.Add(newEl.NodeUI);

            newEl.NodeUI.Opacity = 0.7;

            x -= eleOffset.X;
            y -= eleOffset.Y;

            //Set its initial position
            Canvas.SetLeft(newEl.NodeUI, x);
            Canvas.SetTop(newEl.NodeUI, y);

            this.draggedNode = newEl.NodeUI;

            this.overlayCanvas.IsHitTestVisible = true;
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
                if (this.UILocked)
                {
                    Controller.QueueLoad(xmlPath);
                    return;
                }

                LockUI();
                if (!Controller.OpenDefinition(xmlPath))
                {
                    //MessageBox.Show("Workbench could not be opened.");
                    Log("Workbench could not be opened.");

                    //dynSettings.Writer.WriteLine("Workbench could not be opened.");
                    //dynSettings.Writer.WriteLine(xmlPath);

                    if (DynamoCommands.WriteToLogCmd.CanExecute(null))
                    {
                        DynamoCommands.WriteToLogCmd.Execute("Workbench could not be opened.");
                        DynamoCommands.WriteToLogCmd.Execute(xmlPath);
                    }
                }
                UnlockUI();
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            if (sw != null)
            {
                sw.Close();
                if (DynamoCommands.WriteToLogCmd.CanExecute(null))
                {
                    DynamoCommands.WriteToLogCmd.Execute("Dynamo ended " + System.DateTime.Now.ToString());
                }
                dynSettings.Writer.Close();
            }

            //end the transaction 
            //dynSettings.MainTransaction.Commit();
        }

        public void Log(string message)
        {
            sw.WriteLine(message);
            LogText = sw.ToString();
            //LogScroller.ScrollToEnd();

            //dynSettings.Writer.WriteLine(message);
            if (DynamoCommands.WriteToLogCmd.CanExecute(null))
            {
                DynamoCommands.WriteToLogCmd.Execute(message);
            }

            LogScroller.ScrollToBottom();
        }

        void OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //if you click on the canvas and you're connecting
            //then drop the connector, otherwise do nothing
            if (activeConnector != null)
            {
                activeConnector.Kill();
                WorkBench.IsConnecting = false;
                activeConnector = null;
            }

            if (editingName && !hoveringEditBox)
            {
                DisableEditNameBox();
            }

        }

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
                if (!dynSettings.Workbench.Children.Contains(dynToolFinder.Instance))
                {
                    dynSettings.Workbench.Children.Add(dynToolFinder.Instance);

                    Point p = Mouse.GetPosition(dynSettings.Workbench);
                    Canvas.SetLeft(dynToolFinder.Instance, p.X);
                    Canvas.SetTop(dynToolFinder.Instance, p.Y);
                    e.Handled = true;
                }
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.N))
            {
                Dictionary<string, object> paramDict = new Dictionary<string, object>();
                paramDict.Add("x", Mouse.GetPosition(dynSettings.Workbench).X);
                paramDict.Add("y", Mouse.GetPosition(dynSettings.Workbench).Y);
                paramDict.Add("workspace", Controller.CurrentSpace);
                paramDict.Add("text", "New Note");
                DynamoCommands.AddNoteCmd.Execute(paramDict);

                e.Handled = true;
            }

            IInputElement focusElement = FocusManager.GetFocusedElement(this);

            if (focusElement != null && 
                focusElement.GetType() != typeof(System.Windows.Controls.TextBox) &&
                focusElement.GetType() !=typeof(dynTextBox))
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
                    Controller.SaveNameEdit();
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
                SearchBox.Focus();
                SearchBox.SelectAll();
            }
        }

        //void toolFinder_ToolFinderFinished(object sender, EventArgs e)
        //{
        //    dynSettings.Workbench.Children.Remove(toolFinder);
        //    toolFinder = null;
        //}

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            LockUI();
            Controller.CleanWorkbench();

            //don't save the file path
            Controller.CurrentSpace.FilePath = "";

            UnlockUI();
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            //Controller.RunExpression(this.debugCheckBox.IsChecked == true);
            if (DynamoCommands.RunExpressionCmd.CanExecute(this.debugCheckBox.IsChecked))
            {
                DynamoCommands.RunExpressionCmd.Execute(this.debugCheckBox.IsChecked);
            }
        }

        //private void SaveFunction_Click(object sender, RoutedEventArgs e)
        //{
        //   SaveFunction(this.CurrentSpace);
        //}

        //private Dictionary<string, System.Windows.Controls.MenuItem> addMenuItemsDict
        //   = new Dictionary<string, System.Windows.Controls.MenuItem>();

        internal Dictionary<string, System.Windows.Controls.MenuItem> viewMenuItemsDict
           = new Dictionary<string, System.Windows.Controls.MenuItem>();

        internal Dictionary<string, Expander> addMenuCategoryDict
           = new Dictionary<string, Expander>();

        internal Dictionary<string, dynNodeUI> addMenuItemsDictNew
           = new Dictionary<string, dynNodeUI>();

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

                if (Controller.FunctionDict.ContainsKey(name))
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

            Controller.NewFunction(name, category, true);
        }

        internal void ChangeView_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem item = sender as System.Windows.Controls.MenuItem;

            Controller.DisplayFunction(item.Header.ToString());
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            Controller.ViewHomeWorkspace();
        }


        internal void setFunctionBackground()
        {
            //var bgBrush = (LinearGradientBrush)this.outerCanvas.Background;
            //bgBrush.GradientStops[0].Color = Color.FromArgb(0xFF, 0x6B, 0x6B, 0x6B); //Dark
            //bgBrush.GradientStops[1].Color = Color.FromArgb(0xFF, 0xBA, 0xBA, 0xBA); //Light

            var bgBrush = (SolidColorBrush)this.outerCanvas.Background;
            bgBrush.Color = Color.FromArgb(0xFF, 0xBA, 0xBA, 0xBA); //Light
        }


        internal void setHomeBackground()
        {
            //var bgBrush = (LinearGradientBrush)this.outerCanvas.Background;
            //bgBrush.GradientStops[0].Color = Color.FromArgb(0xFF, 0x4B, 0x4B, 0x4B); //Dark
            //bgBrush.GradientStops[1].Color = Color.FromArgb(0xFF, 0x7A, 0x7A, 0x7A); //Light

            var bgBrush = (SolidColorBrush)this.outerCanvas.Background;
            bgBrush.Color = Color.FromArgb(0xFF, 0x4B, 0x4B, 0x4B); //Dark
        }

        internal void RemoveConnector(dynConnector c)
        {
            Controller.CurrentSpace.Connectors.Remove(c);
        }

        internal void CenterViewOnElement(dynNodeUI e)
        {
            var left = Canvas.GetLeft(e);
            var top = Canvas.GetTop(e);

            var x = left + e.Width / 2 - this.outerCanvas.ActualWidth / 2;
            var y = top + e.Height / 2 - (this.outerCanvas.ActualHeight / 2 - this.LogScroller.ActualHeight);

            this.CurrentX = -x;
            this.CurrentY = -y;

            this.Zoom = 1;
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
                    Controller.SaveNameEdit();
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
            this.editNameBox.Text = Controller.CurrentSpace.Name;
            this.editNameBox.SelectAll();

            editingName = true;
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
        private dynNodeUI draggedElementMenuItem;

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
            if (this.UILocked)
                return;

            var el = draggedNode;

            var pos = e.GetPosition(overlayCanvas);

            Canvas.SetLeft(el, pos.X - dragOffset.X);
            Canvas.SetTop(el, pos.Y - dragOffset.Y);
        }

        private void OverlayCanvas_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.UILocked)
                return;

            var el = draggedNode;

            var pos = e.GetPosition(this.WorkBench);

            this.overlayCanvas.Children.Clear();
            this.overlayCanvas.IsHitTestVisible = false;

            draggedElementMenuItem.Visibility = System.Windows.Visibility.Visible;
            draggedElementMenuItem = null;

            var outerPos = e.GetPosition(this.outerCanvas);

            if (outerPos.X >= 0 && outerPos.X <= this.overlayCanvas.ActualWidth
                && outerPos.Y >= 0 && outerPos.Y <= this.overlayCanvas.ActualHeight)
            {
                this.WorkBench.Children.Add(el);

                Controller.Nodes.Add(el.NodeLogic);

                el.NodeLogic.WorkSpace = Controller.CurrentSpace;

                el.Opacity = 1;

                Canvas.SetLeft(el, Math.Max(pos.X - dragOffset.X, 0));
                Canvas.SetTop(el, Math.Max(pos.Y - dragOffset.Y, 0));

                el.EnableInteraction();

                if (Controller.ViewingHomespace)
                    el.NodeLogic.SaveResult = true;
            }

            dragOffset = new Point();
        }

        internal void FilterAddMenu(HashSet<dynNodeUI> elements)
        {
            foreach (Expander ex in this.SideStackPanel.Children)
            {
                Controller.filterCategory(elements, ex);
            }
        }

        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Controller.UpdateSearch(this.SearchBox.Text.Trim());
        }

        private void searchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (this.searchBox.Text.Equals(""))
            //   this.searchBox.Text = "Search";
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Controller.RunCancelled = true;
        }

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

        private void settings_curves_Checked(object sender, RoutedEventArgs e)
        {
            if (settings_plines != null)
            {
                this.connectorType = ConnectorType.BEZIER;
                settings_plines.IsChecked = false;
            }
        }

        private void settings_plines_Checked(object sender, RoutedEventArgs e)
        {
            if (settings_curves != null)
            {
                this.connectorType = ConnectorType.POLYLINE;
                settings_curves.IsChecked = false;
            }
        }

        private void saveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG Image|*.png";
            sfd.Title = "Save you Workbench to an Image";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string imagePath = sfd.FileName;

                Transform trans = WorkBench.LayoutTransform;
                WorkBench.LayoutTransform = null;
                Size size = new Size(WorkBench.Width, WorkBench.Height);
                WorkBench.Measure(size);
                WorkBench.Arrange(new Rect(size));

                //calculate the necessary width and height
                double width = 0;
                double height = 0;
                foreach (dynNodeUI n in Controller.Nodes.Select(x => x.NodeUI))
                {
                    Point relativePoint = n.TransformToAncestor(WorkBench)
                          .Transform(new Point(0, 0));

                    width = Math.Max(relativePoint.X + n.Width, width);
                    height = Math.Max(relativePoint.Y + n.Height, height);
                }

                Rect rect = VisualTreeHelper.GetDescendantBounds(WorkBench);

                RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right + 50,
                  (int)rect.Bottom + 50, 96, 96, System.Windows.Media.PixelFormats.Default);
                rtb.Render(WorkBench);
                //endcode as PNG
                BitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

                using (var stm = System.IO.File.Create(sfd.FileName))
                {
                    pngEncoder.Save(stm);
                }
            }
        }

        public static void SaveCanvas(double width, double height, Canvas canvas, int dpi, string filename)
        {

            //Size size = new Size(width, height);
            //canvas.Measure(size);
            //canvas.Arrange(new Rect(size));

            var rtb = new RenderTargetBitmap(
                (int)width, //width 
                (int)height, //height 
                dpi, //dpi x 
                dpi, //dpi y 
                PixelFormats.Pbgra32 // pixelformat 
                );
            rtb.Render(canvas);

            SaveRTBAsPNG(rtb, filename);
        }

        private static void SaveRTBAsPNG(RenderTargetBitmap bmp, string filename)
        {
            var enc = new System.Windows.Media.Imaging.PngBitmapEncoder();
            enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));

            using (var stm = System.IO.File.Create(filename))
            {
                enc.Save(stm);
            }
        }

        private void layoutAll_Click(object sender, RoutedEventArgs e)
        {
            LockUI();
            Controller.CleanWorkbench();

            double x = 0;
            double y = 0;
            double maxWidth = 0;    //track max width of current column
            double colGutter = 40;     //the space between columns
            double rowGutter = 40;
            int colCount = 0;

            Hashtable typeHash = new Hashtable();

            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                object[] attribs = t.GetCustomAttributes(typeof(NodeCategoryAttribute), false);

                if (t.Namespace == "Dynamo.Nodes" &&
                    !t.IsAbstract &&
                    attribs.Length > 0 &&
                    t.IsSubclassOf(typeof(dynNode)))
                {
                    NodeCategoryAttribute elCatAttrib = attribs[0] as NodeCategoryAttribute;

                    List<Type> catTypes = null;

                    if (typeHash.ContainsKey(elCatAttrib.ElementCategory))
                    {
                        catTypes = typeHash[elCatAttrib.ElementCategory] as List<Type>;
                    }
                    else
                    {
                        catTypes = new List<Type>();
                        typeHash.Add(elCatAttrib.ElementCategory, catTypes);
                    }

                    catTypes.Add(t);
                }
            }

            foreach (DictionaryEntry de in typeHash)
            {
                List<Type> catTypes = de.Value as List<Type>;

                //add the name of the category here
                //AddNote(de.Key.ToString(), x, y, Controller.CurrentSpace);
                Dictionary<string, object> paramDict = new Dictionary<string, object>();
                paramDict.Add("x", x);
                paramDict.Add("y", y);
                paramDict.Add("text", de.Key.ToString());
                paramDict.Add("workspace", Controller.CurrentSpace);
                DynamoCommands.AddNoteCmd.Execute(paramDict);

                y += 60;

                foreach (Type t in catTypes)
                {
                    object[] attribs = t.GetCustomAttributes(typeof(NodeNameAttribute), false);

                    NodeNameAttribute elNameAttrib = attribs[0] as NodeNameAttribute;
                    dynNode el = Controller.AddDynElement(
                           t, elNameAttrib.Name, Guid.NewGuid(), x, y,
                           Controller.CurrentSpace
                        );

                    el.DisableReporting();

                    maxWidth = Math.Max(el.NodeUI.Width, maxWidth);

                    colCount++;

                    y += el.NodeUI.Height + rowGutter;
                }

                y = 0;
                colCount = 0;
                x += maxWidth + colGutter;
                maxWidth = 0;

            }

            UnlockUI();
        }

        public bool UILocked { get; private set; }

        private void nodeFromSelection_Click(object sender, RoutedEventArgs e)
        {
            //if (this.NodeFromSelectionCmd.CanExecute(null))
            //{
            //    this.NodeFromSelectionCmd.Execute(null);
            //}
        }

        private void WorkBench_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
       
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            
        }

    }

    public class dynSelection : ObservableCollection<System.Windows.Controls.UserControl>
    {
        public dynSelection() : base() { }
    }

    public class TypeLoadData
    {
        public Assembly Assembly;
        public Type Type;

        public TypeLoadData(Assembly assemblyIn, Type typeIn)
        {
            Assembly = assemblyIn;
            Type = typeIn;
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
