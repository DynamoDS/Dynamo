using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Dynamo.Nodes;
using Dynamo.Utilities;
using HelixToolkit.Wpf;

namespace Dynamo.Controls
{
    public class Watch3DFullscreenViewModel : dynViewModelBase
    {
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

        public dynWorkspaceViewModel ParentWorkspace { get; set; }

        public Watch3DFullscreenViewModel(dynWorkspaceViewModel parentWorkspace)
        {
            ParentWorkspace = parentWorkspace;
            //dynSettings.Controller.RunCompleted += new DynamoController.RunCompletedHandler(Watch3DFullscreenViewModel_RunCompleted);
            //dynSettings.Controller.RequestsRedraw += Controller_RequestsRedraw;
        }

        //void Controller_RequestsRedraw(object sender, EventArgs e)
        //{
        //    RenderDrawables();
        //}

        //public void Watch3DFullscreenViewModel_RunCompleted(object controller, bool success)
        //{
        //    if (!_parentWorkspace.IsCurrentSpace)
        //        return;

        //    if (!dynSettings.Controller.DynamoViewModel.FullscreenWatchShowing)
        //        return;


        //    if (dynSettings.Controller.UIDispatcher != null)
        //    {
        //        dynSettings.Controller.UIDispatcher.Invoke(new Action(
        //           RenderDrawables
        //        ));
        //    }
        //}

    }
}
