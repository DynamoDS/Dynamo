using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.Views
{
    /// <summary>
    /// Interaction logic for dynWorkspaceView.xaml
    /// </summary>
    public partial class dynWorkspaceView : UserControl
    {
        private bool isWindowSelecting;
        private Point mouseDownPos;
        private Dynamo.Controls.DragCanvas WorkBench = null;
        public WorkspaceViewModel ViewModel
        {
            get
            {
                if (this.DataContext is WorkspaceViewModel)
                    return this.DataContext as WorkspaceViewModel;
                else
                    return null;
            }
        }

        public dynWorkspaceView()
        {
            InitializeComponent();

            selectionCanvas.Loaded += new RoutedEventHandler(selectionCanvas_Loaded);
            DataContextChanged += new DependencyPropertyChangedEventHandler(dynWorkspaceView_DataContextChanged);

            this.Loaded += new RoutedEventHandler(dynWorkspaceView_Loaded);
        }

        void dynWorkspaceView_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Workspace loaded.");
            DynamoSelection.Instance.Selection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Selection_CollectionChanged);
        }

        void Selection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ViewModel.NodeFromSelectionCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Handler for the DataContextChangedEvent. Hanndles registration of event listeners.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dynWorkspaceView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel.Loaded();
            ViewModel.CurrentOffsetChanged += new PointEventHandler(vm_CurrentOffsetChanged);
            ViewModel.ZoomChanged += new ZoomEventHandler(vm_ZoomChanged);
            ViewModel.StopDragging += new EventHandler(vm_StopDragging);
            ViewModel.RequestCenterViewOnElement += new NodeEventHandler(CenterViewOnElement);
            ViewModel.RequestNodeCentered += new NodeEventHandler(vm_RequestNodeCentered);
            ViewModel.RequestAddViewToOuterCanvas += new ViewEventHandler(vm_RequestAddViewToOuterCanvas);
            ViewModel.WorkspacePropertyEditRequested -= VmOnWorkspacePropertyEditRequested;
            ViewModel.WorkspacePropertyEditRequested += VmOnWorkspacePropertyEditRequested;
        }

        private void VmOnWorkspacePropertyEditRequested(WorkspaceModel workspace)
        {

            // copy these strings
            var newName = workspace.Name.Substring(0);
            var newCategory = workspace.Category.Substring(0);
            var newDescription = workspace.Description.Substring(0);

            var args = new FunctionNamePromptEventArgs
                {
                    Name = newName,
                    Description = newDescription,
                    Category = newCategory

                };

            dynSettings.Controller.DynamoModel.OnRequestsFunctionNamePrompt(this, args);

            if(args.Success)
            {

                if (workspace is FuncWorkspace)
                {
                    var def = dynSettings.CustomNodeManager.GetDefinitionFromWorkspace(workspace);
                    dynSettings.CustomNodeManager.Refactor(def.FunctionId, args.Name, args.Category, args.Description);
                }

                workspace.Name = args.Name;
                workspace.Description = args.Description;
                workspace.Category = args.Category;
                // workspace.Author = "";

            }
        }

        public void WorkspacePropertyEditClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var vm = DataContext as WorkspaceViewModel;
            if (vm != null)
            {
                vm.OnWorkspacePropertyEditRequested();
            }
        }

        void selectionCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            DrawGrid();
            sw.Stop();
            DynamoLogger.Instance.Log(string.Format("{0} elapsed for drawing grid.", sw.Elapsed));
        }

        void vm_RequestAddViewToOuterCanvas(object sender, EventArgs e)
        {
            UserControl view = (UserControl)((e as ViewEventArgs).View);
            outerCanvas.Children.Add(view);
            Canvas.SetBottom(view, 0);
            Canvas.SetRight(view, 0);
        }

        private double currentNodeCascadeOffset = 0.0;

        void vm_RequestNodeCentered(object sender, EventArgs e)
        {
            double x = 0;
            double y = 0;
            ModelBase node = (e as ModelEventArgs).Model;
            Dictionary<string, object> data = (e as ModelEventArgs).Data;

            x = outerCanvas.ActualWidth / 2.0;
            y = outerCanvas.ActualHeight / 2.0;

            // apply small perturbation
            // so node isn't right on top of last placed node
            if (currentNodeCascadeOffset > 96.0)
            {
                currentNodeCascadeOffset = 0.0;
            }

            x += currentNodeCascadeOffset;
            y += currentNodeCascadeOffset;

            currentNodeCascadeOffset += 24.0;
            
            var transformFromOuterCanvas = data.ContainsKey("transformFromOuterCanvasCoordinates");

            if (data.ContainsKey("x"))
                x = (double)data["x"];

            if (data.ContainsKey("y"))
                y = (double)data["y"];

            Point dropPt = new Point(x, y);

            // Transform dropPt from outerCanvas space into zoomCanvas space
            if (transformFromOuterCanvas)
            {
                if (WorkBench != null)
                {
                    var a = outerCanvas.TransformToDescendant(WorkBench);
                    dropPt = a.Transform(dropPt);
                } 
            }
            
            // center the node at the drop point
            if (!Double.IsNaN(node.Width))
                dropPt.X -= (node.Width / 2.0);

            if (!Double.IsNaN(node.Height))
                dropPt.Y -= (node.Height / 2.0);

            if (!Double.IsNaN(node.Width))
                dropPt.X -= (node.Height / 2.0);

            if (!Double.IsNaN(node.Height))
                dropPt.Y -= (node.Height / 2.0);

            node.X = dropPt.X;
            node.Y = dropPt.Y;
        }

        void vm_StopDragging(object sender, EventArgs e)
        {
            WorkBench.CancelDragOperation();
            WorkBench.ignoreClick = true;
        }

        void zoomBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.MiddleButton == MouseButtonState.Pressed)
                (DataContext as WorkspaceViewModel).SetCurrentOffsetCommand.Execute((sender as ZoomBorder).GetTranslateTransformOrigin());
        }

        void zoomBorder_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
                (DataContext as WorkspaceViewModel).SetCurrentOffsetCommand.Execute((sender as ZoomBorder).GetTranslateTransformOrigin());
        }

        void vm_CurrentOffsetChanged(object sender, EventArgs e)
        {
            zoomBorder.SetTranslateTransformOrigin((e as PointEventArgs).Point);
        }

        void vm_ZoomChanged(object sender, EventArgs e)
        {
            zoomBorder.SetZoom((e as ZoomEventArgs).Zoom);
        }

        private void dynWorkspaceView_KeyDown(object sender, KeyEventArgs e)
        {
            Button source = e.Source as Button;

            if (source == null)
                return;

            if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl)
                return;

        }

        private void dynWorkspaceView_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void DynWorkspaceView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WorkspaceViewModel vm = (DataContext as WorkspaceViewModel);

            if (!(DataContext as WorkspaceViewModel).IsConnecting)
            {
                #region window selection

                //WorkBench.ClearSelection();
                DynamoSelection.Instance.ClearSelection();

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
            if (vm != null)
            {
                if (vm.ActiveConnector != null)
                {
                    vm.IsConnecting = false;
                    vm.ActiveConnector = null;
                }
            }

            //if (editingName && !hoveringEditBox)
            //{
            //    DisableEditNameBox();
            //}

            dynSettings.ReturnFocusToSearch();
        }

        private void DynWorkspaceView_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Starting mouse up.");

            WorkspaceViewModel vm = (DataContext as WorkspaceViewModel);

            if (e.ChangedButton == MouseButton.Left)
            {
                //MVVM: this is in the bench, should it be here?
                //beginNameEditClick = false;

                if (isWindowSelecting)
                {
                    #region release window selection

                    //DEBUG WINDOW SELECTION
                    // Release the mouse capture and stop tracking it.
                    isWindowSelecting = false;
                    //workBench.ReleaseMouseCapture();

                    // Hide the drag selection box.
                    selectionBox.Visibility = Visibility.Collapsed;

                    #endregion
                }
            }
        }

        private void DynWorkspaceView_OnMouseMove(object sender, MouseEventArgs e)
        {
            var vm = (DataContext as WorkspaceViewModel);

            //Canvas.SetLeft(debugPt, e.GetPosition(dynSettings.Workbench).X - debugPt.Width/2);
            //Canvas.SetTop(debugPt, e.GetPosition(dynSettings.Workbench).Y - debugPt.Height / 2);

            //If we are currently connecting and there is an active connector,
            //redraw it to match the new mouse coordinates.
            if (vm.IsConnecting && vm.ActiveConnector != null)
            {
                vm.ActiveConnector.Redraw(e.GetPosition(WorkBench));
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

                //clear the selected elements
                DynamoSelection.Instance.ClearSelection();

                var rect =
                    new Rect(
                        Canvas.GetLeft(selectionBox),
                        Canvas.GetTop(selectionBox),
                        selectionBox.Width,
                        selectionBox.Height);

                if (mousePos.X > mouseDownPos.X)
                {
                    #region contain select

                    selectionBox.StrokeDashArray = null;
                    vm.ContainSelectCommand.Execute(rect);

                    #endregion
                }
                else if (mousePos.X < mouseDownPos.X)
                {
                    #region crossing select

                    selectionBox.StrokeDashArray = new DoubleCollection { 4 };
                    vm.CrossSelectCommand.Execute(rect);

                    #endregion
                }
            }
        }

        private void WorkBench_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
        }

        /// <summary>
        /// Centers the view on a node by changing the workspace's CurrentOffset.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void CenterViewOnElement(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action) delegate
                {

                    var vm = (DataContext as WorkspaceViewModel);

                    var n = (e as ModelEventArgs).Model;

                    if (WorkBench != null)
                    {
                        var b = WorkBench.TransformToAncestor(outerCanvas);

                        Point outerCenter = new Point(outerCanvas.ActualWidth/2, outerCanvas.ActualHeight/2);
                        Point nodeCenterInCanvas = new Point(n.X + n.Width / 2, n.Y + n.Height / 2);
                        Point nodeCenterInOverlay = b.Transform(nodeCenterInCanvas);

                        double deltaX = nodeCenterInOverlay.X - outerCenter.X;
                        double deltaY = nodeCenterInOverlay.Y - outerCenter.Y;

                        //var offset = new Point(vm.CurrentOffset.X - deltaX, vm.CurrentOffset.Y - deltaY);

                        //vm.CurrentOffset = offset;

                        zoomBorder.SetTranslateTransformOrigin(new Point(vm.Model.X - deltaX, vm.Model.Y - deltaY));
                    } 
                });
        }

        private void DrawGrid()
        {
            //clear the canvas's children
            //WorkBench.Children.Clear();
            double gridSpacing = 100.0;

            selectionCanvas.UseLayoutRounding = true;

            // draw vertical lines on grid
            for (double i = 0.0; i < selectionCanvas.ActualWidth; i += gridSpacing)
            {
                var xLine = new Line();
                xLine.Stroke = new SolidColorBrush(Color.FromArgb(255, 180, 180, 180));
                xLine.X1 = i;
                xLine.Y1 = 0;
                xLine.X2 = i;
                xLine.Y2 = selectionCanvas.ActualHeight;
                xLine.HorizontalAlignment = HorizontalAlignment.Left;
                xLine.VerticalAlignment = VerticalAlignment.Center;
                xLine.StrokeThickness = 1;
                selectionCanvas.Children.Add(xLine);

                Line xLine2 = null;
                if (i == 0.0)
                {

                    xLine.Stroke = new SolidColorBrush(Color.FromArgb(255, 140, 140, 140));

                    xLine2 = new Line();
                    xLine2.Stroke = new SolidColorBrush(Color.FromArgb(70, 180, 180, 180));
                    xLine2.HorizontalAlignment = HorizontalAlignment.Left;
                    xLine2.VerticalAlignment = VerticalAlignment.Center;
                    xLine2.StrokeThickness = 6;
                    xLine2.Y1 = xLine.Y1 + 6.5;
                    xLine2.X1 = xLine.X1 + 3.5;
                    xLine2.X2 = xLine.X2 + 3.5;
                    xLine2.Y2 = selectionCanvas.ActualHeight;
                    xLine2.IsHitTestVisible = false;
                    selectionCanvas.Children.Add(xLine2);
                    
                }
 
                //Dynamo.Controls.DragCanvas.SetCanBeDragged(xLine, false);
                xLine.IsHitTestVisible = false;

                Binding binding = new Binding() 
                { 
                    Path = new PropertyPath("FullscreenWatchVisible"), 
                    Converter = new InverseBoolToVisibilityConverter(),
                    Mode = BindingMode.OneWay,
                };
                xLine.SetBinding(UIElement.VisibilityProperty, binding);
                if (xLine2 != null)
                {
                    xLine2.SetBinding(UIElement.VisibilityProperty, binding);
                }
            }

            // draw horizontal lines on grid
            for (double i = 0.0; i < selectionCanvas.ActualHeight; i += gridSpacing)
            {
                var yLine = new Line();
                yLine.Stroke = new SolidColorBrush(Color.FromArgb(255, 180, 180, 180));
                yLine.X1 = -0.5;
                yLine.Y1 = i;
                yLine.X2 = selectionCanvas.ActualWidth;
                yLine.Y2 = i;
                yLine.HorizontalAlignment = HorizontalAlignment.Left;
                yLine.VerticalAlignment = VerticalAlignment.Center;
                yLine.StrokeThickness = 1;
                selectionCanvas.Children.Add(yLine);

                Line yLine2 = null;
                if (i == 0.0)
                {
                    yLine.Stroke = new SolidColorBrush(Color.FromArgb(255, 140, 140, 140));

                    yLine2 = new Line();
                    yLine2.Stroke = new SolidColorBrush(Color.FromArgb(70, 180, 180, 180));
                    yLine2.StrokeThickness = 6;
                    yLine2.X1 = 0;
                    yLine2.X2 = selectionCanvas.ActualWidth;
                    yLine2.Y1 = yLine.Y1 + 3.5;
                    yLine2.Y2 = yLine.Y2 + 3.5;
                    yLine2.HorizontalAlignment = HorizontalAlignment.Left;
                    yLine2.VerticalAlignment = VerticalAlignment.Center;
                    yLine2.IsHitTestVisible = false;
                    selectionCanvas.Children.Add(yLine2);
                }
                
                //Dynamo.Controls.DragCanvas.SetCanBeDragged(yLine, false);
                
                yLine.IsHitTestVisible = false;

                Binding binding = new Binding()
                {
                    Path = new PropertyPath("FullscreenWatchVisible"),
                    Converter = new InverseBoolToVisibilityConverter(),
                    Mode = BindingMode.OneWay,
                };
                yLine.SetBinding(UIElement.VisibilityProperty, binding);
                if (yLine2 != null)
                {
                    yLine2.SetBinding(UIElement.VisibilityProperty, binding);
                }
            }
        }

        private void WorkBench_OnLoaded(object sender, RoutedEventArgs e)
        {
            WorkBench = sender as Dynamo.Controls.DragCanvas;
            WorkBench.owningWorkspace = this;
            //DrawGrid();
        }
    }
}
