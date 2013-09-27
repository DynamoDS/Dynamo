using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
        
        protected List<MeshVisual3D> _meshes = new List<MeshVisual3D>();
        public List<Point3D> _pointsCache = new List<Point3D>();
        public List<Point3D> _linesCache = new List<Point3D>();
        public List<Point3D> _xAxisCache = new List<Point3D>();
        public List<Point3D> _yAxisCache = new List<Point3D>();
        public List<Point3D> _zAxisCache = new List<Point3D>();
        public MeshGeometry3D _meshCache = new MeshGeometry3D();

        public System.Windows.Media.Media3D.Material HelixMeshMaterial
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

        public MeshGeometry3D HelixMesh
        {
            get { return _meshCache; }
            set
            {
                _meshCache = value;
                NotifyPropertyChanged("HelixMesh");
            }
        }
        
        public WatchView()
        {
            InitializeComponent();

            //set the data context for the watch control
            //to bind to properties on this class
            watch_view.DataContext = this;

            MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(view_MouseRightButtonUp);
            PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(view_PreviewMouseRightButtonDown);

            this.Loaded += new System.Windows.RoutedEventHandler(WatchView_Loaded);
        }

        void WatchView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            dynSettings.Controller.VisualizationManager.VisualizationUpdateComplete += new EventHandler(VisualizationManager_VisualizationUpdateComplete);
        }

        void VisualizationManager_VisualizationUpdateComplete(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(RenderDrawables));
        }

        private void RenderDrawables()
        {
            Debug.WriteLine(string.Format("Rendering Watch3D on thread {0}.", System.Threading.Thread.CurrentThread.ManagedThreadId));

            //when the visualization update is complete, rebind geometry
            //in this watch to collections of geometry composed from upstream
            //geometry

            var rd = dynSettings.Controller.VisualizationManager.RenderUpstream(this.DataContext as NodeModel);

            //aggregate all the render descriptions into one for this node.
            HelixPoints = null;
            HelixLines = null;
            HelixMesh = null;
            HelixXAxes = null;
            HelixYAxes = null;
            HelixZAxes = null;

            HelixPoints = rd.Points;
            HelixLines = rd.Lines;
            HelixMesh = VisualizationManager.MergeMeshes(rd.Meshes);
            HelixXAxes = rd.XAxisPoints;
            HelixYAxes = rd.YAxisPoints;
            HelixZAxes = rd.ZAxisPoints;

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
