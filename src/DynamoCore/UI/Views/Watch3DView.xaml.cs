using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Dynamo.DSEngine;
using Dynamo.Utilities;
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
                NotifyPropertyChanged("Points");
            }
        }

        public Point3DCollection Lines
        {
            get { return _lines; }
            set
            {
                _lines = value;
                NotifyPropertyChanged("Lines");
            }
        }

        public Point3DCollection XAxes
        {
            get { return _xAxis; }
            set
            {
                _xAxis = value;
                NotifyPropertyChanged("XAxes");
            }
        }

        public Point3DCollection YAxes
        {
            get { return _yAxis; }
            set
            {
                _yAxis = value;
                NotifyPropertyChanged("YAxes");
            }
        }

        public Point3DCollection ZAxes
        {
            get { return _zAxis; }
            set
            {
                _zAxis = value;
                NotifyPropertyChanged("ZAxes");
            }
        }

        public MeshGeometry3D Mesh
        {
            get { return _mesh; }
            set
            {
                _mesh = value;
                NotifyPropertyChanged("Mesh");
            }
        }

        public Point3DCollection PointsSelected
        {
            get { return _pointsSelected; }
            set
            {
                _pointsSelected = value;
                NotifyPropertyChanged("PointsSelected");
            }
        }

        public Point3DCollection LinesSelected
        {
            get { return _linesSelected; }
            set
            {
                _linesSelected = value;
                NotifyPropertyChanged("LinesSelected");
            }
        }

        public MeshGeometry3D MeshSelected
        {
            get { return _meshSelected; }
            set
            {
                _meshSelected = value;
                NotifyPropertyChanged("MeshSelected");
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
                NotifyPropertyChanged("Text");
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

        public Watch3DView()
        {
            InitializeComponent();
            watch_view.DataContext = this;
            Loaded += OnViewLoaded;
        }

        public Watch3DView(string id)
        {
            InitializeComponent();
            watch_view.DataContext = this;
            Loaded += OnViewLoaded;
            _id = id;
        }

        void OnViewLoaded(object sender, RoutedEventArgs e)
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
        private void RenderDrawables(VisualizationEventArgs e)
        {
            //check the id, if the id is meant for another watch,
            //then ignore it
            if (e.Id != _id)
            {
                return;
            }

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
            var packages = e.Packages.Where(x => x.Selected == false).ToArray();
            var selPackages = e.Packages.Where(x => x.Selected).ToArray();

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

            points.Freeze();
            pointsSelected.Freeze();
            Points = points;
            PointsSelected = pointsSelected;

            lines.Freeze();
            linesSelected.Freeze();
            redLines.Freeze();
            greenLines.Freeze();
            blueLines.Freeze();
            Lines = lines;
            LinesSelected = linesSelected;
            XAxes = redLines;
            YAxes = greenLines;
            ZAxes = blueLines;

            verts.Freeze();
            norms.Freeze();
            tris.Freeze();
            vertsSel.Freeze();
            normsSel.Freeze();
            trisSel.Freeze();

            mesh.Positions = verts;
            mesh.Normals = norms;
            mesh.TriangleIndices = tris;
            meshSel.Positions = vertsSel;
            meshSel.Normals = normsSel;
            meshSel.TriangleIndices = trisSel;

            Mesh = mesh;
            MeshSelected = meshSel;

            Text = text;

            sw.Stop();
                
            GC.Collect();

            Debug.WriteLine(string.Format("{0} ellapsed for updating background preview.", sw.Elapsed));
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
            //Point mousePos = e.GetPosition(watch_view);
            //PointHitTestParameters hitParams = new PointHitTestParameters(mousePos);
            //VisualTreeHelper.HitTest(watch_view, null, ResultCallback, hitParams);
            //e.Handled = true;
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
