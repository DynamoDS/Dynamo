using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Serialization;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Services;
using Dynamo.UI.Commands;
using Dynamo.Wpf.Rendering;
using DynamoUtilities;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using Color = SharpDX.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
using Model3D = HelixToolkit.Wpf.SharpDX.Model3D;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;
using Quaternion = SharpDX.Quaternion;
using TextInfo = HelixToolkit.Wpf.SharpDX.TextInfo;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    public class CameraData
    {
        private readonly Vector3D defaultCameraLookDirection = new Vector3D(-10, -10, -10);
        private readonly Point3D defaultCameraPosition = new Point3D(10, 15, 10);
        private readonly Vector3D defaultCameraUpDirection = new Vector3D(0, 1, 0);
        private const double defaultNearPlaneDistance = 0.1;
        private const double defaultFarPlaneDistance = 10000000;

        public Point3D EyePosition { get; set; }
        public Vector3D UpDirection { get; set; }
        public Vector3D LookDirection { get; set; }
        public string Name { get; set; }
        public double NearPlaneDistance { get; set; }
        public double FarPlaneDistance { get; set; }

        public CameraData()
        {
            Name = "Default Camera";
            EyePosition = defaultCameraPosition;
            UpDirection = defaultCameraUpDirection;
            LookDirection = defaultCameraLookDirection;
            NearPlaneDistance = defaultNearPlaneDistance;
            FarPlaneDistance = defaultFarPlaneDistance;
        }
    }

    public class HelixWatch3DViewModel : Watch3DViewModelBase
    {
        #region private members

        private double lightAzimuthDegrees = 45.0;
        private double lightElevationDegrees = 35.0;
        private LineGeometry3D worldGrid;
        private LineGeometry3D worldAxes;
        private RenderTechnique renderTechnique;
        private PerspectiveCamera camera;
        private double nearPlaneDistanceFactor = 0.01;
        private Vector3 directionalLightDirection = new Vector3(-0.5f, -1.0f, 0.0f);
        private DirectionalLight3D directionalLight;
        private bool showWatchSettingsControl = false;

        private readonly Color4 directionalLightColor = new Color4(0.9f, 0.9f, 0.9f, 1.0f);
        private readonly Color4 defaultSelectionColor = new Color4(new Color3(0, 158.0f / 255.0f, 1.0f));
        private readonly Color4 defaultMaterialColor = new Color4(new Color3(1.0f, 1.0f, 1.0f));
        private readonly Size defaultPointSize = new Size(8, 8);
        private readonly Color4 defaultLineColor = new Color4(new Color3(0, 0, 0));
        private readonly Color4 defaultPointColor = new Color4(new Color3(0, 0, 0));

        internal const string DefaultGridName = "Grid";
        internal const string DefaultAxesName = "Axes";
        internal const string DefaultLightName = "DirectionalLight";

        private const string PointsKey = ":points";
        private const string LinesKey = ":lines";
        private const string MeshKey = ":mesh";
        private const string TextKey = ":text";

#if DEBUG
        private readonly Stopwatch renderTimer = new Stopwatch();
#endif

        #endregion

        #region events

        public Object Model3DDictionaryMutex = new object();
        private Dictionary<string, Model3D> model3DDictionary = new Dictionary<string, Model3D>();

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

        internal Dictionary<string, Model3D> Model3DDictionary
        {
            get
            {
                lock (Model3DDictionaryMutex)
                {
                    return model3DDictionary;
                }
            }

            set
            {
                lock (Model3DDictionaryMutex)
                {
                    model3DDictionary = value;
                }
            }
        }

        public LineGeometry3D Grid
        {
            get { return worldGrid; }
            set
            {
                worldGrid = value;
                RaisePropertyChanged(DefaultGridName);
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
                RaisePropertyChanged("NearPlaneDistanceFactor");
            }
        }

        public bool IsPanning
        {
            get
            {
                return CurrentSpaceViewModel != null && CurrentSpaceViewModel.IsPanning;
            }
        }

        public bool IsOrbiting
        {
            get
            {
                return CurrentSpaceViewModel != null && CurrentSpaceViewModel.IsOrbiting;
            }
        }

        public DelegateCommand TogglePanCommand { get; set; }

        public DelegateCommand ToggleOrbitCommand { get; set; }

        public DelegateCommand ToggleCanNavigateBackgroundCommand { get; set; }

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
                if (IsPanning) return ViewportCommands.Pan;
                if (IsOrbiting) return ViewportCommands.Rotate;

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

        public IEnumerable<Model3D> SceneItems
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

        private bool canNavigateBackground = false;
        public bool CanNavigateBackground
        {
            get
            {
                return canNavigateBackground || navigationKeyIsDown; 
            }
            set
            {
                canNavigateBackground = value;
                RaisePropertyChanged("CanNavigateBackground");
            }
        }

        private bool navigationKeyIsDown = false;
        public bool NavigationKeyIsDown
        {
            get { return navigationKeyIsDown; }
            set 
            { 
                navigationKeyIsDown = value;
                RaisePropertyChanged("NavigationKeyIsDown");
                RaisePropertyChanged("CanNavigateBackground");
            }
        }
        
        #endregion

        protected HelixWatch3DViewModel(Watch3DViewModelStartupParams parameters) : base(parameters)
        {
            IsResizable = false;

            TogglePanCommand = new DelegateCommand(TogglePan, CanTogglePan);
            ToggleOrbitCommand = new DelegateCommand(ToggleOrbit, CanToggleOrbit);
            ToggleCanNavigateBackgroundCommand = new DelegateCommand(ToggleCanNavigateBackground, CanToggleCanNavigateBackground);
        }

        public static HelixWatch3DViewModel Start(Watch3DViewModelStartupParams parameters)
        {
            var vm = new HelixWatch3DViewModel(parameters);
            vm.OnStartup();
            return vm;
        }

        public void SerializeCamera(XmlElement camerasElement)
        {
            try
            {
                var node = XmlHelper.AddNode(camerasElement, "Camera");
                XmlHelper.AddAttribute(node, "Name", Name);
                XmlHelper.AddAttribute(node, "eyeX", camera.Position.X.ToString(CultureInfo.InvariantCulture));
                XmlHelper.AddAttribute(node, "eyeY", camera.Position.Y.ToString(CultureInfo.InvariantCulture));
                XmlHelper.AddAttribute(node, "eyeZ", camera.Position.Z.ToString(CultureInfo.InvariantCulture));
                XmlHelper.AddAttribute(node, "lookX", camera.LookDirection.X.ToString(CultureInfo.InvariantCulture));
                XmlHelper.AddAttribute(node, "lookY", camera.LookDirection.Y.ToString(CultureInfo.InvariantCulture));
                XmlHelper.AddAttribute(node, "lookZ", camera.LookDirection.Z.ToString(CultureInfo.InvariantCulture));
                XmlHelper.AddAttribute(node, "upX", camera.UpDirection.X.ToString(CultureInfo.InvariantCulture));
                XmlHelper.AddAttribute(node, "upY", camera.UpDirection.Y.ToString(CultureInfo.InvariantCulture));
                XmlHelper.AddAttribute(node, "upZ", camera.UpDirection.Z.ToString(CultureInfo.InvariantCulture));
                camerasElement.AppendChild(node);

            }
            catch (Exception ex)
            {
                logger.LogError(Properties.Resources.CameraDataSaveError);
                logger.Log(ex);
            }
        }

        /// <summary>
        /// Create a CameraData object from an XmlNode representing the Camera's serialized
        /// position data.
        /// </summary>
        /// <param name="cameraNode">The XmlNode containing the camera position data.</param>
        /// <returns></returns>
        public CameraData DeserializeCamera(XmlNode cameraNode)
        {
            if (cameraNode.Attributes == null || cameraNode.Attributes.Count == 0)
            {
                return new CameraData();
            }

            try
            {
                var name = cameraNode.Attributes["Name"].Value;
                var ex = float.Parse(cameraNode.Attributes["eyeX"].Value, CultureInfo.InvariantCulture);
                var ey = float.Parse(cameraNode.Attributes["eyeY"].Value, CultureInfo.InvariantCulture);
                var ez = float.Parse(cameraNode.Attributes["eyeZ"].Value, CultureInfo.InvariantCulture);
                var lx = float.Parse(cameraNode.Attributes["lookX"].Value, CultureInfo.InvariantCulture);
                var ly = float.Parse(cameraNode.Attributes["lookY"].Value, CultureInfo.InvariantCulture);
                var lz = float.Parse(cameraNode.Attributes["lookZ"].Value, CultureInfo.InvariantCulture);
                var ux = float.Parse(cameraNode.Attributes["upX"].Value, CultureInfo.InvariantCulture);
                var uy = float.Parse(cameraNode.Attributes["upY"].Value, CultureInfo.InvariantCulture);
                var uz = float.Parse(cameraNode.Attributes["upZ"].Value, CultureInfo.InvariantCulture);

                var camData = new CameraData
                {
                    Name = name,
                    EyePosition = new Point3D(ex, ey, ez),
                    LookDirection = new Vector3D(lx, ly, lz),
                    UpDirection = new Vector3D(ux, uy, uz)
                };

                return camData;
            }
            catch (Exception ex)
            {
                logger.LogError(Properties.Resources.CameraDataLoadError);
                logger.Log(ex);
            }

            return new CameraData();
        }

        protected override void OnStartup()
        {
            SetupScene();
            InitializeHelix();
        }

        protected override void OnBeginUpdate(IEnumerable<IRenderPackage> packages)
        {
            if (Active == false)
            {
                return;
            }

            // Raise request for model objects to be
            // created on the UI thread.
            OnRequestCreateModels(packages);
        }

        protected override void OnClear()
        {
            lock (Model3DDictionaryMutex)
            {
                var keysList = new List<string> { DefaultLightName, DefaultGridName, DefaultAxesName };

                foreach (var key in Model3DDictionary.Keys.Except(keysList).ToList())
                {
                    var model = Model3DDictionary[key] as GeometryModel3D;
                    model.Detach();
                    Model3DDictionary.Remove(key);
                }
            }

            RaisePropertyChanged("SceneItems");
            OnRequestViewRefresh();
        }

        protected override void OnActiveStateChanged()
        {
            preferences.IsBackgroundPreviewActive = active;

            if (active == false && CanNavigateBackground)
            {
                CanNavigateBackground = false;
            }
        }

        protected override void OnWorkspaceCleared(WorkspaceModel workspace)
        {
            SetCameraData(new CameraData());
            base.OnWorkspaceCleared(workspace);
        }

        protected override void OnWorkspaceOpening(XmlDocument doc)
        {
            var camerasElements = doc.GetElementsByTagName("Cameras");
            if (camerasElements.Count == 0)
            {
                return;
            }

            foreach (XmlNode cameraNode in camerasElements[0].ChildNodes)
            {
                try
                {
                    var camData = DeserializeCamera(cameraNode);
                    SetCameraData(camData);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    logger.Log(ex);
                }
            }
        }

        protected override void OnWorkspaceSaving(XmlDocument doc)
        {
            var root = doc.DocumentElement;
            if (root == null)
            {
                return;
            }

            var camerasElement = doc.CreateElement("Cameras");
            SerializeCamera(camerasElement);
            root.AppendChild(camerasElement);
        }

        protected override void SelectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
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

        protected override void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = sender as NodeModel;
            if (node == null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case "IsUpdated":
                    // Only request updates when this is true. All nodes are marked IsUpdated=false
                    // before an evaluation occurs.

                    if (node.IsUpdated)
                    {
                        node.RequestVisualUpdateAsync(scheduler, engineManager.EngineController, renderPackageFactory);
                    }
                    break;

                case "DisplayLabels":
                    node.RequestVisualUpdateAsync(scheduler, engineManager.EngineController, renderPackageFactory);
                    break;

                case "IsVisible":
                    var geoms = FindAllGeometryModel3DsForNode(node.AstIdentifierBase);
                    foreach(var g in geoms)
                    {
                        g.Value.Visibility = node.IsVisible ? Visibility.Visible : Visibility.Hidden;
                        //RaisePropertyChanged("SceneItems");
                    }

                    node.IsUpdated = true;
                    node.RequestVisualUpdateAsync(scheduler, engineManager.EngineController, renderPackageFactory);

                    break;
            }
        }

        public override void GenerateViewGeometryFromRenderPackagesAndRequestUpdate(IEnumerable<IRenderPackage> taskPackages)
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
            var packages = taskPackages
                .Cast<HelixRenderPackage>().Where(rp => rp.MeshVertexCount % 3 == 0);

            RemoveGeometryForUpdatedPackages(packages);

            AggregateRenderPackages(packages);

