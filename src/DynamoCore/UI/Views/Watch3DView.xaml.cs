using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Windows.Threading;
using Dynamo.DSEngine;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using HelixToolkit.Wpf;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for WatchControl.xaml
    /// </summary>
    public partial class Watch3DView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private readonly string _id="";
        Point _rightMousePoint;

        protected ThreadSafeList<MeshVisual3D> _meshes = new ThreadSafeList<MeshVisual3D>();
        public ThreadSafeList<Point3D> _pointsCache = new ThreadSafeList<Point3D>();
        public ThreadSafeList<Point3D> _linesCache = new ThreadSafeList<Point3D>();
        public ThreadSafeList<Point3D> _xAxisCache = new ThreadSafeList<Point3D>();
        public ThreadSafeList<Point3D> _yAxisCache = new ThreadSafeList<Point3D>();
        public ThreadSafeList<Point3D> _zAxisCache = new ThreadSafeList<Point3D>();
        public MeshGeometry3D _meshCache = new MeshGeometry3D();
        public ThreadSafeList<Point3D> _pointsCacheSelected = new ThreadSafeList<Point3D>();
        public ThreadSafeList<Point3D> _linesCacheSelected = new ThreadSafeList<Point3D>();
        public MeshGeometry3D _meshCacheSelected = new MeshGeometry3D();
        private ThreadSafeList<Point3D> _gridCache = new ThreadSafeList<Point3D>();
        private ThreadSafeList<BillboardTextItem> _text = new ThreadSafeList<BillboardTextItem>();

        public Material HelixMeshMaterial
        {
            get { return Materials.White; }
        }

        public ThreadSafeList<Point3D> HelixGrid
        {
            get { return _gridCache; }
            set
            {
                _gridCache = value;
                NotifyPropertyChanged("HelixGrid");
            }
        }

        public ThreadSafeList<Point3D> HelixPoints
        {
            get { return _pointsCache; }
            set
            {
                _pointsCache = value;
                NotifyPropertyChanged("HelixPoints");
            }
        }

        public ThreadSafeList<Point3D> HelixLines
        {
            get { return _linesCache; }
            set
            {
                _linesCache = value;
                NotifyPropertyChanged("HelixLines");
            }
        }

        public ThreadSafeList<Point3D> HelixXAxes
        {
            get { return _xAxisCache; }
            set
            {
                _xAxisCache = value;
                NotifyPropertyChanged("HelixXAxes");
            }
        }

        public ThreadSafeList<Point3D> HelixYAxes
        {
            get { return _yAxisCache; }
            set
            {
                _yAxisCache = value;
                NotifyPropertyChanged("HelixYAxes");
            }
        }

        public ThreadSafeList<Point3D> HelixZAxes
        {
            get { return _zAxisCache; }
            set
            {
                _zAxisCache = value;
                NotifyPropertyChanged("HelixZAxes");
            }
        }

        public MeshGeometry3D HelixMesh
        {
            get { return _meshCache; }
            set
            {
                _meshCache = value;
                NotifyPropertyChanged("HelixMesh");
            }
        }

        public ThreadSafeList<Point3D> HelixPointsSelected
        {
            get { return _pointsCacheSelected; }
            set
            {
                _pointsCacheSelected = value;
                NotifyPropertyChanged("HelixPointsSelected");
            }
        }

        public ThreadSafeList<Point3D> HelixLinesSelected
        {
            get { return _linesCacheSelected; }
            set
            {
                _linesCacheSelected = value;
                NotifyPropertyChanged("HelixLinesSelected");
            }
        }

        public MeshGeometry3D HelixMeshSelected
        {
            get { return _meshCacheSelected; }
            set
            {
                _meshCacheSelected = value;
                NotifyPropertyChanged("HelixMeshSelected");
            }
        }

        public ThreadSafeList<BillboardTextItem> HelixText
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                NotifyPropertyChanged("HelixText");
            }
        }

        public HelixViewport3D View
        {
            get { return watch_view; }
        }

        public Watch3DView()
        {
            InitializeComponent();
            watch_view.DataContext = this;
            Loaded += WatchViewFullscreen_Loaded;
        }

        public Watch3DView(string id)
        {
            InitializeComponent();
            watch_view.DataContext = this;
            Loaded += WatchViewFullscreen_Loaded;
            _id = id;
        }

        void WatchViewFullscreen_Loaded(object sender, RoutedEventArgs e)
        {
            MouseLeftButtonDown += new MouseButtonEventHandler(view_MouseButtonIgnore);
            MouseLeftButtonUp += new MouseButtonEventHandler(view_MouseButtonIgnore);
            MouseRightButtonUp += new MouseButtonEventHandler(view_MouseRightButtonUp);
            PreviewMouseRightButtonDown += new MouseButtonEventHandler(view_PreviewMouseRightButtonDown);

            var mi = new MenuItem { Header = "Zoom to Fit" };
            mi.Click += new RoutedEventHandler(mi_Click);

            MainContextMenu.Items.Add(mi);

            //check this for null so the designer can load the preview
            if (dynSettings.Controller != null)
            {
                dynSettings.Controller.VisualizationManager.VisualizationUpdateComplete += VisualizationManager_VisualizationUpdateComplete;
                dynSettings.Controller.VisualizationManager.ResultsReadyToVisualize += VisualizationManager_ResultsReadyToVisualize;
            }

            DrawGrid();
        }

        /// <summary>
        /// Handler for the visualization manager's ResultsReadyToVisualize event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void VisualizationManager_ResultsReadyToVisualize(object sender, VisualizationEventArgs e)
        {
            Dispatcher.Invoke(new Action<VisualizationEventArgs>(RenderDrawables), DispatcherPriority.Render,
                                new object[] {e});
        }

        /// <summary>
        /// When visualization is complete, the view requests it's visuals. For Full
        /// screen watch, this will be all renderables. For a Watch 3D node, this will
        /// be the subset of the renderables associated with the node.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void VisualizationManager_VisualizationUpdateComplete(object sender, EventArgs e)
        {
            if (dynSettings.Controller == null)
                return;

            Dispatcher.Invoke(new Action(delegate
            {
                var vm = (IWatchViewModel) DataContext;
                   
                if (vm.GetBranchVisualizationCommand.CanExecute(null))
                {
                    vm.GetBranchVisualizationCommand.Execute(null);
                }
            }));

        }

        /// <summary>
        /// Create the grid
        /// </summary>
        private void DrawGrid()
        {
            HelixGrid = null;

            var newLines = new ThreadSafeList<Point3D>();

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

            HelixGrid = newLines;
        }

        /// <summary>
        /// Use the render description returned from the visualization manager to update the visuals.
        /// The visualization event arguments will contain a render description and an id representing 
        /// the associated node. Visualizations for the background preview will return an empty id.
        /// </summary>
        /// <param name="e"></param>
        private void RenderDrawables(VisualizationEventArgs e)
        {
            //Debug.WriteLine(string.Format("Rendering full screen Watch3D on thread {0}.", System.Threading.Thread.CurrentThread.ManagedThreadId));
            
            //check the id, if the id is meant for another watch,
            //then ignore it
            if (e.Id != _id)
            {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();

            HelixPoints = null;
            HelixLines = null;
            HelixMesh = null;
            HelixXAxes = null;
            HelixYAxes = null;
            HelixZAxes = null;
            HelixPointsSelected = null;
            HelixLinesSelected = null;
            HelixMeshSelected = null;
            HelixText = null;

            var points = new ThreadSafeList<Point3D>();
            var pointsSelected = new ThreadSafeList<Point3D>();
            var lines = new ThreadSafeList<Point3D>();
            var linesSelected = new ThreadSafeList<Point3D>();
            var redLines = new ThreadSafeList<Point3D>();
            var greenLines = new ThreadSafeList<Point3D>();
            var blueLines = new ThreadSafeList<Point3D>();
            var text = new ThreadSafeList<BillboardTextItem>();
            var meshes = new ThreadSafeList<MeshGeometry3D>();
            var meshesSelected = new ThreadSafeList<MeshGeometry3D>();

            foreach (var package in e.Packages)
            {
                ConvertPoints(package, points, pointsSelected, text );
                ConvertLines(package, lines, linesSelected, redLines, greenLines, blueLines, text);
                ConvertMeshes(package, meshes, meshesSelected);
            }

            HelixPoints = points;
            HelixPointsSelected = pointsSelected;
            HelixLines = lines;
            HelixLinesSelected = linesSelected;
            HelixXAxes = redLines;
            HelixYAxes = greenLines;
            HelixZAxes = blueLines;
            HelixMesh = MergeMeshes(meshes);
            HelixMeshSelected = MergeMeshes(meshesSelected);
            HelixText = text;

            //var sb = new StringBuilder();
            //sb.AppendLine();
            //sb.AppendLine(string.Format("Rendering complete:"));
            //sb.AppendLine(string.Format("Points: {0}", rd.Points.Count + rd.SelectedPoints.Count));
            //sb.AppendLine(string.Format("Line segments: {0}", rd.Lines.Count / 2 + rd.SelectedLines.Count / 2));
            //sb.AppendLine(string.Format("Mesh vertices: {0}",
            //    rd.Meshes.SelectMany(x => x.Positions).Count() +
            //    rd.SelectedMeshes.SelectMany(x => x.Positions).Count()));
            //sb.Append(string.Format("Mesh faces: {0}",
            //    rd.Meshes.SelectMany(x => x.TriangleIndices).Count() / 3 +
            //    rd.SelectedMeshes.SelectMany(x => x.TriangleIndices).Count() / 3));
            ////DynamoLogger.Instance.Log(sb.ToString());
            //Debug.WriteLine(sb.ToString());
             
            sw.Stop();
            //DynamoLogger.Instance.Log(string.Format("{0} ellapsed for updating background preview.", sw.Elapsed));

            Debug.WriteLine(string.Format("{0} ellapsed for updating background preview.", sw.Elapsed));

        }

        private void ConvertPoints(RenderPackage p, 
            ThreadSafeList<Point3D> points,
            ThreadSafeList<Point3D> pointsSelected, 
            ThreadSafeList<BillboardTextItem> text)
        {
            for (int i = 0; i < p.PointVertices.Count; i += 3)
            {
                var pos = new Point3D(
                    p.PointVertices[i],
                    p.PointVertices[i + 1],
                    p.PointVertices[i + 2]);

                if (p.Selected)
                {
                    pointsSelected.Add(pos);
                }
                else
                {
                    points.Add(pos);
                }

                if (p.DisplayLabels)
                {
                    text.Add(new BillboardTextItem {Text = p.Tag, Position = pos});
                }
            }
        }

        private void ConvertLines(RenderPackage p, 
            ThreadSafeList<Point3D> lines,
            ThreadSafeList<Point3D> linesSelected, 
            ThreadSafeList<Point3D> redLines,
            ThreadSafeList<Point3D> greenLines,
            ThreadSafeList<Point3D> blueLines,
            ThreadSafeList<BillboardTextItem> text)
        {
            int colorCount = 0;
            for (int i = 0; i < p.LineStripVertices.Count - 3; i += 3)
            {
                var start = new Point3D(
                    p.LineStripVertices[i],
                    p.LineStripVertices[i + 1],
                    p.LineStripVertices[i + 2]);

                var end = new Point3D(
                    p.LineStripVertices[i + 3],
                    p.LineStripVertices[i + 4],
                    p.LineStripVertices[i + 5]);

                //HACK: test for line color using only 
                //the start value
                var startColor = Color.FromRgb(
                    p.LineStripVertexColors[colorCount],
                    p.LineStripVertexColors[colorCount + 1],
                    p.LineStripVertexColors[colorCount + 2]);

                //var endColor = new Point3D(
                //    p.LineStripVertexColors[i + 3],
                //    p.LineStripVertexColors[i + 4],
                //    p.LineStripVertexColors[i + 5]);

                //draw a label at the start of the curve
                if (p.DisplayLabels && i == 0)
                {
                    text.Add(new BillboardTextItem {Text = p.Tag, Position = start});
                }

                bool isAxis = false;
                if (startColor == Color.FromRgb(255,0,0))
                {
                    redLines.Add(start);
                    redLines.Add(end);
                    isAxis = true;
                }
                else if (startColor == Color.FromRgb(0,255,0))
                {
                    greenLines.Add(start);
                    greenLines.Add(end);
                    isAxis = true;
                }
                else if (startColor == Color.FromRgb(0,255,255))
                {
                    blueLines.Add(start);
                    blueLines.Add(end);
                    isAxis = true;
                }

                if (!isAxis)
                {
                    if (p.Selected)
                    {
                        linesSelected.Add(start);
                        linesSelected.Add(end);
                    }
                    else
                    {
                        lines.Add(start);
                        lines.Add(end);
                    }
                }

                colorCount += 4;
            }
        }

        private void ConvertMeshes(RenderPackage p,
            ThreadSafeList<MeshGeometry3D> meshes,
            ThreadSafeList<MeshGeometry3D> meshesSelected)
        {
            //var sw = new Stopwatch();
            //sw.Start();

            var builder = new MeshBuilder();
            var points = new Point3DCollection();
            var tex = new PointCollection();
            var norms = new Vector3DCollection();
            var tris = new List<int>();

            for (int i = 0; i < p.TriangleVertices.Count; i+=3)
            {
                var new_point = new Point3D(p.TriangleVertices[i],
                                            p.TriangleVertices[i + 1],
                                            p.TriangleVertices[i + 2]);

                var normal = new Vector3D(p.TriangleNormals[i],
                                            p.TriangleNormals[i + 1],
                                            p.TriangleNormals[i + 2]);

                //find a matching point
                //compare the angle between the normals
                //to discern a 'break' angle for adjacent faces
                //int foundIndex = -1;
                //for (int j = 0; j < points.Count; j++)
                //{
                //    var testPt = points[j];
                //    var testNorm = norms[j];
                //    var ang = Vector3D.AngleBetween(normal, testNorm);

                //    if (new_point.X == testPt.X &&
                //        new_point.Y == testPt.Y &&
                //        new_point.Z == testPt.Z &&
                //        ang > 90.0000)
                //    {
                //        foundIndex = j;
                //        break;
                //    }
                //}

                //if (foundIndex != -1)
                //{
                //    tris.Add(foundIndex);
                //    continue;
                //}
                    
                tris.Add(points.Count);
                points.Add(new_point);
                norms.Add(normal);
                tex.Add(new System.Windows.Point(0,0));

                //octree.AddNode(new_point.X, new_point.Y, new_point.Z, node.GUID.ToString());
            }

            builder.Append(points, tris, norms, tex);

            //don't add empty meshes
            if (builder.Positions.Count > 0)
            {
                if (p.Selected)
                {
                    meshesSelected.Add(builder.ToMesh(true));
                }
                else
                {
                    meshes.Add(builder.ToMesh(true));
                }
            }
        }


        /// <summary>
        /// A utility method for merging multiple meshes into one.
        /// </summary>
        /// <param name="meshes"></param>
        /// <returns></returns>
        private MeshGeometry3D MergeMeshes(ThreadSafeList<MeshGeometry3D> meshes)
        {
            if (meshes.Count == 0)
                return null;

            int offset = 0;

            var builder = new MeshBuilder();

            foreach (MeshGeometry3D m in meshes)
            {
                foreach (var pos in m.Positions)
                {
                    builder.Positions.Add(pos);
                }
                foreach (var index in m.TriangleIndices)
                {
                    builder.TriangleIndices.Add(index + offset);
                }
                foreach (var norm in m.Normals)
                {
                    builder.Normals.Add(norm);
                }
                foreach (var tc in m.TextureCoordinates)
                {
                    builder.TextureCoordinates.Add(tc);
                }

                offset += m.Positions.Count;
            }

            return builder.ToMesh(false);
        }

        protected void mi_Click(object sender, RoutedEventArgs e)
        {
            watch_view.ZoomExtents();
        }

        private void MainContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
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
            Point mousePos = e.GetPosition(watch_view);
            PointHitTestParameters hitParams = new PointHitTestParameters(mousePos);
            VisualTreeHelper.HitTest(watch_view, null, ResultCallback, hitParams);
            e.Handled = true;
        }

        public HitTestResultBehavior ResultCallback(HitTestResult result)
        {
            // Did we hit 3D?
            var rayResult = result as RayHitTestResult;
            if (rayResult != null)
            {
                // Did we hit a MeshGeometry3D?
                var rayMeshResult =
                    rayResult as RayMeshGeometry3DHitTestResult;

                if (rayMeshResult != null)
                {
                    // Yes we did!
                    var pt = rayMeshResult.PointHit;
                    ((IWatchViewModel)DataContext).SelectVisualizationInViewCommand.Execute(new double[] { pt.X, pt.Y, pt.Z });
                    return HitTestResultBehavior.Stop;
                }
            }

            return HitTestResultBehavior.Continue;
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
    }
}
