using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Windows.Threading;
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
 
        public Material HelixMeshMaterial
        {
            get { return Materials.White; }
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

        public WatchViewFullscreen()
        {
            InitializeComponent();
            watch_view.DataContext = this;
            this.Loaded += WatchViewFullscreen_Loaded;
        }

        void WatchViewFullscreen_Loaded(object sender, RoutedEventArgs e)
        {
            MouseLeftButtonDown += view_MouseButtonIgnore;
            MouseLeftButtonUp += view_MouseButtonIgnore;
            MouseRightButtonUp += view_MouseRightButtonUp;
            PreviewMouseRightButtonDown += view_PreviewMouseRightButtonDown;

            var mi = new MenuItem { Header = "Zoom to Fit" };
            mi.Click += mi_Click;

            MainContextMenu.Items.Add(mi);

            //check this for null so the designer can load the preview
            if(dynSettings.Controller != null)
                dynSettings.Controller.VisualizationManager.VisualizationUpdateComplete += VisualizationManager_VisualizationUpdateComplete;

            _vm = DataContext as Watch3DFullscreenViewModel;
        }

        void VisualizationManager_VisualizationUpdateComplete(object sender, VisualizationEventArgs e)
        {
            if (dynSettings.Controller == null)
                return;

            if (!dynSettings.Controller.DynamoViewModel.FullscreenWatchShowing)
                return;

            Dispatcher.Invoke(new Action<RenderDescription>(RenderDrawables),DispatcherPriority.Render, new object[]{e.Description});
        }

        private void RenderDrawables(RenderDescription rd)
        {
            //Debug.WriteLine(string.Format("Rendering full screen Watch3D on thread {0}.", System.Threading.Thread.CurrentThread.ManagedThreadId));
            
            HelixPoints = null;
            HelixLines = null;
            HelixMesh = null;
            HelixXAxes = null;
            HelixYAxes = null;
            HelixZAxes = null;
            HelixPointsSelected = null;
            HelixLinesSelected = null;
            HelixMeshSelected = null;

            HelixPoints = rd.Points;
            HelixLines = rd.Lines;
            HelixPointsSelected = rd.SelectedPoints;
            HelixLinesSelected = rd.SelectedLines;
            HelixXAxes = rd.XAxisPoints;
            HelixYAxes = rd.YAxisPoints;
            HelixZAxes = rd.ZAxisPoints;
            HelixMesh = VisualizationManager.MergeMeshes(rd.Meshes);
            HelixMeshSelected = VisualizationManager.MergeMeshes(rd.SelectedMeshes);
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
