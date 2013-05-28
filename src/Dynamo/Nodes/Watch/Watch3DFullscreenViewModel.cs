using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Collections.ObjectModel;
using System.Windows.Threading;

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
            get{return _pointsCache;}
            set
            {
                _pointsCache = value;
                RaisePropertyChanged("HelixPoints");
            }
        }

        public List<Point3D> HelixLines
        {
            get { return _linesCache; }
            set
            {
                _linesCache = value;
                RaisePropertyChanged("HelixLines");
            }
        }

        public List<Point3D> HelixXAxes
        {
            get { return _xAxisCache; }
            set
            {
                _xAxisCache = value;
            }
        }

        public List<Point3D> HelixYAxes
        {
            get { return _yAxisCache; }
            set
            {
                _yAxisCache = value;
            }
        }

        public List<Point3D> HelixZAxes
        {
            get { return _zAxisCache; }
            set
            {
                _zAxisCache = value;
            }
        }

        public Mesh3D HelixMesh
        {
            get { return _meshCache; }
            set
            {
                _meshCache = value;
                RaisePropertyChanged("HelixMesh");
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

            if (!dynSettings.Controller.DynamoViewModel.FullscreenWatchShowing)
                return;

            List<IDrawable> drawables = new List<IDrawable>();

            foreach (dynNodeViewModel nodeViewModel in _parentWorkspace.Nodes)
            {
                dynNodeModel nodeModel = nodeViewModel.NodeLogic;

                IDrawable drawable = nodeModel as IDrawable;

                if (drawable != null)
                    drawables.Add(drawable);

                //if the node is function then get all the 
                //drawables inside that node. only do this if the
                //node's workspace is the home space to avoid infinite
                //recursion in the case of custom nodes in custom nodes
                if (nodeModel is dynFunction && nodeModel.WorkSpace == dynSettings.Controller.DynamoModel.HomeSpace)
                {
                    dynFunction func = (dynFunction)nodeModel;
                    foreach(dynNodeModel innerNode in func.Definition.Workspace.Nodes)
                    {
                        if (innerNode is IDrawable)
                        {
                            drawables.Add(innerNode as IDrawable);
                        }
                    }
                }
            }

            if (dynSettings.Controller.UIDispatcher != null)
            {
                dynSettings.Controller.UIDispatcher.Invoke(new Action(
                   delegate
                   {
                       RenderDrawables(drawables);
                   }
                ));
            }
        }

        private void RenderDrawables(List<IDrawable> drawables)
        {
            List<Point3D> points = new List<Point3D>();
            List<Point3D> lines = new List<Point3D>();
            List<Mesh3D> meshes = new List<Mesh3D>();
            List<Point3D> xAxes = new List<Point3D>();
            List<Point3D> yAxes = new List<Point3D>();
            List<Point3D> zAxes = new List<Point3D>();

            foreach (IDrawable d in drawables)
            {
                d.Draw();

                foreach (Point3D p in d.RenderDescription.points)
                {
                    points.Add(p);
                }

                foreach (Point3D p in d.RenderDescription.lines)
                {
                    lines.Add(p);
                }

                foreach (Mesh3D m in d.RenderDescription.meshes)
                {
                    meshes.Add(m);
                }

                foreach (Point3D p in d.RenderDescription.xAxisPoints)
                {
                    xAxes.Add(p);
                }

                foreach (Point3D p in d.RenderDescription.yAxisPoints)
                {
                    yAxes.Add(p);
                }

                foreach (Point3D p in d.RenderDescription.zAxisPoints)
                {
                    zAxes.Add(p);
                }
            }

            _pointsCache = points;
            _linesCache = lines;
            _meshCache = MergeMeshes(meshes);
            _xAxisCache = xAxes;
            _yAxisCache = yAxes;
            _zAxisCache = zAxes;

            RaisePropertyChanged("HelixPoints");
            RaisePropertyChanged("HelixLines");
            RaisePropertyChanged("HelixMesh");
            RaisePropertyChanged("HelixXAxes");
            RaisePropertyChanged("HelixYAxes");
            RaisePropertyChanged("HelixZAxes");
        }

        Mesh3D MergeMeshes(List<Mesh3D> meshes)
        {
            if (meshes.Count == 0)
                return null;

            Mesh3D fullMesh = new Mesh3D();

            List<Point3D> positions = new List<Point3D>();
            List<int> triangleIndices = new List<int>();

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

    }
}
