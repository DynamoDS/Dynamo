using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Dynamo.DSEngine;
using Dynamo.ViewModels;
using HelixToolkit.Wpf;
using Color = System.Windows.Media.Color;

namespace Dynamo.Controls
{

    /// <summary>
    /// Interaction logic for WatchControl.xaml
    /// </summary>
    public partial class Watch3DView : UserControl, INotifyPropertyChanged
    {
        #region events

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region private members

        private readonly Guid _id=Guid.Empty;
        private Point _rightMousePoint;
        private Point3DCollection _points = new Point3DCollection();
        private Point3DCollection _lines = new Point3DCollection();
        private Point3DCollection _xAxis = new Point3DCollection();
        private Point3DCollection _yAxis = new Point3DCollection();
        private Point3DCollection _zAxis = new Point3DCollection();
        private MeshGeometry3D _mesh = new MeshGeometry3D();
        private Point3DCollection _pointsSelected = new Point3DCollection();
        private Point3DCollection _linesSelected = new Point3DCollection();
        private MeshGeometry3D _meshSelected = new MeshGeometry3D();
        private List<Point3D> _grid = new List<Point3D>();
        private List<BillboardTextItem> _text = new List<BillboardTextItem>();

        #endregion

        #region public properties

        public Material MeshMaterial
        {
            get { return Materials.White; }
        }

        public List<Point3D> Grid
        {
            get { return _grid; }
            set
            {
                _grid = value;
                NotifyPropertyChanged("Grid");
            }
        }

        public Point3DCollection Points
        {
            get { return _points; }
            set
            {
                _points = value;
            }
        }

        public Point3DCollection Lines
        {
            get { return _lines; }
            set
            {
                _lines = value;
            }
        }

        public Point3DCollection XAxes
        {
            get { return _xAxis; }
            set
            {
                _xAxis = value;
            }
        }

        public Point3DCollection YAxes
        {
            get { return _yAxis; }
            set
            {
                _yAxis = value;
            }
        }

        public Point3DCollection ZAxes
        {
            get { return _zAxis; }
            set
            {
                _zAxis = value;
            }
        }

        public MeshGeometry3D Mesh
        {
            get { return _mesh; }
            set
            {
                _mesh = value;
            }
        }

        public Point3DCollection PointsSelected
        {
            get { return _pointsSelected; }
            set
            {
                _pointsSelected = value;
            }
        }

        public Point3DCollection LinesSelected
        {
            get { return _linesSelected; }
            set
            {
                _linesSelected = value;
            }
        }

        public MeshGeometry3D MeshSelected
        {
            get { return _meshSelected; }
            set
            {
                _meshSelected = value;
            }
        }

