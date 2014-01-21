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
using Dynamo.UI;

namespace Dynamo.Views
{
    /// <summary>
    /// Interaction logic for dynWorkspaceView.xaml
    /// </summary>
    /// 
    public partial class dynWorkspaceView : UserControl
    {
        public enum CursorState
        {
            ArcAdding,
            ArcRemoving,
            ArcSelecting,
            NodeCondensation,
            NodeExpansion,
            LibraryClick,
            Drag,
            Move,
            Pan,
            ActivePan,
            UsualPointer,
            RectangularSelection,
            ResizeDiagonal,
            ResizeVertical,
            ResizeHorizontal
        }

        private Dynamo.Controls.DragCanvas WorkBench = null;
        private ZoomAndPanControl zoomAndPanControl = null;

        private PortViewModel snappedPort = null;
        private List<DependencyObject> hitResultsList = new List<DependencyObject>();
        Dictionary<CursorState, String> cursorSet = new Dictionary<CursorState, string>();

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

        internal bool IsSnappedToPort
        {
            get
            {
                return (this.snappedPort != null);
            }
        }

        public dynWorkspaceView()
        {
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoModernDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoColorsAndBrushesDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DataTemplatesDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.ConnectorsDictionary);

            InitializeComponent();

            selectionCanvas.Loaded += new RoutedEventHandler(selectionCanvas_Loaded);
            DataContextChanged += new DependencyPropertyChangedEventHandler(dynWorkspaceView_DataContextChanged);

            this.Loaded += new RoutedEventHandler(dynWorkspaceView_Loaded);
        }

        void dynWorkspaceView_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            // Add EndlessGrid
            endlessGrid = new EndlessGrid(outerCanvas);
            selectionCanvas.Children.Add(endlessGrid);
            zoomBorder.EndlessGrid = endlessGrid; // Register with ZoomBorder

