using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;

using Microsoft.FSharp.Collections;

using HelixToolkit.Wpf;

using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.FSchemeInterop;
using Dynamo.Controls;

using Value = Dynamo.FScheme.Value;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for WatchControl.xaml
    /// </summary>
    public partial class WatchViewFullscreen : UserControl
    {
        private Mutex drawMutex = new Mutex();

        System.Windows.Point _rightMousePoint;

        protected PointsVisual3D _points = new PointsVisual3D();
        protected LinesVisual3D _lines = new LinesVisual3D();
        protected List<MeshVisual3D> _meshes = new List<MeshVisual3D>();

        Point3DCollection _pointsCache = new Point3DCollection();
        Point3DCollection _linesCache = new Point3DCollection();

        List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();

        private bool _requiresRedraw = false;

        private List<IDrawable> _drawables = new List<IDrawable>();

        public HelixViewport3D HelixView()
        {
            return watch_view;
        }

        public void SetVisiblePoints(Point3DCollection pointPoints)
        {
            lock (_pointsCache)
            {
                _pointsCache = new Point3DCollection(pointPoints);
            }

            _requiresRedraw = true;
        }

        public void SetVisibleLines(Point3DCollection linePoints)
        {
            lock (_linesCache)
            {
                _linesCache = new Point3DCollection(linePoints);
            }
            
            _requiresRedraw = true;
        }

        public PointsVisual3D HelixPoints
        {
            get
            {
                return _points;
            }
            set
            {
                _points = value;
            }
        }

        public LinesVisual3D HelixLines
        {
            get
            {
                return _lines;
            }
            set
            {
                _lines = value;
            }
        }

        void WatchViewFullscreen_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.FullscreenView = this;
        }

        public WatchViewFullscreen()
        {
            InitializeComponent();

            watch_view.DataContext = this;

            // The DataContext isn't set in the constructor, so set a callback
            this.Loaded += new RoutedEventHandler(WatchViewFullscreen_Loaded);

            MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(view_MouseButtonIgnore);
            MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(view_MouseButtonIgnore);
            MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(view_MouseRightButtonUp);
            PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(view_PreviewMouseRightButtonDown);

            MenuItem mi = new MenuItem();
            mi.Header = "Zoom to Fit";
            mi.Click += new RoutedEventHandler(mi_Click);

            MainContextMenu.Items.Add(mi);

            HelixPoints = new PointsVisual3D { Color = Colors.Red, Size = 6 };
            HelixLines = new LinesVisual3D { Color = Colors.Blue, Thickness = 1 };

            watch_view.Children.Add(_lines);
            watch_view.Children.Add(_points);

            watch_view.Children.Add(new DefaultLights());

            System.Windows.Shapes.Rectangle backgroundRect = new System.Windows.Shapes.Rectangle();
            Canvas.SetZIndex(backgroundRect, -10);
            backgroundRect.IsHitTestVisible = false;
            BrushConverter bc = new BrushConverter();
            Brush strokeBrush = (Brush)bc.ConvertFrom("#313131");
            backgroundRect.Stroke = strokeBrush;
            backgroundRect.StrokeThickness = 1;
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 250, 216));
            backgroundRect.Fill = backgroundBrush;

            inputGrid.Children.Add(backgroundRect);

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        protected void mi_Click(object sender, RoutedEventArgs e)
        {
            watch_view.ZoomExtents();
        }

        private void MainContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
        }

        void view_MouseButtonIgnore(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        void view_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _rightMousePoint = e.GetPosition(topControl);
        }

        void view_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
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


        public void SetLatestDrawables(List<IDrawable> drawables)
        {
            _drawables = drawables;
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (!_requiresRedraw)
                return;

            lock (_points)
            {
                lock (_pointsCache)
                {
                    _points.Points = _pointsCache;
                }
            }

            //lock (_lines)
            //{
            //    lock (_linesCache)
            //    {
            //        _lines.Points = _linesCache;
            //    }
            //}

            //_points.Points = ViewModel._pointsCache;
            //_lines.Points = ViewModel._linesCache;

            //watch_view.Children.Clear();

            //PointsVisual3D pts = new PointsVisual3D { Color = Colors.Red, Size = 6 };
            //LinesVisual3D lines = new LinesVisual3D { Color = Colors.Blue, Thickness = 1 };

            //Point3DCollection foo = new Point3DCollection();
            //foo.Add(new Point3D(1, 2, 3));

            //_points.Points = _pointsCache;
            //lines.Points = _linesCache;

            //watch_view.Children.Add(pts);
            //watch_view.Children.Add(lines);
            
            _requiresRedraw = false;
        }

        public void Render() 
        {
            ////if (_isRendering)
            ////    return;

            ////if (!_requiresRedraw)
            ////    return;

            //_isRendering = true;

            //Points = null;
            //Lines = null;
            //_lines.Points = null;
            //_points.Points = null;

            //Points = new Point3DCollection();
            //Lines = new Point3DCollection();
            //Meshes = new List<Mesh3D>();

            //// a list of all the upstream IDrawable nodes
            //List<IDrawable> drawables = _drawables;

            //foreach (IDrawable d in drawables)
            //{
            //    RenderDescription rd = d.Draw/clear();

            //    foreach (Point3D p in rd.points)
            //    {
            //        Points.Add(p);
            //    }

            //    foreach (Point3D p in rd.lines)
            //    {
            //        Lines.Add(p);
            //    }

            //    foreach (Mesh3D mesh in rd.meshes)
            //    {
            //        Meshes.Add(mesh);
            //    }
            //}

            //_lines.Points = Lines;
            //_points.Points = Points;

            //// remove old meshes from the renderer
            //foreach (MeshVisual3D mesh in _meshes)
            //{
            //    watch_view.Children.Remove(mesh);
            //}

            //_meshes.Clear();

            //foreach (Mesh3D mesh in Meshes)
            //{
            //    MeshVisual3D vismesh = MakeMeshVisual3D(mesh);
            //    watch_view.Children.Add(vismesh);
            //    _meshes.Add(vismesh);
            //}

            //_requiresRedraw = false;
            //_isRendering = false;
        }

        //MeshVisual3D MakeMeshVisual3D(Mesh3D mesh)
        //{
        //    MeshVisual3D vismesh = new MeshVisual3D { 
        //        Content = new GeometryModel3D { Geometry = 
        //            mesh.ToMeshGeometry3D(), Material = Materials.White } };
        //    return vismesh;
        //}

        public Watch3DFullscreenViewModel ViewModel
        {
            get
            {
                if (this.DataContext is Watch3DFullscreenViewModel)
                    return (Watch3DFullscreenViewModel)this.DataContext;
                else
                    return null;
            }
        }

    }
}
