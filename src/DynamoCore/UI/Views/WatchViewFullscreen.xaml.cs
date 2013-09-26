using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Linq;
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

            //dynSettings.Controller.RequestsRedraw += new System.EventHandler(Controller_RequestsRedraw);
            //dynSettings.Controller.RunCompleted += new DynamoController.RunCompletedHandler(Controller_RunCompleted);

            dynSettings.Controller.VisualizationManager.VisualizationUpdateComplete += new EventHandler(VisualizationManager_VisualizationUpdateComplete);

            _vm = DataContext as Watch3DFullscreenViewModel;
        }

        void VisualizationManager_VisualizationUpdateComplete(object sender, EventArgs e)
        {
            if (!_vm.ParentWorkspace.IsCurrentSpace)
                return;

            if (!dynSettings.Controller.DynamoViewModel.FullscreenWatchShowing)
                return;

            Dispatcher.Invoke(new Action(RenderDrawables));
        }

        //void Controller_RunCompleted(object controller, bool success)
        //{
        //    if (!_vm.ParentWorkspace.IsCurrentSpace)
        //        return;

        //    if (!dynSettings.Controller.DynamoViewModel.FullscreenWatchShowing)
        //        return;

        //    Dispatcher.Invoke(new Action(RenderDrawables));
        //}

        //void Controller_RequestsRedraw(object sender, System.EventArgs e)
        //{
        //    Dispatcher.Invoke(new Action(RenderDrawables));
        //}

        private void RenderDrawables()
        {
            var vizManager = dynSettings.Controller.VisualizationManager;

            HelixPoints.Clear();
            HelixLines.Clear();
            HelixMesh = null;
            HelixXAxes.Clear();
            HelixYAxes.Clear();

            HelixZAxes.Clear();

            var pts = vizManager.Visualizations.Values.SelectMany(x => x.Description.Points).ToList();
            var lines = vizManager.Visualizations.Values.SelectMany(x => x.Description.Lines).ToList();
            var xs = vizManager.Visualizations.Values.SelectMany(x => x.Description.XAxisPoints).ToList();
            var ys = vizManager.Visualizations.Values.SelectMany(x => x.Description.YAxisPoints).ToList();
            var zs = vizManager.Visualizations.Values.SelectMany(x => x.Description.ZAxisPoints).ToList();

            HelixPoints.AddRange(pts);
            HelixLines.AddRange(lines);
            var meshes = vizManager.Visualizations.Values.SelectMany(x => x.Description.Meshes).ToList();
            HelixMesh = VisualizationManager.MergeMeshes(meshes);
            HelixXAxes.AddRange(xs);
            HelixYAxes.AddRange(ys);
            HelixZAxes.AddRange(zs);
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
