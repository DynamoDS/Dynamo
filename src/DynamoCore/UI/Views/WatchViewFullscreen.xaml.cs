using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using HelixToolkit.Wpf;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for WatchControl.xaml
    /// </summary>
    public partial class WatchViewFullscreen : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        Point _rightMousePoint;
        private Watch3DFullscreenViewModel _vm;

        protected List<MeshVisual3D> _meshes = new List<MeshVisual3D>();
        public List<Point3D> _pointsCache = new List<Point3D>();
        public List<Point3D> _linesCache = new List<Point3D>();
        public List<Point3D> _xAxisCache = new List<Point3D>();
        public List<Point3D> _yAxisCache = new List<Point3D>();
        public List<Point3D> _zAxisCache = new List<Point3D>();
        public Mesh3D _meshCache = new Mesh3D();

        public Material HelixMeshMaterial
        {
            get { return Materials.White; }
        }

        public List<Point3D> HelixPoints
        {
            get { return _pointsCache; }
            set
            {
                _pointsCache = value;
                NotifyPropertyChanged("HelixPoints");
            }
        }

        public List<Point3D> HelixLines
        {
            get { return _linesCache; }
            set
            {
                _linesCache = value;
                NotifyPropertyChanged("HelixLines");
            }
        }

        public List<Point3D> HelixXAxes
        {
            get { return _xAxisCache; }
            set
            {
                _xAxisCache = value;
                NotifyPropertyChanged("HelixXAxes");
            }
        }

        public List<Point3D> HelixYAxes
        {
            get { return _yAxisCache; }
            set
            {
                _yAxisCache = value;
                NotifyPropertyChanged("HelixYAxes");
            }
        }

        public List<Point3D> HelixZAxes
        {
            get { return _zAxisCache; }
            set
            {
                _zAxisCache = value;
                NotifyPropertyChanged("HelixZAxes");
            }
        }

        public Mesh3D HelixMesh
        {
            get { return _meshCache; }
            set
            {
                _meshCache = value;
                NotifyPropertyChanged("HelixMesh");
            }
        }

        public WatchViewFullscreen()
        {
            InitializeComponent();
            watch_view.DataContext = this;
            this.Loaded += new RoutedEventHandler(WatchViewFullscreen_Loaded);
        }

        void WatchViewFullscreen_Loaded(object sender, RoutedEventArgs e)
        {
            MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(view_MouseButtonIgnore);
            MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(view_MouseButtonIgnore);
            MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(view_MouseRightButtonUp);
            PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(view_PreviewMouseRightButtonDown);

            var mi = new MenuItem { Header = "Zoom to Fit" };
            mi.Click += new RoutedEventHandler(mi_Click);

            MainContextMenu.Items.Add(mi);

            dynSettings.Controller.RequestsRedraw += new System.EventHandler(Controller_RequestsRedraw);
            dynSettings.Controller.RunCompleted += new DynamoController.RunCompletedHandler(Controller_RunCompleted);
            _vm = DataContext as Watch3DFullscreenViewModel;
        }

        void Controller_RunCompleted(object controller, bool success)
        {
            if (!_vm.ParentWorkspace.IsCurrentSpace)
                return;

            if (!dynSettings.Controller.DynamoViewModel.FullscreenWatchShowing)
                return;

            Dispatcher.Invoke(new Action(RenderDrawables));
            //RenderDrawables();
        }

        void Controller_RequestsRedraw(object sender, System.EventArgs e)
        {
            Dispatcher.Invoke(new Action(RenderDrawables));
            //RenderDrawables();
        }

        private void RenderDrawables()
        {
            var points = new List<Point3D>();
            var lines = new List<Point3D>();
            var meshes = new List<Mesh3D>();
            var xAxes = new List<Point3D>();
            var yAxes = new List<Point3D>();
            var zAxes = new List<Point3D>();

            foreach (KeyValuePair<Guid, RenderDescription> kvp in dynSettings.Controller.RenderDescriptions)
            {
                var rd = kvp.Value as RenderDescription;

                if (rd == null)
                    continue;

                points.AddRange(rd.points.ConvertAll(x=>(Point3D)x));
                lines.AddRange(rd.lines.ConvertAll(x => (Point3D)x));
                meshes.AddRange(rd.meshes.ConvertAll(x=>(Mesh3D)x));
                xAxes.AddRange(rd.xAxisPoints.ConvertAll(x => (Point3D)x));
                yAxes.AddRange(rd.yAxisPoints.ConvertAll(x => (Point3D)x));
                zAxes.AddRange(rd.zAxisPoints.ConvertAll(x => (Point3D)x));
            }

            HelixPoints = points;
            HelixLines = lines;
            HelixMesh = MergeMeshes(meshes);
            HelixXAxes = xAxes;
            HelixYAxes = yAxes;
            HelixZAxes = zAxes;
        }

        Mesh3D MergeMeshes(List<Mesh3D> meshes)
        {
            if (meshes.Count == 0)
                return null;

            var positions = new List<Point3D>();
            var triangleIndices = new List<int>();

            int offset = 0;
            foreach (Mesh3D m in meshes)
            {
                positions.AddRange(m.Vertices);

                foreach (int[] face in m.Faces)
                {
                    triangleIndices.Add(face[0] + offset);
                    triangleIndices.Add(face[1] + offset);
                    triangleIndices.Add(face[2] + offset);
                }

                offset = positions.Count;
            }

            return new Mesh3D(positions, triangleIndices);
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
