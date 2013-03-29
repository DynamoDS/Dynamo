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

        public StringWriter sw;
        private string logText;
        private bool isWindowSelecting = false;
        private Point mouseDownPos;
        private SortedDictionary<string, TypeLoadData> builtinTypes = new SortedDictionary<string, TypeLoadData>();
        Point dragOffset;

        private ConnectorType connectorType;
        public ConnectorType ConnectorType
        {
            get { return connectorType; }
            set
            {
                connectorType = value;
                NotifyPropertyChanged("ConnectorType");
            }
        }

        Point transformOrigin;
        public Point TransformOrigin
        {
            get { return transformOrigin; }
            set 
            {
                transformOrigin = value;
                NotifyPropertyChanged("TransformOrigin");
            }
        }

        private bool consoleShowing = false;
        public bool ConsoleShowing
        {
            get { return consoleShowing; }
            set
            {
                consoleShowing = value;
                NotifyPropertyChanged("ConsoleShowing");
            }
        }

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
            this.ConnectorType = ConnectorType.BEZIER;
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

        public const int CANVAS_OFFSET_Y = 0;
        public const int CANVAS_OFFSET_X = 0;

        public Point CurrentOffset
        {
            get { return zoomBorder.GetTranslateTransformOrigin(); }
            set
            {
                if (zoomBorder != null)
                {
                    zoomBorder.SetTranslateTransformOrigin(value);
                }
                NotifyPropertyChanged("CurrentOffset");
            }
        }

        dynNodeUI draggedNode;

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

        void DrawGrid()
        {
            //clear the canvas's children
            //WorkBench.Children.Clear();
            double gridSpacing = 100.0;

            for (double i = 0.0; i < WorkBench.Width; i += gridSpacing)
            {
                Line xLine = new Line();
                xLine.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(100,100,100));
                xLine.X1 = i;
                xLine.Y1 = 0;
                xLine.X2 = i;
                xLine.Y2 = WorkBench.Height;
                xLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                xLine.VerticalAlignment = VerticalAlignment.Center;
                xLine.StrokeThickness = 1;
                WorkBench.Children.Add(xLine);
                Dynamo.Controls.DragCanvas.SetCanBeDragged(xLine, false);
                xLine.IsHitTestVisible = false;
            }
            for (double i = 0.0; i < WorkBench.Height; i += gridSpacing)
            {
                Line yLine = new Line();
                yLine.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100));
                yLine.X1 = 0;
                yLine.Y1 = i;
                yLine.X2 = WorkBench.Width;
                yLine.Y2 = i;
                yLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                yLine.VerticalAlignment = VerticalAlignment.Center;
                yLine.StrokeThickness = 1;
                WorkBench.Children.Add(yLine);
                Dynamo.Controls.DragCanvas.SetCanBeDragged(yLine, false);
                yLine.IsHitTestVisible = false;
            }
        }

        /// <summary>
        /// Called when the mouse has been moved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Canvas.SetLeft(debugPt, e.GetPosition(dynSettings.Workbench).X - debugPt.Width/2);
            Canvas.SetTop(debugPt, e.GetPosition(dynSettings.Workbench).Y - debugPt.Height / 2);
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

                if (mousePos.X > mouseDownPos.X)
                {
                    #region contain select
                    selectionBox.StrokeDashArray = null;
                    #endregion
                }
                else if (mousePos.X < mouseDownPos.X)
                {
                    #region crossing select
                    selectionBox.StrokeDashArray = new DoubleCollection() { 4 };
                    #endregion
                }
            }
        }

        /// <summary>
        /// Called when a mouse button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }

        void OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!WorkBench.IsConnecting)
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

        /// <summary>
        /// Called when a mouse button is released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Starting mouse up.");

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



        //private void Open_Click(object sender, RoutedEventArgs e)
        //{
        //    //string xmlPath = "C:\\test\\myWorkbench.xml";
        //    string xmlPath = "";

        //    System.Windows.Forms.OpenFileDialog openDialog = new OpenFileDialog()
        //    {
        //        Filter = "Dynamo Definitions (*.dyn; *.dyf)|*.dyn;*.dyf|All files (*.*)|*.*"
        //    };

        //    if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        xmlPath = openDialog.FileName;
        //    }

        //    if (!string.IsNullOrEmpty(xmlPath))
        //    {
        //        if (this.UILocked)
        //        {
        //            Controller.QueueLoad(xmlPath);
        //            return;
        //        }

        //        LockUI();
        //        if (!Controller.OpenDefinition(xmlPath))
        //        {
        //            //MessageBox.Show("Workbench could not be opened.");
        //            Log("Workbench could not be opened.");

        //            //dynSettings.Writer.WriteLine("Workbench could not be opened.");
        //            //dynSettings.Writer.WriteLine(xmlPath);

        //            if (DynamoCommands.WriteToLogCmd.CanExecute(null))
        //            {
        //                DynamoCommands.WriteToLogCmd.Execute("Workbench could not be opened.");
        //                DynamoCommands.WriteToLogCmd.Execute(xmlPath);
        //            }
        //        }
        //        UnlockUI();
        //    }
        //}

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

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //handle key presses for the bench in the bubbling event
            //if no other element has already handled this event it will 
            //start at the bench and move up to root, not raising the event
            //on any other elements

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
                focusElement.GetType() !=typeof(dynTextBox) && 
                !Keyboard.IsKeyDown(Key.LeftCtrl) &&
                !Keyboard.IsKeyDown(Key.RightCtrl) && 
                !Keyboard.IsKeyDown(Key.LeftShift) && 
                !Keyboard.IsKeyDown(Key.RightShift))
            {
                double x = 0;
                double y = 0;

                if (Keyboard.IsKeyDown(Key.Left))
                {
                    x = 20;
                    e.Handled = true;
                }
                if (Keyboard.IsKeyDown(Key.Right))
                {
                    x = -20;
                    e.Handled = true;
                }
                if (Keyboard.IsKeyDown(Key.Up))
                {
                    y = 20;
                    e.Handled = true;
                }
                if (Keyboard.IsKeyDown(Key.Down))
                {
                    y = -20;
                    e.Handled = true;
                }

                zoomBorder.IncrementTranslateOrigin(x, y);
                
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

            CurrentOffset = new Point(-x, -y);
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

            foreach (FrameworkElement ex in this.SideStackPanel.Children)
            {
                if (ex.GetType() == typeof (StackPanel)) // if search results
                {
                    ex.Visibility = Visibility.Collapsed;
                }
                else if (ex.GetType() == typeof(Expander))
                {
                    ex.Visibility = Visibility.Visible;
                    Controller.filterCategory(elements, (Expander) ex);
                }
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

        private static void SaveRTBAsPNG(RenderTargetBitmap bmp, string filename)
        {
            var enc = new System.Windows.Media.Imaging.PngBitmapEncoder();
            enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));

            using (var stm = System.IO.File.Create(filename))
            {
                enc.Save(stm);
            }
        }

        public bool UILocked { get; private set; }

        private void WorkBench_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
       
        }

        private void _this_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGrid();
            this.mainGrid.Focus();
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
