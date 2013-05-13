using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Collections.ObjectModel;

using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Selection;
using HelixToolkit.Wpf;

namespace Dynamo.Controls
{
    public class Watch3DFullscreenViewModel : dynViewModelBase
    {
        dynWorkspaceViewModel _parentWorkspace;
        protected List<MeshVisual3D> _meshes = new List<MeshVisual3D>();
        public ObservableCollection<Point3D> _pointsCache = new ObservableCollection<Point3D>();
        public ObservableCollection<Point3D> _linesCache = new ObservableCollection<Point3D>();

        public ObservableCollection<Point3D> HelixPoints
        {
            get{return _pointsCache;}
            set
            {
                _pointsCache = value;
                RaisePropertyChanged("HelixPoints");
            }
        }

        public ObservableCollection<Point3D> HelixLines
        {
            get { return _linesCache; }
            set
            {
                _linesCache = value;
                RaisePropertyChanged("HelixLines");
            }
        }

        public Watch3DFullscreenViewModel(dynWorkspaceViewModel parentWorkspace)
        {
            _parentWorkspace = parentWorkspace;
            dynSettings.Controller.RunCompleted += new DynamoController.RunCompletedHandler(Watch3DFullscreenViewModel_RunCompleted);
        }

        public void Watch3DFullscreenViewModel_RunCompleted(object controller, bool success)
        {
            if (!_parentWorkspace.IsCurrentSpace)
                return;

            List<IDrawable> drawables = new List<IDrawable>();

            foreach (dynNodeViewModel nodeViewModel in _parentWorkspace.Nodes)
            {
                dynNodeModel nodeModel = nodeViewModel.NodeLogic;

                IDrawable drawable = nodeModel as IDrawable;

                if (drawable == null)
                    continue;

                drawables.Add(drawable);
            }

            RenderDrawables(drawables);
        }

        private void RenderDrawables(List<IDrawable> drawables)
        {

            Point3DCollection points = new Point3DCollection();
            Point3DCollection lines = new Point3DCollection();
            //Meshes = new List<Mesh3D>();

            foreach (IDrawable d in drawables)
            {
                RenderDescription rd = d.Draw();

                foreach (Point3D p in rd.points)
                {
                    _pointsCache.Add(p);
                }

                foreach (Point3D p in rd.lines)
                {
                    _linesCache.Add(p);
                }

                //foreach (Mesh3D mesh in rd.meshes)
                //{
                //    Meshes.Add(mesh);
                //}
            }

            RaisePropertyChanged("HelixPoints");
            RaisePropertyChanged("HelixLines");

            //// remove old meshes from the renderer
            //foreach (MeshVisual3D mesh in _meshes)
            //{
            //    _watchView.watch_view.Children.Remove(mesh);
            //}

            //_meshes.Clear();

            //foreach (Mesh3D mesh in Meshes)
            //{
            //    MeshVisual3D vismesh = MakeMeshVisual3D(mesh);
            //    _watchView.watch_view.Children.Add(vismesh);
            //    _meshes.Add(vismesh);
            //}
        }
    }
}
