using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Dynamo.UI;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using Dynamo.Wpf.Rendering;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using Color = System.Windows.Media.Color;
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
        private PerspectiveCamera camera;
        private Color4 selectionColor;
        private Color4 materialColor;
        private bool showShadows;
        private Vector3 directionalLightDirection;
        private Color4 directionalLightColor;
        private Color4 defaultLineColor;
        private Color4 defaultPointColor;
        private double lightAzimuthDegrees = 45.0;
        private double lightElevationDegrees = 35.0;

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

        public MeshGeometry3D PerVertexMesh { get; set; }

        public MeshGeometry3D MeshSelected { get; set; }

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

        public PhongMaterial SelectedMaterial { get; private set; }

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

            protected set
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

#if DEBUG
        /// <summary>
        /// The TestSelectionCommand is used in the WatchSettingsControl
        /// to test the ability to toggle a boolean effect variable
        /// representing the selection state.
        /// </summary>
        public Dynamo.UI.Commands.DelegateCommand TestSelectionCommand { get; set; }
#endif

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
            var ptColor = (Color)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PointColor"];
            defaultPointColor = new Color4(ptColor.R/255.0f, ptColor.G/255.0f, ptColor.B/255.0f, ptColor.A/255.0f);

            var lineColor = (Color)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["EdgeColor"];
            defaultLineColor = new Color4(lineColor.R/255.0f, lineColor.G/255.0f, lineColor.B/255.0f, lineColor.A/255.0f);

            DirectionalLightColor = new Color4(0.9f, 0.9f, 0.9f, 1.0f);
            DirectionalLightDirection = new Vector3(-0.5f, -1.0f, 0.0f);

            var matColor = (Color)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["MaterialColor"];
            materialColor = new Color4(matColor.R/255.0f, matColor.G/255.0f, matColor.B/255.0f, matColor.A/255.0f);
            RenderTechnique = Techniques.RenderPhong;
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
            Camera = new PerspectiveCamera
            {
                Position = new Point3D(10, 15, 10),
                LookDirection = new Vector3D(-10, -10, -10),
                UpDirection = new Vector3D(0, 1, 0),
                NearPlaneDistance = .1,
                FarPlaneDistance = 10000000,
                
            };

            DrawGrid();
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

#if DEBUG
            TestSelectionCommand = new Dynamo.UI.Commands.DelegateCommand(TestSelection, CanTestSelection);
#endif
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

            var cf = new Vector3((float)camera.LookDirection.X, (float)camera.LookDirection.Y, (float)camera.LookDirection.Z).Normalized();
            var cu = new Vector3((float)camera.UpDirection.X, (float)camera.UpDirection.Y, (float)camera.UpDirection.Z).Normalized();
            var right = Vector3.Cross(cf, cu);

            var qel = SharpDX.Quaternion.RotationAxis(right, (float)((-LightElevationDegrees * Math.PI) / 180));
            var qaz = SharpDX.Quaternion.RotationAxis(cu, (float)((LightAzimuthDegrees * Math.PI) / 180));
            var v = Vector3.Transform(cf, qaz*qel);

            if (!DirectionalLightDirection.Equals(v))
            {
                DirectionalLightDirection = v; 
            }
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
                return;
            }

#if DEBUG
            renderTimer.Start();
#endif
            Points = null;
            Lines = null;
            LinesSelected = null;
            Mesh = null;
            PerVertexMesh = null;
            MeshSelected = null;
            Text = null;
            MeshCount = 0;

            var packages = e.Packages.Concat(e.SelectedPackages)
                .Cast<HelixRenderPackage>().Where(rp=>rp.MeshVertexCount % 3 == 0);

            var points = HelixRenderPackage.InitPointGeometry();
            var lines = HelixRenderPackage.InitLineGeometry();
            var linesSel = HelixRenderPackage.InitLineGeometry();
            var mesh = HelixRenderPackage.InitMeshGeometry();
            var meshSel = HelixRenderPackage.InitMeshGeometry();
            var perVertexMesh = HelixRenderPackage.InitMeshGeometry();
            var text = HelixRenderPackage.InitText3D();

            var aggParams = new PackageAggregationParams
            {
                Packages = packages,
                Points = points,
                Lines = lines,
                SelectedLines = linesSel,
                Mesh = mesh,
                PerVertexMesh = perVertexMesh,
                SelectedMesh = meshSel,
                Text = text
            };

            AggregateRenderPackages(aggParams);

            if (!points.Positions.Any())
                points = null;

            if (!lines.Positions.Any())
                lines = null;

            if (!linesSel.Positions.Any())
                linesSel = null;

            if (!text.TextInfo.Any())
                text = null;

            if (!mesh.Positions.Any())
                mesh = null;

            if (!meshSel.Positions.Any())
                meshSel = null;

            if (!perVertexMesh.Positions.Any())
                perVertexMesh = null;

#if DEBUG
            renderTimer.Stop();
            Debug.WriteLine(string.Format("RENDER: {0} ellapsed for compiling assets for rendering.", renderTimer.Elapsed));
            renderTimer.Reset();
            renderTimer.Start();