#if DEBUG
            renderTimer.Stop();
            Debug.WriteLine(string.Format("RENDER: {0} ellapsed for compiling assets for rendering.", renderTimer.Elapsed));
            renderTimer.Reset();
            renderTimer.Start();
#endif

            RaisePropertyChanged("SceneItems");
            OnRequestViewRefresh();
        }

        protected override void DeleteGeometryForIdentifier(string identifier, bool requestUpdate = true)
        {
            lock (Model3DDictionaryMutex)
            {
                var geometryModels = FindAllGeometryModel3DsForNode(identifier);

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
                    }
                    Model3DDictionary.Remove(kvp.Key);
                }
            }

            if (!requestUpdate) return;

            RaisePropertyChanged("SceneItems");
            OnRequestViewRefresh();
        }

        protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentWorkspace":
                    OnClear();
                    foreach (var node in model.CurrentWorkspace.Nodes)
                    {
                        node.IsUpdated = true;
                        node.RequestVisualUpdateAsync(scheduler, engineManager.EngineController, renderPackageFactory);
                    }
                    break;
            }
        }

        #region internal methods

        internal void ComputeFrameUpdate()
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
        }

        #endregion

        private KeyValuePair<string, Model3D>[] FindAllGeometryModel3DsForNode(string identifier)
        {
            KeyValuePair<string, Model3D>[] geometryModels;

            lock (Model3DDictionaryMutex)
            {
                geometryModels =
                    Model3DDictionary
                        .Where(x => x.Key.Contains(identifier))
                        .Where(x => x.Value is GeometryModel3D)
                        .Select(x => x).ToArray();
            }

            return geometryModels;
        }

        #region private methods

        private void SetSelection(IEnumerable items, bool isSelected)
        {
            foreach (var item in items)
            {
                var node = item as NodeModel;
                if (node == null)
                {
                    continue;
                }

                var geometryModels = FindAllGeometryModel3DsForNode(node.AstIdentifierBase);

                if (!geometryModels.Any())
                {
                    continue;
                }

                var modelValues = geometryModels.Select(x => x.Value);
                modelValues.Cast<GeometryModel3D>().ToList().ForEach(g => g.IsSelected = isSelected);
            }
        }

        private void LogCameraWarning(string msg, Exception ex)
        {
            logger.LogWarning(msg, WarningLevel.Mild);
            logger.Log(msg);
            logger.Log(ex.Message);
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

        private void SetupScene()
        {
            RenderTechnique = Techniques.RenderDynamo;

            WhiteMaterial = new PhongMaterial
            {
                Name = "White",
                AmbientColor = PhongMaterials.ToColor(0.1, 0.1, 0.1, 1.0),
                DiffuseColor = defaultMaterialColor,
                SpecularColor = PhongMaterials.ToColor(0.0225, 0.0225, 0.0225, 1.0),
                EmissiveColor = PhongMaterials.ToColor(0.0, 0.0, 0.0, 1.0),
                SpecularShininess = 12.8f,
            };

            SelectedMaterial = new PhongMaterial
            {
                Name = "White",
                AmbientColor = PhongMaterials.ToColor(0.1, 0.1, 0.1, 1.0),
                DiffuseColor = defaultSelectionColor,
                SpecularColor = PhongMaterials.ToColor(0.0225, 0.0225, 0.0225, 1.0),
                EmissiveColor = PhongMaterials.ToColor(0.0, 0.0, 0.0, 1.0),
                SpecularShininess = 12.8f,
            };

            Model1Transform = new TranslateTransform3D(0, -0, 0);

            // camera setup
            Camera = new PerspectiveCamera();

            SetCameraData(new CameraData());

            DrawGrid();
        }

        /// <summary>
        /// Initialize the Helix with these values. These values should be attached before the 
        /// visualization starts. Deleting them and attaching them does not make any effect on helix.         
        /// So they are initialized before the process starts.
        /// </summary>
        private void InitializeHelix()
        {
            if (Model3DDictionary == null)
            {
                throw new Exception("Helix could not be initialized.");
            }

            directionalLight = new DirectionalLight3D
            {
                Color = directionalLightColor,
                Direction = directionalLightDirection,
                Name = DefaultLightName
            };

            if (!Model3DDictionary.ContainsKey(DefaultLightName))
            {
                Model3DDictionary.Add(DefaultLightName, directionalLight);
            }

            var gridModel3D = new LineGeometryModel3D
            {
                Geometry = Grid,
                Transform = Model1Transform,
                Color = Color.White,
                Thickness = 0.3,
                IsHitTestVisible = false,
                Name = DefaultGridName
            };

            if (!Model3DDictionary.ContainsKey(DefaultGridName))
            {
                Model3DDictionary.Add(DefaultGridName, gridModel3D);
            }

            var axesModel3D = new LineGeometryModel3D
            {
                Geometry = Axes,
                Transform = Model1Transform,
                Color = Color.White,
                Thickness = 0.3,
                IsHitTestVisible = false,
                Name = DefaultAxesName
            };

            if (!Model3DDictionary.ContainsKey(DefaultAxesName))
            {
                Model3DDictionary.Add(DefaultAxesName, axesModel3D);
            }

            AttachAllGeometryModel3DToRenderHost();
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

        public void SetCameraData(CameraData data)
        {
            Camera.LookDirection = data.LookDirection;
            Camera.Position = data.EyePosition;
            Camera.UpDirection = data.UpDirection;
            Camera.NearPlaneDistance = data.NearPlaneDistance;
            Camera.FarPlaneDistance = data.FarPlaneDistance;
        }

        private double CalculateNearClipPlane(double maxDim)
        {
            return maxDim * NearPlaneDistanceFactor;
        }

        private void RemoveGeometryForUpdatedPackages(IEnumerable<IRenderPackage> packages)
        {
            lock (Model3DDictionaryMutex)
            {
                var packageDescrips = packages.Select(p => p.Description.Split(':')[0]).Distinct();

                foreach (var id in packageDescrips)
                {
                    DeleteGeometryForIdentifier(id, false);
                }
            }
        }

        private void AggregateRenderPackages(IEnumerable<HelixRenderPackage> pacakges)
        {
            lock (Model3DDictionaryMutex)
            {
                foreach (var rp in pacakges)
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
                        id = baseId + PointsKey;

                        PointGeometryModel3D pointGeometry3D;

                        if (Model3DDictionary.ContainsKey(id))
                        {
                            pointGeometry3D = Model3DDictionary[id] as PointGeometryModel3D;
                        }
                        else
                        {
                            pointGeometry3D = CreatePointGeometryModel3D(rp);
                            Model3DDictionary.Add(id, pointGeometry3D);
                        }

                        var points = pointGeometry3D.Geometry as PointGeometry3D;
                        var startIdx = points.Positions.Count;

                        points.Positions.AddRange(p.Positions);
                        points.Colors.AddRange(p.Colors.Any()
                            ? p.Colors
                            : Enumerable.Repeat(defaultPointColor, points.Positions.Count));
                        points.Indices.AddRange(p.Indices.Select(i => i + startIdx));

                        if (rp.DisplayLabels)
                        {
                            CreateOrUpdateText(baseId, p.Positions[0], rp);
                        }

                        pointGeometry3D.Geometry = points;
                        pointGeometry3D.Name = baseId;
                    }

                    var l = rp.Lines;
                    if (l.Positions.Any())
                    {
                        id = baseId + LinesKey;

                        LineGeometryModel3D lineGeometry3D;

                        if (Model3DDictionary.ContainsKey(id))
                        {
                            lineGeometry3D = Model3DDictionary[id] as LineGeometryModel3D;
                        }
                        else
                        {
                            lineGeometry3D = CreateLineGeometryModel3D(rp);
                            Model3DDictionary.Add(id, lineGeometry3D);
                        }

                        var lineSet = lineGeometry3D.Geometry as LineGeometry3D;
                        var startIdx = lineSet.Positions.Count;

                        lineSet.Positions.AddRange(l.Positions);
                        lineSet.Colors.AddRange(l.Colors.Any()
                            ? l.Colors
                            : Enumerable.Repeat(defaultLineColor, l.Positions.Count));
                        lineSet.Indices.AddRange(l.Indices.Any()
                            ? l.Indices.Select(i => i + startIdx)
                            : Enumerable.Range(startIdx, startIdx + l.Positions.Count));

                        if (rp.DisplayLabels)
                        {
                            var pt = lineSet.Positions[startIdx];
                            CreateOrUpdateText(baseId, pt, rp);
                        }

                        lineGeometry3D.Geometry = lineSet;
                        lineGeometry3D.Name = baseId;
                    }

                    var m = rp.Mesh;
                    if (!m.Positions.Any()) continue;

                    id = ((rp.RequiresPerVertexColoration || rp.Colors != null) ? rp.Description : baseId) + MeshKey;

                    DynamoGeometryModel3D meshGeometry3D;

                    if (Model3DDictionary.ContainsKey(id))
                    {
                        meshGeometry3D = Model3DDictionary[id] as DynamoGeometryModel3D;
                    }
                    else
                    {
                        meshGeometry3D = CreateDynamoGeometryModel3D(rp);
                        Model3DDictionary.Add(id, meshGeometry3D);
                    }

                    var mesh = meshGeometry3D.Geometry == null
                        ? HelixRenderPackage.InitMeshGeometry()
                        : meshGeometry3D.Geometry as MeshGeometry3D;
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
                        CreateOrUpdateText(baseId, pt, rp);
                    }

                    meshGeometry3D.Geometry = mesh;
                    meshGeometry3D.Name = baseId;
                }

                AttachAllGeometryModel3DToRenderHost();
            }
        }

        private void CreateOrUpdateText(string baseId, Vector3 pt, IRenderPackage rp)
        {
            var textId = baseId + TextKey;
            BillboardTextModel3D bbText;
            if (Model3DDictionary.ContainsKey(textId))
            {
                bbText = Model3DDictionary[textId] as BillboardTextModel3D;
            }
            else
            {
                bbText = new BillboardTextModel3D()
                {
                    Geometry = HelixRenderPackage.InitText3D(),
                };
                Model3DDictionary.Add(textId, bbText);
            }
            var geom = bbText.Geometry as BillboardText3D;
            geom.TextInfo.Add(new TextInfo(HelixRenderPackage.CleanTag(rp.Description),
                new Vector3(pt.X + 0.025f, pt.Y + 0.025f, pt.Z + 0.025f)));
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
                        DiffuseColor = defaultMaterialColor,
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
            ((MaterialGeometryModel3D)meshGeometry3D).SelectionColor = defaultSelectionColor;

            return meshGeometry3D;
        }

        private LineGeometryModel3D CreateLineGeometryModel3D(HelixRenderPackage rp)
        {
            var lineGeometry3D = new LineGeometryModel3D()
            {
                Geometry = HelixRenderPackage.InitLineGeometry(),
                Transform = Model1Transform,
                Color = Color.White,
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
                Color = Color.White,
                Figure = PointGeometryModel3D.PointFigure.Ellipse,
                Size = defaultPointSize,
                IsHitTestVisible = true,
                IsSelected = rp.IsSelected
            };
            return pointGeometry3D;
        }

        private void AttachAllGeometryModel3DToRenderHost()
        {
            foreach (var model3D in Model3DDictionary.Select(kvp => kvp.Value))
            {
                OnRequestAttachToScene(model3D);
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
        /// This method attempts to maximize the near clip plane in order to 
        /// achiever higher z-buffer precision.
        /// </summary>
        internal void UpdateNearClipPlaneForSceneBounds(Rect3D sceneBounds)
        {
            // http: //www.sjbaker.org/steve/omniv/love_your_z_buffer.html
            var maxDim = Math.Max(Math.Max(sceneBounds.SizeX, sceneBounds.Y), sceneBounds.SizeZ);
            Camera.NearPlaneDistance = Math.Max(CalculateNearClipPlane(maxDim), 0.1);
        }

        internal override void ExportToSTL(string path, string modelName)
        {
            var geoms = SceneItems.Where(i => i is DynamoGeometryModel3D).
                Cast<DynamoGeometryModel3D>();

            using (TextWriter tw = new StreamWriter(path))
            {
                tw.WriteLine("solid {0}", model.CurrentWorkspace.Name);
                foreach (var g in geoms)
                {
                    var n = ((MeshGeometry3D) g.Geometry).Normals.ToList();
                    var t = ((MeshGeometry3D)g.Geometry).Triangles.ToList();

                    for (var i = 0; i < t.Count(); i ++)
                    {
                        var nCount = i*3;
                        tw.WriteLine("\tfacet normal {0} {1} {2}", n[nCount].X, n[nCount].Y, n[nCount].Z);
                        tw.WriteLine("\t\touter loop");
                        tw.WriteLine("\t\t\tvertex {0} {1} {2}", t[i].P0.X, t[i].P0.Y, t[i].P0.Z);
                        tw.WriteLine("\t\t\tvertex {0} {1} {2}", t[i].P1.X, t[i].P1.Y, t[i].P1.Z);
                        tw.WriteLine("\t\t\tvertex {0} {1} {2}", t[i].P2.X, t[i].P2.Y, t[i].P2.Z);
                        tw.WriteLine("\t\tendloop");
                        tw.WriteLine("\tendfacet");
                    }
                }
                tw.WriteLine("endsolid {0}", model.CurrentWorkspace.Name);
            }
        }

        #endregion

        #region command methods

        internal override void TogglePan(object parameter)
        {
            CurrentSpaceViewModel.RequestTogglePanMode();

            // Since panning and orbiting modes are exclusive from one another,
            // turning one on may turn the other off. This is the reason we must
            // raise property change for both at the same time to update visual.
            RaisePropertyChanged("IsPanning");
            RaisePropertyChanged("IsOrbiting");
            RaisePropertyChanged("LeftClickCommand");
        }

        private static bool CanTogglePan(object parameter)
        {
            return true;
        }

        private void ToggleOrbit(object parameter)
        {
            CurrentSpaceViewModel.RequestToggleOrbitMode();

            // Since panning and orbiting modes are exclusive from one another,
            // turning one on may turn the other off. This is the reason we must
            // raise property change for both at the same time to update visual.
            RaisePropertyChanged("IsPanning");
            RaisePropertyChanged("IsOrbiting");
            RaisePropertyChanged("LeftClickCommand");
        }

        private static bool CanToggleOrbit(object parameter)
        {
            return true;
        }

        public void ToggleCanNavigateBackground(object parameter)
        {
            if (!Active)
                return;

            CanNavigateBackground = !CanNavigateBackground;

            InstrumentationLogger.LogAnonymousScreen(CanNavigateBackground ? "Geometry" : "Nodes");
        }

        internal bool CanToggleCanNavigateBackground(object parameter)
        {
            return true;
        }

        #endregion
    }

    internal static class CameraExtensions
    {
        public static CameraData ToCameraData(this PerspectiveCamera camera, string name)
        {
            var camData = new CameraData
            {
                Name = name,
                LookDirection = camera.LookDirection,
                EyePosition = camera.Position,
                UpDirection = camera.UpDirection,
                NearPlaneDistance = camera.NearPlaneDistance,
                FarPlaneDistance = camera.FarPlaneDistance
            };

            return camData;
        }
    }
}
