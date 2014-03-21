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

        private List<Point3D> _points = new List<Point3D>();
        private List<Point3D> _lines = new List<Point3D>();
        private List<Point3D> _xAxis = new List<Point3D>();
        private List<Point3D> _yAxis = new List<Point3D>();
        private List<Point3D> _zAxis = new List<Point3D>();
        private MeshGeometry3D _mesh = new MeshGeometry3D();
        private List<Point3D> _pointsSelected = new List<Point3D>();
        private List<Point3D> _linesSelected = new List<Point3D>();
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

        public List<Point3D> Points
        {
            get { return _points; }
            set
            {
                _points = value;
                NotifyPropertyChanged("Points");
            }
        }

        public List<Point3D> Lines
        {
            get { return _lines; }
            set
            {
                _lines = value;
                NotifyPropertyChanged("Lines");
            }
        }

        public List<Point3D> XAxes
        {
            get { return _xAxis; }
            set
            {
                _xAxis = value;
                NotifyPropertyChanged("XAxes");
            }
        }

        public List<Point3D> YAxes
        {
            get { return _yAxis; }
            set
            {
                _yAxis = value;
                NotifyPropertyChanged("YAxes");
            }
        }

        public List<Point3D> ZAxes
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

        public List<Point3D> PointsSelected
        {
            get { return _pointsSelected; }
            set
            {
                _pointsSelected = value;
                NotifyPropertyChanged("PointsSelected");
            }
        }

        public List<Point3D> LinesSelected
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
            try
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

                var points = new List<Point3D>();
                var pointsSelected = new List<Point3D>();
                var lines = new List<Point3D>();
                var linesSelected = new List<Point3D>();
                var redLines = new List<Point3D>();
                var greenLines = new List<Point3D>();
                var blueLines = new List<Point3D>();
                var text = new List<BillboardTextItem>();
                var meshes = new List<MeshGeometry3D>();
                var meshesSelected = new List<MeshGeometry3D>();

                foreach (var package in e.Packages)
                {
                    ConvertPoints(package, points, pointsSelected, text);
                    ConvertLines(package, lines, linesSelected, redLines, greenLines, blueLines, text);
                    ConvertMeshes(package, meshes, meshesSelected);
                }

                Points = points;
                PointsSelected = pointsSelected;
                Lines = lines;
                LinesSelected = linesSelected;
                XAxes = redLines;
                YAxes = greenLines;
                ZAxes = blueLines;

                MeshCount += meshes.Count + meshesSelected.Count;

                Mesh = MergeMeshes(meshes);
                MeshSelected = MergeMeshes(meshesSelected);
                Text = text;

                sw.Stop();
                //DynamoLogger.Instance.Log(string.Format("{0} ellapsed for updating background preview.", sw.Elapsed));

                Debug.WriteLine(string.Format("{0} ellapsed for updating background preview.", sw.Elapsed));
            }
            catch (InvalidOperationException exp)
            {

                Debug.WriteLine("WARNING: Exception occured in rendering " + exp.ToString());
            }
        }

        private void ConvertPoints(RenderPackage p,
            List<Point3D> points,
            List<Point3D> pointsSelected,
            List<BillboardTextItem> text)
        {
            var pointColl = p.Selected ? pointsSelected : points;
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
            List<Point3D> lines,
            List<Point3D> linesSelected,
            List<Point3D> redLines,
            List<Point3D> greenLines,
            List<Point3D> blueLines,
            List<BillboardTextItem> text)
        {
            //int colorCount = 0;
            int idx = 0;
            int color_idx = 0;

            var lineColl = p.Selected ? linesSelected : lines;
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
            List<MeshGeometry3D> meshes,
            List<MeshGeometry3D> meshesSelected)
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

        /// <summary>
        /// A utility method for merging multiple meshes into one.
        /// </summary>
        /// <param name="meshes"></param>
        /// <returns></returns>
        private MeshGeometry3D MergeMeshes(List<MeshGeometry3D> meshes)
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