            // Binding for grid lines HitTest and Visibility
            var binding = new Binding()
            {
                Path = new PropertyPath("DataContext.FullscreenWatchShowing"),
                Converter = new InverseBoolToVisibilityConverter(),
                Mode = BindingMode.OneWay,
            };
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(TabControl), 1);
            endlessGrid.SetBinding(UIElement.VisibilityProperty, binding);*/

            //============
            //LoadCursorState();
            //============

            Debug.WriteLine("Workspace loaded.");
            DynamoSelection.Instance.Selection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Selection_CollectionChanged);

            ViewModel.DragSelectionStarted += ViewModel_DragSelectionStarted;
            ViewModel.DragSelectionEnded += ViewModel_DragSelectionEnded;
        }

        /// <summary>
        /// Handler for the state machine's drag start event.
        /// Instructs the visualization manager to bypass updating the visualization.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ViewModel_DragSelectionEnded(object sender, EventArgs e)
        {
            if (ViewModel.PauseVisualizationManagerUpdatesCommand.CanExecute(false))
            {
                ViewModel.PauseVisualizationManagerUpdatesCommand.Execute(false);
            }
        }

        /// <summary>
        /// Handler for the state machine's drag end event.
        /// Instructs the visualization manager to update visualizations and begin tracking selections again.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ViewModel_DragSelectionStarted(object sender, EventArgs e)
        {
            if (ViewModel.PauseVisualizationManagerUpdatesCommand.CanExecute(true))
            {
                ViewModel.PauseVisualizationManagerUpdatesCommand.Execute(true);
            }
        }

        private void LoadCursorState()
        {
            cursorSet = new Dictionary<CursorState, string>();

            cursorSet.Add(CursorState.ArcAdding, "arc_add.cur");
            cursorSet.Add(CursorState.ArcRemoving, "arc_remove.cur");
            cursorSet.Add(CursorState.UsualPointer, "pointer.cur");
            cursorSet.Add(CursorState.RectangularSelection, "rectangular_selection.cur");
            cursorSet.Add(CursorState.ResizeDiagonal, "resize_diagonal.cur");
            cursorSet.Add(CursorState.ResizeHorizontal, "resize_horizontal.cur");
            cursorSet.Add(CursorState.ResizeVertical, "resize_vertical.cur");
            cursorSet.Add(CursorState.Pan, "hand_pan.cur");
            cursorSet.Add(CursorState.ActivePan, "hand_pan_active.cur");
            cursorSet.Add(CursorState.NodeExpansion, "expand.cur");
            cursorSet.Add(CursorState.NodeCondensation, "condense.cur");
            cursorSet.Add(CursorState.ArcRemoving, "arc_remove.cur");
            cursorSet.Add(CursorState.LibraryClick, "hand.cur");

        }

        void Selection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ViewModel.NodeFromSelectionCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// This WorkspaceView will be supporting multiple WorkspaceViewModel
        /// E.g. Home Workspace, Custom Workspaces
        /// 
        /// Handler for the DataContextChangedEvent. Handles registration of event listeners.
        /// Called during switching of workspace. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dynWorkspaceView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Remove before adding registration of event listener to prevent multiple registration 
            // to the same WorkspaceViewModel

            // Remove registration of event listener
            if (e.OldValue != null)
            {
                WorkspaceViewModel oldViewModel = (WorkspaceViewModel)e.OldValue;

                oldViewModel.CurrentOffsetChanged -= new PointEventHandler(vm_CurrentOffsetChanged);
                oldViewModel.ZoomChanged -= new ZoomEventHandler(vm_ZoomChanged);
                oldViewModel.RequestZoomToViewportCenter -= new ZoomEventHandler(vm_ZoomAtViewportCenter);
                oldViewModel.RequestZoomToViewportPoint -= new ZoomEventHandler(vm_ZoomAtViewportPoint);
                oldViewModel.RequestZoomToFitView -= new ZoomEventHandler(vm_ZoomToFitView);
                oldViewModel.RequestCenterViewOnElement -= new NodeEventHandler(CenterViewOnElement);
                oldViewModel.RequestNodeCentered -= new NodeEventHandler(vm_RequestNodeCentered);
                oldViewModel.RequestAddViewToOuterCanvas -= new ViewEventHandler(vm_RequestAddViewToOuterCanvas);
                oldViewModel.WorkspacePropertyEditRequested -= VmOnWorkspacePropertyEditRequested;
                oldViewModel.RequestSelectionBoxUpdate -= VmOnRequestSelectionBoxUpdate;
            }
            

            // Adding registration of event listener
            ViewModel.CurrentOffsetChanged += new PointEventHandler(vm_CurrentOffsetChanged);
            ViewModel.ZoomChanged += new ZoomEventHandler(vm_ZoomChanged);
            ViewModel.RequestZoomToViewportCenter += new ZoomEventHandler(vm_ZoomAtViewportCenter);
            ViewModel.RequestZoomToViewportPoint += new ZoomEventHandler(vm_ZoomAtViewportPoint);
            ViewModel.RequestZoomToFitView += new ZoomEventHandler(vm_ZoomToFitView);
            ViewModel.RequestCenterViewOnElement += new NodeEventHandler(CenterViewOnElement);
            ViewModel.RequestNodeCentered += new NodeEventHandler(vm_RequestNodeCentered);
            ViewModel.RequestAddViewToOuterCanvas += new ViewEventHandler(vm_RequestAddViewToOuterCanvas);
            ViewModel.WorkspacePropertyEditRequested += VmOnWorkspacePropertyEditRequested;
            ViewModel.RequestSelectionBoxUpdate += VmOnRequestSelectionBoxUpdate;

            ViewModel.Loaded();

            outerCanvas.Children.Remove(zoomAndPanControl);
            zoomAndPanControl = new ZoomAndPanControl(DataContext as WorkspaceViewModel);
            Canvas.SetRight(zoomAndPanControl, 10);
            Canvas.SetTop(zoomAndPanControl, 10);
            Canvas.SetZIndex(zoomAndPanControl, 8000);
            zoomAndPanControl.Focusable = false;
            outerCanvas.Children.Add(zoomAndPanControl);
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
                    Category = newCategory,
                    CanEditName = false
                };

            dynSettings.Controller.DynamoModel.OnRequestsFunctionNamePrompt(this, args);

            if (args.Success)
            {
                if (workspace is CustomNodeWorkspaceModel)
                {
                    var def = (workspace as CustomNodeWorkspaceModel).FunctionDefinition;
                    dynSettings.CustomNodeManager.Refactor(def.FunctionId, args.CanEditName ? args.Name : workspace.Name, args.Category, args.Description);
                }

                if (args.CanEditName) workspace.Name = args.Name;
                workspace.Description = args.Description;
                workspace.Category = args.Category;
            }
        }

        private void VmOnRequestSelectionBoxUpdate(object sender, SelectionBoxUpdateArgs e)
        {
            if (e.UpdatedProps.HasFlag(SelectionBoxUpdateArgs.UpdateFlags.Position))
            {
                Canvas.SetLeft(this.selectionBox, e.X);
                Canvas.SetTop(this.selectionBox, e.Y);
            }

            if (e.UpdatedProps.HasFlag(SelectionBoxUpdateArgs.UpdateFlags.Dimension))
            {
                selectionBox.Width = e.Width;
                selectionBox.Height = e.Height;
            }

            if (e.UpdatedProps.HasFlag(SelectionBoxUpdateArgs.UpdateFlags.Visibility))
                selectionBox.Visibility = e.Visibility;

            if (e.UpdatedProps.HasFlag(SelectionBoxUpdateArgs.UpdateFlags.Mode))
            {
                if (e.IsCrossSelection && (null == selectionBox.StrokeDashArray))
                    selectionBox.StrokeDashArray = new DoubleCollection { 4 };
                else if (!e.IsCrossSelection && (null != selectionBox.StrokeDashArray))
                    selectionBox.StrokeDashArray = null;
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
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //DrawGrid();
            //sw.Stop();
            //DynamoLogger.Instance.Log(string.Format("{0} elapsed for drawing grid.", sw.Elapsed));
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
            ModelEventArgs args = e as ModelEventArgs;
            ModelBase node = args.Model;

            double x = outerCanvas.ActualWidth / 2.0;
            double y = outerCanvas.ActualHeight / 2.0;

            // apply small perturbation
            // so node isn't right on top of last placed node
            if (currentNodeCascadeOffset > 96.0)
                currentNodeCascadeOffset = 0.0;

            x += currentNodeCascadeOffset;
            y += currentNodeCascadeOffset;

            currentNodeCascadeOffset += 24.0;

            if (args.PositionSpecified)
            {
                x = args.X;
                y = args.Y;
            }

            Point dropPt = new Point(x, y);

            // Transform dropPt from outerCanvas space into zoomCanvas space
            if (args.TransformCoordinates)
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
            node.ReportPosition();
        }

        void vm_CurrentOffsetChanged(object sender, EventArgs e)
        {
            zoomBorder.SetTranslateTransformOrigin((e as PointEventArgs).Point);
        }

        void vm_ZoomChanged(object sender, EventArgs e)
        {
            zoomBorder.SetZoom((e as ZoomEventArgs).Zoom);
        }

        void vm_ZoomAtViewportCenter(object sender, EventArgs e)
        {
            double zoom = AdjustZoomForCurrentZoomAmount((e as ZoomEventArgs).Zoom);

            // Limit Zoom
            double resultZoom = ViewModel._model.Zoom + zoom;
            if (resultZoom < WorkspaceModel.ZOOM_MINIMUM)
                resultZoom = WorkspaceModel.ZOOM_MINIMUM;
            else if (resultZoom > WorkspaceModel.ZOOM_MAXIMUM)
                resultZoom = WorkspaceModel.ZOOM_MAXIMUM;

            // Get Viewpoint Center point
            Point centerPoint = new Point();
            centerPoint.X = outerCanvas.ActualWidth / 2;
            centerPoint.Y = outerCanvas.ActualHeight / 2;

            // Get relative point of ZoomBorder child in relates to viewpoint center point
            Point relativePoint = new Point();
            relativePoint.X = (centerPoint.X - ViewModel._model.X) / ViewModel._model.Zoom;
            relativePoint.Y = (centerPoint.Y - ViewModel._model.Y) / ViewModel._model.Zoom;

            ZoomAtViewportPoint(zoom, relativePoint);
        }

        private double AdjustZoomForCurrentZoomAmount(double zoom)
        {
            const double upperLimit = .6;
            const double lowerLimit = .01;

            //quadratic adjustment
            //var adjustedZoom = (lowerLimit + Math.Pow((ViewModel._model.Zoom / WorkspaceModel.ZOOM_MAXIMUM), 2) * (upperLimit - lowerLimit)) * zoom;

            //linear adjustment
            var adjustedZoom = (lowerLimit + (ViewModel._model.Zoom / WorkspaceModel.ZOOM_MAXIMUM) * (upperLimit - lowerLimit)) * zoom;

            return adjustedZoom;
        }

        void vm_ZoomAtViewportPoint(object sender, EventArgs e)
        {
            double zoom = AdjustZoomForCurrentZoomAmount((e as ZoomEventArgs).Zoom);
            Point point = (e as ZoomEventArgs).Point;

            ZoomAtViewportPoint(zoom, point);
        }

        private void ZoomAtViewportPoint(double zoom, Point relative)
        {
            // Limit zoom
            double resultZoom = ViewModel._model.Zoom + zoom;
            if (resultZoom < WorkspaceModel.ZOOM_MINIMUM)
                resultZoom = WorkspaceModel.ZOOM_MINIMUM;
            else if (resultZoom > WorkspaceModel.ZOOM_MAXIMUM)
                resultZoom = WorkspaceModel.ZOOM_MAXIMUM;

            double absoluteX, absoluteY;
            absoluteX = relative.X * ViewModel._model.Zoom + ViewModel._model.X;
            absoluteY = relative.Y * ViewModel._model.Zoom + ViewModel._model.Y;
            Point resultOffset = new Point();
            resultOffset.X = absoluteX - (relative.X * resultZoom);
            resultOffset.Y = absoluteY - (relative.Y * resultZoom);

            ViewModel._model.Zoom = resultZoom;
            ViewModel._model.X = resultOffset.X;
            ViewModel._model.Y = resultOffset.Y;

            vm_CurrentOffsetChanged(this, new PointEventArgs(resultOffset));
            vm_ZoomChanged(this, new ZoomEventArgs(resultZoom));
        }

        void vm_ZoomToFitView(object sender, EventArgs e)
        {
            ZoomEventArgs zoomArgs = (e as ZoomEventArgs);

            double viewportPadding = 30;
            double fitWidth = outerCanvas.ActualWidth - 2 * viewportPadding;
            double fitHeight = outerCanvas.ActualHeight - 2 * viewportPadding;

            // Find the zoom required for fitview
            double scaleRequired = 1; // 100% zoom
            if (zoomArgs.hasZoom()) // FitView
                scaleRequired = zoomArgs.Zoom;
            else
            {
                double scaleX = fitWidth / zoomArgs.FocusWidth;
                double scaleY = fitHeight / zoomArgs.FocusHeight;
                scaleRequired = scaleX > scaleY ? scaleY : scaleX; // get least zoom required
            }

            // Limit Zoom
            if (scaleRequired > WorkspaceModel.ZOOM_MAXIMUM)
                scaleRequired = WorkspaceModel.ZOOM_MAXIMUM;
            else if (scaleRequired < WorkspaceModel.ZOOM_MINIMUM)
                scaleRequired = WorkspaceModel.ZOOM_MINIMUM;

            // Center position
            double centerOffsetX = viewportPadding + (fitWidth - (zoomArgs.FocusWidth * scaleRequired)) / 2;
            double centerOffsetY = viewportPadding + (fitHeight - (zoomArgs.FocusHeight * scaleRequired)) / 2;

            Point resultOffset = new Point();
            resultOffset.X = -(zoomArgs.Offset.X * scaleRequired) + centerOffsetX;
            resultOffset.Y = -(zoomArgs.Offset.Y * scaleRequired) + centerOffsetY;

            // Apply on model
            ViewModel._model.Zoom = scaleRequired;
            ViewModel._model.X = resultOffset.X;
            ViewModel._model.Y = resultOffset.Y;

            vm_CurrentOffsetChanged(this, new PointEventArgs(resultOffset));
            vm_ZoomChanged(this, new ZoomEventArgs(scaleRequired));
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

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WorkspaceViewModel wvm = (DataContext as WorkspaceViewModel);

            if (this.snappedPort != null)
                wvm.HandlePortClicked(this.snappedPort);
            else
            {
                wvm.HandleLeftButtonDown(this.WorkBench, e);
            }
        }

        private void OnMouseRelease(object sender, MouseButtonEventArgs e)
        {
            this.snappedPort = null;
            WorkspaceViewModel wvm = (DataContext as WorkspaceViewModel);
            wvm.HandleMouseRelease(this.WorkBench, e);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            this.snappedPort = null;

            bool mouseMessageHandled = false;
            WorkspaceViewModel wvm = (DataContext as WorkspaceViewModel);
            var currentState = wvm.CurrentState;

            switch (wvm.CurrentState)
            {
                // If we are currently connecting and there is an active 
                // connector, redraw it to match the new mouse coordinates.
                case WorkspaceViewModel.StateMachine.State.Connection:
                {
                    //Point mouse = e.GetPosition((UIElement)sender);
                    //this.snappedPort = GetSnappedPort(mouse);

                    // Check for nearby port to snap
                    //if (this.snappedPort != null)
                    //{
                    //    // Nearby port must be compatible for connection
                    //    if (wvm.CheckActiveConnectorCompatibility(this.snappedPort))
                    //    {
                    //        mouseMessageHandled = true;
                    //        wvm.HandleMouseMove(this.WorkBench, this.snappedPort.Center);
                    //    }
                    //    else
                    //        this.snappedPort = null; // remove non-compatible port
                    //}
                    //else
                    wvm.CurrentCursor = CursorLibrary.GetCursor(CursorSet.ArcSelect);
                    
                    break;
                }

                case WorkspaceViewModel.StateMachine.State.None:
                {
                    // Find the dependency object directly under the mouse 
                    // cursor, then see if it represents a port. If it does,
                    // then determine its type, we would like to show the 
                    // "ArcRemoving" cursor when the mouse is over an out port.
                    Point mouse = e.GetPosition((UIElement)sender);
                    var dependencyObject = ElementUnderMouseCursor(mouse);
                    PortViewModel pvm = PortFromHitTestResult(dependencyObject);

                    if (null != pvm && (pvm.PortType == PortType.INPUT))
                        this.Cursor = CursorLibrary.GetCursor(CursorSet.ArcSelect);
                    else
                        this.Cursor = null;

                    break;
                }
            }

            if (false == mouseMessageHandled)
                wvm.HandleMouseMove(this.WorkBench, e);
        }

        private PortViewModel GetSnappedPort(Point mouseCursor)
        {
            if (this.FindNearestPorts(mouseCursor).Count <= 0)
                return null;

            double curDistance = 1000000;
            PortViewModel nearestPort = null;

            for (int i = 0; i < this.hitResultsList.Count; i++)
            {
                try
                {
                    DependencyObject depObject = this.hitResultsList[i];
                    PortViewModel pvm = PortFromHitTestResult(depObject);

                    if (pvm == null)
                        continue;

                    double distance = Distance(mouseCursor, pvm.Center);
                    if (distance < curDistance)
                    {
                        curDistance = distance;
                        nearestPort = pvm;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return nearestPort;
        }

        private double Distance(Point mouse, Point point)
        {
            return Math.Sqrt(Math.Pow(mouse.X - point.X, 2) + Math.Pow(mouse.Y - point.Y, 2));
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
            this.Dispatcher.BeginInvoke((Action)delegate
                {

                    var vm = (DataContext as WorkspaceViewModel);

                    var n = (e as ModelEventArgs).Model;

                    if (WorkBench != null)
                    {
                        var b = WorkBench.TransformToAncestor(outerCanvas);

                        Point outerCenter = new Point(outerCanvas.ActualWidth / 2, outerCanvas.ActualHeight / 2);
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

        private void WorkBench_OnLoaded(object sender, RoutedEventArgs e)
        {
            WorkBench = sender as Dynamo.Controls.DragCanvas;
            WorkBench.owningWorkspace = this;
        }

        private PortViewModel PortFromHitTestResult(DependencyObject depObject)
        {
            Grid grid = depObject as Grid;
            if (null != grid)
                return grid.DataContext as PortViewModel;

            return null;
        }

        private DependencyObject ElementUnderMouseCursor(Point mouseCursor)
        {
            hitResultsList.Clear();
            var hitParams = new PointHitTestParameters(mouseCursor);
            VisualTreeHelper.HitTest(this, null,
                new HitTestResultCallback(DirectHitTestCallback),
                new PointHitTestParameters(mouseCursor));

            return ((hitResultsList.Count > 0) ? hitResultsList[0] : null);
        }

        private List<DependencyObject> FindNearestPorts(System.Windows.Point mouse)
        {
            hitResultsList.Clear();
            EllipseGeometry expandedHitTestArea = new EllipseGeometry(mouse, 50.0, 7.0);

            try
            {
                VisualTreeHelper.HitTest(this,
                    new HitTestFilterCallback(HitTestFilter),
                    new HitTestResultCallback(VisualCallback),
                    new GeometryHitTestParameters(expandedHitTestArea));
            }
            catch
            {
                hitResultsList.Clear();
            }

            return this.hitResultsList;
        }

        private HitTestFilterBehavior HitTestFilter(DependencyObject o)
        {
            if (o.GetType() == typeof(Viewport3D))
            {
                return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
            }
            else
            {
                return HitTestFilterBehavior.Continue;
            }
        }

        private HitTestResultBehavior VisualCallback(HitTestResult result)
        {
            if (result == null || result.VisualHit == null)
                throw new ArgumentNullException();

            if (result.VisualHit.GetType() == typeof(Grid))
                hitResultsList.Add(result.VisualHit);

            return HitTestResultBehavior.Continue;
        }

        private HitTestResultBehavior DirectHitTestCallback(HitTestResult result)
        {
            if (null != result && (null != result.VisualHit))
            {
                hitResultsList.Add(result.VisualHit);
                return HitTestResultBehavior.Stop;
            }

            return HitTestResultBehavior.Continue;
        }
    }
}
