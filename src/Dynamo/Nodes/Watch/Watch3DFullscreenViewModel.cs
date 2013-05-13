using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Selection;

namespace Dynamo.Controls
{
    public class Watch3DFullscreenViewModel : dynViewModelBase
    {
        dynWorkspaceViewModel _parentWorkspace;
        WatchViewFullscreen _fullscreenView = null;

        public Point3DCollection _pointsCache = new Point3DCollection();
        public Point3DCollection _linesCache = new Point3DCollection();

        public WatchViewFullscreen FullscreenView
        {
            get
            {
                return _fullscreenView;
            }
            set
            {
                _fullscreenView = value;
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

            if (_fullscreenView == null)
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
                    points.Add(p);
                }

                foreach (Point3D p in rd.lines)
                {
                    lines.Add(p);
                }

                //foreach (Mesh3D mesh in rd.meshes)
                //{
                //    Meshes.Add(mesh);
                //}
            }

            _fullscreenView.SetVisiblePoints(points);
            _fullscreenView.SetVisibleLines(lines);

            //_pointsCache = points;
            //_linesCache = lines;

            //_fullscreenView.HelixView().Children.Clear();


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
