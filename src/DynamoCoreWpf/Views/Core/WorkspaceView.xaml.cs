using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Controls;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Search.SearchElements;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Controls;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.UI;
using Dynamo.Wpf.Utilities;
using ModifierKeys = System.Windows.Input.ModifierKeys;

namespace Dynamo.Views
{
    /// <summary>
    /// Interaction logic for WorkspaceView.xaml
    /// </summary>
    public partial class WorkspaceView
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

        private DragCanvas workBench;
        private readonly DataTemplate draggedSelectionTemplate;
        private DraggedAdorner draggedAdorner;
        private object draggedData;
        private Point startMousePosition;
        private Point initialMousePosition;
        private PortViewModel snappedPort;
        private double currentNodeCascadeOffset;
        private Point inCanvasSearchPosition;
        private List<DependencyObject> hitResultsList = new List<DependencyObject>();

        public WorkspaceViewModel ViewModel
        {
            get
            {
                return DataContext as WorkspaceViewModel;
            }
        }

        internal bool IsSnappedToPort
        {
            get
            {
                return snappedPort != null;
            }
        }

        internal GeometryScalingPopup GeoScalingPopup { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkspaceView()
        {
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoModernDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoColorsAndBrushesDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DataTemplatesDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.ConnectorsDictionary);

            InitializeComponent();

            DataContextChanged += OnWorkspaceViewDataContextChanged;

            // view of items to drag
            draggedSelectionTemplate = (DataTemplate)FindResource("DraggedSelectionTemplate");
            var dictionaries = draggedSelectionTemplate.Resources.MergedDictionaries;

            // let draggedSelectionTemplate know about views of node, note, annotation, connector
            dictionaries.Add(SharedDictionaryManager.ConnectorsDictionary);
            dictionaries.Add(SharedDictionaryManager.DataTemplatesDictionary);
        }

        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentSpace":
                case "ShowStartPage":
                    // When workspace is changed(e.g. from home to custom) 
                    // or when go to start page, close InCanvasSearch and ContextMenu.
                    ShowHideInCanvasControl(ShowHideFlags.Hide);
                    ShowHideContextMenu(ShowHideFlags.Hide);
                    break;
            }
        }

        /// <summary>
        /// clean up view model subscriptions to prevent memory leak
        /// </summary>
        /// <param name="ViewModel"></param>
        private void RemoveViewModelsubscriptions(WorkspaceViewModel ViewModel)
        {
            ViewModel.RequestShowInCanvasSearch -= ShowHideInCanvasControl;
            ViewModel.RequestHideAllPopup -= HideAllPopUp;
            ViewModel.RequestNodeAutoCompleteSearch -= ShowHideNodeAutoCompleteControl;
            ViewModel.RequestPortContextMenu -= ShowHidePortContextMenu;
            ViewModel.DynamoViewModel.PropertyChanged -= ViewModel_PropertyChanged;

            ViewModel.ZoomChanged -= vm_ZoomChanged;
            ViewModel.RequestZoomToViewportCenter -= vm_ZoomAtViewportCenter;
            ViewModel.RequestZoomToViewportPoint -= vm_ZoomAtViewportPoint;
            ViewModel.RequestZoomToFitView -= vm_ZoomToFitView;
            ViewModel.RequestCenterViewOnElement -= CenterViewOnElement;

            ViewModel.RequestAddViewToOuterCanvas -= vm_RequestAddViewToOuterCanvas;
            ViewModel.WorkspacePropertyEditRequested -= VmOnWorkspacePropertyEditRequested;
            ViewModel.RequestSelectionBoxUpdate -= VmOnRequestSelectionBoxUpdate;

            ViewModel.Model.RequestNodeCentered -= vm_RequestNodeCentered;
            ViewModel.Model.CurrentOffsetChanged -= vm_CurrentOffsetChanged;
            DynamoSelection.Instance.Selection.CollectionChanged -= OnSelectionCollectionChanged;
            infiniteGridView.DetachFromZoomBorder(zoomBorder);
        }

        /// <summary>
        /// Attach view model subscriptions
        /// </summary>
        /// <param name="ViewModel"></param>
        private void AttachViewModelsubscriptions(WorkspaceViewModel ViewModel)
        {
            ViewModel.RequestShowInCanvasSearch += ShowHideInCanvasControl;
            ViewModel.RequestHideAllPopup += HideAllPopUp;
            ViewModel.RequestNodeAutoCompleteSearch += ShowHideNodeAutoCompleteControl;
            ViewModel.RequestPortContextMenu += ShowHidePortContextMenu;
            ViewModel.DynamoViewModel.PropertyChanged += ViewModel_PropertyChanged;

            ViewModel.ZoomChanged += vm_ZoomChanged;
            ViewModel.RequestZoomToViewportCenter += vm_ZoomAtViewportCenter;
            ViewModel.RequestZoomToViewportPoint += vm_ZoomAtViewportPoint;
            ViewModel.RequestZoomToFitView += vm_ZoomToFitView;
            ViewModel.RequestCenterViewOnElement += CenterViewOnElement;

            ViewModel.RequestAddViewToOuterCanvas += vm_RequestAddViewToOuterCanvas;
            ViewModel.WorkspacePropertyEditRequested += VmOnWorkspacePropertyEditRequested;
            ViewModel.RequestSelectionBoxUpdate += VmOnRequestSelectionBoxUpdate;

            ViewModel.Model.RequestNodeCentered += vm_RequestNodeCentered;
            ViewModel.Model.CurrentOffsetChanged += vm_CurrentOffsetChanged;
            DynamoSelection.Instance.Selection.CollectionChanged += OnSelectionCollectionChanged;
            infiniteGridView.AttachToZoomBorder(zoomBorder);
        }

        private void ShowHideNodeAutoCompleteControl(ShowHideFlags flag)
        {
            ShowHidePopup(flag, NodeAutoCompleteSearchBar);
        }

        private void ShowHidePortContextMenu(ShowHideFlags flag, PortViewModel portViewModel)
        {
            PortContextMenu.DataContext = portViewModel;
            ShowHidePopup(flag, PortContextMenu);
        }

        private void ShowHideInCanvasControl(ShowHideFlags flag)
        {
            ShowHidePopup(flag, InCanvasSearchBar);
        }

        private void ShowHideContextMenu(ShowHideFlags flag)
        {
            ShowHidePopup(flag, ContextMenuPopup);
        }

        private void ShowHideGeoScalingPopup(ShowHideFlags flag)
        {
            if (GeoScalingPopup != null)
                ShowHidePopup(flag, GeoScalingPopup);
        }

        private void ShowHidePopup(ShowHideFlags flag, Popup popup)
        {
            // Reset popup display state
            popup.IsOpen = false;
            switch (flag)
            {
                case ShowHideFlags.Hide:
                    break;
                case ShowHideFlags.Show:
                    // Show InCanvas search just in case, when mouse is over workspace.
                    var displayPopup = DynamoModel.IsTestMode || IsMouseOver;

                    if (displayPopup)
                    {
                        if (popup == NodeAutoCompleteSearchBar)
                        {
                            if (ViewModel.NodeAutoCompleteSearchViewModel.PortViewModel == null) return;
                            // Force the Child visibility to change here because
                            // 1. Popup isOpen change does not necessarily update the child control before it take effect
                            // 2. Dynamo rely on child visibility change hander to setup Node AutoComplete control
                            // 3. This should not be set to in canvas search control
                            popup.Child.Visibility = Visibility.Collapsed;
                            ViewModel.NodeAutoCompleteSearchViewModel.PortViewModel.SetupNodeAutocompleteWindowPlacement(popup);
                        }

                        else if (popup == PortContextMenu)
                        {
                            popup.Child.Visibility = Visibility.Hidden;
                            if (!(PortContextMenu.DataContext is PortViewModel portViewModel)) return;

                            if (portViewModel is OutPortViewModel outPortViewModel)
                            {
                                outPortViewModel.RefreshHideWiresState();

                                if (portViewModel.NodeViewModel.NodeModel is PythonNodeModels.PythonNode)
                                {
                                    outPortViewModel.EnableRenamePort();
                                }
                            }


                            portViewModel.SetupPortContextMenuPlacement(popup);
                        }
                    }

                    // We need to use the dispatcher here to make sure that
                    // the popup is fully updated before we show it.
                    // This was mainly an issue with the PortContextMenu as
                    // it uses a DataTemplate bound to the WorkspaceViewModel
                    // to display the correct content.
                    // If the dispatcher is not used in this scenario when switching
                    // from inputPort context menu to Output port context menu,
                    // the popup will display before the new content is fully rendered
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                        popup.Child.Visibility = Visibility.Visible;
                        popup.Child.UpdateLayout();
                        popup.IsOpen = displayPopup;
                        popup.CustomPopupPlacementCallback = null;
                    }));

                    ViewModel.InCanvasSearchViewModel.SearchText = string.Empty;
                    ViewModel.InCanvasSearchViewModel.InCanvasSearchPosition = inCanvasSearchPosition;
                    break;
            }
        }

        /// <summary>
        /// Hides all popups in the view, the amount of popup hidden will be different depending on
        /// if the hide view command is triggered on node level or workspace level
        /// </summary>
        public void HideAllPopUp(object sender)
        {
            // First make sure workspace level popups are hidden
            if (InCanvasSearchBar.IsOpen || ContextMenuPopup.IsOpen)
            {
                ShowHideContextMenu(ShowHideFlags.Hide);
                ShowHideInCanvasControl(ShowHideFlags.Hide);
            }

            var imageButtonGeoScalingPopup = Mouse.DirectlyOver as ImageRepeatButton;
            //When imageButtonGeoScalingPopup is null means that the user is clicking the Geometry Scaling button so we should not close the Popup
            if (imageButtonGeoScalingPopup == null && GeoScalingPopup != null && GeoScalingPopup.IsOpen)
            {
                ShowHideGeoScalingPopup(ShowHideFlags.Hide);
            }
            // If triggered on node level, make sure node popups are also hidden
            if(sender is NodeView && (PortContextMenu.IsOpen || NodeAutoCompleteSearchBar.IsOpen) )
            {
                ShowHidePopup(ShowHideFlags.Hide, PortContextMenu);
                ShowHidePopup(ShowHideFlags.Hide, NodeAutoCompleteSearchBar);
            }
        }

        internal Point GetCenterPoint()
        {
            var x = outerCanvas.ActualWidth / 2.0;
            var y = outerCanvas.ActualHeight / 2.0;
            var centerPt = new Point(x, y);
            var transform = outerCanvas.TransformToDescendant(WorkspaceElements);
            return transform.Transform(centerPt);
        }

        internal Rect GetVisibleBounds()
        {
            var t = outerCanvas.TransformToDescendant(WorkspaceElements);
            var topLeft = t.Transform(new Point());
            var bottomRight = t.Transform(new Point(outerCanvas.ActualWidth, outerCanvas.ActualHeight));
            return new Rect(topLeft, bottomRight);
        }

        internal void SaveWorkspaceAsImage(string path)
        {
            var initialized = false;
            var bounds = new Rect();

            double minX = 0.0, minY = 0.0;
            var dragCanvas = WpfUtilities.ChildOfType<DragCanvas>(this);
            var childrenCount = VisualTreeHelper.GetChildrenCount(dragCanvas);
            for (int index = 0; index < childrenCount; ++index)
            {
                ContentPresenter contentPresenter = VisualTreeHelper.GetChild(dragCanvas, index) as ContentPresenter;
                if (contentPresenter.Children().Count() < 1) continue;
                
                var firstChild = VisualTreeHelper.GetChild(contentPresenter, 0);

                switch (firstChild.GetType().Name)
                {
                    case "NodeView":
                    case "NoteView":
                    case "AnnotationView":
                        break;

                    // Until we completely removed InfoBubbleView (or fixed its broken 
                    // size calculation), we will not be including it in our size 
                    // calculation here. This means that the info bubble, if any, will 
                    // still go beyond the boundaries of the final PNG file. I would 
                    // prefer not to add this hack here as it introduces multiple issues 
                    // (including NaN for Grid inside the view and the fix would be too 
                    // ugly to type in). Suffice to say that InfoBubbleView is not 
                    // included in the size calculation for screen capture (work-around 
                    // should be obvious).
                    // 
                    // case "InfoBubbleView":
                    //     child = WpfUtilities.ChildOfType<Grid>(child);
                    //     break;

                    // We do not take anything other than those above 
                    // into consideration when the canvas size is measured.
                    default:
                        continue;
                }

                // Determine the smallest corner of all given visual elements on the 
                // graph. This smallest top-left corner value will be useful in making 
                // the offset later on.
                // 
                var childBounds = VisualTreeHelper.GetDescendantBounds(contentPresenter as Visual);
                minX = childBounds.X < minX ? childBounds.X : minX;
                minY = childBounds.Y < minY ? childBounds.Y : minY;
                childBounds.X = (double)(contentPresenter as Visual).GetValue(Canvas.LeftProperty);
                childBounds.Y = (double)(contentPresenter as Visual).GetValue(Canvas.TopProperty);

                if (initialized)
                {
                    bounds.Union(childBounds);
                }
                else
                {
                    initialized = true;
                    bounds = childBounds;
                }
            }

            // Nothing found in the canvas, bail out.
            if (!initialized) return;

            // Add padding to the edge and make them multiples of two (pad 10px on each side).
            bounds.Width = 20 + ((((int)Math.Ceiling(bounds.Width)) + 1) & ~0x01);
            bounds.Height = 20 + ((((int)Math.Ceiling(bounds.Height)) + 1) & ~0x01);

            var currentTransformGroup = WorkspaceElements.RenderTransform as TransformGroup;
            WorkspaceElements.RenderTransform = new TranslateTransform(10.0 - bounds.X - minX, 10.0 - bounds.Y - minY);
            WorkspaceElements.UpdateLayout();

            var rtb = new RenderTargetBitmap(((int)bounds.Width),
                ((int)bounds.Height), 96, 96, PixelFormats.Default);

            rtb.Render(WorkspaceElements);
            WorkspaceElements.RenderTransform = currentTransformGroup;

            try
            {
                using (var stm = System.IO.File.Create(path))
                {
                    // Encode as PNG format
                    var pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
                    pngEncoder.Save(stm);
                }
            }
            catch (Exception)
            {
            }
        }

        void OnSelectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            ViewModel.NodeFromSelectionCommand.RaiseCanExecuteChanged();
            ViewModel.NodeToCodeCommand.RaiseCanExecuteChanged();
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
                RemoveViewModelsubscriptions(oldViewModel);
            }

            if (ViewModel != null)
            {
                // Adding registration of event listener
                AttachViewModelsubscriptions(ViewModel);
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
            var originalLt = new Point(e.X, e.Y);
            var translatedLt = workBench.TranslatePoint(originalLt, outerCanvas);

            if (e.UpdatedProps.HasFlag(SelectionBoxUpdateArgs.UpdateFlags.Position))
            {
                Canvas.SetLeft(this.selectionBox, translatedLt.X);
                Canvas.SetTop(this.selectionBox, translatedLt.Y);
            }

            if (e.UpdatedProps.HasFlag(SelectionBoxUpdateArgs.UpdateFlags.Dimension))
            {
                var originalRb = new Point(e.X + e.Width, e.Y + e.Height);
                var translatedRb = workBench.TranslatePoint(originalRb, outerCanvas);

                selectionBox.Width = translatedRb.X - translatedLt.X;
                selectionBox.Height = translatedRb.Y - translatedLt.Y;
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

        void vm_RequestAddViewToOuterCanvas(object sender, EventArgs e)
        {
            UserControl view = (UserControl)((e as ViewEventArgs).View);
            outerCanvas.Children.Add(view);
            Canvas.SetBottom(view, 0);
            Canvas.SetRight(view, 0);
        }

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
                if (workBench != null)
                {
                    var a = outerCanvas.TransformToDescendant(workBench);
                    dropPt = a.Transform(dropPt);
                }
            }

            // center the node at the drop point
            if (!Double.IsNaN(node.Width))
                dropPt.X -= (node.Width / 2.0);

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
            if (PortContextMenu.IsOpen) DestroyPortContextMenu();
        }

        void vm_ZoomAtViewportCenter(object sender, EventArgs e)
        {
            double zoom = AdjustZoomForCurrentZoomAmount((e as ZoomEventArgs).Zoom);

            // Limit Zoom
            double resultZoom = ViewModel.Zoom + zoom;
            if (resultZoom < WorkspaceViewModel.ZOOM_MINIMUM)
                resultZoom = WorkspaceViewModel.ZOOM_MINIMUM;
            else if (resultZoom > WorkspaceViewModel.ZOOM_MAXIMUM)
                resultZoom = WorkspaceViewModel.ZOOM_MAXIMUM;

            // Get Viewpoint Center point
            var centerPoint = new Point2D();
            centerPoint.X = outerCanvas.ActualWidth / 2;
            centerPoint.Y = outerCanvas.ActualHeight / 2;

            // Get relative point of ZoomBorder child in relates to viewpoint center point
            var relativePoint = new Point2D();
            relativePoint.X = (centerPoint.X - ViewModel.Model.X) / ViewModel.Zoom;
            relativePoint.Y = (centerPoint.Y - ViewModel.Model.Y) / ViewModel.Zoom;

            ZoomAtViewportPoint(zoom, relativePoint);
        }

        private double AdjustZoomForCurrentZoomAmount(double zoom)
        {
            const double upperLimit = .6;
            const double lowerLimit = .01;

            //quadratic adjustment
            //var adjustedZoom = (lowerLimit + Math.Pow((ViewModel.Model.Zoom / WorkspaceModel.ZOOM_MAXIMUM), 2) * (upperLimit - lowerLimit)) * zoom;

            //linear adjustment
            var adjustedZoom = (lowerLimit + (ViewModel.Zoom / WorkspaceViewModel.ZOOM_MAXIMUM) * (upperLimit - lowerLimit)) * zoom;

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
            double resultZoom = ViewModel.Zoom + zoom;
            if (resultZoom < WorkspaceViewModel.ZOOM_MINIMUM)
                resultZoom = WorkspaceViewModel.ZOOM_MINIMUM;
            else if (resultZoom > WorkspaceViewModel.ZOOM_MAXIMUM)
                resultZoom = WorkspaceViewModel.ZOOM_MAXIMUM;

            double absoluteX, absoluteY;
            absoluteX = relative.X * ViewModel.Zoom + ViewModel.Model.X;
            absoluteY = relative.Y * ViewModel.Zoom + ViewModel.Model.Y;
            var resultOffset = new Point2D();
            resultOffset.X = absoluteX - (relative.X * resultZoom);
            resultOffset.Y = absoluteY - (relative.Y * resultZoom);

            ViewModel.Zoom = resultZoom;
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
            if (scaleRequired > WorkspaceViewModel.ZOOM_MAXIMUM)
                scaleRequired = WorkspaceViewModel.ZOOM_MAXIMUM;
            else if (scaleRequired < WorkspaceViewModel.ZOOM_MINIMUM)
                scaleRequired = WorkspaceViewModel.ZOOM_MINIMUM;

            // Center position
            double centerOffsetX = viewportPadding + (fitWidth - (zoomArgs.FocusWidth * scaleRequired)) / 2;
            double centerOffsetY = viewportPadding + (fitHeight - (zoomArgs.FocusHeight * scaleRequired)) / 2;

            var resultOffset = new Point2D();
            resultOffset.X = -(zoomArgs.Offset.X * scaleRequired) + centerOffsetX;
            resultOffset.Y = -(zoomArgs.Offset.Y * scaleRequired) + centerOffsetY;

            // Apply on model
            ViewModel.Zoom = scaleRequired;
            ViewModel.Model.X = resultOffset.X;
            ViewModel.Model.Y = resultOffset.Y;

            vm_CurrentOffsetChanged(this, new PointEventArgs(resultOffset));
            vm_ZoomChanged(this, new ZoomEventArgs(scaleRequired));
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                startMousePosition = e.GetPosition(null);
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (snappedPort != null)
            {
                ViewModel.HandlePortClicked(snappedPort);
            }
            else if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                ViewModel.HandleLeftButtonDown(workBench, e);
            }

            if (!ViewModel.IsDragging) return;

            var nodesToHidePreview = this.ChildrenOfType<NodeView>().Where(view =>
                view.HasPreviewControl && !view.PreviewControl.IsHidden && !view.PreviewControl.StaysOpen);

            foreach (var node in nodesToHidePreview)
            {
                node.PreviewControl.HidePreviewBubble();
            }
        }

        private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenuPopup.IsOpen = false;
            InCanvasSearchBar.IsOpen = false;
            if (GeoScalingPopup != null)
                GeoScalingPopup.IsOpen = false;
            
            if(PortContextMenu.IsOpen) DestroyPortContextMenu();
        }

        /// <summary>
        /// Closes the port's context menu and sets its references to null.
        /// </summary>
        private void DestroyPortContextMenu() => PortContextMenu.IsOpen = false;
        
        private void OnMouseRelease(object sender, MouseButtonEventArgs e)
        {
            if (e == null) return; // in certain bizarre cases, e can be null

            snappedPort = null;
            if (ViewModel == null) return;

            // check IsInIdleState and IsPanning before finishing an action with HandleMouseRelease
            var returnToSearch = (ViewModel.IsInIdleState || ViewModel.IsPanning)
                && e.ChangedButton == MouseButton.Right && Keyboard.Modifiers == ModifierKeys.Control;

            ViewModel.HandleMouseRelease(workBench, e);
            ContextMenuPopup.IsOpen = false;
            if (GeoScalingPopup != null)
                GeoScalingPopup.IsOpen = false;
            if (returnToSearch)
            {
                ViewModel.DynamoViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.OnRequestFocusSearch();
            }
            else if (e.ChangedButton == MouseButton.Right && e.OriginalSource == zoomBorder)
            {
                // Setting the focus back to workspace explicitly as the workspace-focus is lost after interacting with a WebView2 component.
                this.Focusable = true;
                this.Focus();

                // open if workspace is right-clicked itself 
                // (not node, note, not buttons from viewControlPanel such as zoom, pan and so on)
                ContextMenuPopup.IsOpen = true;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            this.snappedPort = null;

            bool mouseMessageHandled = false;

            // If we are currently connecting and there is an active 
            // connector, redraw it to match the new mouse coordinates.
            if (ViewModel.IsSnapping)
            {
                if (ViewModel.portViewModel != null)
                {
                    if (ViewModel.CheckActiveConnectorCompatibility(ViewModel.portViewModel))
                    {
                        mouseMessageHandled = true;
                        ViewModel.HandleMouseMove(workBench, ViewModel.portViewModel.Center);
                    }
                }
                else
                    ViewModel.CurrentCursor = CursorLibrary.GetCursor(CursorSet.ArcSelect);
            }

            if (ViewModel.IsInIdleState)
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

            // If selection is going to be dragged and ctrl is pressed.
            if (ViewModel.IsDragging && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var currentMousePosition = e.GetPosition(null);

                // Set initialMousePosition here, so that we can use it in OnDragOver.
                initialMousePosition = e.GetPosition(WorkspaceElements);

                // Check that current mouse position is far enough from start position.
                var canDrag =
                    (Math.Abs(currentMousePosition.X - startMousePosition.X) >
                     SystemParameters.MinimumHorizontalDragDistance) &&
                    (Math.Abs(currentMousePosition.Y - startMousePosition.Y) >
                     SystemParameters.MinimumVerticalDragDistance) &&
                    e.OriginalSource is DragCanvas;

                if (canDrag)
                {
                    DragAndDrop(e.GetPosition(WorkspaceElements));
                    mouseMessageHandled = true;
                }
            }

            if (!mouseMessageHandled)
            {
                ViewModel.HandleMouseMove(workBench, e);
            }
        }

        /// <summary>
        /// Drag and drop nodes, notes, annotations and connectors.
        /// </summary>
        /// <param name="mouse">Relative position to WorkspaceElements</param>
        private void DragAndDrop(Point mouse)
        {
            // disable clearing selection while dragged data is being generated
            // new AnnotationViewModel unnecessarily clears selection 
            DynamoSelection.Instance.ClearSelectionDisabled = true;
            var selection = DynamoSelection.Instance.Selection;
            var nodes = selection.OfType<NodeModel>();
            var notes = selection.OfType<NoteModel>();
            var pins = selection.OfType<ConnectorPinModel>();
            var annotations = selection.OfType<AnnotationModel>();

            var connectors = nodes.SelectMany(n =>
                n.OutPorts.SelectMany(port => port.Connectors)
                    .Where(c => c.End != null && c.End.Owner.IsSelected)).Distinct();

            // set list of selected viewmodels
            draggedData = connectors.Select(c => (ViewModelBase)new ConnectorViewModel(ViewModel, c))
                .Concat(pins.Select(p=> new ConnectorPinViewModel(ViewModel, p)))
                .Concat(notes.Select(n => new NoteViewModel(ViewModel, n)))
                .Concat(annotations.Select(a => new AnnotationViewModel(ViewModel, a)))
                .Concat(nodes.Select(n =>
                {
                    var node = this.ChildrenOfType<NodeView>()
                        .FirstOrDefault(view => view.ViewModel.NodeModel == n);
                    if (node == null) return new NodeViewModel(ViewModel, n);

                    var nodeRect = node.nodeBorder;
                    var size = new Size(nodeRect.ActualWidth, nodeRect.ActualHeight);
                    // set fixed size for dragged nodes, 
                    // so that they will correspond to origin nodes
                    return new NodeViewModel(ViewModel, n, size);
                })).ToList();

            var locatableModels = nodes.Concat<ModelBase>(notes).Concat<ModelBase>(pins);
            var minX = locatableModels.Any() ? locatableModels.Min(mb => mb.X) : 0;
            var minY = locatableModels.Any() ? locatableModels.Min(mb => mb.Y) : 0;
            // compute offset to correctly place selected items right under mouse cursor 
            var mouseOffset = new Point2D(mouse.X - minX, mouse.Y - minY);

            DynamoSelection.Instance.ClearSelectionDisabled = false;
            DragDrop.DoDragDrop(this, mouseOffset, DragDropEffects.Copy);

            // remove dragged selection view 
            if (draggedAdorner != null)
            {
                draggedData = null;
                draggedAdorner.Detach();
                draggedAdorner = null;
            }
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            var currentPosition = e.GetPosition(WorkspaceElements);
            // create adorner if it is necessary
            if (draggedAdorner == null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(WorkspaceElements);
                draggedAdorner = new DraggedAdorner(draggedData,
                    draggedSelectionTemplate, WorkspaceElements, adornerLayer);
            }

            var zoom = ViewModel.Zoom;

            var xOffset = currentPosition.X - initialMousePosition.X;
            var yOffset = currentPosition.Y - initialMousePosition.Y;
            // compute (x; y) so that dragged selection has mouse cursor 
            // in the same place as origin selection does
            var x = xOffset * zoom;
            var y = yOffset * zoom;

            // compute bounds of dragged content so that it does not go outside dragged canvas
            var x1 = -ViewModel.Model.X / zoom - xOffset;
            var y1 = -ViewModel.Model.Y / zoom - yOffset;
            var x2 = WorkspaceElements.RenderSize.Width / zoom;
            var y2 = WorkspaceElements.RenderSize.Height / zoom;
            var bounds = new Rect(x1, y1, x2, y2);
            draggedAdorner.SetPosition(x, y, bounds);
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

                    if (workBench != null)
                    {
                        var b = workBench.TransformToAncestor(outerCanvas);

                        Point outerCenter = new Point(outerCanvas.ActualWidth / 2, outerCanvas.ActualHeight / 2);
                        Point nodeCenterInCanvas = new Point(n.X + n.Width / 2, n.Y + n.Height / 2);
                        Point nodeCenterInOverlay = b.Transform(nodeCenterInCanvas);

                        double deltaX = nodeCenterInOverlay.X - outerCenter.X;
                        double deltaY = nodeCenterInOverlay.Y - outerCenter.Y;
                        
                        vm.Model.X -= deltaX;
                        vm.Model.Y -= deltaY;

                        zoomBorder.SetTranslateTransformOrigin(new Point2D(vm.Model.X, vm.Model.Y));
                    }
                });
        }

        private void workBench_OnLoaded(object sender, RoutedEventArgs e)
        {
            workBench = sender as DragCanvas;
            workBench.owningWorkspace = this;
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
            VisualTreeHelper.HitTest(this, null, DirectHitTestCallback,
                new PointHitTestParameters(mouseCursor));

            return ((hitResultsList.Count > 0) ? hitResultsList[0] : null);
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

        private void OnWorkspaceDrop(object sender, DragEventArgs e)
        {

            var mousePosition = e.GetPosition(WorkspaceElements);
            var pointObj = e.Data.GetData(typeof(Point2D));
            if (pointObj is Point2D)
            {
                var offset = (Point2D)pointObj;
                // compute a point where (minX, minY) will be pasted -
                // location of selection top left corner
                var targetX = mousePosition.X - offset.X;
                var targetY = mousePosition.Y - offset.Y;
                var targetPoint = new Point2D(targetX, targetY);

                ViewModel.PasteSelection(targetPoint);

                return;
            }

            var nodeInfo = e.Data.GetData(typeof(DragDropNodeSearchElementInfo)) as DragDropNodeSearchElementInfo;
            if (nodeInfo == null)
                return;

            var nodeModel = nodeInfo.SearchElement.CreateNode();
            ViewModel.DynamoViewModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(
                nodeModel, mousePosition.X, mousePosition.Y, false, true));
        }

        private void OnInCanvasSearchContextMenuKeyDown(object sender, KeyEventArgs e)
        {
            var contextMenu = sender as Popup;
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
            var contextMenu = sender as Popup;
            if (!(e.OriginalSource is Thumb) && !(e.OriginalSource is TextBox)
                && contextMenu != null)
            {
                contextMenu.IsOpen = false;
            }
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

        private void OnContextMenuOpened(object sender, EventArgs e)
        {
            // If in-canvas search box is open already, close it
            // to avoid opening multiple instances.
            if (InCanvasSearchBar.IsOpen)
            {
                InCanvasSearchBar.IsOpen = false;
            }
            ViewModel.InCanvasSearchViewModel.SearchText = string.Empty;
        }

        private void OnGeometryScaling_Click(object sender, RoutedEventArgs e)
        {
            if (GeoScalingPopup == null)
            {
                GeoScalingPopup = new GeometryScalingPopup(ViewModel.DynamoViewModel);
                GeoScalingPopup.Placement = PlacementMode.Bottom;
                GeoScalingPopup.PlacementTarget = geometryScalingButton;
            }
            GeoScalingPopup.IsOpen = true;
        }
    }
}