        public List<BillboardTextItem> Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
            }
        }

        public HelixViewport3D View
        {
            get { return watch_view; }
        }

        /// <summary>
        /// Used for testing to track the number of meshes that are merged
        /// during render.
        /// </summary>
        public int MeshCount { get; set; }

        #endregion

        #region constructors

        public Watch3DView()
        {
            InitializeComponent();
            watch_view.DataContext = this;
            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;
        }

        public Watch3DView(Guid id)
        {
            InitializeComponent();
            watch_view.DataContext = this;
            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;

            _id = id;

        }

        public Watch3DView(Guid id, IWatchViewModel dataContext)
        {
            this.DataContext = dataContext;

            InitializeComponent();
            watch_view.DataContext = this;
            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;

            _id = id;
        }

        #endregion

        #region event handlers

        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Watch 3D view unloaded.");

            var vm = DataContext as IWatchViewModel;
            if (vm != null)
            {
                vm.VisualizationManager.RenderComplete -= VisualizationManagerRenderComplete;
                vm.VisualizationManager.ResultsReadyToVisualize -= VisualizationManager_ResultsReadyToVisualize;
            }
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            MouseLeftButtonDown += view_MouseButtonIgnore;
            MouseLeftButtonUp += view_MouseButtonIgnore;
            MouseRightButtonUp += view_MouseRightButtonUp;
            PreviewMouseRightButtonDown += view_PreviewMouseRightButtonDown;

            var vm = DataContext as IWatchViewModel;
            
            //check this for null so the designer can load the preview
            if (vm != null)
            {
                vm.VisualizationManager.RenderComplete += VisualizationManagerRenderComplete;
                vm.VisualizationManager.ResultsReadyToVisualize += VisualizationManager_ResultsReadyToVisualize;
            }

            DrawGrid();
        }

        /// <summary>
        /// Handler for the visualization manager's ResultsReadyToVisualize event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VisualizationManager_ResultsReadyToVisualize(object sender, VisualizationEventArgs e)
        {
            if (CheckAccess())
                RenderDrawables(e);
            else
            {
                // Scheduler invokes ResultsReadyToVisualize on background thread.
                Dispatcher.BeginInvoke(new Action(() => RenderDrawables(e)));
            }
        }

        /// <summary>
        /// When visualization is complete, the view requests it's visuals. For Full
        /// screen watch, this will be all renderables. For a Watch 3D node, this will
        /// be the subset of the renderables associated with the node.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VisualizationManagerRenderComplete(object sender, RenderCompletionEventArgs e)
        {
            var executeCommand = new Action(delegate
            {
                var vm = (IWatchViewModel)DataContext;
                if (vm.GetBranchVisualizationCommand.CanExecute(e.TaskId))
                    vm.GetBranchVisualizationCommand.Execute(e.TaskId);
            });

            if (CheckAccess())
                executeCommand();
            else
                Dispatcher.BeginInvoke(executeCommand);
        }

        /// <summary>
        /// Callback for thumb control's DragStarted event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResizeThumb_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Callbcak for thumb control's DragCompleted event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResizeThumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Callback for thumb control's DragDelta event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResizeThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

            //Debug.WriteLine("d_x:" + e.HorizontalChange + "," + "d_y:" + e.VerticalChange);
            //Debug.WriteLine("Size:" + _nodeUI.Width + "," + _nodeUI.Height);
            //Debug.WriteLine("ActualSize:" + _nodeUI.ActualWidth + "," + _nodeUI.ActualHeight);
            //Debug.WriteLine("Grid size:" + _nodeUI.ActualWidth + "," + _nodeUI.ActualHeight);

            if (xAdjust >= inputGrid.MinWidth)
            {
                Width = xAdjust;
            }

            if (yAdjust >= inputGrid.MinHeight)
            {
                Height = yAdjust;
            }
        }

        protected void OnZoomToFitClicked(object sender, RoutedEventArgs e)
        {
            watch_view.ZoomExtents();
        }

        void view_MouseButtonIgnore(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        void view_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _rightMousePoint = e.GetPosition(topControl);
        }

        void view_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if the mouse has moved, and this is a right click, we assume 
            // rotation. handle the event so we don't show the context menu
            // if the user wants the contextual menu they can click on the
            // node sidebar or top bar
            if (e.GetPosition(topControl) != _rightMousePoint)
            {
                e.Handled = true;
            }
        }

        private void Watch_view_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Point mousePos = e.GetPosition(watch_view);
            //PointHitTestParameters hitParams = new PointHitTestParameters(mousePos);
            //VisualTreeHelper.HitTest(watch_view, null, ResultCallback, hitParams);
            //e.Handled = true;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Create the grid
        /// </summary>
        private void DrawGrid()
        {
            Grid = null;

            var newLines = new List<Point3D>();

            for (int x = -10; x <= 10; x++)
            {
                newLines.Add(new Point3D(x, -10, -.001));
                newLines.Add(new Point3D(x, 10, -.001));
            }

            for (int y = -10; y <= 10; y++)
            {
                newLines.Add(new Point3D(-10, y, -.001));
                newLines.Add(new Point3D(10, y, -.001));
            }

            Grid = newLines;
        }

        /// <summary>
        /// Use the render packages returned from the visualization manager to update the visuals.
        /// The visualization event arguments will contain a set of render packages and an id representing 
        /// the associated node. Visualizations for the background preview will return an empty id.
        /// </summary>
        /// <param name="e"></param>
        public void RenderDrawables(VisualizationEventArgs e)
        {
            //check the id, if the id is meant for another watch,
            //then ignore it
            if (e.Id != _id)
            {
                return;
            }

            Debug.WriteLine(string.Format("Rendering visuals for {0}", e.Id));

            var sw = new Stopwatch();
            sw.Start();

            Points = null;
            Lines = null;
            Mesh = null;
            XAxes = null;
            YAxes = null;
            ZAxes = null;
            PointsSelected = null;
            LinesSelected = null;
            MeshSelected = null;
            Text = null;
            MeshCount = 0;

            //separate the selected packages
            var packages = e.Packages.Where(x => x.Selected == false)
                .Where(rp=>rp.TriangleVertices.Count % 9 == 0)
                .ToArray();
            var selPackages = e.Packages
                .Where(x => x.Selected)
                .Where(rp => rp.TriangleVertices.Count % 9 == 0)
                .ToArray();

            //pre-size the points collections
            var pointsCount = packages.Select(x => x.PointVertices.Count/3).Sum();
            var selPointsCount = selPackages.Select(x => x.PointVertices.Count / 3).Sum();
            var points = new Point3DCollection(pointsCount);
            var pointsSelected = new Point3DCollection(selPointsCount);

            //pre-size the lines collections
            //these sizes are conservative as the axis lines will be
            //taken from the linestripvertex collections as well.
            var lineCount = packages.Select(x => x.LineStripVertices.Count/3).Sum();
            var lineSelCount = selPackages.Select(x => x.LineStripVertices.Count / 3).Sum();
            var lines = new Point3DCollection(lineCount);
            var linesSelected = new Point3DCollection(lineSelCount);
            var redLines = new Point3DCollection(lineCount);
            var greenLines = new Point3DCollection(lineCount);
            var blueLines = new Point3DCollection(lineCount);

            //pre-size the text collection
            var textCount = e.Packages.Count(x => x.DisplayLabels);
            var text = new List<BillboardTextItem>(textCount);

            //http://blogs.msdn.com/b/timothyc/archive/2006/08/31/734308.aspx
            //presize the mesh collections
            var meshVertCount = packages.Select(x => x.TriangleVertices.Count / 3).Sum();
            var meshVertSelCount = selPackages.Select(x => x.TriangleVertices.Count / 3).Sum();

            var mesh = new MeshGeometry3D();
            var meshSel = new MeshGeometry3D();
            var verts = new Point3DCollection(meshVertCount);
            var vertsSel = new Point3DCollection(meshVertSelCount);
            var norms = new Vector3DCollection(meshVertCount);
            var normsSel = new Vector3DCollection(meshVertSelCount);
            var tris = new Int32Collection(meshVertCount);
            var trisSel = new Int32Collection(meshVertSelCount);
                
            foreach (var package in packages)
            {
                ConvertPoints(package, points, text);
                ConvertLines(package, lines, redLines, greenLines, blueLines, text);
                ConvertMeshes(package, verts, norms, tris);
            }

            foreach (var package in selPackages)
            {
                ConvertPoints(package, pointsSelected, text);
                ConvertLines(package, linesSelected, redLines, greenLines, blueLines, text);
                ConvertMeshes(package, vertsSel, normsSel, trisSel);
            }

            sw.Stop();
            Debug.WriteLine(string.Format("RENDER: {0} ellapsed for updating background preview.", sw.Elapsed));

            var vm = (IWatchViewModel)DataContext;
            if (vm.CheckForLatestRenderCommand.CanExecute(e.TaskId))
            {
                vm.CheckForLatestRenderCommand.Execute(e.TaskId);
            }

            points.Freeze();
            pointsSelected.Freeze();
            lines.Freeze();
            linesSelected.Freeze();
            redLines.Freeze();
            greenLines.Freeze();
            blueLines.Freeze();
            verts.Freeze();
            norms.Freeze();
            tris.Freeze();
            vertsSel.Freeze();
            normsSel.Freeze();
            trisSel.Freeze();

            Dispatcher.Invoke(new Action<Point3DCollection, Point3DCollection,
                Point3DCollection, Point3DCollection, Point3DCollection, Point3DCollection,
                Point3DCollection, Point3DCollection, Vector3DCollection, Int32Collection, 
                Point3DCollection, Vector3DCollection, Int32Collection, MeshGeometry3D,
                MeshGeometry3D, List<BillboardTextItem>>(SendGraphicsToView), DispatcherPriority.Render,
                               new object[] {points, pointsSelected, lines, linesSelected, redLines, 
                                   greenLines, blueLines, verts, norms, tris, vertsSel, normsSel, 
                                   trisSel, mesh, meshSel, text});
        }

        private void SendGraphicsToView(Point3DCollection points, Point3DCollection pointsSelected,
            Point3DCollection lines, Point3DCollection linesSelected, Point3DCollection redLines, Point3DCollection greenLines,
            Point3DCollection blueLines, Point3DCollection verts, Vector3DCollection norms, Int32Collection tris,
            Point3DCollection vertsSel, Vector3DCollection normsSel, Int32Collection trisSel, MeshGeometry3D mesh,
            MeshGeometry3D meshSel, List<BillboardTextItem> text)
        {
            Points = points;
            PointsSelected = pointsSelected;
            Lines = lines;
            LinesSelected = linesSelected;
            XAxes = redLines;
            YAxes = greenLines;
            ZAxes = blueLines;
            mesh.Positions = verts;
            mesh.Normals = norms;
            mesh.TriangleIndices = tris;
            meshSel.Positions = vertsSel;
            meshSel.Normals = normsSel;
            meshSel.TriangleIndices = trisSel;
            Mesh = mesh;
            MeshSelected = meshSel;
            Text = text;

            // Send property changed notifications for everything
            NotifyPropertyChanged(string.Empty);
        }

        private void ConvertPoints(RenderPackage p,
            ICollection<Point3D> pointColl,
            ICollection<BillboardTextItem> text)
        {
            for (int i = 0; i < p.PointVertices.Count; i += 3)
            {

                var pos = new Point3D(
                    p.PointVertices[i],
                    p.PointVertices[i + 1],
                    p.PointVertices[i + 2]);

                pointColl.Add(pos);

                if (p.DisplayLabels)
                {
                    text.Add(new BillboardTextItem {Text = CleanTag(p.Tag), Position = pos});
                }
            }
        }

        private void ConvertLines(RenderPackage p,
            ICollection<Point3D> lineColl,
            ICollection<Point3D> redLines,
            ICollection<Point3D> greenLines,
            ICollection<Point3D> blueLines,
            ICollection<BillboardTextItem> text)
        {
            int idx = 0;
            int color_idx = 0;

            int outerCount = 0;
            foreach (var count in p.LineStripVertexCounts)
            {
                for (int i = 0; i < count; ++i)
                {
                    var point = new Point3D(p.LineStripVertices[idx], p.LineStripVertices[idx + 1],
                        p.LineStripVertices[idx + 2]);

                    if (i == 0 && outerCount == 0 && p.DisplayLabels)
                    {
                        text.Add(new BillboardTextItem { Text = CleanTag(p.Tag), Position = point });
                    }

                    if (i != 0 && i != count - 1)
                    {
                        lineColl.Add(point);
                    }
                    
                    bool isAxis = false;
                    var startColor = Color.FromRgb(
                                            p.LineStripVertexColors[color_idx],
                                            p.LineStripVertexColors[color_idx + 1],
                                            p.LineStripVertexColors[color_idx + 2]);

                    if (startColor == Color.FromRgb(255, 0, 0))
                    {
                        redLines.Add(point);
                        isAxis = true;
                    }
                    else if (startColor == Color.FromRgb(0, 255, 0))
                    {
                        greenLines.Add(point);
                        isAxis = true;
                    }
                    else if (startColor == Color.FromRgb(0, 0, 255))
                    {
                        blueLines.Add(point);
                        isAxis = true;
                    }

                    if (!isAxis)
                    {
                        lineColl.Add(point);
                    } 

                    idx += 3;
                    color_idx += 4;
                }
                outerCount++;
            }
        }

        private void ConvertMeshes(RenderPackage p,
            ICollection<Point3D> points, ICollection<Vector3D> norms,
            ICollection<int> tris)
        {
            for (int i = 0; i < p.TriangleVertices.Count; i+=3)
            {
                var new_point = new Point3D(p.TriangleVertices[i],
                                            p.TriangleVertices[i + 1],
                                            p.TriangleVertices[i + 2]);

                var normal = new Vector3D(p.TriangleNormals[i],
                                            p.TriangleNormals[i + 1],
                                            p.TriangleNormals[i + 2]);
                    
                tris.Add(points.Count);
                points.Add(new_point);
                norms.Add(normal);
            }

            if (tris.Count > 0)
            {
                MeshCount++;
            }
        }

        private string CleanTag(string tag)
        {
            var splits = tag.Split(':');
            if (splits.Count() <= 1) return tag;

            var sb = new StringBuilder();
            for (int i = 1; i < splits.Count(); i++)
            {
                sb.AppendFormat("[{0}]", splits[i]);
            }
            return sb.ToString();
        }

        #endregion
    }
}
