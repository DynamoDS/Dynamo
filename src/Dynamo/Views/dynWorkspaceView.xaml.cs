using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using DragCanvas = Dynamo.Controls.DragCanvas;

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

        public dynWorkspaceView()
        {
            InitializeComponent();

            selectionCanvas.Loaded += new RoutedEventHandler(selectionCanvas_Loaded);
            DataContextChanged += new DependencyPropertyChangedEventHandler(dynWorkspaceView_DataContextChanged);

            // Make new Watch3DFullscreenViewModel
            // Make new Watch3DFullscreenView(input: viewmodel)
            // attach to bench through mainGrid

            

            // dynSettings.Bench.sidebarGrid.Children.Add(search);
        }

        /// <summary>
        /// Handler for the DataContextChangedEvent. Hanndles registration of event listeners.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dynWorkspaceView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = DataContext as dynWorkspaceViewModel;
            vm.CurrentOffsetChanged += new PointEventHandler(vm_CurrentOffsetChanged);
            vm.StopDragging += new EventHandler(vm_StopDragging);
            vm.RequestCenterViewOnElement += new NodeEventHandler(CenterViewOnElement);
            vm.RequestNodeCentered += new NodeEventHandler(vm_RequestNodeCentered);
            //vm.UILocked += new EventHandler(LockUI);
            //vm.UIUnlocked += new EventHandler(UnlockUI);
            vm.RequestAddViewToOuterCanvas += new ViewEventHandler(vm_RequestAddViewToOuterCanvas);
        }

        void selectionCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGrid();

            //Watch3DFullscreenViewModel watchFullscreenViewModel = new Watch3DFullscreenViewModel();
            //WatchViewFullscreen fullscreen_view = new WatchViewFullscreen();

            //outerCanvas.Children.Add(fullscreen_view);

            //dynSettings.Bench.overlayCanvas.Children.Add(fullscreen_view);

          
            //dynWatch3DFullscreen fullscreen_watch3D = new dynWatch3DFullscreen();
            //fullscreen_watch3D.SetupCustomUIElements(selectionCanvas);
            //selectionCanvas.Children.Add(fullscreen_watch3D.FullscreenWatchView());
        }

        void vm_RequestAddViewToOuterCanvas(object sender, EventArgs e)
        {
            UserControl view = (e as ViewEventArgs).View;
            outerCanvas.Children.Add(view);
            Canvas.SetBottom(view, 0);
            Canvas.SetRight(view, 0);
        }

        void vm_RequestNodeCentered(object sender, EventArgs e)
        {
            double x = 0;
            double y = 0;
            dynNodeModel node = (e as NodeEventArgs).Node;
            Dictionary<string, object> data = (e as NodeEventArgs).Data;

            x = outerCanvas.ActualWidth / 2.0;
            y = outerCanvas.ActualHeight / 2.0;

            // apply small perturbation
            // so node isn't right on top of last placed node
            var r = new Random();
            x += (r.NextDouble() - 0.5) * 50;
            y += (r.NextDouble() - 0.5) * 50;
            
            var transformFromOuterCanvas = data.ContainsKey("transformFromOuterCanvasCoordinates");

            if (data.ContainsKey("x"))
                x = (double)data["x"];

            if (data.ContainsKey("y"))
                y = (double)data["y"];

            Point dropPt = new Point(x, y);

            // Transform dropPt from outerCanvas space into zoomCanvas space
            if (transformFromOuterCanvas)
            {
                var a = outerCanvas.TransformToDescendant(WorkBench);
                dropPt = a.Transform(dropPt);
            }
            
            // center the node at the drop point
            if (!Double.IsNaN(node.Width))
                dropPt.X -= (node.Width / 2.0);

            if (!Double.IsNaN(node.Height))
                dropPt.Y -= (node.Height / 2.0);

            //MVVM: Don't do direct canvas manipulation here
            //Canvas.SetLeft(node, dropPt.X);
            //Canvas.SetTop(node, dropPt.Y);

            if (!Double.IsNaN(node.Width))
                dropPt.X -= (node.Height / 2.0);

            if (!Double.IsNaN(node.Height))
                dropPt.Y -= (node.Height / 2.0);

            node.X = dropPt.X;
            node.Y = dropPt.Y;
        }

        void vm_StopDragging(object sender, EventArgs e)
        {
            WorkBench.isDragInProgress = false;
            WorkBench.ignoreClick = true;
        }

        void zoomBorder_MouseMove(object sender, MouseEventArgs e)
        {
            (DataContext as dynWorkspaceViewModel).SetCurrentOffsetCommand.Execute((sender as ZoomBorder).GetTranslateTransformOrigin());
        }

        void zoomBorder_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            (DataContext as dynWorkspaceViewModel).SetCurrentOffsetCommand.Execute((sender as ZoomBorder).GetTranslateTransformOrigin());
        }

        void vm_CurrentOffsetChanged(object sender, EventArgs e)
        {
            zoomBorder.SetTranslateTransformOrigin((e as PointEventArgs).Point);
        }

        private void dynWorkspaceView_KeyDown(object sender, KeyEventArgs e)
        {
            Button source = e.Source as Button;

            if (source == null)
                return;

            if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl)
                return;


            System.Console.WriteLine("Hello World");

        }

        private void dynWorkspaceView_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void DynWorkspaceView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dynWorkspaceViewModel vm = (DataContext as dynWorkspaceViewModel);

            if (!(DataContext as dynWorkspaceViewModel).IsConnecting)
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

            dynWorkspaceViewModel vm = (DataContext as dynWorkspaceViewModel);

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
            var vm = (DataContext as dynWorkspaceViewModel);

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

        internal void CenterViewOnElement(object sender, EventArgs e)
        {
            dynWorkspaceViewModel vm = (DataContext as dynWorkspaceViewModel);

            //double left = Canvas.GetLeft(e);
            //double top = Canvas.GetTop(e);
            dynNodeModel n = (e as NodeEventArgs).Node;

            double left = n.X;
            double top = n.Y;

            //MVVM:can't access the logscroller here - we'll have to just center the node for now.
            double x = left + n.Width / 2 - outerCanvas.ActualWidth / 2;
            double y = top + n.Height/2 - outerCanvas.ActualHeight/2;  // - LogScroller.ActualHeight);

            //MVVM : replaced direct set with command call on view model
            //CurrentOffset = new Point(-x, -y);
            vm.SetCurrentOffsetCommand.Execute(new Point(-x, -y));
        }

        private void DrawGrid()
        {
            //clear the canvas's children
            //WorkBench.Children.Clear();
            double gridSpacing = 100.0;

            for (double i = 0.0; i < selectionCanvas.ActualWidth; i += gridSpacing)
            {
                var xLine = new Line();
                xLine.Stroke = new SolidColorBrush(Color.FromArgb(50, 100, 100, 100));
                xLine.X1 = i;
                xLine.Y1 = 0;
                xLine.X2 = i;
                xLine.Y2 = selectionCanvas.ActualHeight;
                xLine.HorizontalAlignment = HorizontalAlignment.Left;
                xLine.VerticalAlignment = VerticalAlignment.Center;
                xLine.StrokeThickness = 1;
                
                selectionCanvas.Children.Add(xLine);
                //Dynamo.Controls.DragCanvas.SetCanBeDragged(xLine, false);
                xLine.IsHitTestVisible = false;

                Binding binding = new Binding() 
                { 
                    Path = new PropertyPath("FullscreenWatchVisible"), 
                    Converter = new InverseBoolToVisibilityConverter(),
                    Mode = BindingMode.OneWay,
                };
                xLine.SetBinding(UIElement.VisibilityProperty, binding);
            }
            for (double i = 0.0; i < selectionCanvas.ActualHeight; i += gridSpacing)
            {
                var yLine = new Line();
                yLine.Stroke = new SolidColorBrush(Color.FromArgb(50, 100, 100, 100));
                yLine.X1 = 0;
                yLine.Y1 = i;
                yLine.X2 = selectionCanvas.ActualWidth;
                yLine.Y2 = i;
                yLine.HorizontalAlignment = HorizontalAlignment.Left;
                yLine.VerticalAlignment = VerticalAlignment.Center;
                yLine.StrokeThickness = 1;
                selectionCanvas.Children.Add(yLine);
                //Dynamo.Controls.DragCanvas.SetCanBeDragged(yLine, false);
                
                yLine.IsHitTestVisible = false;

                Binding binding = new Binding()
                {
                    Path = new PropertyPath("FullscreenWatchVisible"),
                    Converter = new InverseBoolToVisibilityConverter(),
                    Mode = BindingMode.OneWay,
                };
                yLine.SetBinding(UIElement.VisibilityProperty, binding);
            }
        }

        private void WorkBench_OnLoaded(object sender, RoutedEventArgs e)
        {
            WorkBench = sender as Dynamo.Controls.DragCanvas;
            //DrawGrid();
        }
    }
}
