using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using HelixToolkit.Wpf;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for WatchControl.xaml
    /// </summary>
    public partial class WatchView : IViewModelView<NodeViewModel>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        System.Windows.Point _rightMousePoint;

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

        public System.Windows.Media.Media3D.Material HelixMeshMaterial
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

        public WatchView()
        {
            InitializeComponent();

            //set the data context for the watch control
            //to bind to properties on this class
            watch_view.DataContext = this;

            MouseRightButtonUp += view_MouseRightButtonUp;
            PreviewMouseRightButtonDown += view_PreviewMouseRightButtonDown;

            this.Loaded += WatchView_Loaded;
        }

        void WatchView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            dynSettings.Controller.VisualizationManager.VisualizationUpdateComplete += VisualizationManager_VisualizationUpdateComplete;

            var node = DataContext as Watch3D;
            node.WatchResultsReadyToVisualize += RenderDrawables;
        }

        void VisualizationManager_VisualizationUpdateComplete(object sender, VisualizationEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                var node = DataContext as Watch3D;
                if (node != null)
                {
                    node.GetBranchVisualizationCommand.Execute(null);
                }
            }));
        }

        private void RenderDrawables(object sender, EventArgs e)
        {
            var renderArgs = e as VisualizationEventArgs;
            var rd = renderArgs.Description;

            //aggregate all the render descriptions into one for this node.
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
            HelixMesh = VisualizationManager.MergeMeshes(rd.Meshes);
            HelixXAxes = rd.XAxisPoints;
            HelixYAxes = rd.YAxisPoints;
            HelixZAxes = rd.ZAxisPoints;
            HelixPointsSelected = rd.SelectedPoints;
            HelixLinesSelected = rd.SelectedLines;
            HelixMeshSelected = VisualizationManager.MergeMeshes(rd.SelectedMeshes);
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

        public NodeViewModel ViewModel
        {
            get
            {
                if (this.DataContext is NodeViewModel)
                    return (NodeViewModel)this.DataContext;
                else
                    return null;
            }
        }
    }
}
