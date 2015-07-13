using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.ViewModels;
using Dynamo.Wpf.Rendering;
using DynamoUtilities;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
using Model3D = HelixToolkit.Wpf.SharpDX.Model3D;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;
using Point = System.Windows.Point;
using Quaternion = SharpDX.Quaternion;
using TextInfo = HelixToolkit.Wpf.SharpDX.TextInfo;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for WatchControl.xaml
    /// </summary>
    public partial class Watch3DView : UserControl, INotifyPropertyChanged
    {
        #region events

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region private members

        private readonly Guid _id=Guid.Empty;
        private Point rightMousePoint;
        private LineGeometry3D worldGrid;
        private LineGeometry3D worldAxes;
        private RenderTechnique renderTechnique;
        private PerspectiveCamera camera;
        private Color4 selectionColor;
        private Color4 materialColor;
        private Vector3 directionalLightDirection;
        private Color4 directionalLightColor;
        private DirectionalLight3D directionalLight;
        private Color4 defaultLineColor;
        private Color4 defaultPointColor;
        private double lightAzimuthDegrees = 45.0;
        private double lightElevationDegrees = 35.0;
        private int renderingTier;
        private DynamoViewModel viewModel;
        private double nearPlaneDistanceFactor = 0.01;
        internal readonly Vector3D defaultCameraLookDirection = new Vector3D(-10, -10, -10);
        internal readonly Point3D defaultCameraPosition = new Point3D(10, 15, 10);
        internal readonly Vector3D defaultCameraUpDirection = new Vector3D(0, 1, 0); 
        private readonly Size defaultPointSize = new Size(8,8);

        private Dictionary<string, Model3D> model3DDictionary = new Dictionary<string, Model3D>();
        private Dictionary<string, Model3D> Model3DDictionary
        {
            get
            {
                return model3DDictionary;
            }

            set
            {
                model3DDictionary = value;
            }
        }

#if DEBUG
        private Stopwatch renderTimer = new Stopwatch();
#endif

        #endregion

        /// <summary>
        /// The LeftClickCommand is set according to the
        /// ViewModel's IsPanning or IsOrbiting properties.
        /// When those properties are changed, this command is
        /// set to ViewportCommand.Pan or ViewportCommand.Rotate depending. 
        /// If neither panning or rotating is set, this property is set to null 
        /// and left clicking should have no effect.
        /// </summary>
        public RoutedCommand LeftClickCommand
        {
            get
            {
                var vm = DataContext as DynamoViewModel;
                if (vm == null) return null;

                if (vm.IsPanning) return ViewportCommands.Pan;
                if (vm.IsOrbiting) return ViewportCommands.Rotate;

                return null;
            }
        }

        #region public properties

        public LineGeometry3D Grid
        {
            get { return worldGrid; }
            set
            {
                worldGrid = value;
                NotifyPropertyChanged("Grid");
            }
        }

        public LineGeometry3D Axes
        {
            get { return worldAxes; }
            set
            {
                worldAxes = value;
                NotifyPropertyChanged("Axes");
            }
        }

        public BillboardText3D Text { get; set; }

        public Viewport3DX View
        {
            get { return watch_view; }
        }

        public PhongMaterial WhiteMaterial { get; set; }

        public PhongMaterial SelectedMaterial { get; set; }

        public Transform3D Model1Transform { get; private set; }
        
        public RenderTechnique RenderTechnique
        {
            get
            {
                return this.renderTechnique;
            }
            set
            {
                renderTechnique = value;
                NotifyPropertyChanged("RenderTechnique");
            }
        }

        public PerspectiveCamera Camera
        {
            get
            {
                return this.camera;
            }

            set
            {
                camera = value;
                NotifyPropertyChanged("Camera");
            }
        }

        public double LightAzimuthDegrees
        {
            get { return lightAzimuthDegrees; }
            set { lightAzimuthDegrees = value; }
        }

        public double LightElevationDegrees
        {
            get { return lightElevationDegrees; }
            set { lightElevationDegrees = value; }
        }
      
        public List<Model3D> Model3DValues
        {
            get
            {
                return Model3DDictionary == null ? new List<Model3D>() :
                   Model3DDictionary.Select(x => x.Value).ToList();
            }
        }

        public double NearPlaneDistanceFactor
        {
            get { return nearPlaneDistanceFactor; }
            set
            {
                nearPlaneDistanceFactor = value;
                NotifyPropertyChanged("");
            }
        }

        #endregion

        #region constructors

        public Watch3DView()
        {
            SetupScene();

            InitializeComponent();
            watch_view.DataContext = this;
            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;
            InitializeHelix();
        }

        public Watch3DView(Guid id)
        {
            SetupScene();

            InitializeComponent();
            watch_view.DataContext = this;
            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;

            _id = id;
            
            InitializeHelix();
        }

        public Watch3DView(Guid id, IWatchViewModel dataContext)
        {
            DataContext = dataContext;          
            SetupScene();

            InitializeComponent();
            watch_view.DataContext = this;
            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;

            _id = id;
             
            InitializeHelix();             
        }

        private void SetupScene()
        {
            var ptColor = (Color)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PointColor"];
            defaultPointColor = new Color4(ptColor.R/255.0f, ptColor.G/255.0f, ptColor.B/255.0f, ptColor.A/255.0f);

            var lineColor = (Color)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["EdgeColor"];
            defaultLineColor = new Color4(lineColor.R/255.0f, lineColor.G/255.0f, lineColor.B/255.0f, lineColor.A/255.0f);

            directionalLightColor = new Color4(0.9f, 0.9f, 0.9f, 1.0f);
            directionalLightDirection = new Vector3(-0.5f, -1.0f, 0.0f);

            var matColor = (Color)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["MaterialColor"];
            materialColor = new Color4(matColor.R/255.0f, matColor.G/255.0f, matColor.B/255.0f, matColor.A/255.0f);
            
            RenderTechnique = Techniques.RenderDynamo;

            WhiteMaterial = new PhongMaterial
            {
                Name = "White",
                AmbientColor = PhongMaterials.ToColor(0.1, 0.1, 0.1, 1.0),
                DiffuseColor = materialColor,
                SpecularColor = PhongMaterials.ToColor(0.0225, 0.0225, 0.0225, 1.0),
                EmissiveColor = PhongMaterials.ToColor(0.0, 0.0, 0.0, 1.0),
                SpecularShininess = 12.8f,
            };

            var selColor = (Color)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["SelectionColor"];
            selectionColor = new Color4(selColor.R/255.0f, selColor.G/255.0f, selColor.B/255.0f, selColor.A/255.0f);
            SelectedMaterial = new PhongMaterial
            {
                Name = "White",
                AmbientColor = PhongMaterials.ToColor(0.1, 0.1, 0.1, 1.0),
                DiffuseColor = selectionColor,
                SpecularColor = PhongMaterials.ToColor(0.0225, 0.0225, 0.0225, 1.0),
                EmissiveColor = PhongMaterials.ToColor(0.0, 0.0, 0.0, 1.0),
                SpecularShininess = 12.8f,
            };

            Model1Transform = new TranslateTransform3D(0, -0, 0);
            
            // camera setup
            Camera = new PerspectiveCamera();

            SetCameraToDefaultOrientation();

            DrawGrid();
        }

        private void SetCameraToDefaultOrientation()
        {
            Camera.LookDirection = defaultCameraLookDirection;
            Camera.Position = defaultCameraPosition;
            Camera.UpDirection = defaultCameraUpDirection;
            Camera.NearPlaneDistance = CalculateNearClipPlane(1000000);
            Camera.FarPlaneDistance = 10000000;
        }

        /// <summary>
        /// Initialize the Helix with these values. These values should be attached before the 
        /// visualization starts. Deleting them and attaching them does not make any effect on helix.         
        /// So they are initialized before the process starts.
        /// </summary>
        private void InitializeHelix()
        {
            directionalLight = new DirectionalLight3D
            {
                Color = directionalLightColor,
                Direction = directionalLightDirection
            };

            if (model3DDictionary != null && !model3DDictionary.ContainsKey("DirectionalLight"))
            {
                model3DDictionary.Add("DirectionalLight", directionalLight);
            }

            LineGeometryModel3D gridModel3D = new LineGeometryModel3D
            {
                Geometry = Grid,
                Transform = Model1Transform,
                Color = SharpDX.Color.White,
                Thickness = 0.3,
                IsHitTestVisible = false
            };

            if (model3DDictionary != null && !model3DDictionary.ContainsKey("Grid"))
            {
                model3DDictionary.Add("Grid", gridModel3D);
            }

            LineGeometryModel3D axesModel3D = new LineGeometryModel3D
            {
                Geometry = Axes,
                Transform = Model1Transform,
                Color = SharpDX.Color.White,
                Thickness = 0.3,
                IsHitTestVisible = false
            };

            if (model3DDictionary != null && !model3DDictionary.ContainsKey("Axes"))
            {
                model3DDictionary.Add("Axes", axesModel3D);
            }
        }

        private static MeshGeometry3D DrawTestMesh()
        {
            var b1 = new MeshBuilder();
            for (var x = 0; x < 4; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    for (var z = 0; z < 4; z++)
                    {
                        b1.AddBox(new Vector3(x, y, z), 0.5, 0.5, 0.5, BoxFaces.All);
                    }
                }
            }
            var mesh = b1.ToMeshGeometry3D();
            
            mesh.Colors = new Color4Collection();
            foreach (var v in mesh.Positions)
            {
                mesh.Colors.Add(new Color4(1f,0f,0f,1f));
            }

            return mesh;
        }
        
        #endregion

        #region event handlers

        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            UnregisterEventHandlers();
        }

        private void UnregisterEventHandlers()
        {
            UnregisterButtonHandlers();

            if (viewModel == null) return;

            UnregisterVisualizationManagerEventHandlers();

            viewModel.PropertyChanged -= ViewModel_PropertyChanged;

            CompositionTarget.Rendering -= CompositionTarget_Rendering;

            UnregisterModelEventHandlers(viewModel.Model);

            UnregisterWorkspaceEventHandlers(viewModel.Model);
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            viewModel = DataContext as DynamoViewModel;

            CompositionTarget.Rendering += CompositionTarget_Rendering;

            RegisterButtonHandlers();

            //check this for null so the designer can load the preview
            if (viewModel == null) return;

            RegisterVisualizationManagerEventHandlers();

            LogVisualizationCapabilities();

            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            RegisterModelEventhandlers(viewModel.Model);

            RegisterWorkspaceEventHandlers(viewModel.Model);  
        }

        private void RegisterButtonHandlers()
        {
            MouseLeftButtonDown += view_MouseButtonIgnore;
            MouseLeftButtonUp += view_MouseButtonIgnore;
            MouseRightButtonUp += view_MouseRightButtonUp;
            PreviewMouseRightButtonDown += view_PreviewMouseRightButtonDown;
        }

        private void UnregisterButtonHandlers()
        {
            MouseLeftButtonDown -= view_MouseButtonIgnore;
            MouseLeftButtonUp -= view_MouseButtonIgnore;
            MouseRightButtonUp -= view_MouseRightButtonUp;
            PreviewMouseRightButtonDown -= view_PreviewMouseRightButtonDown;
        }

        private void LogVisualizationCapabilities()
        {
            renderingTier = (RenderCapability.Tier >> 16);
            var pixelShader3Supported = RenderCapability.IsPixelShaderVersionSupported(3, 0);
            var pixelShader4Supported = RenderCapability.IsPixelShaderVersionSupported(4, 0);
            var softwareEffectSupported = RenderCapability.IsShaderEffectSoftwareRenderingSupported;
            var maxTextureSize = RenderCapability.MaxHardwareTextureSize;

            viewModel.Model.Logger.Log(string.Format("RENDER : Rendering Tier: {0}", renderingTier), LogLevel.File);
            viewModel.Model.Logger.Log(string.Format("RENDER : Pixel Shader 3 Supported: {0}", pixelShader3Supported),
                LogLevel.File);
            viewModel.Model.Logger.Log(string.Format("RENDER : Pixel Shader 4 Supported: {0}", pixelShader4Supported),
                LogLevel.File);
            viewModel.Model.Logger.Log(
                string.Format("RENDER : Software Effect Rendering Supported: {0}", softwareEffectSupported), LogLevel.File);
            viewModel.Model.Logger.Log(string.Format("RENDER : Maximum hardware texture size: {0}", maxTextureSize),
                LogLevel.File);
        }

        private void RegisterVisualizationManagerEventHandlers()
        {
            viewModel.VisualizationManager.RenderComplete += VisualizationManagerRenderComplete;
            viewModel.VisualizationManager.ResultsReadyToVisualize += VisualizationManager_ResultsReadyToVisualize;
            viewModel.VisualizationManager.SelectionHandled += VisualizationManager_SelectionHandled;
            viewModel.VisualizationManager.DeletionHandled += VisualizationManager_DeletionHandled;
            viewModel.VisualizationManager.WorkspaceOpenedClosedHandled += VisualizationManager_WorkspaceOpenedClosedHandled;
        }

        private void UnregisterVisualizationManagerEventHandlers()
        {
            viewModel.VisualizationManager.RenderComplete -= VisualizationManagerRenderComplete;
            viewModel.VisualizationManager.ResultsReadyToVisualize -= VisualizationManager_ResultsReadyToVisualize;
            viewModel.VisualizationManager.SelectionHandled -= VisualizationManager_SelectionHandled;
            viewModel.VisualizationManager.DeletionHandled -= VisualizationManager_DeletionHandled;
            viewModel.VisualizationManager.WorkspaceOpenedClosedHandled -= VisualizationManager_WorkspaceOpenedClosedHandled;
        }

        private void RegisterModelEventhandlers(DynamoModel model)
        {
            model.WorkspaceCleared += Model_WorkspaceCleared;
            model.ShutdownStarted += Model_ShutdownStarted;
        }

        private void UnregisterModelEventHandlers(DynamoModel model)
        {
            model.WorkspaceCleared -= Model_WorkspaceCleared;
            model.ShutdownStarted -= Model_ShutdownStarted;
        }

        private void UnregisterWorkspaceEventHandlers(DynamoModel model)
        {
            model.WorkspaceAdded -= Model_WorkspaceAdded;
            model.WorkspaceRemoved -= Model_WorkspaceRemoved;
            model.WorkspaceOpening -= Model_WorkspaceOpening;

            foreach (var ws in model.Workspaces)
            {
                ws.Saving -= workspace_Saving;
            }
        }

        private void RegisterWorkspaceEventHandlers(DynamoModel model)
        {
            model.WorkspaceAdded += Model_WorkspaceAdded;
            model.WorkspaceRemoved += Model_WorkspaceRemoved;
            model.WorkspaceOpening += Model_WorkspaceOpening;

            foreach (var ws in model.Workspaces)
            {
                ws.Saving += workspace_Saving;
            }
        }

        void Model_WorkspaceCleared(object sender, EventArgs e)
        {
            SetCameraToDefaultOrientation();
        }

        void Model_WorkspaceOpening(XmlDocument doc)
        {
            var vm = DataContext as DynamoViewModel;
            if (vm == null)
            {
                return;
            }

            var camerasElements = doc.GetElementsByTagName("Cameras");
            if (camerasElements.Count == 0)
            {
                return;
            }

            foreach (XmlNode cameraNode in camerasElements[0].ChildNodes)
            {
                LoadCamera(cameraNode);
            }
        }

        /// <summary>
        /// Initialize the Geometry everytime a workspace is opened or closed. 
        /// Always, keep these DirectionalLight,Grid,Axes. These values are rendered
        /// only once by helix, attaching them again will make no effect on helix 
        /// </summary> 
        private void VisualizationManager_WorkspaceOpenedClosedHandled()
        {
            List<string> keysList = new List<string> { "DirectionalLight", "Grid", "Axes","BillBoardText"};
            if (Text != null && Text.TextInfo.Any())
            {
                Text.TextInfo.Clear();               
            }
            foreach (var key in Model3DDictionary.Keys.Except(keysList).ToList())
            {
                var model = Model3DDictionary[key] as GeometryModel3D;
                model.Detach();
                Model3DDictionary.Remove(key);
            }

            NotifyPropertyChanged("");
            View.InvalidateRender();
        }

        /// <summary>
        /// When a model is selected,then select only that Geometry
        /// If any of the model is in selected mode, then unselect that model
        /// </summary>
        /// <param name="items">The items.</param>
        private void VisualizationManager_SelectionHandled(IEnumerable items)
        {
            if (items == null)
            {
                var geom3dList =
                    watch_view.Items.Cast<object>().Where(x => x is GeometryModel3D).Cast<GeometryModel3D>().ToList();

                var list = geom3dList.Select(y =>
                {
                    y.IsSelected = false;
                    return y;
                }).ToList();
               
            }
            else
            {
                foreach (var item in items)
                {
                    var node = item as NodeModel;
                    if (node == null)
                    {
                        continue;
                    }

                    var geometryModels = FindGeometryModel3DsForNode(node);

                    if (!geometryModels.Any())
                    {
                        continue;
                    }

                    var modelValues = geometryModels.Select(x => x.Value);
                    var selectedGeom = modelValues.Cast<GeometryModel3D>().Select(z =>
                    {
                        z.IsSelected = !z.IsSelected;
                        return z;
                    }).ToList();                    
                }
            }            
        }

        /// <summary>
        /// when a node is deleted, then update the Geometry 
        /// and notify helix        
        /// </summary>
        /// <param name="node">The node.</param>
        private void VisualizationManager_DeletionHandled(NodeModel node)
        {
            var geometryModels = FindGeometryModel3DsForNode(node);

            if (!geometryModels.Any())
            {
                return;
            }
            
            foreach (var kvp in geometryModels)
            {
                var model = Model3DDictionary[kvp.Key] as GeometryModel3D;
                if (model != null)
                {
                    model.Detach();
                    model.MouseDown3D -= meshGeometry3D_MouseDown3D;
                }
                Model3DDictionary.Remove(kvp.Key);                
            }

            NotifyPropertyChanged("");
        }

        void Model_WorkspaceAdded(Models.WorkspaceModel workspace)
        {
            workspace.Saving += workspace_Saving;
        }

        void Model_WorkspaceRemoved(Models.WorkspaceModel workspace)
        {
            workspace.Saving -= workspace_Saving;
        }

        void workspace_Saving(XmlDocument doc)
        {
            var root = doc.DocumentElement;
            if (root == null)
            {
                return;
            }

            var camerasElement = doc.CreateElement("Cameras");
            SaveCamera(camerasElement);
            root.AppendChild(camerasElement);  
        }

        /// <summary>
        /// A utility method for finding all the geometry model objects in the geometry
        /// dictionary which correspond to a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private KeyValuePair<string, Model3D>[] FindGeometryModel3DsForNode(NodeModel node)
        {
            var geometryModels =
                Model3DDictionary
                    .Where(x => x.Key.Contains(node.AstIdentifierBase))
                    .Where(x => x.Value is GeometryModel3D)
                    .Select(x => x).ToArray();

            return geometryModels;   
        }

        void Model_ShutdownStarted(Models.DynamoModel model)
        {
            UnregisterEventHandlers();
        }

        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsPanning":
                case "IsOrbiting":
                    NotifyPropertyChanged("LeftClickCommand");
                    break;
            }
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
#if DEBUG
            if (renderTimer.IsRunning)
            {
                renderTimer.Stop();
                Debug.WriteLine(string.Format("RENDER: {0} ellapsed for setting properties and rendering.", renderTimer.Elapsed));
                renderTimer.Reset();
            }
