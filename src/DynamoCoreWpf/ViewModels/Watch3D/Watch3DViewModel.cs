using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.ViewModels;
using Dynamo.Wpf.Rendering;
using DynamoUtilities;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
using Model3D = HelixToolkit.Wpf.SharpDX.Model3D;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;
using Quaternion = SharpDX.Quaternion;
using TextInfo = HelixToolkit.Wpf.SharpDX.TextInfo;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    public class Watch3DViewModel : NotificationObject
    {
        #region private memmbers

        protected DynamoModel model;
        protected IRenderPackageFactory factory;
        private DynamoViewModel viewModel;
        private List<NodeModel> recentlyAddedNodes = new List<NodeModel>();
        private int renderingTier;
        private Color4 defaultLineColor = new Color4(new Color3(0, 0, 0));
        private Color4 defaultPointColor = new Color4(new Color3(0, 0, 0));
        private double lightAzimuthDegrees = 45.0;
        private double lightElevationDegrees = 35.0;
        private LineGeometry3D worldGrid;
        private LineGeometry3D worldAxes;
        private RenderTechnique renderTechnique;
        private PerspectiveCamera camera;
        private double nearPlaneDistanceFactor = 0.01;
        private Color4 selectionColor = new Color4(new Color3(0, 158.0f / 255.0f, 1.0f));
        private Color4 materialColor = new Color4(new Color3(1.0f, 1.0f, 1.0f));
        private Vector3 directionalLightDirection = new Vector3(-0.5f, -1.0f, 0.0f);
        private Color4 directionalLightColor = new Color4(0.9f, 0.9f, 0.9f, 1.0f);
        internal readonly Vector3D defaultCameraLookDirection = new Vector3D(-10, -10, -10);
        internal readonly Point3D defaultCameraPosition = new Point3D(10, 15, 10);
        internal readonly Vector3D defaultCameraUpDirection = new Vector3D(0, 1, 0);
        private readonly Size defaultPointSize = new Size(8, 8);
        private DirectionalLight3D directionalLight;
        private bool showWatchSettingsControl = false;

#if DEBUG
        private Stopwatch renderTimer = new Stopwatch();
#endif

        #endregion

        #region events

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

        public event Action GeometryDictionaryReset;
        private void OnGeometryDictionaryReset()
        {
            if (GeometryDictionaryReset != null)
            {
                GeometryDictionaryReset();
            }
        }

        public event Action RequestViewRefresh;
        protected void OnRequestViewRefresh()
        {
            if (RequestViewRefresh != null)
            {
                RequestViewRefresh();
            }
        }

        public event Action<Model3D> RequestAttachToScene;
        protected void OnRequestAttachToScene(Model3D model3D)
        {
            if (RequestAttachToScene != null)
            {
                RequestAttachToScene(model3D);
            }
        }

        public event Action<IEnumerable<IRenderPackage>> RequestCreateModels;
        public void OnRequestCreateModels(IEnumerable<IRenderPackage> packages)
        {
            if (RequestCreateModels != null)
            {
                RequestCreateModels(packages);
            }
        }

        #endregion

        #region properties

        public List<Model3D> Model3DValues
        {
            get
            {
                if (Model3DDictionary == null)
                {
                    return new List<Model3D>();
                }

                var values = Model3DDictionary.
                    Select(x => x.Value).
                    ToList();

                values.Sort((a, b) =>
                {
                    var aType = a.GetType() == typeof(BillboardTextModel3D);
                    var bType = b.GetType() == typeof(BillboardTextModel3D);
                    return aType.CompareTo(bType);
                });

                return values;
            }
        }

        public LineGeometry3D Grid
        {
            get { return worldGrid; }
            set
            {
                worldGrid = value;
                RaisePropertyChanged("Grid");
            }
        }

        public LineGeometry3D Axes
        {
            get { return worldAxes; }
            set
            {
                worldAxes = value;
                RaisePropertyChanged("Axes");
            }
        }

        public BillboardText3D Text { get; set; }

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
                RaisePropertyChanged("RenderTechnique");
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
                RaisePropertyChanged("Camera");
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

        public double NearPlaneDistanceFactor
        {
            get { return nearPlaneDistanceFactor; }
            set
            {
                nearPlaneDistanceFactor = value;
                RaisePropertyChanged("");
            }
        }

        internal string Name { get; set; }

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
                if (viewModel.IsPanning) return ViewportCommands.Pan;
                if (viewModel.IsOrbiting) return ViewportCommands.Rotate;

                return null;
            }
        }

        public bool IsResizable { get; protected set; }

        public bool ShowWatchSettingsControl
        {
            get { return showWatchSettingsControl; }
            set
            {
                showWatchSettingsControl = value;
                RaisePropertyChanged("ShowWatchSettingsControl");
            }
        }

        #endregion

        public Watch3DViewModel(DynamoModel model, IRenderPackageFactory factory, DynamoViewModel viewModel)
        {
            this.model = model;
            this.factory = factory;
            this.viewModel = viewModel;

            IsResizable = false;
            Name = "background_preview";

            RegisterEventHandlers();

            SetupScene();
            InitializeHelix();
        }

        #region public methods

        public void GenerateViewGeometryFromRenderPackagesAndRequestUpdate(IEnumerable<IRenderPackage> taskPackages)
        {
            recentlyAddedNodes.Clear();

            // Don't render if the user's system is incapable.
            if (renderingTier == 0)
            {
                return;
            }

#if DEBUG
            renderTimer.Start();
#endif
            Text = null;

            var packages = taskPackages
                .Cast<HelixRenderPackage>().Where(rp => rp.MeshVertexCount % 3 == 0);

            RemoveGeometryFromDisconnectedNodes();

            RemoveGeometryForUpdatedPackages(packages);

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
                    OnRequestAttachToScene(billBoardModel3D);
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

            RaisePropertyChanged("Model3DValues");
            OnRequestViewRefresh();
        }

        #endregion

        #region private methods

        private void RegisterEventHandlers()
        {
            DynamoSelection.Instance.Selection.CollectionChanged += SelectionChangedHandler;

            LogVisualizationCapabilities();

            viewModel.PropertyChanged += OnViewModelPropertyChanged;

            viewModel.RenderPackageFactoryViewModel.PropertyChanged += OnRenderPackageFactoryViewModelPropertyChanged;

            RegisterModelEventhandlers(model);

            RegisterWorkspaceEventHandlers(model);

        }

        private void UnregisterEventHandlers()
        {
            DynamoSelection.Instance.Selection.CollectionChanged -= SelectionChangedHandler;

            viewModel.PropertyChanged -= OnViewModelPropertyChanged;

            viewModel.RenderPackageFactoryViewModel.PropertyChanged -= OnRenderPackageFactoryViewModelPropertyChanged;

            UnregisterModelEventHandlers(model);

            UnregisterWorkspaceEventHandlers(model);
        }

        private void OnModelShutdownStarted(DynamoModel model)
        {
            UnregisterEventHandlers();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsPanning":
                case "IsOrbiting":
                    RaisePropertyChanged("LeftClickCommand");
                    break;
            }
        }

        private void LogVisualizationCapabilities()
        {
            renderingTier = (RenderCapability.Tier >> 16);
            var pixelShader3Supported = RenderCapability.IsPixelShaderVersionSupported(3, 0);
            var pixelShader4Supported = RenderCapability.IsPixelShaderVersionSupported(4, 0);
            var softwareEffectSupported = RenderCapability.IsShaderEffectSoftwareRenderingSupported;
            var maxTextureSize = RenderCapability.MaxHardwareTextureSize;

            model.Logger.Log(string.Format("RENDER : Rendering Tier: {0}", renderingTier), LogLevel.File);
            model.Logger.Log(string.Format("RENDER : Pixel Shader 3 Supported: {0}", pixelShader3Supported),
                LogLevel.File);
            model.Logger.Log(string.Format("RENDER : Pixel Shader 4 Supported: {0}", pixelShader4Supported),
                LogLevel.File);
            model.Logger.Log(
                string.Format("RENDER : Software Effect Rendering Supported: {0}", softwareEffectSupported), LogLevel.File);
            model.Logger.Log(string.Format("RENDER : Maximum hardware texture size: {0}", maxTextureSize),
                LogLevel.File);
        }

        private void SelectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    Model3DDictionary.Values.
                        Where(v => v is GeometryModel3D).
                        Cast<GeometryModel3D>().ToList().ForEach(g => g.IsSelected = false);
                    return;

                case NotifyCollectionChangedAction.Remove:
                    SetSelection(e.OldItems, false);
                    return;

                case NotifyCollectionChangedAction.Add:

                    // When a node is added to the workspace, it is also added
                    // to the selection. When running automatically, this addition
                    // also triggers an execution. This would successive calls to render.
                    // To prevent this, we maintain a collection of recently added nodes, and
                    // we check if the selection is an addition and if all of the recently
                    // added nodes are contained in that selection. if so, we skip the render
                    // as this render will occur after the upcoming execution.
                    if (e.Action == NotifyCollectionChangedAction.Add && recentlyAddedNodes.Any()
                        && recentlyAddedNodes.TrueForAll(n => e.NewItems.Contains((object)n)))
                    {
                        recentlyAddedNodes.Clear();
                        return;
                    }

                    SetSelection(e.NewItems, true);
                    return;
            }
        }

        private void SetSelection(IEnumerable items, bool isSelected)
        {
            foreach (var item in items)
            {
                var node = item as NodeModel;
                if (node == null)
                {
                    continue;
                }

                var geometryModels = FindAllGeometryModel3DsForNode(node);

                if (!geometryModels.Any())
                {
                    continue;
                }

                var modelValues = geometryModels.Select(x => x.Value);
                modelValues.Cast<GeometryModel3D>().ToList().ForEach(g => g.IsSelected = isSelected);
            }
        }

        private void DeleteGeometryForNode(NodeModel node)
        {
            var geometryModels = FindAllGeometryModel3DsForNode(node);

            if (!geometryModels.Any())
            {
                return;
            }

            foreach (var kvp in geometryModels)
            {
                var model = Model3DDictionary[kvp.Key] as GeometryModel3D;
                if (model != null)
                {
                    model.MouseDown3D -= MeshGeometry3DMouseDown3DHandler;
                }
                Model3DDictionary.Remove(kvp.Key);
            }

            RaisePropertyChanged("Model3DValues");
        }

        private void LogCameraWarning(string msg, Exception ex)
        {
            model.Logger.Log(msg, LogLevel.Console);
            model.Logger.Log(msg, LogLevel.File);
            model.Logger.Log(ex.Message, LogLevel.File);
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

        private void RegisterModelEventhandlers(DynamoModel model)
        {
            model.WorkspaceCleared += OnWorkspaceCleared;
            model.ShutdownStarted += OnModelShutdownStarted;
            model.CleaningUp += ResetGeometryDictionary;
        }

        private void UnregisterModelEventHandlers(DynamoModel model)
        {
            model.WorkspaceCleared -= OnWorkspaceCleared;
            model.ShutdownStarted -= OnModelShutdownStarted;
            model.CleaningUp -= ResetGeometryDictionary;
        }

        private void UnregisterWorkspaceEventHandlers(DynamoModel model)
        {
            model.WorkspaceAdded -= OnWorkspaceAdded;
            model.WorkspaceRemoved -= OnWorkspaceRemoved;
            model.WorkspaceOpening -= OnWorkspaceOpening;

            foreach (var ws in model.Workspaces)
            {
                ws.Saving -= OnWorkspaceSaving;
            }
        }

        private void RegisterWorkspaceEventHandlers(DynamoModel model)
        {
            model.WorkspaceAdded += OnWorkspaceAdded;
            model.WorkspaceRemoved += OnWorkspaceRemoved;
            model.WorkspaceOpening += OnWorkspaceOpening;

            foreach (var ws in model.Workspaces)
            {
                ws.Saving += OnWorkspaceSaving;
                ws.NodeAdded += OnNodeAddedToWorkspace;
                ws.NodeRemoved += OnNodeRemovedFromWorkspace;

                foreach (var node in ws.Nodes)
                {
                    node.PropertyChanged += OnNodePropertyChanged;
                    node.UpdatedRenderPackagesAvailable += OnUpdatedRenderPackagesAvailable;
                }
            }
        }

        private void OnRenderPackageFactoryViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ShowEdges":
                    model.PreferenceSettings.ShowEdges =
                        factory.TessellationParameters.ShowEdges;
                    break;
            }

            model.CurrentWorkspace.Nodes.Select(n => n.IsUpdated = true);

            foreach (var node in
                model.CurrentWorkspace.Nodes)
            {
                node.RequestVisualUpdateAsync(model.Scheduler, model.EngineController,
                        factory);
            }
        }

        private void OnWorkspaceCleared(object sender, EventArgs e)
        {
            SetCameraToDefaultOrientation();
            ResetGeometryDictionary();
        }

        private void OnWorkspaceOpening(XmlDocument doc)
        {
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

        private void OnWorkspaceAdded(WorkspaceModel workspace)
        {
            workspace.Saving += OnWorkspaceSaving;
            workspace.NodeAdded += OnNodeAddedToWorkspace;
            workspace.NodeRemoved += OnNodeRemovedFromWorkspace;

            foreach (var node in workspace.Nodes)
            {
                RegisterNodeEventHandlers(node);
            }
        }

        private void OnWorkspaceRemoved(WorkspaceModel workspace)
        {
            workspace.Saving -= OnWorkspaceSaving;
            workspace.NodeAdded -= OnNodeAddedToWorkspace;
            workspace.NodeRemoved -= OnNodeRemovedFromWorkspace;
        }

        private void OnWorkspaceSaving(XmlDocument doc)
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

        private void OnNodeAddedToWorkspace(NodeModel node)
        {
            RegisterNodeEventHandlers(node);
        }

        private void OnNodeRemovedFromWorkspace(NodeModel node)
        {
            UnregisterNodeEventHandlers(node);
            DeleteGeometryForNode(node);
        }

        private void RegisterNodeEventHandlers(NodeModel node)
        {
            node.PropertyChanged += OnNodePropertyChanged;
            node.UpdatedRenderPackagesAvailable += OnUpdatedRenderPackagesAvailable;
        }

        private void UnregisterNodeEventHandlers(NodeModel node)
        {
            node.PropertyChanged -= OnNodePropertyChanged;
            node.UpdatedRenderPackagesAvailable -= OnUpdatedRenderPackagesAvailable;
        }

        private void SetupScene()
        {
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

        /// <summary>
        /// Create the grid
        /// </summary>
        private void DrawGrid()
        {
            Grid = new LineGeometry3D();
            var positions = new Vector3Collection();
            var indices = new IntCollection();
            var colors = new Color4Collection();

            for (var i = 0; i < 10; i += 1)
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
            var c1 = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#c5d1d8");
            c1.Clamp();
            var c2 = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ddeaf2");
            c2.Clamp();

            var darkGridColor = new Color4(new Vector4(c1.ScR, c1.ScG, c1.ScB, 1));
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

        private void SetCameraToDefaultOrientation()
        {
            Camera.LookDirection = defaultCameraLookDirection;
            Camera.Position = defaultCameraPosition;
            Camera.UpDirection = defaultCameraUpDirection;
            Camera.NearPlaneDistance = CalculateNearClipPlane(1000000);
            Camera.FarPlaneDistance = 10000000;
        }

        private double CalculateNearClipPlane(double maxDim)
        {
            return maxDim * NearPlaneDistanceFactor;
        }

        private void RemoveGeometryFromDisconnectedNodes()
        {
            var noRenderNodes = viewModel.Model.CurrentWorkspace.Nodes.
                Where(n => n.IsUpdated).
                Where(n => !n.RenderPackages.Any());

            foreach (var node in noRenderNodes)
            {
                var idBase = node.AstIdentifierBase;
                var pointsId = idBase + ":points";
                var linesId = idBase + ":lines";
                var meshId = idBase + ":mesh";

                if (Model3DDictionary.ContainsKey(pointsId))
                {
                    Model3DDictionary[pointsId].Detach();
                    Model3DDictionary.Remove(pointsId);
                }

                if (Model3DDictionary.ContainsKey(linesId))
                {
                    Model3DDictionary[linesId].Detach();
                    Model3DDictionary.Remove(linesId);
                }

                if (Model3DDictionary.ContainsKey(meshId))
                {
                    Model3DDictionary[meshId].Detach();
                    Model3DDictionary.Remove(meshId);
                }
            }
        }

        private void RemoveGeometryForUpdatedPackages(IEnumerable<IRenderPackage> packages)
        {
            var packageDescrips = packages.Select(p => p.Description.Split(':')[0]).Distinct();

            foreach (var id in packageDescrips)
            {
                var pointsId = id + ":points";
                var linesId = id + ":lines";
                var meshId = id + ":mesh";

                if (Model3DDictionary.ContainsKey(pointsId))
                {
                    Model3DDictionary[pointsId].Detach();
                    Model3DDictionary.Remove(pointsId);
                }

                if (Model3DDictionary.ContainsKey(linesId))
                {
                    Model3DDictionary[linesId].Detach();
                    Model3DDictionary.Remove(linesId);
                }

                if (Model3DDictionary.ContainsKey(meshId))
                {
                    Model3DDictionary[meshId].Detach();
                    Model3DDictionary.Remove(meshId);
                }
            }
        }

        private void AggregateRenderPackages(PackageAggregationParams parameters)
        {
            foreach (var rp in parameters.Packages)
            {
                // Each node can produce multiple render packages. We want all the geometry of the
                // same kind stored inside a RenderPackage to be pushed into one GeometryModel3D object.
                // We strip the unique identifier for the package (i.e. the bit after the `:` in var12345:0), and replace it
                // with `points`, `lines`, or `mesh`. For each RenderPackage, we check whether the geometry dictionary
                // has entries for the points, lines, or mesh already. If so, we add the RenderPackage's geometry
                // to those geometry objects.

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
                        pointGeometry3D = CreatePointGeometryModel3D(rp);
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
                    pointGeometry3D.MouseDown3D += MeshGeometry3DMouseDown3DHandler;
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
                        lineGeometry3D = CreateLineGeometryModel3D(rp);
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
                    lineGeometry3D.MouseDown3D += MeshGeometry3DMouseDown3DHandler;
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
                    meshGeometry3D = CreateDynamoGeometryModel3D(rp);
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
                meshGeometry3D.MouseDown3D += MeshGeometry3DMouseDown3DHandler;
            }

            AttachAllGeometryModel3DToRenderHost();
        }

        private DynamoGeometryModel3D CreateDynamoGeometryModel3D(HelixRenderPackage rp)
        {
            var meshGeometry3D = new DynamoGeometryModel3D()
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
                    var diffMap = BitmapSource.Create(rp.ColorsStride / 4, rp.Colors.Count() / rp.ColorsStride, 96.0, 96.0, pf, null,
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
            ((MaterialGeometryModel3D)meshGeometry3D).SelectionColor = selectionColor;

            return meshGeometry3D;
        }

        private LineGeometryModel3D CreateLineGeometryModel3D(HelixRenderPackage rp)
        {
            var lineGeometry3D = new LineGeometryModel3D()
            {
                Geometry = HelixRenderPackage.InitLineGeometry(),
                Transform = Model1Transform,
                Color = SharpDX.Color.White,
                Thickness = 0.5,
                IsHitTestVisible = true,
                IsSelected = rp.IsSelected
            };
            return lineGeometry3D;
        }

        private PointGeometryModel3D CreatePointGeometryModel3D(HelixRenderPackage rp)
        {
            var pointGeometry3D = new PointGeometryModel3D
            {
                Geometry = HelixRenderPackage.InitPointGeometry(),
                Transform = Model1Transform,
                Color = SharpDX.Color.White,
                Figure = PointGeometryModel3D.PointFigure.Ellipse,
                Size = defaultPointSize,
                IsHitTestVisible = true,
                IsSelected = rp.IsSelected
            };
            return pointGeometry3D;
        }

        private void AttachAllGeometryModel3DToRenderHost()
        {
            foreach (var model3D in model3DDictionary.Select(kvp => kvp.Value))
            {
                OnRequestAttachToScene(model3D);
            }
        }

        private void MeshGeometry3DMouseDown3DHandler(object sender, RoutedEventArgs e)
        {
            var args = e as Mouse3DEventArgs;
            if (args == null) return;
            if (args.Viewport == null) return;

            foreach (var node in model.CurrentWorkspace.Nodes)
            {
                var foundNode = node.AstIdentifierBase.Contains(((GeometryModel3D)e.OriginalSource).Name);
                if (!foundNode) continue;
                DynamoSelection.Instance.ClearSelection();
                viewModel.Model.AddToSelection(node);
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
                mesh.Colors.Add(new Color4(1f, 0f, 0f, 1f));
            }

            return mesh;
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

            var gridModel3D = new LineGeometryModel3D
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

            var axesModel3D = new LineGeometryModel3D
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

        /// <summary>
        /// This method attempts to maximize the near clip plane in order to 
        /// achiever higher z-buffer precision.
        /// </summary>
        private void UpdateNearClipPlaneForSceneBounds(Rect3D sceneBounds)
        {
            // http: //www.sjbaker.org/steve/omniv/love_your_z_buffer.html
            var maxDim = Math.Max(Math.Max(sceneBounds.SizeX, sceneBounds.Y), sceneBounds.SizeZ);
            Camera.NearPlaneDistance = Math.Max(CalculateNearClipPlane(maxDim), 0.1);
        }

        #endregion

        #region internal methods

        internal void ComputeFrameUpdate(Rect3D sceneBounds)
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
            var v = Vector3.Transform(cf, qaz * qel);
            directionalLightDirection = v;

            if (!directionalLight.Direction.Equals(directionalLightDirection))
            {
                directionalLight.Direction = v;
            }

            UpdateNearClipPlaneForSceneBounds(sceneBounds);
        }

        #endregion

        #region protected methods

        protected virtual void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = sender as NodeModel;
            if (node == null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case "DisplayLabels":
                    node.RequestVisualUpdateAsync(model.Scheduler, model.EngineController, viewModel.RenderPackageFactoryViewModel.Factory);
                    break;

                case "IsVisible":
                    var geoms = FindAllGeometryModel3DsForNode(node);
                    if (geoms.Any())
                    {
                        geoms.ToList()
                            .ForEach(g => g.Value.Visibility = node.IsVisible ? Visibility.Visible : Visibility.Hidden);
                        RaisePropertyChanged("Model3DValues");
                    }
                    else
                    {
                        node.RequestVisualUpdateAsync(model.Scheduler, model.EngineController, factory);
                    }
                    break;

                case "IsUpdated":
                    node.RequestVisualUpdateAsync(model.Scheduler,
                        model.EngineController,
                        factory);
                    break;
            }
        }

        /// <summary>
        /// Reset the geometry dictionary, keeping the DirectionalLight ,Grid, and Axes.
        /// These values are rendered only once by helix, attaching them again will make 
        /// no effect on helix.
        /// </summary> 
        protected void ResetGeometryDictionary()
        {
            var keysList = new List<string> { "DirectionalLight", "Grid", "Axes", "BillBoardText" };
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

            RaisePropertyChanged("");
            OnGeometryDictionaryReset();
        }

        protected KeyValuePair<string, Model3D>[] FindAllGeometryModel3DsForNode(NodeModel node)
        {
            var geometryModels =
                Model3DDictionary
                    .Where(x => x.Key.Contains(node.AstIdentifierBase))
                    .Where(x => x.Value is GeometryModel3D)
                    .Select(x => x).ToArray();

            return geometryModels;
        }

        protected virtual void OnUpdatedRenderPackagesAvailable(NodeModel source, IEnumerable<IRenderPackage> renderPackages)
        {
            // Raise request for model objects to be
            // created on the UI thread.

            var packages = renderPackages.ToArray();
            OnRequestCreateModels(packages);
        }

        #endregion
    }
}
