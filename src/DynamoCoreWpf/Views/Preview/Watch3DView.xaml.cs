using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using Autodesk.DesignScript.Interfaces;

using Dynamo.ViewModels;
using Dynamo.DSEngine;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
//using HelixToolkit.Wpf.SharpDX.Model.Geometry;

using SharpDX;

using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using Color = SharpDX.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;
using Point = System.Windows.Point;
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
        private Camera camera;
        private Color4 selectionColor = new Color4(0,158.0f/255.0f,1,1);
        private bool showShadows;
        private Vector3 directionalLightDirection;
        private Color4 directionalLightColor;
        private Vector3 fillLightDirection;
        private Color4 fillLightColor;
        private Color4 ambientLightColor;

#if DEBUG
        private Stopwatch renderTimer = new Stopwatch();
#endif

        #endregion

        #region public properties

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

        public PointGeometry3D Points { get; set; }

        public LineGeometry3D Lines { get; set; }

        public LineGeometry3D LinesSelected { get; set; }

        public MeshGeometry3D Mesh { get; set; }

        public BillboardText3D Text { get; set; }

        public Viewport3DX View
        {
            get { return watch_view; }
        }

        /// <summary>
        /// Used for testing to track the number of meshes that are merged
        /// during render.
        /// </summary>
        public int MeshCount { get; set; }

        public PhongMaterial WhiteMaterial { get; private set; }

        public Vector3 DirectionalLightDirection
        {
            get { return directionalLightDirection; }
            private set
            {
                directionalLightDirection = value;
                NotifyPropertyChanged("DirectionalLightDirection");
            }
        }

        public Color4 DirectionalLightColor
        {
            get { return directionalLightColor; }
            private set
            {
                directionalLightColor = value;
                NotifyPropertyChanged("DirectionalLightColor");
            }
        }

        public Vector3 FillLightDirection
        {
            get { return fillLightDirection; }
            private set
            {
                fillLightDirection = value; 
                NotifyPropertyChanged("FillLightDirection");
            }
        }

        public Color4 FillLightColor
        {
            get { return fillLightColor; }
            private set
            {
                fillLightColor = value; 
                NotifyPropertyChanged("FillLightColor");
            }
        }

        public Color4 AmbientLightColor
        {
            get { return ambientLightColor; }
            private set
            {
                ambientLightColor = value;
                NotifyPropertyChanged("AmbientLightColor");
            }
        }

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

        public Camera Camera
        {
            get
            {
                return this.camera;
            }

            protected set
            {
                camera = value;
                NotifyPropertyChanged("Camera");
            }
        }
        
        public Vector2 ShadowMapResolution { get; private set; }

        public bool ShowShadows
        {
            get { return showShadows; }
            set
            {
                showShadows = value;
                NotifyPropertyChanged("ShowShadows");
            }
        }

        public string KeyX
        {
            get { return DirectionalLightDirection.X.ToString(CultureInfo.InvariantCulture); }
            set
            {
                DirectionalLightDirection = new Vector3(float.Parse(value, CultureInfo.InvariantCulture), DirectionalLightDirection.Y, DirectionalLightDirection.Z);
                NotifyPropertyChanged("KeyX");
            }
        }

        public string KeyY
        {
            get { return DirectionalLightDirection.Y.ToString(CultureInfo.InvariantCulture); }
            set
            {
                DirectionalLightDirection = new Vector3(DirectionalLightDirection.X, float.Parse(value, CultureInfo.InvariantCulture), DirectionalLightDirection.Z);
                NotifyPropertyChanged("KeyY");
            }
        }

        public string KeyZ
        {
            get { return DirectionalLightDirection.Z.ToString(CultureInfo.InvariantCulture); }
            set
            {
                DirectionalLightDirection = new Vector3(DirectionalLightDirection.X, DirectionalLightDirection.Y, float.Parse(value, CultureInfo.InvariantCulture));
                NotifyPropertyChanged("KeyZ");
            }
        }

        public string KeyR
        {
            get { return DirectionalLightColor.Red.ToString(CultureInfo.InvariantCulture); }
            set
            {
                DirectionalLightColor = new Color4(float.Parse(value, CultureInfo.InvariantCulture), DirectionalLightColor.Green, DirectionalLightColor.Blue, 1.0f);
                NotifyPropertyChanged("KeyR");
            }
        }

        public string KeyG
        {
            get { return DirectionalLightColor.Green.ToString(CultureInfo.InvariantCulture); }
            set
            {
                DirectionalLightColor = new Color4(DirectionalLightColor.Red, float.Parse(value, CultureInfo.InvariantCulture), DirectionalLightColor.Blue, 1.0f);
                NotifyPropertyChanged("KeyG");
            }
        }

        public string KeyB
        {
            get { return DirectionalLightColor.Blue.ToString(CultureInfo.InvariantCulture); }
            set
            {
                DirectionalLightColor = new Color4(DirectionalLightColor.Red, DirectionalLightColor.Green, float.Parse(value, CultureInfo.InvariantCulture), 1.0f);
                NotifyPropertyChanged("KeyB");
            }
        }

        public string FillX
        {
            get { return FillLightDirection.X.ToString(CultureInfo.InvariantCulture); }
            set
            {
                FillLightDirection = new Vector3(float.Parse(value, CultureInfo.InvariantCulture), FillLightDirection.Y, FillLightDirection.Z);
                NotifyPropertyChanged("FillX");
            }
        }

        public string FillY
        {
            get { return FillLightDirection.Y.ToString(CultureInfo.InvariantCulture); }
            set
            {
                FillLightDirection = new Vector3(FillLightDirection.X, float.Parse(value, CultureInfo.InvariantCulture), FillLightDirection.Z);
                NotifyPropertyChanged("FillY");
            }
        }

        public string FillZ
        {
            get { return FillLightDirection.Z.ToString(CultureInfo.InvariantCulture); }
            set
            {
                FillLightDirection = new Vector3(FillLightDirection.X, FillLightDirection.Y, float.Parse(value, CultureInfo.InvariantCulture));
                NotifyPropertyChanged("FillZ");
            }
        }

        public string FillR
        {
            get { return FillLightColor.Red.ToString(CultureInfo.InvariantCulture); }
            set
            {
                FillLightColor = new Color4(float.Parse(value, CultureInfo.InvariantCulture), FillLightColor.Green, FillLightColor.Blue, 1.0f);
                NotifyPropertyChanged("FillR");
            }
        }

        public string FillG
        {
            get { return FillLightColor.Green.ToString(CultureInfo.InvariantCulture); }
            set
            {
                FillLightColor = new Color4(FillLightColor.Red, float.Parse(value, CultureInfo.InvariantCulture), FillLightColor.Blue, 1.0f);
                NotifyPropertyChanged("FillG");
            }
        }

        public string FillB
        {
            get { return FillLightColor.Blue.ToString(CultureInfo.InvariantCulture); }
            set
            {
                FillLightColor = new Color4(FillLightColor.Red, FillLightColor.Green, float.Parse(value, CultureInfo.InvariantCulture), 1.0f);
                NotifyPropertyChanged("FillB");
            }
        }

        public string AmbientR
        {
            get { return AmbientLightColor.Red.ToString(CultureInfo.InvariantCulture); }
            set
            {
                AmbientLightColor = new Color4(float.Parse(value, CultureInfo.InvariantCulture), AmbientLightColor.Green, AmbientLightColor.Blue, 1.0f);
                NotifyPropertyChanged("AmbientR");
            }
        }

        public string AmbientG
        {
            get { return AmbientLightColor.Green.ToString(CultureInfo.InvariantCulture); }
            set
            {
                AmbientLightColor = new Color4(AmbientLightColor.Red, float.Parse(value, CultureInfo.InvariantCulture), AmbientLightColor.Blue, 1.0f);
                NotifyPropertyChanged("AmbientG");
            }
        }

        public string AmbientB
        {
            get { return AmbientLightColor.Blue.ToString(CultureInfo.InvariantCulture); }
            set
            {
                AmbientLightColor = new Color4(AmbientLightColor.Red, AmbientLightColor.Green, float.Parse(value, CultureInfo.InvariantCulture), 1.0f);
                NotifyPropertyChanged("AmbientB");
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
        }

        public Watch3DView(Guid id)
        {

            SetupScene();

            InitializeComponent();
            watch_view.DataContext = this;
            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;

            _id = id;

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
        }

        private void SetupScene()
        {
            ShadowMapResolution = new Vector2(2048, 2048);
            ShowShadows = false;
            
            // setup lighting            
            //AmbientLightColor = new Color4(0.3f, 0.3f, 0.3f, 1.0f);
            AmbientLightColor = new Color4(0.0f, 0.0f, 0.0f, 1.0f);

            DirectionalLightColor = new Color4(0.9f, 0.9f, 0.9f, 1.0f);
            DirectionalLightDirection = new Vector3(-0.5f, -1.0f, 0.0f);
            
            //FillLightColor = new Color4(new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            FillLightColor = new Color4(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            FillLightDirection = new Vector3(0.5f, 1.0f, 0f);

            var matColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString("#efede4");
            RenderTechnique = Techniques.RenderPhong;
            WhiteMaterial = new PhongMaterial
            {
                Name = "White",
                AmbientColor = PhongMaterials.ToColor(0.1, 0.1, 0.1, 1.0),
                //DiffuseColor = PhongMaterials.ToColor(0.992157, 0.992157, 0.992157, 1.0),
                DiffuseColor = PhongMaterials.ToColor(matColor.R, matColor.G, matColor.B, 1.0f),
                SpecularColor = PhongMaterials.ToColor(0.0225, 0.0225, 0.0225, 1.0),
                EmissiveColor = PhongMaterials.ToColor(0.0, 0.0, 0.0, 1.0),
                SpecularShininess = 12.8f,
            };


            Model1Transform = new TranslateTransform3D(0, -0, 0);

            // camera setup
            Camera = new PerspectiveCamera
            {
                Position = new Point3D(10, 15, 10),
                LookDirection = new Vector3D(-10, -10, -10),
                UpDirection = new Vector3D(0, 1, 0),
            };

            DrawGrid();
        }

        private void DrawTestMesh()
        {
            var b1 = new MeshBuilder();
            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    for (int z = 0; z < 20; z++)
                    {
                        b1.AddBox(new Vector3(x, y, z), 0.5, 0.5, 0.5, BoxFaces.All);
                        //b1.AddSphere(new Vector3(x, y, z), 0.25);
                    }
                }
            }
            Mesh = b1.ToMeshGeometry3D();
            NotifyPropertyChanged("Mesh");
        }
        
        #endregion

        #region event handlers

        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as IWatchViewModel;
            if (vm == null) return;
            vm.VisualizationManager.RenderComplete -= VisualizationManagerRenderComplete;
            vm.VisualizationManager.ResultsReadyToVisualize -= VisualizationManager_ResultsReadyToVisualize;
            vm.ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

            CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            MouseLeftButtonDown += view_MouseButtonIgnore;
            MouseLeftButtonUp += view_MouseButtonIgnore;
            MouseRightButtonUp += view_MouseRightButtonUp;
            PreviewMouseRightButtonDown += view_PreviewMouseRightButtonDown;

            var vm = DataContext as IWatchViewModel;
            
            //check this for null so the designer can load the preview
            if (vm == null) return;

            vm.VisualizationManager.RenderComplete += VisualizationManagerRenderComplete;
            vm.VisualizationManager.ResultsReadyToVisualize += VisualizationManager_ResultsReadyToVisualize;

            var renderingTier = (RenderCapability.Tier >> 16);
            var pixelShader3Supported = RenderCapability.IsPixelShaderVersionSupported(3, 0);
            var pixelShader4Supported = RenderCapability.IsPixelShaderVersionSupported(4, 0);
            var softwareEffectSupported = RenderCapability.IsShaderEffectSoftwareRenderingSupported;
            var maxTextureSize = RenderCapability.MaxHardwareTextureSize;

            vm.ViewModel.Model.Logger.Log(string.Format("RENDER : Rendering Tier: {0}", renderingTier), LogLevel.File);
            vm.ViewModel.Model.Logger.Log(string.Format("RENDER : Pixel Shader 3 Supported: {0}", pixelShader3Supported), LogLevel.File);
            vm.ViewModel.Model.Logger.Log(string.Format("RENDER : Pixel Shader 4 Supported: {0}", pixelShader4Supported), LogLevel.File);
            vm.ViewModel.Model.Logger.Log(string.Format("RENDER : Software Effect Rendering Supported: {0}", softwareEffectSupported), LogLevel.File);
            vm.ViewModel.Model.Logger.Log(string.Format("RENDER : Maximum hardware texture size: {0}", maxTextureSize), LogLevel.File);

            vm.ViewModel.PropertyChanged += ViewModel_PropertyChanged;
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
            var c = new Vector3((float)camera.LookDirection.X, (float)camera.LookDirection.Y, (float)camera.LookDirection.Z);
            DirectionalLightDirection = c;
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

        private void Watch_view_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Point mousePos = e.GetPosition(watch_view);
            //PointHitTestParameters hitParams = new PointHitTestParameters(mousePos);
            //VisualTreeHelper.HitTest(watch_view, null, ResultCallback, hitParams);
            //e.Handled = true;
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
            axesColors.Add(Color.Red);
            axesColors.Add(Color.Red);

            axesPositions.Add(new Vector3());
            axesIndices.Add(axesPositions.Count - 1);
            axesPositions.Add(new Vector3(0, 5, 0));
            axesIndices.Add(axesPositions.Count - 1);
            axesColors.Add(Color.Blue);
            axesColors.Add(Color.Blue);

            axesPositions.Add(new Vector3());
            axesIndices.Add(axesPositions.Count - 1);
            axesPositions.Add(new Vector3(0, 0, -50));
            axesIndices.Add(axesPositions.Count - 1);
            axesColors.Add(Color.Green);
            axesColors.Add(Color.Green);

            Axes.Positions = axesPositions;
            Axes.Indices = axesIndices;
            Axes.Colors = axesColors;

        }

        private static void DrawGridPatch(
            Vector3Collection positions, IntCollection indices, Color4Collection colors, int startX, int startY)
        {
            var c1 = (System.Windows.Media.Color)ColorConverter.ConvertFromString("#c5d1d8");
            c1.Clamp();
            var c2 = (System.Windows.Media.Color)ColorConverter.ConvertFromString("#ddeaf2");
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
                return;
            }

