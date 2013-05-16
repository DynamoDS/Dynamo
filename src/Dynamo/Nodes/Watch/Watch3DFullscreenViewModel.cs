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
        public List<Point3D> _pointsCache = new List<Point3D>();
        public List<Point3D> _linesCache = new List<Point3D>();

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

                if (drawable == null)
                    continue;

                drawables.Add(drawable);
            }

            RenderDrawables(drawables);
        }

        private void RenderDrawables(List<IDrawable> drawables)
        {
            List<Point3D> points = new List<Point3D>();
            List<Point3D> lines = new List<Point3D>();
            List<Mesh3D> meshes = new List<Mesh3D>();

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

                foreach (Mesh3D m in rd.meshes)
                {
                    meshes.Add(m);
                }
            }

            _pointsCache = points;
            _linesCache = lines;

            _meshCache = MergeMeshes(meshes);

            RaisePropertyChanged("HelixPoints");
            RaisePropertyChanged("HelixLines");
            RaisePropertyChanged("HelixMesh");
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
