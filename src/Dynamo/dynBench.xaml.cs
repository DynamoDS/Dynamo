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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dynamo.Commands;
using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Controls
{
    /// <summary>
    ///     Interaction logic for DynamoForm.xaml
    /// </summary>
    public partial class dynBench : Window, INotifyPropertyChanged
    {
        public const int CANVAS_OFFSET_Y = 0;
        public const int CANVAS_OFFSET_X = 0;
        private dynConnector activeConnector;

        internal Dictionary<string, Expander> addMenuCategoryDict
            = new Dictionary<string, Expander>();

        internal Dictionary<string, dynNodeUI> addMenuItemsDictNew
            = new Dictionary<string, dynNodeUI>();

        private bool beginNameEditClick;

        private SortedDictionary<string, TypeLoadData> builtinTypes = new SortedDictionary<string, TypeLoadData>();

        private ConnectorType connectorType;
        private bool consoleShowing;
        private DynamoController controller;
        private Point dragOffset;
        private dynNodeUI draggedElementMenuItem;
        private dynNodeUI draggedNode;
        private bool editingName;
        private bool hoveringEditBox;
        private bool isWindowSelecting;
        private string logText;
        private Point mouseDownPos;
        public StringWriter sw;

        private Point transformOrigin;

        internal dynBench(DynamoController controller)
        {
            Controller = controller;
            sw = new StringWriter();
            ConnectorType = ConnectorType.BEZIER;
        }

        public ConnectorType ConnectorType
        {
            get { return connectorType; }
            set
            {
                connectorType = value;
                NotifyPropertyChanged("ConnectorType");
            }
        }

        public Point TransformOrigin
        {
            get { return transformOrigin; }
            set
            {
                transformOrigin = value;
                NotifyPropertyChanged("TransformOrigin");
            }
        }

        public bool ConsoleShowing
        {
            get { return consoleShowing; }
            set
            {
                consoleShowing = value;
                NotifyPropertyChanged("ConsoleShowing");
            }
        }

        public dynConnector ActiveConnector
        {
            get { return activeConnector; }
            set { activeConnector = value; }
        }

        public DynamoController Controller
        {
            get { return controller; }
            set
            {
                controller = value;
                NotifyPropertyChanged("ViewModel");
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

        public bool UILocked { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Used by various properties to notify observers that a property has changed.
        /// </summary>
        /// <param name="info">What changed.</param>
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void LockUI()
        {
            UILocked = true;
            saveButton.IsEnabled = false;
            clearButton.IsEnabled = false;

            overlayCanvas.IsHitTestVisible = true;
            overlayCanvas.Cursor = Cursors.AppStarting;
            overlayCanvas.ForceCursor = true;

            //this.workBench.Visibility = System.Windows.Visibility.Hidden;
        }

        public void UnlockUI()
        {
            UILocked = false;
            saveButton.IsEnabled = true;
            clearButton.IsEnabled = true;

            overlayCanvas.IsHitTestVisible = false;
            overlayCanvas.Cursor = null;
            overlayCanvas.ForceCursor = false;

            //this.workBench.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        ///     Updates an element and all its ports.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UpdateElement(object sender, MouseButtonEventArgs e)
        {
            var el = sender as dynNodeUI;
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

        private void DrawGrid()
        {
            //clear the canvas's children
            //WorkBench.Children.Clear();
            double gridSpacing = 100.0;

            for (double i = 0.0; i < WorkBench.Width; i += gridSpacing)
            {
                var xLine = new Line();
                xLine.Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                xLine.X1 = i;
                xLine.Y1 = 0;
                xLine.X2 = i;
                xLine.Y2 = WorkBench.Height;
                xLine.HorizontalAlignment = HorizontalAlignment.Left;
                xLine.VerticalAlignment = VerticalAlignment.Center;
                xLine.StrokeThickness = 1;
                WorkBench.Children.Add(xLine);
                DragCanvas.SetCanBeDragged(xLine, false);
                xLine.IsHitTestVisible = false;
            }
            for (double i = 0.0; i < WorkBench.Height; i += gridSpacing)
            {
                var yLine = new Line();
                yLine.Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                yLine.X1 = 0;
                yLine.Y1 = i;
                yLine.X2 = WorkBench.Width;
                yLine.Y2 = i;
                yLine.HorizontalAlignment = HorizontalAlignment.Left;
                yLine.VerticalAlignment = VerticalAlignment.Center;
                yLine.StrokeThickness = 1;
                WorkBench.Children.Add(yLine);
                DragCanvas.SetCanBeDragged(yLine, false);
                yLine.IsHitTestVisible = false;
            }
        }

        /// <summary>
        ///     Called when the mouse has been moved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            //Canvas.SetLeft(debugPt, e.GetPosition(dynSettings.Workbench).X - debugPt.Width/2);
            //Canvas.SetTop(debugPt, e.GetPosition(dynSettings.Workbench).Y - debugPt.Height / 2);

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
                IEnumerable<dynConnector> allConnectors = WorkBench.Selection
                                                                   .Where(x => x is dynNodeUI)
                                                                   .Select(x => x as dynNodeUI)
                                                                   .SelectMany(
                                                                       el => el.OutPorts
                                                                               .SelectMany(x => x.Connectors)
                                                                               .Concat(
                                                                                   el.InPorts.SelectMany(
                                                                                       x => x.Connectors)));

                foreach (dynConnector connector in allConnectors)
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

                    selectionBox.StrokeDashArray = new DoubleCollection {4};

                    #endregion
                }
            }
        }

        /// <summary>
        ///     Called when a mouse button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
        }

        private void OnMouseLeftButtonDown(object sender, MouseEventArgs e)
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
        ///     Called when a mouse button is released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Starting mouse up.");

            if (e.ChangedButton == MouseButton.Left)
            {
                beginNameEditClick = false;

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

                    var rect =
                        new Rect(
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
            Log(e.GetType() + ":");
            Log(e.Message);
            Log(e.StackTrace);
        }


        internal void BeginDragElement(dynNodeUI nodeUI, string name, Point eleOffset)
        {
            if (UILocked)
                return;

            draggedElementMenuItem = nodeUI;

            Point pos = Mouse.GetPosition(overlayCanvas);

            double x = pos.X;
            double y = pos.Y;

            dragOffset = eleOffset;

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

            draggedNode = newEl.NodeUI;

            overlayCanvas.IsHitTestVisible = true;
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
        //            ViewModel.QueueLoad(xmlPath);
        //            return;
        //        }

        //        LockUI();
        //        if (!ViewModel.OpenDefinition(xmlPath))
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
                    DynamoCommands.WriteToLogCmd.Execute("Dynamo ended " + DateTime.Now.ToString());
                }

                dynSettings.FinishLogging();
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

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            //handle key presses for the bench in the bubbling event
            //if no other element has already handled this event it will 
            //start at the bench and move up to root, not raising the event
            //on any other elements

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.N))
            {
                var paramDict = new Dictionary<string, object>();
                paramDict.Add("x", Mouse.GetPosition(dynSettings.Workbench).X);
                paramDict.Add("y", Mouse.GetPosition(dynSettings.Workbench).Y);
                paramDict.Add("workspace", Controller.CurrentSpace);
                paramDict.Add("text", "New Note");
                DynamoCommands.AddNoteCmd.Execute(paramDict);

                e.Handled = true;
            }

            IInputElement focusElement = FocusManager.GetFocusedElement(this);

            if (focusElement != null &&
                focusElement.GetType() != typeof (TextBox) &&
                focusElement.GetType() != typeof (dynTextBox) &&
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


        //internal void ChangeView_Click(object sender, RoutedEventArgs e)
        //{
        //    System.Windows.Controls.MenuItem item = sender as System.Windows.Controls.MenuItem;

        //    ViewModel.DisplayFunction(item.Header.ToString());
        //}

        internal void setFunctionBackground()
        {
            //var bgBrush = (LinearGradientBrush)this.outerCanvas.Background;
            //bgBrush.GradientStops[0].Color = Color.FromArgb(0xFF, 0x6B, 0x6B, 0x6B); //Dark
            //bgBrush.GradientStops[1].Color = Color.FromArgb(0xFF, 0xBA, 0xBA, 0xBA); //Light

            var bgBrush = (SolidColorBrush) outerCanvas.Background;
            bgBrush.Color = Color.FromArgb(0xFF, 0xBA, 0xBA, 0xBA); //Light
        }

        internal void setHomeBackground()
        {
            //var bgBrush = (LinearGradientBrush)this.outerCanvas.Background;
            //bgBrush.GradientStops[0].Color = Color.FromArgb(0xFF, 0x4B, 0x4B, 0x4B); //Dark
            //bgBrush.GradientStops[1].Color = Color.FromArgb(0xFF, 0x7A, 0x7A, 0x7A); //Light

            var bgBrush = (SolidColorBrush) outerCanvas.Background;
            bgBrush.Color = Color.FromArgb(0xFF, 0x4B, 0x4B, 0x4B); //Dark
        }

        internal void RemoveConnector(dynConnector c)
        {
            Controller.CurrentSpace.Connectors.Remove(c);
        }

        internal void CenterViewOnElement(dynNodeUI e)
        {
            double left = Canvas.GetLeft(e);
            double top = Canvas.GetTop(e);

            double x = left + e.Width/2 - outerCanvas.ActualWidth/2;
            double y = top + e.Height/2 - (outerCanvas.ActualHeight/2 - LogScroller.ActualHeight);

            CurrentOffset = new Point(-x, -y);
        }

        private void image1_MouseEnter(object sender, MouseEventArgs e)
        {
            //highlight

            if (beginNameEditClick && e.LeftButton == MouseButtonState.Released)
            {
                beginNameEditClick = false;
            }
        }

        private void image1_MouseLeave(object sender, MouseEventArgs e)
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

        private void EnableEditNameBox()
        {
            workspaceLabel.Visibility = Visibility.Collapsed;

            editNameBox.Visibility = Visibility.Visible;
            editNameBox.IsEnabled = true;
            editNameBox.IsHitTestVisible = true;
            editNameBox.Focusable = true;
            editNameBox.Focus();
            editNameBox.Text = Controller.CurrentSpace.Name;
            editNameBox.SelectAll();

            editingName = true;
        }

        private void DisableEditNameBox()
        {
            editNameBox.Visibility = Visibility.Collapsed;
            editNameBox.IsEnabled = false;
            editNameBox.IsHitTestVisible = false;
            editNameBox.Focusable = false;

            workspaceLabel.Visibility = Visibility.Visible;

            editingName = false;
        }

        private void editNameBox_MouseEnter(object sender, MouseEventArgs e)
        {
            hoveringEditBox = true;
        }

        private void editNameBox_MouseLeave(object sender, MouseEventArgs e)
        {
            hoveringEditBox = false;
        }

        private void OverlayCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (UILocked)
                return;

            dynNodeUI el = draggedNode;

            Point pos = e.GetPosition(overlayCanvas);

            Canvas.SetLeft(el, pos.X - dragOffset.X);
            Canvas.SetTop(el, pos.Y - dragOffset.Y);
        }

        private void OverlayCanvas_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (UILocked)
                return;

            dynNodeUI el = draggedNode;

            Point pos = e.GetPosition(WorkBench);

            overlayCanvas.Children.Clear();
            overlayCanvas.IsHitTestVisible = false;

            draggedElementMenuItem.Visibility = Visibility.Visible;
            draggedElementMenuItem = null;

            Point outerPos = e.GetPosition(outerCanvas);

            if (outerPos.X >= 0 && outerPos.X <= overlayCanvas.ActualWidth
                && outerPos.Y >= 0 && outerPos.Y <= overlayCanvas.ActualHeight)
            {
                WorkBench.Children.Add(el);

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
            foreach (FrameworkElement ex in SideStackPanel.Children)
            {
                if (ex.GetType() == typeof (StackPanel)) // if search results
                {
                    ex.Visibility = Visibility.Collapsed;
                }
                else if (ex.GetType() == typeof (Expander))
                {
                    ex.Visibility = Visibility.Visible;
                    Controller.filterCategory(elements, (Expander) ex);
                }
            }
        }

        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Controller.UpdateSearch(SearchBox.Text.Trim());
        }

        private void searchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (this.searchBox.Text.Equals(""))
            //   this.searchBox.Text = "Filter";
        }

        private static void SaveRTBAsPNG(RenderTargetBitmap bmp, string filename)
        {
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmp));

            using (FileStream stm = File.Create(filename))
            {
                enc.Save(stm);
            }
        }

        private void WorkBench_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
        }

        private void _this_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGrid();
            mainGrid.Focus();
        }
    }

    public class dynSelection : ObservableCollection<UserControl>
    {
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
            Force = force;
        }
    }
}