#if DEBUG
            renderTimer.Start();
#endif

            Points = null;
            Lines = null;
            LinesSelected = null;
            Mesh = null;
            Text = null;
            MeshCount = 0;

            var packages = e.Packages
                .Where(rp=>rp.TriangleVertices.Count % 9 == 0);

            var points = InitPointGeometry();
            var lines = InitLineGeometry();
            var linesSelected = InitLineGeometry();
            var text = InitText3D(); 
            var mesh = InitMeshGeometry();

            foreach (RenderPackage package in packages)
            {
                ConvertPoints(package, points, text);
                ConvertLines(package, package.Selected ? linesSelected : lines, text);
                ConvertMeshes(package, mesh);
            }

            if (!points.Positions.Any())
                points = null;

            if (!lines.Positions.Any())
                lines = null;

            if (!linesSelected.Positions.Any())
                linesSelected = null;

            if (!text.TextInfo.Any())
                text = null;

            if (!mesh.Positions.Any())
                mesh = null;

#if DEBUG
            renderTimer.Stop();
            Debug.WriteLine(string.Format("RENDER: {0} ellapsed for compiling assets for rendering.", renderTimer.Elapsed));
            renderTimer.Reset();
            renderTimer.Start();
#endif

            SendGraphicsToView(points, lines, linesSelected, mesh, text);

            //DrawTestMesh();
        }

        private static LineGeometry3D InitLineGeometry()
        {
            var lines = new LineGeometry3D
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };

            return lines;
        }

        private static PointGeometry3D InitPointGeometry()
        {
            var points = new PointGeometry3D()
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };

            return points;
        }

        private static MeshGeometry3D InitMeshGeometry()
        {
            var mesh = new MeshGeometry3D()
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection(),
                Normals = new Vector3Collection(),
            };

            return mesh;
        }

        private static BillboardText3D InitText3D()
        {
            var text3D = new BillboardText3D();

            return text3D;
        }

        private void SendGraphicsToView(
            PointGeometry3D points,
            LineGeometry3D lines,
            LineGeometry3D linesSelected,
            MeshGeometry3D mesh,
            BillboardText3D text)
        {
            Points = points;
            Lines = lines;
            LinesSelected = linesSelected;
            Mesh = mesh;
            Text = text;
            
            // Send property changed notifications for everything
            NotifyPropertyChanged(string.Empty);
        }

        private void ConvertPoints(IRenderPackage p, PointGeometry3D points, BillboardText3D text)
        {
            var color_idx = 0;

            for (int i = 0; i < p.PointVertices.Count; i += 3)
            {
                var x = (float)p.PointVertices[i];
                var y = (float)p.PointVertices[i + 1];
                var z = (float)p.PointVertices[i + 2];

                // DirectX convention - Y Up
                var pt = new Vector3(x, z, -y);

                if (i == 0 && ((RenderPackage)p).DisplayLabels)
                {
                    text.TextInfo.Add(new TextInfo(CleanTag(p.Tag), new Vector3(pt.X + 0.025f, pt.Y + 0.025f, pt.Z + 0.025f)));
                }

                // The default point color is black. If the point
                // colors array is large enough, then we pull the 
                // point color from that.
                var ptColor = Color4.Black;
                if (p.PointVertexColors.Count >= color_idx + 4)
                {
                    ptColor = new Color4(
                                        (p.PointVertexColors[color_idx] / 255.0f),
                                        (p.PointVertexColors[color_idx + 1] / 255.0f),
                                        (p.PointVertexColors[color_idx + 2] / 255.0f), 1);
                }

                points.Positions.Add(pt);
                points.Indices.Add(points.Positions.Count);

                points.Colors.Add(((RenderPackage)p).Selected ? selectionColor : ptColor);

                color_idx += 4;
            }

        }

        private void ConvertLines(IRenderPackage p, LineGeometry3D geom, BillboardText3D text)
        {
            int color_idx = 0;
            var idx = 0;
            int outerCount = 0;

            foreach (var count in p.LineStripVertexCounts)
            {
                for (int i = 0; i < count; ++i)
                {
                    var x1 = (float)p.LineStripVertices[idx];
                    var y1 = (float)p.LineStripVertices[idx + 1];
                    var z1 = (float)p.LineStripVertices[idx + 2];

                    // DirectX convention - Y Up
                    var pt = new Vector3(x1, z1, -y1);

                    if (i == 0 && outerCount == 0 && ((RenderPackage)p).DisplayLabels)
                    {
                        text.TextInfo.Add(new TextInfo(CleanTag(p.Tag), new Vector3(pt.X + 0.025f, pt.Y + 0.025f, pt.Z + 0.025f)));
                    }

                    Color4 startColor = Color.Black;

                    if (p.LineStripVertexColors.Count >= color_idx + 2)
                    {
                        startColor = new Color4(
                            (p.LineStripVertexColors[color_idx] / 255.0f),
                            (p.LineStripVertexColors[color_idx + 1] / 255.0f),
                            (p.LineStripVertexColors[color_idx + 2] / 255.0f),
                            1);
                    }

                    // Line segments are represented as a 
                    // start point and an end point. Except
                    // where we are starting the curve or ending it,
                    // we duplicate the point.
                    if (i != 0 && i != count - 1)
                    {
                        geom.Indices.Add(geom.Indices.Count);
                        geom.Positions.Add(pt);
                        geom.Colors.Add(((RenderPackage)p).Selected ? selectionColor : startColor);
                    }

                    geom.Indices.Add(geom.Indices.Count);
                    geom.Positions.Add(pt);
                    geom.Colors.Add(((RenderPackage)p).Selected ? selectionColor : startColor);
                    
                    idx += 3;
                    color_idx += 4;
                }

                outerCount++;
            }
        }

        private void ConvertMeshes(IRenderPackage p, MeshGeometry3D mesh)
        { 
            // DirectX has a different winding than we store in
            // render packages. Re-wind triangles here...
            var color_idx = 0;
            var pt_idx = mesh.Positions.Count;

            for (int i = 0; i < p.TriangleVertices.Count; i += 9)
            {
                var a = GetVertex(p.TriangleVertices, i);
                var b = GetVertex(p.TriangleVertices, i + 3);
                var c = GetVertex(p.TriangleVertices, i + 6);

                var an = GetVertex(p.TriangleNormals, i);
                var bn = GetVertex(p.TriangleNormals, i + 3);
                var cn = GetVertex(p.TriangleNormals, i + 6);
                an.Normalize();
                bn.Normalize();
                cn.Normalize();

                var ca = GetColor(p, color_idx);
                var cb = GetColor(p, color_idx + 4);
                var cc = GetColor(p, color_idx + 8);

                mesh.Positions.Add(a);
                mesh.Positions.Add(c);
                mesh.Positions.Add(b);

                mesh.Indices.Add(pt_idx);
                mesh.Indices.Add(pt_idx + 1);
                mesh.Indices.Add(pt_idx + 2);

                mesh.Normals.Add(an);
                mesh.Normals.Add(cn);
                mesh.Normals.Add(bn);

                if (((RenderPackage)p).Selected)
                {
                    mesh.Colors.Add(selectionColor);
                    mesh.Colors.Add(selectionColor);
                    mesh.Colors.Add(selectionColor);
                }
                else
                {
                    mesh.Colors.Add(ca);
                    mesh.Colors.Add(cc);
                    mesh.Colors.Add(cb); 
                }

                color_idx += 12;
                pt_idx += 3;
            }

            if (mesh.Indices.Count > 0)
            {
                MeshCount++;
            }
        }

        private static Color4 GetColor(IRenderPackage p, int color_idx)
        {
            var color = new Color4(1,1,1,1);

            if (color_idx <= p.TriangleVertexColors.Count-3)
            {
                color = new Color4(
                (float)(p.TriangleVertexColors[color_idx] / 255.0),
                (float)(p.TriangleVertexColors[color_idx + 1] / 255.0),
                (float)(p.TriangleVertexColors[color_idx + 2] / 255.0),
                (float)(p.TriangleVertexColors[color_idx + 3] / 255.0));
            }
           
            return color;
        }

        private static Vector3 GetVertex(List<double> p, int i)
        {
            var x = (float)p[i];
            var y = (float)p[i + 1];
            var z = (float)p[i + 2];

            // DirectX convention - Y Up
            var new_point = new Vector3(x, z, -y);
            return new_point;
        }

        private string CleanTag(string tag)
        {
            var splits = tag.Split(':');
            if (splits.Count() <= 1) return tag;

            var sb = new StringBuilder();
            for (int i = 1; i < splits.Count(); i++)
            {
                sb.AppendFormat("[{0}]", splits[i]);
            }
            return sb.ToString();
        }

        #endregion
    }
}