#endif

            var updateGraphicsParams = new GraphicsUpdateParams
            {
                Points = points,
                Lines = lines,
                SelectedLines = linesSel,
                Mesh = mesh,
                SelectedMesh = meshSel,
                PerVertexMesh = perVertexMesh,
                Text = text
            };

            SendGraphicsToView(updateGraphicsParams);

            //DrawTestMesh();
        }

        private void AggregateRenderPackages(PackageAggregationParams parameters)
        {


            MeshCount = 0;
            foreach (var rp in parameters.Packages)
            {
                var p = rp.Points;
                if (p.Positions.Any())
                {
                    var points = parameters.Points;

                    var startIdx = points.Positions.Count;

                    points.Positions.AddRange(p.Positions);
                    points.Colors.AddRange(p.Colors.Any() ? p.Colors : Enumerable.Repeat(defaultPointColor, points.Positions.Count));
                    points.Indices.AddRange(p.Indices.Select(i=> i + startIdx));

                    var endIdx = points.Positions.Count;

                    if (rp.IsSelected)
                    {
                        for (var i = startIdx; i < endIdx; i++)
                        {
                            points.Colors[i] = selectionColor;
                        }
                    }

                    if (rp.DisplayLabels)
                    {
                        var pt = p.Positions[0];
                        parameters.Text.TextInfo.Add(new TextInfo(HelixRenderPackage.CleanTag(rp.Description), new Vector3(pt.X + 0.025f, pt.Y + 0.025f, pt.Z + 0.025f)));
                    }
                }

                var l = rp.Lines;
                if (l.Positions.Any())
                {
                    // Choose a collection to store the line data.
                    var lineSet = rp.IsSelected ? parameters.SelectedLines : parameters.Lines;

                    var startIdx = lineSet.Positions.Count;

                    lineSet.Positions.AddRange(l.Positions);
                    lineSet.Colors.AddRange(l.Colors.Any() ? l.Colors : Enumerable.Repeat(defaultLineColor, l.Positions.Count));
                    lineSet.Indices.AddRange(l.Indices.Any()? l.Indices.Select(i=>i + startIdx) : Enumerable.Range(startIdx, startIdx + l.Positions.Count));

                    var endIdx = lineSet.Positions.Count;

                    if (rp.IsSelected)
                    {
                        for (var i = startIdx; i < endIdx; i++)
                        {
                            lineSet.Colors[i] = selectionColor;
                        }
                    }

                    if (rp.DisplayLabels)
                    {
                        var pt = lineSet.Positions[startIdx];
                        parameters.Text.TextInfo.Add(new TextInfo(HelixRenderPackage.CleanTag(rp.Description), new Vector3(pt.X + 0.025f, pt.Y + 0.025f, pt.Z + 0.025f)));
                    }
                }

                var m = rp.Mesh;
                if (m.Positions.Any())
                {
                    // Pick a mesh to use to store the data. Selected geometry
                    // goes into the selected mesh. Geometry with
                    // colors goes into the per vertex mesh. Everything else
                    // goes into the plain mesh.

                    var meshSet = rp.IsSelected ? 
                        parameters.SelectedMesh :
                        rp.RequiresPerVertexColoration ? parameters.PerVertexMesh : parameters.Mesh;

                    var idxCount = meshSet.Positions.Count;

                    meshSet.Positions.AddRange(m.Positions);

                    meshSet.Colors.AddRange(m.Colors);
                    meshSet.Normals.AddRange(m.Normals);
                    meshSet.TextureCoordinates.AddRange(m.TextureCoordinates);
                    meshSet.Indices.AddRange(m.Indices.Select(i => i + idxCount));

                    if (rp.DisplayLabels)
                    {
                        var pt = meshSet.Positions[idxCount];
                        parameters.Text.TextInfo.Add(new TextInfo(HelixRenderPackage.CleanTag(rp.Description), new Vector3(pt.X + 0.025f, pt.Y + 0.025f, pt.Z + 0.025f)));
                    }

                    MeshCount++;
                }
            }
        }

        private void SendGraphicsToView(GraphicsUpdateParams parameters)
        {
            Points = parameters.Points;
            Lines = parameters.Lines;
            LinesSelected = parameters.SelectedLines;
            Mesh = parameters.Mesh;
            MeshSelected = parameters.SelectedMesh;
            PerVertexMesh = parameters.PerVertexMesh;
            Text = parameters.Text;

            // Send property changed notifications for everything
            NotifyPropertyChanged(string.Empty);
        }

        #endregion

#if DEBUG
        private bool CanTestSelection(object parameters)
        {
            return true;
        }

        private void TestSelection(object parameters)
        {
            foreach (var item in watch_view.Items)
            {
                var geom = item as HelixToolkit.Wpf.SharpDX.GeometryModel3D;
                if (geom != null)
                {
                    geom.IsSelected = !geom.IsSelected;
                }
            }
        }
#endif

    }

    internal class GraphicsUpdateParams
    {
        public PointGeometry3D Points { get; set; }
        public LineGeometry3D Lines { get; set; }
        public LineGeometry3D SelectedLines { get; set; }
        public MeshGeometry3D Mesh { get; set; }
        public MeshGeometry3D SelectedMesh { get; set; }
        public MeshGeometry3D PerVertexMesh { get; set; }
        public BillboardText3D Text { get; set; }
    }

    internal class PackageAggregationParams
    {
        public IEnumerable<HelixRenderPackage> Packages { get; set; } 
        public PointGeometry3D Points { get; set; }
        public LineGeometry3D Lines { get; set; }
        public LineGeometry3D SelectedLines { get; set; }
        public MeshGeometry3D Mesh { get; set; }
        public MeshGeometry3D PerVertexMesh { get; set; }
        public MeshGeometry3D SelectedMesh { get; set; }
        public BillboardText3D Text { get; set; }
    }
}
