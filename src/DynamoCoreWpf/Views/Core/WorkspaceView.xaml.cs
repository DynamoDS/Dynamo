using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.UI;
using System.Collections.Specialized;
using System.Threading;

using Dynamo.Wpf.Utilities;
using Dynamo.Search.SearchElements;
using Dynamo.UI.Controls;


namespace Dynamo.Views
{
    /// <summary>
    /// Interaction logic for WorkspaceView.xaml
    /// </summary>
    /// 
    public partial class WorkspaceView : UserControl
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

        public WorkspaceView()
        {
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoModernDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoColorsAndBrushesDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DataTemplatesDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.ConnectorsDictionary);

            InitializeComponent();

            selectionCanvas.Loaded += OnSelectionCanvasLoaded;
            DataContextChanged += OnWorkspaceViewDataContextChanged;

            Loaded += OnWorkspaceViewLoaded;
            Unloaded += OnWorkspaceViewUnloaded;
        }

        void OnWorkspaceViewLoaded(object sender, RoutedEventArgs e)
        {
            DynamoSelection.Instance.Selection.CollectionChanged += new NotifyCollectionChangedEventHandler(OnSelectionCollectionChanged);

            ViewModel.RequestShowInCanvasSearch += ShowHideInCanvasControl;

            var ctrl = InCanvasSearchBar.Child as InCanvasSearchControl;
            if (ctrl != null)
            {
                ctrl.RequestShowInCanvasSearch += ShowHideInCanvasControl;
            }
        }

        void OnWorkspaceViewUnloaded(object sender, RoutedEventArgs e)
        {
            DynamoSelection.Instance.Selection.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnSelectionCollectionChanged);
            
            if (ViewModel != null)
                ViewModel.RequestShowInCanvasSearch -= ShowHideInCanvasControl;

            var ctrl = InCanvasSearchBar.Child as InCanvasSearchControl;
            if (ctrl != null)
            {
                ctrl.RequestShowInCanvasSearch -= ShowHideInCanvasControl;
            }
        }

        /// <summary>
        /// Handler for the state machine's drag start event.
        /// Instructs the visualization manager to bypass updating the visualization.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnViewModelDragSelectionEnded(object sender, EventArgs e)
        {
            if (ViewModel.UnPauseVisualizationManagerCommand.CanExecute(true))
            {
                ViewModel.UnPauseVisualizationManagerCommand.Execute(true);
            }
        }

        /// <summary>
        /// Handler for the state machine's drag end event.
        /// Instructs the visualization manager to update visualizations and begin tracking selections again.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnViewModelDragSelectionStarted(object sender, EventArgs e)
        {
            if (ViewModel.PauseVisualizationManagerCommand.CanExecute(true))
            {
                ViewModel.PauseVisualizationManagerCommand.Execute(true);
            }
        }

        private void LoadCursorState()
        {
            cursorSet = new Dictionary<CursorState, string>
            {
                { CursorState.ArcAdding, "arc_add.cur" },
                { CursorState.ArcRemoving, "arc_remove.cur" },
                { CursorState.UsualPointer, "pointer.cur" },
                { CursorState.RectangularSelection, "rectangular_selection.cur" },
                { CursorState.ResizeDiagonal, "resize_diagonal.cur" },
                { CursorState.ResizeHorizontal, "resize_horizontal.cur" },
                { CursorState.ResizeVertical, "resize_vertical.cur" },
                { CursorState.Pan, "hand_pan.cur" },
                { CursorState.ActivePan, "hand_pan_active.cur" },
                { CursorState.NodeExpansion, "expand.cur" },
                { CursorState.NodeCondensation, "condense.cur" },
                { CursorState.ArcRemoving, "arc_remove.cur" },
                { CursorState.LibraryClick, "hand.cur" }
            };
        }

        void OnSelectionCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            ViewModel.NodeFromSelectionCommand.RaiseCanExecuteChanged();
            ViewModel.DynamoViewModel.AddAnnotationCommand.RaiseCanExecuteChanged();
            ViewModel.DynamoViewModel.UngroupAnnotationCommand.RaiseCanExecuteChanged();
            ViewModel.DynamoViewModel.UngroupModelCommand.RaiseCanExecuteChanged();
            ViewModel.DynamoViewModel.AddModelsToGroupModelCommand.RaiseCanExecuteChanged();
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
        void OnWorkspaceViewDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Remove before adding registration of event listener to prevent multiple registration 
            // to the same WorkspaceViewModel

            // Remove registration of event listener
            if (e.OldValue != null)
            {
                WorkspaceViewModel oldViewModel = (WorkspaceViewModel)e.OldValue;

                oldViewModel.Model.CurrentOffsetChanged -= vm_CurrentOffsetChanged;
                oldViewModel.Model.ZoomChanged -= vm_ZoomChanged;
                oldViewModel.RequestZoomToViewportCenter -= vm_ZoomAtViewportCenter;
                oldViewModel.RequestZoomToViewportPoint -= vm_ZoomAtViewportPoint;
                oldViewModel.RequestZoomToFitView -= vm_ZoomToFitView;
                oldViewModel.RequestCenterViewOnElement -= CenterViewOnElement;
                oldViewModel.Model.RequestNodeCentered -= vm_RequestNodeCentered;
                oldViewModel.RequestAddViewToOuterCanvas -= vm_RequestAddViewToOuterCanvas;
                oldViewModel.WorkspacePropertyEditRequested -= VmOnWorkspacePropertyEditRequested;
                oldViewModel.RequestSelectionBoxUpdate -= VmOnRequestSelectionBoxUpdate;
                oldViewModel.DragSelectionStarted -= OnViewModelDragSelectionStarted;
                oldViewModel.DragSelectionEnded -= OnViewModelDragSelectionEnded;
            }

            if (ViewModel != null)
            {
                // Adding registration of event listener
                ViewModel.Model.CurrentOffsetChanged += vm_CurrentOffsetChanged;
                ViewModel.Model.ZoomChanged +=vm_ZoomChanged;
                ViewModel.RequestZoomToViewportCenter += vm_ZoomAtViewportCenter;
                ViewModel.RequestZoomToViewportPoint += vm_ZoomAtViewportPoint;
                ViewModel.RequestZoomToFitView += vm_ZoomToFitView;
                ViewModel.RequestCenterViewOnElement += CenterViewOnElement;
                ViewModel.Model.RequestNodeCentered += vm_RequestNodeCentered;
                ViewModel.RequestAddViewToOuterCanvas += vm_RequestAddViewToOuterCanvas;
                ViewModel.WorkspacePropertyEditRequested += VmOnWorkspacePropertyEditRequested;
                ViewModel.RequestSelectionBoxUpdate += VmOnRequestSelectionBoxUpdate;
                ViewModel.DragSelectionStarted += OnViewModelDragSelectionStarted;
                ViewModel.DragSelectionEnded += OnViewModelDragSelectionEnded;

                ViewModel.Loaded();
            }
        }

        private void VmOnWorkspacePropertyEditRequested(WorkspaceModel workspace)
        {
            var customNodeWs = workspace as CustomNodeWorkspaceModel;
            if (customNodeWs != null)
            {
                // copy these strings
                var newName = customNodeWs.Name.Substring(0);
                var newCategory = customNodeWs.Category.Substring(0);
                var newDescription = customNodeWs.Description.Substring(0);

                var args = new FunctionNamePromptEventArgs
                {
                    Name = newName,
                    Description = newDescription,
                    Category = newCategory,
                    CanEditName = false
                };

                this.ViewModel.DynamoViewModel.Model.OnRequestsFunctionNamePrompt(this, args);

                if (args.Success)
                {
                    customNodeWs.SetInfo(
                        args.CanEditName ? args.Name : workspace.Name,
                        args.Category,
                        args.Description);
                }
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

        void OnSelectionCanvasLoaded(object sender, RoutedEventArgs e)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //DrawGrid();
            //sw.Stop();
            //dynamoModel.Logger.Log(string.Format("{0} elapsed for drawing grid.", sw.Elapsed));
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
            double resultZoom = ViewModel.Model.Zoom + zoom;
            if (resultZoom < WorkspaceModel.ZOOM_MINIMUM)
                resultZoom = WorkspaceModel.ZOOM_MINIMUM;
            else if (resultZoom > WorkspaceModel.ZOOM_MAXIMUM)
                resultZoom = WorkspaceModel.ZOOM_MAXIMUM;

            // Get Viewpoint Center point
            var centerPoint = new Point2D();
            centerPoint.X = outerCanvas.ActualWidth / 2;
            centerPoint.Y = outerCanvas.ActualHeight / 2;

            // Get relative point of ZoomBorder child in relates to viewpoint center point
            var relativePoint = new Point2D();
            relativePoint.X = (centerPoint.X - ViewModel.Model.X) / ViewModel.Model.Zoom;
            relativePoint.Y = (centerPoint.Y - ViewModel.Model.Y) / ViewModel.Model.Zoom;

            ZoomAtViewportPoint(zoom, relativePoint);
        }

        private double AdjustZoomForCurrentZoomAmount(double zoom)
        {
            const double upperLimit = .6;
            const double lowerLimit = .01;

            //quadratic adjustment
            //var adjustedZoom = (lowerLimit + Math.Pow((ViewModel.Model.Zoom / WorkspaceModel.ZOOM_MAXIMUM), 2) * (upperLimit - lowerLimit)) * zoom;

            //linear adjustment
            var adjustedZoom = (lowerLimit + (ViewModel.Model.Zoom / WorkspaceModel.ZOOM_MAXIMUM) * (upperLimit - lowerLimit)) * zoom;

            return adjustedZoom;
        }

        void vm_ZoomAtViewportPoint(object sender, EventArgs e)
        {
            double zoom = AdjustZoomForCurrentZoomAmount((e as ZoomEventArgs).Zoom);
            Point2D point = (e as ZoomEventArgs).Point;

            ZoomAtViewportPoint(zoom, point);
        }

        private void ZoomAtViewportPoint(double zoom, Point2D relative)
        {
            // Limit zoom
            double resultZoom = ViewModel.Model.Zoom + zoom;
            if (resultZoom < WorkspaceModel.ZOOM_MINIMUM)
                resultZoom = WorkspaceModel.ZOOM_MINIMUM;
            else if (resultZoom > WorkspaceModel.ZOOM_MAXIMUM)
                resultZoom = WorkspaceModel.ZOOM_MAXIMUM;

            double absoluteX, absoluteY;
            absoluteX = relative.X * ViewModel.Model.Zoom + ViewModel.Model.X;
            absoluteY = relative.Y * ViewModel.Model.Zoom + ViewModel.Model.Y;
            var resultOffset = new Point2D();
            resultOffset.X = absoluteX - (relative.X * resultZoom);
            resultOffset.Y = absoluteY - (relative.Y * resultZoom);

            ViewModel.Model.Zoom = resultZoom;
            ViewModel.Model.X = resultOffset.X;
            ViewModel.Model.Y = resultOffset.Y;

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

            var resultOffset = new Point2D();
            resultOffset.X = -(zoomArgs.Offset.X * scaleRequired) + centerOffsetX;
            resultOffset.Y = -(zoomArgs.Offset.Y * scaleRequired) + centerOffsetY;

            // Apply on model
            ViewModel.Model.Zoom = scaleRequired;
            ViewModel.Model.X = resultOffset.X;
            ViewModel.Model.Y = resultOffset.Y;

            vm_CurrentOffsetChanged(this, new PointEventArgs(resultOffset));
            vm_ZoomChanged(this, new ZoomEventArgs(scaleRequired));
        }

        private void WorkspaceView_KeyDown(object sender, KeyEventArgs e)
        {
            Button source = e.Source as Button;

            if (source == null)
                return;

            if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl)
                return;

        }

        private void WorkspaceView_KeyUp(object sender, KeyEventArgs e)
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

            InCanvasSearchBar.IsOpen = false;
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
            
            // If we are currently connecting and there is an active 
            // connector, redraw it to match the new mouse coordinates.
            if (wvm.IsSnapping)
            {              
                if (wvm.portViewModel != null)
                {
                    if (wvm.CheckActiveConnectorCompatibility(wvm.portViewModel))
                    {
                        mouseMessageHandled = true;
                        wvm.HandleMouseMove(this.WorkBench, wvm.portViewModel.Center);
                    }
                }
                else
                    wvm.CurrentCursor = CursorLibrary.GetCursor(CursorSet.ArcSelect);
            }

            if (wvm.IsInIdleState)
            {
                // Find the dependency object directly under the mouse 
                // cursor, then see if it represents a port. If it does,
                // then determine its type, we would like to show the 
                // "ArcRemoving" cursor when the mouse is over an out port.
                Point mouse = e.GetPosition((UIElement)sender);
                var dependencyObject = ElementUnderMouseCursor(mouse);
                PortViewModel pvm = PortFromHitTestResult(dependencyObject);

                if (null != pvm && (pvm.PortType == PortType.Input))
                    this.Cursor = CursorLibrary.GetCursor(CursorSet.ArcSelect);
                else
                    this.Cursor = null;
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

                        zoomBorder.SetTranslateTransformOrigin(new Point2D(vm.Model.X - deltaX, vm.Model.Y - deltaY));
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

        private Point inCanvasSearchPosition;

        private void ShowHideInCanvasControl(ShowHideFlags flag)
        {
            switch (flag)
            {
                case ShowHideFlags.Hide:
                    InCanvasSearchBar.IsOpen = false;
                    break;
                case ShowHideFlags.Show:
                    // Show InCanvas search just in case, when mouse is over workspace.
                    InCanvasSearchBar.IsOpen = this.IsMouseOver;
                    ViewModel.InCanvasSearchViewModel.InCanvasSearchPosition = inCanvasSearchPosition;
                    break;
            }
        }
        
        private void OnWorkspaceDrop(object sender, DragEventArgs e)
        {
            var nodeInfo = e.Data.GetData(typeof(DragDropNodeSearchElementInfo)) as DragDropNodeSearchElementInfo;
            if (nodeInfo == null)
                return;

            var nodeModel = nodeInfo.SearchElement.CreateNode();
            var mousePosition = e.GetPosition(this.WorkspaceElements);
            ViewModel.DynamoViewModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(
                nodeModel, mousePosition.X, mousePosition.Y, false, true));
        }

        private void OnInCanvasSearchContextMenuKeyDown(object sender, KeyEventArgs e)
        {
            var contextMenu = sender as ContextMenu;
            if (e.Key == Key.Enter)
            {
                ViewModel.InCanvasSearchViewModel.InCanvasSearchPosition = inCanvasSearchPosition;
                if (contextMenu != null)
                    contextMenu.IsOpen = false;
            }
        }

        /// <summary>
        /// MouseUp is used to close context menu. Only if original sender was Thumb(i.e. scroll bar),
        /// then context menu is left open.
        /// Or if original sender was TextBox, then context menu is left open as well.
        /// </summary>
        private void OnInCanvasSearchContextMenuMouseUp(object sender, MouseButtonEventArgs e)
        {
            var contextMenu = sender as ContextMenu;
            if (!(e.OriginalSource is System.Windows.Controls.Primitives.Thumb) && !(e.OriginalSource is TextBox)
                && contextMenu != null)
                contextMenu.IsOpen = false;
        }

        /// <summary>
        /// MouseDown is used to set place, where node will be created.
        /// </summary>
        private void OnInCanvasSearchContextMenuMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.InCanvasSearchViewModel.InCanvasSearchPosition = inCanvasSearchPosition;
        }

        /// <summary>
        /// Determines position of mouse, when user clicks on canvas.
        /// Both left and right mouse clicks.
        /// </summary>
        private void OnCanvasClicked(object sender, MouseButtonEventArgs e)
        {
            inCanvasSearchPosition = Mouse.GetPosition(this.WorkspaceElements);
        }

        private void OnContextMenuOpened(object sender, RoutedEventArgs e)
        {
            ViewModel.InCanvasSearchViewModel.SearchText = String.Empty;
        }

    }
}