#endif
            if (directionalLight == null)
            {
                return;
            }

            var cf = new Vector3((float)camera.LookDirection.X, (float)camera.LookDirection.Y, (float)camera.LookDirection.Z).Normalized();
            var cu = new Vector3((float)camera.UpDirection.X, (float)camera.UpDirection.Y, (float)camera.UpDirection.Z).Normalized();
            var right = Vector3.Cross(cf, cu);

            var qel = Quaternion.RotationAxis(right, (float)((-LightElevationDegrees * Math.PI) / 180));
            var qaz = Quaternion.RotationAxis(cu, (float)((LightAzimuthDegrees * Math.PI) / 180));
            var v = Vector3.Transform(cf, qaz*qel);
            directionalLightDirection = v;

            if ( !directionalLight.Direction.Equals(directionalLightDirection))
            {
                directionalLight.Direction = v;
            }

            UpdateNearClipPlaneForSceneBounds();
        }

        /// <summary>
        /// This method attempts to maximize the near clip plane in order to 
        /// achiever higher z-buffer precision.
        /// </summary>
        private void UpdateNearClipPlaneForSceneBounds()
        {
            // http: //www.sjbaker.org/steve/omniv/love_your_z_buffer.html
            var sceneBounds = watch_view.FindBounds();
            var maxDim = Math.Max(Math.Max(sceneBounds.SizeX, sceneBounds.Y), sceneBounds.SizeZ);
            Camera.NearPlaneDistance = Math.Max(CalculateNearClipPlane(maxDim), 0.1);
        }

        private double CalculateNearClipPlane(double maxDim)
        {
            return maxDim * NearPlaneDistanceFactor;
        }

        /// <summary>
        /// Handler for the visualization manager's ResultsReadyToVisualize event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VisualizationManager_ResultsReadyToVisualize(VisualizationEventArgs args)
        {
            if (CheckAccess())
                RenderDrawables(args);
            else
            {
                // Scheduler invokes ResultsReadyToVisualize on background thread.
                Dispatcher.BeginInvoke(new Action(() => RenderDrawables(args)));
            }
        }

        /// <summary>
        /// When visualization is complete, the view requests it's visuals. For Full
        /// screen watch, this will be all renderables. For a Watch 3D node, this will
        /// be the subset of the renderables associated with the node.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VisualizationManagerRenderComplete()
        {
            var executeCommand = new Action(delegate
            {
                var vm = (IWatchViewModel)DataContext;
                if (vm.GetBranchVisualizationCommand.CanExecute(null))
                    vm.GetBranchVisualizationCommand.Execute(null);
            });

            if (CheckAccess())
                executeCommand();
            else
                Dispatcher.BeginInvoke(executeCommand);
        }

        /// <summary>
        /// Callback for thumb control's DragStarted event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResizeThumb_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Callbcak for thumb control's DragCompleted event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResizeThumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Callback for thumb control's DragDelta event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResizeThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

            //Debug.WriteLine("d_x:" + e.HorizontalChange + "," + "d_y:" + e.VerticalChange);
            //Debug.WriteLine("Size:" + _nodeUI.Width + "," + _nodeUI.Height);
            //Debug.WriteLine("ActualSize:" + _nodeUI.ActualWidth + "," + _nodeUI.ActualHeight);
            //Debug.WriteLine("Grid size:" + _nodeUI.ActualWidth + "," + _nodeUI.ActualHeight);

            if (xAdjust >= inputGrid.MinWidth)
            {
                Width = xAdjust;
            }

            if (yAdjust >= inputGrid.MinHeight)
            {
                Height = yAdjust;
            }
        }

        protected void OnZoomToFitClicked(object sender, RoutedEventArgs e)
        {
            watch_view.ZoomExtents();
        }

        void view_MouseButtonIgnore(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        void view_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            rightMousePoint = e.GetPosition(topControl);
        }

        void view_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if the mouse has moved, and this is a right click, we assume 
            // rotation. handle the event so we don't show the context menu
            // if the user wants the contextual menu they can click on the
            // node sidebar or top bar
            if (e.GetPosition(topControl) != rightMousePoint)
            {
                e.Handled = true;
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Create the grid
        /// </summary>
        private void DrawGrid()
        {
            Grid = new LineGeometry3D();
            var positions = new Vector3Collection();
            var indices = new IntCollection();
            var colors = new Color4Collection();

            for(var i= 0; i < 10; i += 1)
            {
                for (var j = 0; j < 10; j += 1)
                {
                    DrawGridPatch(positions, indices, colors, -50 + i * 10, -50 + j * 10);
                }
            }

            Grid.Positions = positions;
            Grid.Indices = indices;
            Grid.Colors = colors;

            Axes = new LineGeometry3D();
            var axesPositions = new Vector3Collection();
            var axesIndices = new IntCollection();
            var axesColors = new Color4Collection();

            // Draw the coordinate axes
            axesPositions.Add(new Vector3());
            axesIndices.Add(axesPositions.Count - 1);
            axesPositions.Add(new Vector3(50, 0, 0));
            axesIndices.Add(axesPositions.Count - 1);
            axesColors.Add(SharpDX.Color.Red);
            axesColors.Add(SharpDX.Color.Red);

            axesPositions.Add(new Vector3());
            axesIndices.Add(axesPositions.Count - 1);
            axesPositions.Add(new Vector3(0, 5, 0));
            axesIndices.Add(axesPositions.Count - 1);
            axesColors.Add(SharpDX.Color.Blue);
            axesColors.Add(SharpDX.Color.Blue);

            axesPositions.Add(new Vector3());
            axesIndices.Add(axesPositions.Count - 1);
            axesPositions.Add(new Vector3(0, 0, -50));
            axesIndices.Add(axesPositions.Count - 1);
            axesColors.Add(SharpDX.Color.Green);
            axesColors.Add(SharpDX.Color.Green);

            Axes.Positions = axesPositions;
            Axes.Indices = axesIndices;
            Axes.Colors = axesColors;

        }

        private static void DrawGridPatch(
            Vector3Collection positions, IntCollection indices, Color4Collection colors, int startX, int startY)
        {
            var c1 = (Color)ColorConverter.ConvertFromString("#c5d1d8");
            c1.Clamp();
            var c2 = (Color)ColorConverter.ConvertFromString("#ddeaf2");
            c2.Clamp();

            var darkGridColor = new Color4(new Vector4(c1.ScR,c1.ScG ,c1.ScB, 1));
            var lightGridColor = new Color4(new Vector4(c2.ScR, c2.ScG, c2.ScB, 1));

            const int size = 10;

            for (var x = startX; x <= startX + size; x++)
            {
                if (x == 0 && startY < 0) continue;

                var v = new Vector3(x, -.001f, startY);
                positions.Add(v);
                indices.Add(positions.Count - 1);
                positions.Add(new Vector3(x, -.001f, startY + size));
                indices.Add(positions.Count - 1);

                if (x % 5 == 0)
                {
                    colors.Add(darkGridColor);
                    colors.Add(darkGridColor);
                }
                else
                {
                    colors.Add(lightGridColor);
                    colors.Add(lightGridColor);
                }
            }

            for (var y = startY; y <= startY + size; y++)
            {
                if (y == 0 && startX >= 0) continue;

                positions.Add(new Vector3(startX, -.001f, y));
                indices.Add(positions.Count - 1);
                positions.Add(new Vector3(startX + size, -.001f, y));
                indices.Add(positions.Count - 1);

                if (y % 5 == 0)
                {
                    colors.Add(darkGridColor);
                    colors.Add(darkGridColor);
                }
                else
                {
                    colors.Add(lightGridColor);
                    colors.Add(lightGridColor);
                }
            }
        }

        /// <summary>
        /// Use the render packages returned from the visualization manager to update the visuals.
        /// The visualization event arguments will contain a set of render packages and an id representing 
        /// the associated node. Visualizations for the background preview will return an empty id.
        /// </summary>
        /// <param name="e"></param>
        public void RenderDrawables(VisualizationEventArgs e)
        {
            //check the id, if the id is meant for another watch,
            //then ignore it
            if (e.Id != _id)
            {               
                Attach();
                NotifyPropertyChanged("");
                return;
            }
        
            // Don't render if the user's system is incapable.
            if (renderingTier == 0)
            {
                return;
            }

#if DEBUG
            renderTimer.Start();
#endif                  
            Text = null;

            var packages = e.Packages.Concat(e.SelectedPackages)
                .Cast<HelixRenderPackage>().Where(rp=>rp.MeshVertexCount % 3 == 0);
    
            var text = HelixRenderPackage.InitText3D();

            var aggParams = new PackageAggregationParams
            {
                Packages = packages,                          
                Text = text
            };

            AggregateRenderPackages(aggParams);

#if DEBUG
            renderTimer.Stop();
            Debug.WriteLine(string.Format("RENDER: {0} ellapsed for compiling assets for rendering.", renderTimer.Elapsed));
            renderTimer.Reset();
            renderTimer.Start();
#endif        
            
            //Helix render the packages in certain order. Here, the BillBoardText has to be rendered
            //after rendering all the geometry. Otherwise, the Text will not get rendered at the right 
            //position. Also, BillBoardText gets attached only once. It is not removed from the tree everytime.
            //Instead, only the geometry gets updated every time. Once it is attached (after the geometry), helix
            // renders the text at the right position.
            if (Text != null && Text.TextInfo.Any())
            {
                BillboardTextModel3D billboardText3D = new BillboardTextModel3D
                {
                    Transform = Model1Transform
                };

                if (model3DDictionary != null && !model3DDictionary.ContainsKey("BillBoardText"))
                {
                    model3DDictionary.Add("BillBoardText", billboardText3D);
                }

                var billBoardModel3D = model3DDictionary["BillBoardText"] as BillboardTextModel3D;
                billBoardModel3D.Geometry = Text;
                if (!billBoardModel3D.IsAttached)
                {
                    billBoardModel3D.Attach(View.RenderHost);
                }
            }
            else
            {               
                if (model3DDictionary != null && model3DDictionary.ContainsKey("BillBoardText"))
                {
                    var billBoardModel3D = model3DDictionary["BillBoardText"] as BillboardTextModel3D;
                    billBoardModel3D.Geometry = Text;                   
                }                
            }

            //This is required for Dynamo to send property changed notifications to helix.          
            NotifyPropertyChanged("");
        }

        private void AggregateRenderPackages(PackageAggregationParams parameters)
        {
            //Clear the geometry values before adding the package.
            VisualizationManager_WorkspaceOpenedClosedHandled();

            foreach (var rp in parameters.Packages)
            {
                //Node ID gets updated with a ":" everytime this function is called.
                //For example, if the same point node is called multiple times (CBN), the ID has a ":"
                //and this makes the dictionary to have multiple entries for the same node. 
                var baseId = rp.Description;
                if (baseId.IndexOf(":", StringComparison.Ordinal) > 0)
                {
                    baseId = baseId.Split(':')[0];
                }
                var id = baseId;

                var p = rp.Points;
                if (p.Positions.Any())
                {
                    id = baseId + ":points";

                    PointGeometryModel3D pointGeometry3D;

                    if (model3DDictionary.ContainsKey(id))
                    {
                        pointGeometry3D = model3DDictionary[id] as PointGeometryModel3D;
                    }
                    else
                    {
                        pointGeometry3D = new PointGeometryModel3D
                        {
                            Geometry = HelixRenderPackage.InitPointGeometry(),
                            Transform = Model1Transform,
                            Color = SharpDX.Color.White,
                            Figure = PointGeometryModel3D.PointFigure.Ellipse,
                            Size = defaultPointSize,
                            IsHitTestVisible = true,
                            IsSelected = rp.IsSelected
                        };
                        model3DDictionary.Add(id, pointGeometry3D);
                    }

                    var points = pointGeometry3D.Geometry as PointGeometry3D;
                    var startIdx = points.Positions.Count;

                    points.Positions.AddRange(p.Positions);
                    points.Colors.AddRange(p.Colors.Any() ? p.Colors : Enumerable.Repeat(defaultPointColor, points.Positions.Count));
                    points.Indices.AddRange(p.Indices.Select(i => i + startIdx));

                    if (rp.DisplayLabels)
                    {
                        var pt = p.Positions[0];
                        parameters.Text.TextInfo.Add(new TextInfo(HelixRenderPackage.CleanTag(rp.Description), new Vector3(pt.X + 0.025f, pt.Y + 0.025f, pt.Z + 0.025f)));
                        Text = parameters.Text;
                    }

                    pointGeometry3D.Geometry = points;
                    pointGeometry3D.Name = baseId;
                    pointGeometry3D.MouseDown3D += meshGeometry3D_MouseDown3D;
                }

                var l = rp.Lines;
                if (l.Positions.Any())
                {
                    id = baseId + ":lines";

                    LineGeometryModel3D lineGeometry3D;

                    if (model3DDictionary.ContainsKey(id))
                    {
                        lineGeometry3D = model3DDictionary[id] as LineGeometryModel3D;
                    }
                    else
                    {
                        lineGeometry3D = new LineGeometryModel3D()
                        {
                            Geometry = HelixRenderPackage.InitLineGeometry(),
                            Transform = Model1Transform,
                            Color = SharpDX.Color.White,
                            Thickness = 0.5,
                            IsHitTestVisible = true,
                            IsSelected = rp.IsSelected
                        };

                        model3DDictionary.Add(id, lineGeometry3D);
                    }

                    var lineSet = lineGeometry3D.Geometry as LineGeometry3D;
                    var startIdx = lineSet.Positions.Count;

                    lineSet.Positions.AddRange(l.Positions);
                    lineSet.Colors.AddRange(l.Colors.Any() ? l.Colors : Enumerable.Repeat(defaultLineColor, l.Positions.Count));
                    lineSet.Indices.AddRange(l.Indices.Any() ? l.Indices.Select(i => i + startIdx) : Enumerable.Range(startIdx, startIdx + l.Positions.Count));

                    if (rp.DisplayLabels)
                    {
                        var pt = lineSet.Positions[startIdx];
                        parameters.Text.TextInfo.Add(new TextInfo(HelixRenderPackage.CleanTag(rp.Description), new Vector3(pt.X + 0.025f, pt.Y + 0.025f, pt.Z + 0.025f)));
                        Text = parameters.Text;
                    }

                    lineGeometry3D.Geometry = lineSet;
                    lineGeometry3D.Name = baseId;
                    lineGeometry3D.MouseDown3D += meshGeometry3D_MouseDown3D;
                }

                var m = rp.Mesh;
                if (!m.Positions.Any()) continue;

                id = ((rp.RequiresPerVertexColoration || rp.Colors != null) ? rp.Description : baseId) + ":mesh";

                DynamoGeometryModel3D meshGeometry3D;

                if (model3DDictionary.ContainsKey(id))
                {
                    meshGeometry3D = model3DDictionary[id] as DynamoGeometryModel3D;
                }
                else
                {
                    meshGeometry3D = new DynamoGeometryModel3D()
                    {
                        Transform = Model1Transform,
                        Material = WhiteMaterial,
                        IsHitTestVisible = true,
                        RequiresPerVertexColoration = rp.RequiresPerVertexColoration,
                        IsSelected = rp.IsSelected,
                    };
                    
                    if (rp.Colors != null)
                    {
                        var pf = PixelFormats.Bgra32;
                        var stride = (rp.ColorsStride / 4 * pf.BitsPerPixel + 7) / 8;
                        try
                        {
                            var diffMap = BitmapSource.Create(rp.ColorsStride/4, rp.Colors.Count()/rp.ColorsStride, 96.0, 96.0, pf, null,
                                rp.Colors.ToArray(), stride);
                            var diffMat = new PhongMaterial
                            {
                                Name = "White",
                                AmbientColor = PhongMaterials.ToColor(0.1, 0.1, 0.1, 1.0),
                                DiffuseColor = materialColor,
                                SpecularColor = PhongMaterials.ToColor(0.0225, 0.0225, 0.0225, 1.0),
                                EmissiveColor = PhongMaterials.ToColor(0.0, 0.0, 0.0, 1.0),
                                SpecularShininess = 12.8f,
                                DiffuseMap = diffMap
                            };
                            meshGeometry3D.Material = diffMat;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    ((MaterialGeometryModel3D) meshGeometry3D).SelectionColor = selectionColor; 
                    model3DDictionary.Add(id, meshGeometry3D);
                }

                var mesh = meshGeometry3D.Geometry == null ? HelixRenderPackage.InitMeshGeometry() : meshGeometry3D.Geometry as MeshGeometry3D;
                var idxCount = mesh.Positions.Count;

                mesh.Positions.AddRange(m.Positions);

                mesh.Colors.AddRange(m.Colors);
                mesh.Normals.AddRange(m.Normals);
                mesh.TextureCoordinates.AddRange(m.TextureCoordinates);
                mesh.Indices.AddRange(m.Indices.Select(i => i + idxCount));

                if (mesh.Colors.Any(c => c.Alpha < 1.0))
                {
                    meshGeometry3D.HasTransparency = true;
                }

                if (rp.DisplayLabels)
                {
                    var pt = mesh.Positions[idxCount];
                    parameters.Text.TextInfo.Add(new TextInfo(HelixRenderPackage.CleanTag(rp.Description), new Vector3(pt.X + 0.025f, pt.Y + 0.025f, pt.Z + 0.025f)));
                    Text = parameters.Text;
                }

                meshGeometry3D.Geometry = mesh;
                meshGeometry3D.Name = baseId; 
                meshGeometry3D.MouseDown3D += meshGeometry3D_MouseDown3D;
            }

            Attach();
        }

        void meshGeometry3D_MouseDown3D(object sender, RoutedEventArgs e)
        {
            var args = e as Mouse3DEventArgs;
            if (args == null) return;
            if (args.Viewport == null) return;

            var viewModel = DataContext as DynamoViewModel;
            foreach (var node in viewModel.Model.CurrentWorkspace.Nodes)
            {
                var foundNode = node.AstIdentifierBase.Contains(((GeometryModel3D) e.OriginalSource).Name);
                if (!foundNode) continue;
                DynamoSelection.Instance.ClearSelection();
                viewModel.Model.AddToSelection(node);
            }
        }
       
        private void Attach()
        {
            foreach (var kvp in model3DDictionary)
            {
                var model3d = kvp.Value;
                if (model3d is GeometryModel3D)
                {                  
                    if (View != null && View.RenderHost != null)
                    {
                        model3d.Attach(View.RenderHost);
                    }
                }
                else
                {
                    //This is for Directional Light. When a watch is attached,
                    //Directional light has to be attached one more time.
                    if (!model3d.IsAttached && View != null && View.RenderHost != null)
                    {
                        model3d.Attach(View.RenderHost);
                    }
                    //else
                    //{
                    //    model3d.Detach();
                    //    model3d.Attach(View.RenderHost);
                    //}
                }
            }   
        }

        private void SaveCamera(XmlElement camerasElement)
        {
            try
            {
                var node = XmlHelper.AddNode(camerasElement, "Camera");
                XmlHelper.AddAttribute(node, "Name", Name);
                XmlHelper.AddAttribute(node, "eyeX", Camera.Position.X.ToString(CultureInfo.InvariantCulture));
                XmlHelper.AddAttribute(node, "eyeY", Camera.Position.Y.ToString(CultureInfo.InvariantCulture));
                XmlHelper.AddAttribute(node, "eyeZ", Camera.Position.Z.ToString(CultureInfo.InvariantCulture));
                XmlHelper.AddAttribute(node, "lookX", Camera.LookDirection.X.ToString(CultureInfo.InvariantCulture));
                XmlHelper.AddAttribute(node, "lookY", Camera.LookDirection.Y.ToString(CultureInfo.InvariantCulture));
                XmlHelper.AddAttribute(node, "lookZ", Camera.LookDirection.Z.ToString(CultureInfo.InvariantCulture));
                camerasElement.AppendChild(node);
            }
            catch (Exception ex)
            {
                const string msg = "CAMERA: Camera position information could not be saved.";
                LogCameraWarning(msg, ex);
            }
        }

        private void LoadCamera(XmlNode cameraNode)
        {
            if (cameraNode.Attributes.Count == 0)
            {
                return;
            }

            try
            {
                Name = cameraNode.Attributes["Name"].Value;
                var ex = float.Parse(cameraNode.Attributes["eyeX"].Value);
                var ey = float.Parse(cameraNode.Attributes["eyeY"].Value);
                var ez = float.Parse(cameraNode.Attributes["eyeZ"].Value);
                var lx = float.Parse(cameraNode.Attributes["lookX"].Value);
                var ly = float.Parse(cameraNode.Attributes["lookY"].Value);
                var lz = float.Parse(cameraNode.Attributes["lookZ"].Value);

                Camera.LookDirection = new Vector3D(lx, ly, lz);
                Camera.Position = new Point3D(ex, ey, ez);
            }
            catch (Exception ex)
            {
                const string msg = "CAMERA: Camera position information could not be loaded from the file.";
                LogCameraWarning(msg, ex);
            }
        }

        private void LogCameraWarning(string msg, Exception ex)
        {
            viewModel.Model.Logger.Log(msg, LogLevel.Console);
            viewModel.Model.Logger.Log(msg, LogLevel.File);
            viewModel.Model.Logger.Log(ex.Message, LogLevel.File);
        }

        #endregion
    }

    internal class PackageAggregationParams
    {
        public IEnumerable<HelixRenderPackage> Packages { get; set; } 
        public BillboardText3D Text { get; set; }
    }
}
