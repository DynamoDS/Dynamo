using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Xml;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Visualization;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.Rendering;
using DynamoUtilities;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Shaders;
using Newtonsoft.Json;
using SharpDX;
using Color = SharpDX.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;
using MeshBuilder = HelixToolkit.Wpf.SharpDX.MeshBuilder;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;
using TextInfo = HelixToolkit.Wpf.SharpDX.TextInfo;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    public class CameraData
    { 
        // Default camera position data. These values have been rounded
        // to the nearest whole value.
        // eyeX="-16.9655136013663" eyeY="24.341577725171" eyeZ="50.6494323150915" 
        // lookX="12.4441040333119" lookY="-13.0110656299395" lookZ="-58.5449065206009" 
        // upX="-0.0812375695793365" upY="0.920504853452448" upZ="0.3821927158638" />

        private readonly Vector3D defaultCameraLookDirection = new Vector3D(12, -13, -58);
        private readonly Point3D defaultCameraPosition = new Point3D(-17, 24, 50);
        private readonly Vector3D defaultCameraUpDirection = new Vector3D(0, 1, 0);
        private const double defaultNearPlaneDistance = 0.1;
        private const double defaultFarPlaneDistance = 10000000;

        [JsonIgnore]
        public Point3D EyePosition { get { return new Point3D(EyeX, EyeY, EyeZ); } }
        [JsonIgnore]
        public Vector3D UpDirection { get { return new Vector3D(UpX, UpY, UpZ); } }
        [JsonIgnore]
        public Vector3D LookDirection { get { return new Vector3D(LookX, LookY, LookZ); } }
        [JsonIgnore]
        public double NearPlaneDistance { get; set; }
        [JsonIgnore]
        public double FarPlaneDistance { get; set; }

        // JSON camera data
        public string Name { get; set; }
        public double EyeX { get; set; }
        public double EyeY { get; set; }
        public double EyeZ { get; set; }
        public double LookX { get; set; }
        public double LookY { get; set; }
        public double LookZ { get; set; }
        public double UpX { get; set; }
        public double UpY { get; set; }
        public double UpZ { get; set; }

        public CameraData()
        {
            NearPlaneDistance = defaultNearPlaneDistance;
            FarPlaneDistance = defaultFarPlaneDistance;

            Name = "Default Camera";
            EyeX = defaultCameraPosition.X;
            EyeY = defaultCameraPosition.Y;
            EyeZ = defaultCameraPosition.Z;
            LookX = defaultCameraLookDirection.X;
            LookY = defaultCameraLookDirection.Y;
            LookZ = defaultCameraLookDirection.Z;
            UpX = defaultCameraUpDirection.X;
            UpY = defaultCameraUpDirection.Y;
            UpZ = defaultCameraUpDirection.Z;
        }
        
        public override bool Equals(object obj)
        {
            var other = obj as CameraData;
            return obj is CameraData && this.Name == other.Name
                   && Math.Abs(this.EyeX - other.EyeX) < 0.0001
                   && Math.Abs(this.EyeY - other.EyeY) < 0.0001
                   && Math.Abs(this.EyeZ - other.EyeZ) < 0.0001
                   && Math.Abs(this.LookX - other.LookX) < 0.0001
                   && Math.Abs(this.LookY - other.LookY) < 0.0001
                   && Math.Abs(this.LookZ - other.LookZ) < 0.0001
                   && Math.Abs(this.UpX - other.UpX) < 0.0001
                   && Math.Abs(this.UpY - other.UpY) < 0.0001
                   && Math.Abs(this.UpZ - other.UpZ) < 0.0001
                   && Math.Abs(this.NearPlaneDistance - other.NearPlaneDistance) < 0.0001
                   && Math.Abs(this.FarPlaneDistance - other.FarPlaneDistance) < 0.0001;
        }

        /// <summary>
        /// returns true if any of components are NaN.
        /// </summary>
        /// <returns></returns>
        internal bool containsNaN()
        {
            var numericComponents = new List<double>(){ EyeX, EyeY, EyeZ, LookX, LookY, LookZ, UpX, UpY, UpZ, NearPlaneDistance, FarPlaneDistance };
            if (numericComponents.Any(d => (Double.IsNaN(d))))
            {
                return true;
            }
            return false;
        }


    }

    /// <summary>
    /// The HelixWatch3DViewModel establishes a full rendering 
    /// context using the HelixToolkit. An instance of this class
    /// can act as the data source for a <see cref="Watch3DView"/>
    /// </summary>
    [Obsolete("Do not use! This will be moved to a new project in a future version of Dynamo.")]
    public class HelixWatch3DViewModel : DefaultWatch3DViewModel
    {
        #region private members

        private double lightAzimuthDegrees = 45.0;
        private double lightElevationDegrees = 35.0;
        private DynamoLineGeometryModel3D gridModel3D;
        private LineGeometry3D worldGrid;
        private LineGeometry3D worldAxes;
        private Technique renderTechnique;
        private PerspectiveCamera camera;
        private DirectionalLight3D headLight;

        private readonly Color4 defaultSelectionColor = new Color4(new Color3(0, 158.0f / 255.0f, 1.0f));
        private readonly Color4 defaultMaterialColor = new Color4(new Color3(1.0f, 1.0f, 1.0f));
        private readonly Color4 defaultTransparencyColor = new Color4(1.0f, 1.0f, 1.0f, 0.5f);
        private readonly Color4 meshIsolatedTransparencyColor = new Color4(1.0f, 1.0f, 1.0f, 0.1f);
        internal static readonly Color4 ptAndLineIsolatedTransparencyColor = new Color4(1.0f, 1.0f, 1.0f, 0.25f);

        private readonly Size defaultPointSize = new Size(6, 6);
        private readonly Size highlightSize = new Size(8, 8);
        private readonly Color4 highlightColor = new Color4(new Color3(1.0f, 0.0f, 0.0f));

        private static readonly Color4 defaultLineColor = new Color4(new Color3(0, 0, 0));
        private static readonly Color4 defaultPointColor = new Color4(new Color3(0, 0, 0));
        private static readonly Color4 defaultDeadColor = new Color4(new Color3(0.7f, 0.7f, 0.7f));
        private static readonly float defaultDeadAlphaScale = 0.2f;
        private const float defaultLabelOffset = 0.025f;

        internal const string DefaultGridName = "Grid";
        internal const string DefaultAxesName = "Axes";
        internal const string DefaultLightName = "DirectionalLight";
        internal const string HeadLightName = "HeadLight";

        private const string PointsKey = ":points";
        private const string LinesKey = ":lines";
        private const string MeshKey = ":mesh";
        private const string TextKey = ":text";

        private const int FrameUpdateSkipCount = 200;
        private int currentFrameSkipCount;

        private const double EqualityTolerance = 0.000001;
        private double nearPlaneDistanceFactor = 0.001;
        internal const double DefaultNearClipDistance = 0.1f;
        internal const double DefaultFarClipDistance = 100000;
        internal static BoundingBox DefaultBounds = new BoundingBox(new Vector3(-25f, -25f, -25f), new Vector3(25f,25f,25f));

        private ObservableElement3DCollection sceneItems;

        private Dictionary<string, string> nodesSelected = new Dictionary<string, string>();

        private readonly Object element3DDictionaryMutex = new object();

        private Dictionary<string, Element3D> element3DDictionary = new Dictionary<string, Element3D>();

        //internal only for testing.
        //used to cache point and curve color collections so colors can be efficiently switched between selected and non selected. (freeze, isolate etc)
        internal Dictionary<string, Color4Collection> colorCache = new Dictionary<string, Color4Collection>();
        // Dictionary<nodeId, List<Tuple<nodeArrayItemId, labelPosition>>>
        private readonly Dictionary<string, List<Tuple<string, Vector3>>> labelPlaces
            = new Dictionary<string, List<Tuple<string, Vector3>>>();



#if DEBUG
        private readonly Stopwatch renderTimer = new Stopwatch();
#endif

        #endregion

        #region events
        public event Action RequestViewRefresh;
        protected void OnRequestViewRefresh()
        {
            if (RequestViewRefresh != null)
            {
                RequestViewRefresh();
            }
        }

        protected override void OnActiveStateChanged()
        {
            if (!active && CanNavigateBackground)
            {
                CanNavigateBackground = false;
            }

            RaisePropertyChanged("IsGridVisible");
        }

        /// <summary>
        /// An event requesting to create geometries from render packages.
        /// </summary>
        public event Action<RenderPackageCache, bool> RequestCreateModels;
        private void OnRequestCreateModels(RenderPackageCache packages, bool forceAsyncCall = false)
        {
            if (RequestCreateModels != null)
            {
                RequestCreateModels(packages, forceAsyncCall);
            }
        }

        /// <summary>
        /// An event requesting to remove geometries generated by the node.
        /// </summary>
        public event Action<NodeModel> RequestRemoveModels;
        private void OnRequestRemoveModels(NodeModel node)
        {
            if (RequestRemoveModels != null)
            {
                RequestRemoveModels(node);
            }
        }

        /// <summary>
        /// An event requesting a zoom to fit operation around the provided bounds.
        /// </summary>
        public event Action<BoundingBox> RequestZoomToFit;
        protected void OnRequestZoomToFit(BoundingBox bounds)
        {
            if(RequestZoomToFit != null)
            {
                RequestZoomToFit(bounds);
            }
        }
        #endregion

        #region properties

        internal static Color4 DefaultLineColor
        {
            get { return defaultLineColor; }
        }

        internal static Color4 DefaultPointColor
        {
            get { return defaultPointColor; }
        }

        internal static Color4 DefaultDeadColor
        {
            get { return defaultDeadColor; }
        }

        internal Dictionary<string, Element3D> Element3DDictionary
        {
            get
            {
                lock (element3DDictionaryMutex)
                {
                    return element3DDictionary;
                }
            }

            set
            {
                lock (element3DDictionaryMutex)
                {
                    element3DDictionary = value;
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

        public static PhongMaterial WhiteMaterial { get; set; }

        public static PhongMaterial SelectedMaterial { get; set; }

        internal static PhongMaterial FrozenMaterial { get; set; }
        internal static PhongMaterial IsolatedMaterial { get; set; }

        /// <summary>
        /// This is the initial transform applied to 
        /// elements of the scene, like the grid and world axes.
        /// </summary>
        public Transform3D SceneTransform
        {
            get
            {
                return new TranslateTransform3D(0, -0, 0);
            }
        }
        public Technique RenderTechnique
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

        public override bool IsGridVisible
        {
            get { return isGridVisible; }
            set
            {
                if (isGridVisible == value) return;

                base.IsGridVisible = value;
                SetGridVisibility();
            }
        }

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

        private void UpdateSceneItems()
        {
            if (Element3DDictionary == null)
            {
                sceneItems = new ObservableElement3DCollection();
                return;
            }

            var values = Element3DDictionary.Values.ToList();
            if (Camera != null)
            {
                values.Sort(new Element3DComparer(Camera.Position));
            }
            if (sceneItems == null)
                sceneItems = new ObservableElement3DCollection();

            //perform these updates here instead of everytime we add a single render package.
            foreach(var geoModel3d in values)
            {
                if(geoModel3d is HelixToolkit.Wpf.SharpDX.GeometryModel3D)
                {
                    var geo = (geoModel3d as HelixToolkit.Wpf.SharpDX.GeometryModel3D).Geometry;

                    geo.UpdateVertices();
                    geo.UpdateColors();
                    geo.UpdateBounds();
                    geo.UpdateTriangles();
                }
            }

            sceneItems.Clear();
            sceneItems.AddRange(values);
        }

        public IEnumerable<Element3D> SceneItems
        {
            get
            {
                if (sceneItems == null)
                {
                    UpdateSceneItems();
                }

                return sceneItems;
            }
        }

        public IEffectsManager EffectsManager { get; private set; }

        [Obsolete("Not Implemented - Do Not Use.")]
        public bool SupportDeferredRender { get; private set; }

        #endregion

        /// <summary>
        /// Attempt to create a HelixWatch3DViewModel. If one cannot be created,
        /// fall back to creating a DefaultWatch3DViewModel and log the exception.
        /// </summary>
        /// <param name="model">The NodeModel to associate with the returned view model.</param>
        /// <param name="parameters">A Watch3DViewModelStartupParams object.</param>
        /// <param name="logger">A logger to be used to log the exception.</param>
        /// <returns></returns>
        public static DefaultWatch3DViewModel TryCreateHelixWatch3DViewModel(NodeModel model, Watch3DViewModelStartupParams parameters, DynamoLogger logger)
        {
            try
            {
                var vm = new HelixWatch3DViewModel(model, parameters);
                return vm;
            }
            catch (Exception ex)
            {
                logger.Log(Resources.BackgroundPreviewCreationFailureMessage, LogLevel.Console);
                logger.Log(ex.Message, LogLevel.File);

                var vm = new DefaultWatch3DViewModel(model, parameters)
                {
                    Active = false,
                    CanBeActivated = false
                };
                return vm;
            }
        }

        protected internal virtual void UpdateUpstream()
        {
        }

        // This method is about indicating whether watch3d updated its stream or not.
        protected internal virtual void OnWatchExecution()
        {
        }

        protected HelixWatch3DViewModel(NodeModel model, Watch3DViewModelStartupParams parameters) 
        : base(model, parameters)
        {
            Name = Resources.BackgroundPreviewName;
            IsResizable = false;
            EffectsManager = new DynamoEffectsManager();

            SetupScene();
            InitializeHelix();
        }

        public void SerializeCamera(XmlElement camerasElement)
        {
            if (camera == null) return;

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
                    EyeX = ex,
                    EyeY = ey,
                    EyeZ = ez,
                    LookX = lx,
                    LookY = ly,
                    LookZ = lz,
                    UpX = ux,
                    UpY = uy,
                    UpZ = uz
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

        public override void RemoveGeometryForNode(NodeModel node)
        {
            if (Active)
            {
                // Raise request for model objects to be deleted on the UI thread.
                OnRequestRemoveModels(node);
            }
        }

        public override void AddGeometryForRenderPackages(RenderPackageCache packages, bool forceAsyncCall = false)
        {
            if (Active)
            {
                // Raise request for model objects to be created on the UI thread.
                OnRequestCreateModels(packages, forceAsyncCall);
            }
        }

        protected override void OnClear()
        {
            lock (element3DDictionaryMutex)
            {
                var keysList = new List<string> { DefaultLightName, HeadLightName, DefaultGridName, DefaultAxesName };


                foreach (var key in Element3DDictionary.Keys.Except(keysList).ToList())
                {
                    var model = Element3DDictionary[key] as GeometryModel3D;
                    Element3DDictionary.Remove(key);
                    colorCache.Remove(key);

                    model.Dispose();
                }

                labelPlaces.Clear();
                nodesSelected = new Dictionary<string, string>();
            }

            OnSceneItemsChanged();
        }

        protected override void OnWorkspaceCleared(WorkspaceModel workspace)
        {
            SetCameraData(new CameraData());
            base.OnWorkspaceCleared(workspace);
        }

        protected override void OnWorkspaceOpening(object obj)
        {
            XmlDocument doc = obj as XmlDocument;
            if (doc != null)
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
                        //check if the cameraData contains any NaNs
                        if (camData.containsNaN())
                        {
                            SetCameraData(new CameraData());
                        }
                        else
                        {
                            SetCameraData(camData);
                        }
                      
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                        logger.Log(ex);
                    }
                }

                return;
            }

            ExtraWorkspaceViewInfo workspaceViewInfo = obj as ExtraWorkspaceViewInfo;
            if (workspaceViewInfo != null)
            {
                var cameraJson = workspaceViewInfo.Camera.ToString();

                var settings = new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        args.ErrorContext.Handled = true;
                        Console.WriteLine(args.ErrorContext.Error);
                    },
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    Culture = CultureInfo.InvariantCulture
                };

                var cameraData = JsonConvert.DeserializeObject<CameraData>(cameraJson, settings);

                //check if the cameraData contains any NaNs
                if (cameraData.containsNaN())
                {
                    SetCameraData(new CameraData());
                }
                else
                {
                    SetCameraData(cameraData);
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

                    // When deselecting (by clicking on the canvas), all the highlighted HelixWatch3D previews are switched off
                    // This results in a scenario whereby the list item in the WatchTree/PreviewBubble is still selected, and its
                    // labels are still displayed in the preview, but the highlighting has been switched off.
                    // In order to prevent this unintuitive UX behavior, the nodes will first be checked if they are in the 
                    // nodesSelected dictionary - if they are, they will not be switched off.
                    var geometryModels = new Dictionary<string, Element3D>();
                    foreach (var model in Element3DDictionary)
                    {
                        var nodePath = model.Key.Contains(':') ? model.Key.Remove(model.Key.IndexOf(':')) : model.Key;
                        if (model.Value is GeometryModel3D && !nodesSelected.ContainsKey(nodePath))
                        {
                            geometryModels.Add(model.Key, model.Value);
                        }
                    }

                    foreach (var geometryModel in geometryModels)
                    {
                        AttachedProperties.SetShowSelected(geometryModel.Value, false);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    SetSelection(e.OldItems, false);
                    break;

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
                        break;
                    }

                    SetSelection(e.NewItems, true);
                    break;
            }

            if (IsolationMode)
            {
                OnIsolationModeRequestUpdate();
            }
        }

        protected override void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = sender as NodeModel;
            if (node == null)
            {
                return;
            }
            node.WasRenderPackageUpdatedAfterExecution = false;

            switch (e.PropertyName)
            {
                case "CachedValue":
                    Debug.WriteLine(string.Format("Requesting render packages for {0}", node.GUID));
                    node.RequestVisualUpdateAsync(scheduler, engineManager.EngineController, renderPackageFactory);
                    break;

                case "DisplayLabels":
                    node.RequestVisualUpdateAsync(scheduler, engineManager.EngineController, renderPackageFactory, true);
                    break;

                case "IsVisible":
                    var geoms = FindAllGeometryModel3DsForNode(node);
                    foreach(var g in geoms)
                    {
                        g.Value.Visibility = node.IsVisible ? Visibility.Visible : Visibility.Hidden;
                        //RaisePropertyChanged("SceneItems");
                    }

                    node.RequestVisualUpdateAsync(scheduler, engineManager.EngineController, renderPackageFactory, true);
                    break;

                case "IsFrozen":
                    HashSet<NodeModel> gathered = new HashSet<NodeModel>();
                    node.GetDownstreamNodes(node, gathered);
                    SetGeometryFrozen(gathered);
                    break;
            }
        }

        public override void GenerateViewGeometryFromRenderPackagesAndRequestUpdate(RenderPackageCache taskPackages)
        {
            /*foreach (var p in taskPackages)
            {
                Debug.WriteLine(string.Format("Processing render packages for {0}", p.Description));
            }
            */
            recentlyAddedNodes.Clear();

#if DEBUG
            renderTimer.Start();
#endif
            var packages = taskPackages.Packages;
            var meshPackages = packages.Cast<HelixRenderPackage>().Where(rp => rp.MeshVertexCount % 3 == 0);

            RemoveGeometryForUpdatedPackages(meshPackages);
            try
            {
                AggregateRenderPackages(meshPackages);
            }
            catch (OutOfMemoryException)
            {
                // This can happen when the amount of packages to render is too large
                string summary = Resources.RenderingMemoryOutageSummary;
                var description = Resources.RenderingMemoryOutageDescription;
                (dynamoModel as DynamoModel).Report3DPreviewOutage(summary, description);
            }
#if DEBUG
            // Defer stopping the timer until after the rendering has occurred
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                renderTimer.Stop();
                Debug.WriteLine(string.Format("RENDER: {0} ellapsed time rendering.", renderTimer.Elapsed));
                renderTimer.Restart();
            }));
#endif

            OnSceneItemsChanged();
        }

        private void DeleteGeometries(KeyValuePair<string, Element3D>[] geometryModels, bool requestUpdate)
        {
            lock (element3DDictionaryMutex)
            {
                if (!geometryModels.Any())
                {
                    return;
                }

                foreach (var kvp in geometryModels)
                {
                    
                    var model3D = Element3DDictionary[kvp.Key] as GeometryModel3D;
                    // check if the geometry is frozen. if the gemoetry is frozen 
                    // then do not detach from UI.
                    var frozenModel = AttachedProperties.GetIsFrozen(model3D);
                    if (frozenModel) continue;

                    Element3DDictionary.Remove(kvp.Key);
                    colorCache.Remove(kvp.Key);
                    model3D.Dispose();

                    var nodePath = kvp.Key.Split(':')[0];
                    labelPlaces.Remove(nodePath);
                    nodesSelected.Remove(nodePath);
                }
            }

            if (!requestUpdate) return;
            OnSceneItemsChanged();
        }

        /// <summary>
        /// Delete render packages generated by the node.
        /// Note this function should be called from UI thread; otherwise call
        /// RemoveGeometryForNodeAsync().
        /// </summary>
        /// <param name="node"></param>
        /// <param name="requestUpdate"></param>
        public override void DeleteGeometryForNode(NodeModel node, bool requestUpdate = true)
        {
            var geometryModels = FindAllGeometryModel3DsForNode(node);
            DeleteGeometries(geometryModels, requestUpdate);
        }

        /// <summary>
        /// Delete render packages associated with the specified identifier.
        /// Note this function should be called from UI thread.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="requestUpdate"></param>
        public override void DeleteGeometryForIdentifier(string identifier, bool requestUpdate = true)
        {
            var geometryModels = FindAllGeometryModel3DsForNode(identifier);
            DeleteGeometries(geometryModels, requestUpdate); 
        }

        protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentWorkspace":
                    OnClear();

                    IEnumerable<NodeModel> nodesToRender = null;

                    // Get the nodes to render from the current home workspace. For custom
                    // nodes, this will get the workspace in which the custom node is placed.
                    // This will need to be adapted when multiple home workspaces are supported,
                    // so that a specific workspace can be selected to act as the preview context.

                    var hs = dynamoModel.Workspaces.FirstOrDefault(i => i is HomeWorkspaceModel);
                    if (hs != null)
                    {
                        nodesToRender = hs.Nodes;
                    }

                    if (nodesToRender == null)
                    {
                        return;
                    }

                    foreach (var node in nodesToRender)
                    {
                        node.RequestVisualUpdateAsync(scheduler, engineManager.EngineController, renderPackageFactory, true);
                    }

                    break;
            }
        }

        /// <summary>
        /// Update the attached properties and recalculate transparency sorting
        /// after any update under Isolate Selected Geometry mode.
        /// </summary>
        protected override void OnIsolationModeRequestUpdate()
        {
            var values = Element3DDictionary.Values.ToList();

            var geometries = values.OfType<GeometryModel3D>().ToList();
            geometries.ForEach(g => AttachedProperties.SetIsolationMode(g, IsolationMode));

            if (Camera != null)
            {
                values.Sort(new Element3DComparer(Camera.Position));
            }

            if (sceneItems == null)
                sceneItems = new ObservableElement3DCollection();

            sceneItems.Clear();
            sceneItems.AddRange(values);
        }

        protected override void ZoomToFit(object parameter)
        {
            var idents = FindIdentifiersForContext();
            var geoms = SceneItems.Where(item => item is GeometryModel3D).Cast<GeometryModel3D>();
            var targetGeoms = FindGeometryForIdentifiers(geoms, idents);
            var selectionBounds = ComputeBoundsForGeometry(targetGeoms.ToArray());

            // Don't zoom if there is no valid bounds.
            if (selectionBounds.Equals(new BoundingBox())) return;

            OnRequestZoomToFit(selectionBounds);
        }

        public override CameraData GetCameraInformation()
        {
            return camera.ToCameraData(Name);
        }

        /// <summary>
        /// Finds all output identifiers based on the context.
        /// 
        /// Ex. If there are nodes selected, returns all identifiers for outputs
        /// on the selected nodes. If you're in a custom node, returns all identifiers
        /// for the outputs from instances of those custom nodes in the graph. etc.
        /// </summary>
        /// <returns>An <see cref="IEnumerable"/> of <see cref="string"/> containing the output identifiers found in the context.</returns>
        private IEnumerable<string> FindIdentifiersForContext()
        {
            IEnumerable<string> idents = null;

            var hs = dynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            if (hs == null)
            {
                return idents;
            }

            if (InCustomNode())
            {
                idents = FindIdentifiersForCustomNodes(hs);
            }
            else
            {
                if (DynamoSelection.Instance.Selection.Any())
                {
                    var selNodes = DynamoSelection.Instance.Selection.Where(s => s is NodeModel).Cast<NodeModel>().ToArray();
                    idents = FindIdentifiersForSelectedNodes(selNodes);
                }
                else
                {
                    idents = AllOutputIdentifiersInWorkspace(hs);
                }
            }

            return idents;
        } 

        protected override bool CanToggleCanNavigateBackground(object parameter)
        {
            return true;
        }

        #region internal methods

        internal void ComputeFrameUpdate()
        {
            // Raising a property change notification for
            // the SceneItems collections causes a full
            // re-render including sorting for transparency.
            // We don't want to do this every frame, so we
            // do this update only at a fixed interval.
            //if (currentFrameSkipCount == FrameUpdateSkipCount)
            //{
            //    RaisePropertyChanged("SceneItems");
            //    currentFrameSkipCount = 0;
            //}

            currentFrameSkipCount++;
        }

        #endregion

        #region private methods

        private void OnSceneItemsChanged()
        {
            UpdateSceneItems();
            RaisePropertyChanged("SceneItems");
            OnRequestViewRefresh();
        }
   
        private KeyValuePair<string, Element3D>[] FindAllGeometryModel3DsForNode(NodeModel node)
        {
            KeyValuePair<string, Element3D>[] geometryModels;

            lock (element3DDictionaryMutex)
            {
                geometryModels = Element3DDictionary
                        .Where(x => x.Key.Contains(node.AstIdentifierGuid) && x.Value != null).ToArray();
            }

            return geometryModels;
        }

        private KeyValuePair<string, Element3D>[] FindAllGeometryModel3DsForNode(string identifier)
        {
            KeyValuePair<string, Element3D>[] geometryModels;

            lock (element3DDictionaryMutex)
            {
                geometryModels = Element3DDictionary
                        .Where(x => x.Key.Contains(identifier) && x.Value is GeometryModel3D).ToArray();
            }

            return geometryModels;
        }

        private void SetGeometryFrozen(HashSet<NodeModel> gathered)
        {
            
            foreach (var node in gathered)
            {
                var geometryModels = FindAllGeometryModel3DsForNode(node);

                if (!geometryModels.Any())
                {
                    continue;
                }

                var modelValues = geometryModels.Select(x => x.Value);

                foreach (GeometryModel3D g in modelValues)
                {
                    AttachedProperties.SetIsFrozen(g, node.IsFrozen);
                }
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

                foreach (var model in geometryModels)
                {
                    AttachedProperties.SetShowSelected(model.Value, isSelected);
                }
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
                Name = "Selected",
                AmbientColor = PhongMaterials.ToColor(0.1, 0.1, 0.1, 1.0),
                DiffuseColor = defaultSelectionColor,
                SpecularColor = PhongMaterials.ToColor(0.0225, 0.0225, 0.0225, 1.0),
                EmissiveColor = PhongMaterials.ToColor(0.0, 0.0, 0.0, 1.0),
                SpecularShininess = 12.8f,
            };

            FrozenMaterial = new PhongMaterial
            {
                Name = "Frozen",
                AmbientColor = PhongMaterials.ToColor(0.1, 0.1, 0.1, 1.0),
                DiffuseColor = defaultTransparencyColor,
                SpecularColor = PhongMaterials.ToColor(0.0225, 0.0225, 0.0225, 1.0),
                EmissiveColor = PhongMaterials.ToColor(0.0, 0.0, 0.0, 1.0),
                SpecularShininess = 12.8f,
            };

            IsolatedMaterial = new PhongMaterial
            {
                Name = "IsolatedTransparent",
                AmbientColor = PhongMaterials.ToColor(0.1, 0.1, 0.1, 1.0),
                DiffuseColor = meshIsolatedTransparencyColor,
                SpecularColor = PhongMaterials.ToColor(0.0225, 0.0225, 0.0225, 1.0),
                EmissiveColor = PhongMaterials.ToColor(0.0, 0.0, 0.0, 1.0),
                SpecularShininess = 12.8f,
            };

            // camera setup
            Camera = new PerspectiveCamera();
            Camera.FieldOfView = 58.5;
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
            if (Element3DDictionary == null)
            {
                throw new Exception("Helix could not be initialized.");
            }

            // Create the headlight singleton and add it to the dictionary

            headLight = new DirectionalLight3D
            {
                Color = System.Windows.Media.Color.FromRgb(230, 230, 230),
                Direction = new Vector3D(0, 0, 1),
                Name = HeadLightName
            };

            headLight.SetBinding(
                DirectionalLight3D.DirectionProperty,
                new Binding(nameof(PerspectiveCamera.LookDirection))
                {
                    Source = this.Camera
                }
            );

            if (!Element3DDictionary.ContainsKey(HeadLightName))
            {
                AttachedProperties.SetIsSpecialRenderPackage(headLight, true);
                Element3DDictionary.Add(HeadLightName, headLight);
            }

            // Create the grid singleton and add it to the dictionary

            gridModel3D = new DynamoLineGeometryModel3D
            {
                Geometry = Grid,
                Transform = SceneTransform,
                Color = Colors.White,
                Thickness = 0.3,
                IsHitTestVisible = false,
                Name = DefaultGridName
            };

            SetGridVisibility();

            if (!element3DDictionary.ContainsKey(DefaultGridName))
            {
                AttachedProperties.SetIsSpecialRenderPackage(gridModel3D, true);
                Element3DDictionary.Add(DefaultGridName, gridModel3D);
            }

            // Create the axes singleton and add it to the dictionary

            var axesModel3D = new DynamoLineGeometryModel3D
            {
                Geometry = Axes,
                Transform = SceneTransform,
                Color = Colors.White,
                Thickness = 0.3,
                IsHitTestVisible = false,
                Name = DefaultAxesName
            };

            if (!Element3DDictionary.ContainsKey(DefaultAxesName))
            {
                AttachedProperties.SetIsSpecialRenderPackage(axesModel3D, true);
                Element3DDictionary.Add(DefaultAxesName, axesModel3D);
            }

            UpdateSceneItems();
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

        private void SetGridVisibility()
        {
            var visibility = isGridVisible ? Visibility.Visible : Visibility.Hidden;
            //return if there is nothing to change
            if (gridModel3D.Visibility == visibility) return;
            
            gridModel3D.Visibility = visibility;
            OnRequestViewRefresh();
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
            if (Camera == null) return;

            Camera.LookDirection = data.LookDirection;
            Camera.Position = data.EyePosition;
            Camera.UpDirection = data.UpDirection;
            Camera.NearPlaneDistance = data.NearPlaneDistance;
            Camera.FarPlaneDistance = data.FarPlaneDistance;
        }

        private void RemoveGeometryForUpdatedPackages(IEnumerable<IRenderPackage> packages)
        {
            lock (element3DDictionaryMutex)
            {
                var packageDescrips = packages.Select(p => p.Description.Split(':')[0]).Distinct();

                foreach (var id in packageDescrips)
                {
                    DeleteGeometryForIdentifier(id, false);
                }
            }
        }

        private bool InCustomNode()
        {
            return dynamoModel.CurrentWorkspace is CustomNodeWorkspaceModel;
        }

        /// <summary>
        /// Display a label for geometry based on the paths.
        /// </summary>
        public override void AddLabelForPath(string path)
        {
            // make var_guid from var_guid:0:1
            var nodePath = path.Contains(':') ? path.Remove(path.IndexOf(':')) : path;
            var labelName = nodePath + TextKey;
            lock (element3DDictionaryMutex)
            {
                // first, remove current labels of the node
                // it does not crash if there is no such key in dictionary
                var sceneItemsChanged = Element3DDictionary.Remove(labelName);

                // it may be requested an array of items to put labels
                // for example, the first item in 2-dim array - path will look like var_guid:0
                // and it will select var_guid:0:0, var_guid:0:1, var_guid:0:2 and so on.
                // if there is a geometry to add label(s)
                List<Tuple<string, Vector3>> requestedLabelPlaces;
                if (labelPlaces.ContainsKey(nodePath) &&
                    (requestedLabelPlaces = labelPlaces[nodePath]
                        .Where(pair => pair.Item1 == path || pair.Item1.StartsWith(path + ":")).ToList()).Any())
                {
                    // If the nodesSelected Dictionary does not contain the current nodePath as a key,
                    // or if the current value of the nodePath key is not the same as the current path 
                    // (which is currently being selected) then, create new label(s) for the Watch3DView.
                    // Else, remove the label(s) in the Watch 3D View.

                    if (!nodesSelected.ContainsKey(nodePath) || nodesSelected[nodePath] != path)
                    {
                        // second, add requested labels
                        var textGeometry = HelixRenderPackage.InitText3D();
                        var bbText = new BillboardTextModel3D
                        {
                            Geometry = textGeometry
                        };

                        foreach (var id_position in requestedLabelPlaces)
                        {
                            var text = HelixRenderPackage.CleanTag(id_position.Item1);
                            var textPosition = id_position.Item2 + defaultLabelOffset;

                            var textInfo = new TextInfo(text, textPosition);
                            textGeometry.TextInfo.Add(textInfo);
                        }

                        if (nodesSelected.ContainsKey(nodePath))
                        {
                            ToggleTreeViewItemHighlighting(nodesSelected[nodePath], false); // switch off selection for previous path
                        }
                        
                        Element3DDictionary.Add(labelName, bbText);

                        sceneItemsChanged = true;
                        nodesSelected[nodePath] = path;

                        ToggleTreeViewItemHighlighting(path, true); // switch on selection for current path
                    }

                    // if no node is being selected, that means the current node is being unselected
                    // and no node within the parent node is currently selected.
                    else
                    {
                        nodesSelected.Remove(nodePath);
                        ToggleTreeViewItemHighlighting(path, false);
                    }
                }

                if (sceneItemsChanged)
                {
                    OnSceneItemsChanged();
                }
            }
        }

        
        /// <summary>
        /// Remove the labels (in Watch3D View) for geometry once the Watch node is disconnected
        /// </summary>
        /// <param name="path"></param>
        public override void ClearPathLabel(string path)
        {
            var nodePath = path.Contains(':') ? path.Remove(path.IndexOf(':')) : path;
            var labelName = nodePath + TextKey;
            lock (element3DDictionaryMutex)
            {
                var sceneItemsChanged = Element3DDictionary.Remove(labelName);

                if (sceneItemsChanged)
                {
                    OnSceneItemsChanged();
                }
            }
        }

        /// <summary>
        /// Toggles on the highlighting for the specific node (in Helix preview) when
        /// selected in the PreviewBubble as well as the Watch Node
        /// </summary>
        private void ToggleTreeViewItemHighlighting(string path, bool isSelected)
        {
            // First, deselect parentnode in DynamoSelection
            var nodePath = path.Contains(':') ? path.Remove(path.IndexOf(':')) : path;
            if (DynamoSelection.Instance.Selection.Any())
            {
                var selNodes = DynamoSelection.Instance.Selection.ToList().OfType<NodeModel>();
                foreach (var node in selNodes)
                {
                    if (node.AstIdentifierBase == nodePath)
                    {
                        node.Deselect();
                    }
                }
            }

            // Next, deselect the parentnode in HelixWatch3DView
            var nodeGeometryModels = Element3DDictionary.Where(x => x.Key.Contains(nodePath) && x.Value is GeometryModel3D).ToArray();
            foreach (var nodeGeometryModel in nodeGeometryModels)
            {
                AttachedProperties.SetShowSelected(nodeGeometryModel.Value, false);
            }

            // Then, select the individual node only if isSelected is true since all geometryModels' Selected Property is set to false
            if (isSelected)
            {
                var geometryModels = Element3DDictionary.Where(x => x.Key.StartsWith(path + ":") && x.Value is GeometryModel3D).ToArray();
                foreach (var geometryModel in geometryModels)
                {
                    AttachedProperties.SetShowSelected(geometryModel.Value, isSelected);
                }
            }
        }

        /// <summary>
        /// Given a collection of render packages, generates
        /// corresponding <see cref="GeometryModel3D"/> objects for visualization, and
        /// attaches them to the visual scene.
        /// </summary>
        /// <param name="packages">An <see cref="IEnumerable"/> of <see cref="HelixRenderPackage"/>.</param>
        internal virtual void AggregateRenderPackages(IEnumerable<HelixRenderPackage> packages)
        {
            packages = FilterOutInvalidPackages(packages);

            IEnumerable<string> customNodeIdents = null;
            if (InCustomNode())
            {
                var hs = dynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
                if (hs != null)
                {
                    customNodeIdents = FindIdentifiersForCustomNodes(hs);
                }
            }

            lock (element3DDictionaryMutex)
            {
                foreach (var rp in packages)
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
                    
                    //If this render package belongs to special render package, then create
                    //and update the corresponding GeometryModel. Special renderpackage are
                    //defined based on its description containing one of the constants from
                    //RenderDescriptions struct.
                    if (UpdateGeometryModelForSpecialRenderPackage(rp, baseId))
                        continue;

                    var drawDead = InCustomNode() && !customNodeIdents.Contains(baseId);

                    string id;
                    var p = rp.Points;
                    if (p.Positions.Any())
                    {
                        //if we require a custom transform then we need to create a new Geometry3d object.
                        id = ((rp.RequiresCustomTransform) ? rp.Description : baseId) + PointsKey;

                        PointGeometryModel3D pointGeometry3D;

                        if (Element3DDictionary.ContainsKey(id))
                        {
                            pointGeometry3D = Element3DDictionary[id] as PointGeometryModel3D;
                        }
                        else
                        {
                            pointGeometry3D = CreatePointGeometryModel3D(rp);
                            Element3DDictionary.Add(id, pointGeometry3D);
                        }

                        var points = pointGeometry3D.Geometry == null ? HelixRenderPackage.InitPointGeometry()
                        : pointGeometry3D.Geometry as PointGeometry3D;
                        var startIdx = points.Positions.Count;

                        points.Positions.AddRange(p.Positions);

                        if (drawDead)
                        {
                            points.Colors.AddRange(Enumerable.Repeat(defaultDeadColor, points.Positions.Count));
                        }
                        else
                        {
                            points.Colors.AddRange(p.Colors.Any()
                              ? p.Colors
                              : Enumerable.Repeat(defaultPointColor, points.Positions.Count));
                            points.Indices.AddRange(p.Indices.Select(i => i + startIdx));
                        }

                        //always update the color cache if we're aggregating render packages.
                        //(ie updating colors from this method).
                        colorCache[id] = points.Colors;
                      
                        AddLabelPlace(baseId, p.Positions[0], rp);
                        if (pointGeometry3D.Geometry == null)
                        {
                            pointGeometry3D.Geometry = points;
                        }

                        //while the Name of the geometry is the node which created it
                        //we tag it with the id of the graphicModel we store in the scene/viewport for fast lookup.
                        pointGeometry3D.Name = baseId;
                        pointGeometry3D.Tag = id; // this is more specific than the base id.

                    }

                    var l = rp.Lines;
                    if (l.Positions.Any())
                    {
                        //if we require a custom transform then we need to create a new Geometry3d object.
                        id = ((rp.RequiresCustomTransform) ? rp.Description : baseId) + LinesKey;

                        LineGeometryModel3D lineGeometry3D;

                        if (Element3DDictionary.ContainsKey(id))
                        {
                            lineGeometry3D = Element3DDictionary[id] as LineGeometryModel3D;
                        }
                        else
                        {
                            // If the package contains mesh vertices, then the lines represent the 
                            // edges of meshes. Draw them with a different thickness.
                            lineGeometry3D = CreateLineGeometryModel3D(rp, rp.MeshVertices.Any()?0.5:1.0);
                            Element3DDictionary.Add(id, lineGeometry3D);
                        }

                        var lineSet = lineGeometry3D.Geometry == null ? HelixRenderPackage.InitLineGeometry()
                        : lineGeometry3D.Geometry as LineGeometry3D;
                        var startIdx = lineSet.Positions.Count;

                        lineSet.Positions.AddRange(l.Positions);
                        if (drawDead)
                        {
                            lineSet.Colors.AddRange(Enumerable.Repeat(defaultDeadColor, l.Positions.Count));
                        }
                        else
                        {
                            lineSet.Colors.AddRange(l.Colors.Any()
                             ? l.Colors
                             : Enumerable.Repeat(defaultLineColor, l.Positions.Count));
                        }

                        //always update the color cache if we're aggregating render packages.
                        //(ie updating colors from this method).
                        colorCache[id] = lineSet.Colors;

                        lineSet.Indices.AddRange(l.Indices.Any()
                            ? l.Indices.Select(i => i + startIdx)
                            : Enumerable.Range(startIdx, startIdx + l.Positions.Count));

                        AddLabelPlace(baseId, lineSet.Positions[startIdx], rp);
                        if(lineGeometry3D.Geometry == null)
                        {
                            lineGeometry3D.Geometry = lineSet;
                        }
                        //while the Name of the geometry is the node which created it
                        //we tag it with the id of the graphicModel we store in the scene/viewport for fast lookup.
                        lineGeometry3D.Name = baseId;
                        lineGeometry3D.Tag = id;
                    }

                    var m = rp.Mesh;
                    if (!m.Positions.Any()) continue;

                    //if we require a custom transform or vertex coloration then we need to create a new Geometry3d object.
                    id = ((rp.RequiresPerVertexColoration || rp.Colors != null || rp.RequiresCustomTransform) ? rp.Description : baseId) + MeshKey;

                    Element3D element3D;
                    DynamoGeometryModel3D meshGeometry3D;
                    if (Element3DDictionary.TryGetValue(id, out element3D))
                    {
                        meshGeometry3D = element3D as DynamoGeometryModel3D;
                    }
                    else
                    {
                        meshGeometry3D = CreateDynamoGeometryModel3D(rp);
                        Element3DDictionary.Add(id, meshGeometry3D);
                    }

                    var mesh = meshGeometry3D.Geometry == null ? HelixRenderPackage.InitMeshGeometry() 
                        : meshGeometry3D.Geometry as MeshGeometry3D;
                    var idxCount = mesh.Positions.Count;

                    mesh.Positions.AddRange(m.Positions);

  
                    // If we are in a custom node, and the current
                    // package's id is NOT one of the output ids of custom nodes
                    // in the graph, then draw the geometry with transparency.
                    if (drawDead)
                    {
                        meshGeometry3D.RequiresPerVertexColoration = true;
                        mesh.Colors.AddRange(m.Colors.Select(c=>new Color4(c.Red, c.Green, c.Blue, c.Alpha * defaultDeadAlphaScale)));
                    }
                    else
                    {
                        mesh.Colors.AddRange(m.Colors);
                        
                    }

                    mesh.Normals.AddRange(m.Normals);
                    mesh.TextureCoordinates.AddRange(m.TextureCoordinates);
                    mesh.Indices.AddRange(m.Indices.Select(i => i + idxCount));

                    if (mesh.Colors.Any(c => c.Alpha < 1.0))
                    {
                        AttachedProperties.SetHasTransparencyProperty(meshGeometry3D, true);
                    }

                    AddLabelPlace(baseId, mesh.Positions[idxCount], rp);
                    meshGeometry3D.Geometry = mesh;
                    meshGeometry3D.Name = baseId;

                }

            }
        }

        /// <summary>
        /// Filters out packages that are considered invalid by Helix. This includes any package
        /// that has a coordinate with a special value like NaN or Infinite.
        /// </summary>
        /// <param name="packages">Original packages to render</param>
        /// <returns>List of packages considered valid</returns>
        private IEnumerable<HelixRenderPackage> FilterOutInvalidPackages(IEnumerable<HelixRenderPackage> packages)
        {
            List<HelixRenderPackage> result = new List<HelixRenderPackage>();

            foreach (var package in packages)
            {
                if (!package.Points.Positions.Any(v => v.IsInvalid()) &&
                    !package.Lines.Positions.Any(v => v.IsInvalid()) &&
                    !package.Mesh.Positions.Any(v => v.IsInvalid()))
                {
                    result.Add(package);
                }
            }

            return result;
        }

        private void AddLabelPlace(string nodeId, Vector3 pos, IRenderPackage rp)
        {
            if (!labelPlaces.ContainsKey(nodeId))
            {
                labelPlaces[nodeId] = new List<Tuple<string, Vector3>>();
            }

            //if the renderPackage also implements ITransformable then 
            // use the transform property to transform the text labels
           
            SharpDX.Vector3 transformedPos = pos;
            if (rp is HelixRenderPackage && rp is Autodesk.DesignScript.Interfaces.ITransformable)
            {
                transformedPos = (rp as Autodesk.DesignScript.Interfaces.ITransformable)
                   .Transform.ToMatrix3D().Transform((pos).ToPoint3D()).ToVector3();
            }
            

            labelPlaces[nodeId].Add(new Tuple<string, Vector3>(rp.Description, transformedPos));
            if (rp.DisplayLabels)
            {
                CreateOrUpdateText(nodeId, transformedPos, rp);
            }
        }

        /// <summary>
        /// Updates or replaces the GeometryModel3D for special IRenderPackage. Special 
        /// IRenderPackage has a Description field that starts with a string value defined 
        /// in RenderDescriptions. See RenderDescriptions for details of possible values.
        /// </summary>
        /// <param name="rp">The target HelixRenderPackage object</param>
        /// <param name="id">id of the HelixRenderPackage object</param>
        /// <returns>Returns true if rp is a special render package, and its GeometryModel3D
        /// is successfully updated.</returns>
        private bool UpdateGeometryModelForSpecialRenderPackage(HelixRenderPackage rp, string id)
        {
            int desclength = RenderDescriptions.ManipulatorAxis.Length;
            if (rp.Description.Length < desclength)
                return false;

            string description = rp.Description.Substring(0, desclength);
            Element3D model = null;
            Element3DDictionary.TryGetValue(id, out model);
            switch(description)
            {
                case RenderDescriptions.ManipulatorAxis:
                    var manipulator = model as DynamoGeometryModel3D;
                    if (null == manipulator)
                    {
                        manipulator = CreateDynamoGeometryModel3D(rp, false);
                        AttachedProperties.SetIsSpecialRenderPackage(manipulator, true);
                    }
                    
                    var mb = new MeshBuilder();
                    mb.AddArrow(rp.Lines.Positions[0], rp.Lines.Positions[1], 0.1);
                    manipulator.Geometry = mb.ToMeshGeometry3D();
                    
                    if (rp.Lines.Colors[0].Red == 1)
                        manipulator.Material = PhongMaterials.Red;
                    else if (rp.Lines.Colors[0].Green == 1)
                        manipulator.Material = PhongMaterials.Green;
                    else if (rp.Lines.Colors[0].Blue == 1)
                        manipulator.Material = PhongMaterials.Blue;

                    Element3DDictionary[id] = manipulator;
                    return true;
                case RenderDescriptions.AxisLine:
                    var centerline = model as DynamoLineGeometryModel3D;
                    if (null == centerline)
                    {
                        centerline = CreateLineGeometryModel3D(rp, 0.3, false);
                        AttachedProperties.SetIsSpecialRenderPackage(centerline, true);
                    }
                    centerline.Geometry = rp.Lines;
                    Element3DDictionary[id] = centerline;
                    return true;
                case RenderDescriptions.ManipulatorPlane:
                    var plane = model as DynamoLineGeometryModel3D;
                    if (null == plane)
                    {
                        plane = CreateLineGeometryModel3D(rp, 0.7, false);
                        AttachedProperties.SetIsSpecialRenderPackage(plane, true);
                    }
                    plane.Geometry = rp.Lines;
                    Element3DDictionary[id] = plane;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Handles View Click event and performs a hit tests for geometry selection.
        /// </summary>
        protected override void HandleViewClick(object sender, MouseButtonEventArgs e)
        {
            var viewport = sender as Viewport3DX;
            if (null == viewport) return;

            var vm = viewModel as DynamoViewModel;
            if (vm == null) return;

            var nodes = new List<NodeModel>();
            var hits = viewport.FindHits(e.GetPosition(viewport));
            foreach (var hit in hits)
            {
                var model = hit.ModelHit as GeometryModel3D;
                if (model == null) continue;

                foreach (var node in vm.Model.CurrentWorkspace.Nodes)
                {
                    var foundNode = node.AstIdentifierBase.Contains(model.Name);

                    if (!foundNode) continue;

                    nodes.Add(node);
                    break;
                }
            }

            //If any node is selected, clear the current selection and add these
            //nodes to current selection. When nothing is selected, DONOT clear
            //the selection because it may end up removing a manipulator while
            //selecting it's gizmos.
            if(nodes.Any())
            {
                DynamoSelection.Instance.ClearSelection();
                nodes.ForEach(x => vm.Model.AddToSelection(x));
            }
        }

        public override void HighlightNodeGraphics(IEnumerable<NodeModel> nodes)
        {
            HighlightGraphicsOnOff(nodes, true);
        }

        public override void UnHighlightNodeGraphics(IEnumerable<NodeModel> nodes)
        {
            HighlightGraphicsOnOff(nodes, false);
        }

        private void HighlightGraphicsOnOff(IEnumerable<NodeModel> nodes, bool highlightOn)
        {
            foreach (var node in nodes)
            {
                var geometries = FindAllGeometryModel3DsForNode(node);
                foreach (var geometry in geometries)
                {
                    var pointGeom = geometry.Value as PointGeometryModel3D;
                    
                    if (pointGeom == null) continue;
                    
                    var points = pointGeom.Geometry;
                    points.Colors.Clear();
                    
                    points.Colors.AddRange(highlightOn
                        ? Enumerable.Repeat(highlightColor, points.Positions.Count)
                        : Enumerable.Repeat(defaultPointColor, points.Positions.Count));

                    points.UpdateColors();
                    pointGeom.Size = highlightOn ? highlightSize : defaultPointSize;
                }
            }
        }

        private void CreateOrUpdateText(string baseId, Vector3 pt, IRenderPackage rp)
        {
            var textId = baseId + TextKey;
            BillboardTextModel3D bbText;
            if (Element3DDictionary.ContainsKey(textId))
            {
                bbText = Element3DDictionary[textId] as BillboardTextModel3D;
            }
            else
            {
                bbText = new BillboardTextModel3D()
                {
                    Geometry = HelixRenderPackage.InitText3D(),
                };
                Element3DDictionary.Add(textId, bbText);
            }
            var geom = bbText.Geometry as BillboardText3D;
           
            geom.TextInfo.Add(new TextInfo(HelixRenderPackage.CleanTag(rp.Description),
               pt + defaultLabelOffset));
        }

        private DynamoGeometryModel3D CreateDynamoGeometryModel3D(HelixRenderPackage rp, bool isHitTestVisible = true)
        {
          
            var meshGeometry3D = new DynamoGeometryModel3D()
            {
                Transform = new MatrixTransform3D(rp.Transform.ToMatrix3D()),
                Material = WhiteMaterial,
                IsHitTestVisible = isHitTestVisible,
                RequiresPerVertexColoration = rp.RequiresPerVertexColoration,
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
                        DiffuseMap = diffMap.ToMemoryStream(),
                    };
                    meshGeometry3D.Material = diffMat;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }

            return meshGeometry3D;
        }

        private DynamoLineGeometryModel3D CreateLineGeometryModel3D(HelixRenderPackage rp, double thickness = 1.0, 
            bool isHitTestVisible = true)
        {
            var lineGeometry3D = new DynamoLineGeometryModel3D()
            {
                //Do not set Geometry here
                Transform = new MatrixTransform3D(rp.Transform.ToMatrix3D()),
                Color = Colors.White,
                Thickness = thickness,
                IsHitTestVisible = isHitTestVisible,
                IsSelected = rp.IsSelected
            };
            return lineGeometry3D;
        }

        private DynamoPointGeometryModel3D CreatePointGeometryModel3D(HelixRenderPackage rp)
        {
            var pointGeometry3D = new DynamoPointGeometryModel3D
            {
                //Do not set Geometry here
                Transform = new MatrixTransform3D(rp.Transform.ToMatrix3D()),
                Color = Colors.White,
                Figure = PointFigure.Ellipse,
                Size = defaultPointSize,
                IsHitTestVisible = true,
                IsSelected = rp.IsSelected
            };
            return pointGeometry3D;
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

        internal void UpdateNearClipPlane()
        {

            if (camera == null) return;

            var near = camera.NearPlaneDistance;
            var far = camera.FarPlaneDistance;

            ComputeClipPlaneDistances(camera.Position.ToVector3(), camera.LookDirection.ToVector3(), SceneItems,
                NearPlaneDistanceFactor, out near, out far, DefaultNearClipDistance, DefaultFarClipDistance);

            if (Camera.NearPlaneDistance == near && Camera.FarPlaneDistance == far) return;

            Camera.NearPlaneDistance = near;
            Camera.FarPlaneDistance = far;
        }

        /// <summary>
        /// This method clamps the near and far clip planes around the scene geometry
        /// to achiever higher z-buffer precision.
        /// 
        /// It does this by finding the distance from each GeometryModel3D object's corner points
        /// to the camera plane. The camera's far clip plane is set to 2 * dfar, and the camera's 
        /// near clip plane is set to nearPlaneDistanceFactor * dnear
        /// </summary>
        internal static void ComputeClipPlaneDistances(Vector3 cameraPosition, Vector3 cameraLook, IEnumerable<Element3D> geometry, 
            double nearPlaneDistanceFactor, out double near, out double far, double defaultNearClipDistance, double defaultFarClipDistance)
        {
            near = defaultNearClipDistance;
            far = DefaultFarClipDistance;

            var validGeometry = geometry.Where(i => i is GeometryModel3D).ToArray();
            if (!validGeometry.Any()) return;

            var bounds = validGeometry.Cast<GeometryModel3D>().Select(g=>g.Bounds());

            // See http://mathworld.wolfram.com/Point-PlaneDistance.html
            // The plane distance formula will return positive values for points on the same side of the plane
            // as the plane's normal, and negative values for points on the opposite side of the plane. 

            var distances = bounds.SelectMany(b => b.GetCorners()).
                Select(c => c.DistanceToPlane(cameraPosition, cameraLook.Normalized())).
                ToList();

            if (!distances.Any()) return;

            distances.Sort();

            // All behind
            // Set the near and far clip to their defaults
            // because nothing is in front of the camera.
            if (distances.All(d => d < 0))
            {
                near = defaultNearClipDistance;
                far = defaultFarClipDistance;
                return;
            }

            // All in front or some in front and some behind
            // Set the near clip plane to some fraction of the 
            // of the distance to the first point.
            var closest = distances.First(d => d >= 0);
            near = closest.AlmostEqualTo(0, EqualityTolerance) ? DefaultNearClipDistance : Math.Max(DefaultNearClipDistance, closest * nearPlaneDistanceFactor);
            far = distances.Last() * 2;

        }

        internal static IEnumerable<string> FindIdentifiersForSelectedNodes(IEnumerable<NodeModel> selectedNodes)
        {
            return selectedNodes.SelectMany(n => n.OutPorts.Select(p => n.GetAstIdentifierForOutputIndex(p.Index).Value));
        }

        /// <summary>
        /// Find all output identifiers for all custom nodes in the provided workspace. 
        /// </summary>
        /// <param name="workspace">A workspace</param>
        /// <returns>An <see cref="IEnumerable"/> of <see cref="string"/> containing all output identifiers for 
        /// all custom nodes in the provided workspace, or null if the workspace is null.</returns>
        internal static IEnumerable<string> FindIdentifiersForCustomNodes(HomeWorkspaceModel workspace)
        {
            if (workspace == null)
            {
                return null;
            }

            // Remove the output identifier appended to the custom node outputs.
            var rgx = new Regex("_out[0-9]");

            var customNodes = workspace.Nodes.Where(n => n is Function);
            var idents = new List<string>();
            foreach (var n in customNodes)
            {
                if (n.IsPartiallyApplied)
                {
                    // Find output identifiers for the connected map node
                    var mapOutportsIdents =
                        n.OutPorts.SelectMany(
                            np => np.Connectors.SelectMany(
                                    c => c.End.Owner.OutPorts.Select(
                                            mp => rgx.Replace(mp.Owner.GetAstIdentifierForOutputIndex(mp.Index).Value, ""))));
                    
                    idents.AddRange(mapOutportsIdents);
                }
                else
                {
                    idents.AddRange(n.OutPorts.Select(p => rgx.Replace(n.GetAstIdentifierForOutputIndex(p.Index).Value, "")));
                }
            }
            return idents;
        }

        internal static IEnumerable<string> AllOutputIdentifiersInWorkspace(HomeWorkspaceModel workspace)
        {
            if (workspace == null)
            {
                return null;
            }

            return
                workspace.Nodes
                    .SelectMany(n => n.OutPorts.Select(p => n.GetAstIdentifierForOutputIndex(p.Index).Value));
        } 

        internal static IEnumerable<GeometryModel3D> FindGeometryForIdentifiers(IEnumerable<GeometryModel3D> geometry, IEnumerable<string> identifiers)
        {
            return identifiers.SelectMany(id => geometry.Where(item => item.Name.Contains(id))).ToArray();
        }

        /// <summary>
        /// For a set of selected nodes, compute a bounding box which
        /// encompasses all of the nodes' generated geometry.
        /// </summary>
        /// <param name="geometry">A collection of <see cref="GeometryModel3D"/> objects.</param>
        /// <returns>A <see cref="BoundingBox"/> object.</returns>
        internal static BoundingBox ComputeBoundsForGeometry(GeometryModel3D[] geometry)
        {
            if (!geometry.Any()) return DefaultBounds;

            var bounds = geometry.First().Bounds();
            bounds = geometry.Aggregate(bounds, (current, geom) => BoundingBox.Merge(current, geom.Bounds()));

#if DEBUG
            Debug.WriteLine("{0} geometry items referenced by the selection.", geometry.Count());
            Debug.WriteLine("Bounding box of selected geometry:{0}", bounds);
#endif
            return bounds;
        }

        internal override void ExportToSTL(string path, string modelName)
        {
            var geoms = SceneItems.Where(i => i is DynamoGeometryModel3D).
                Cast<DynamoGeometryModel3D>();

            using (TextWriter tw = new StreamWriter(path))
            {
                tw.WriteLine("solid {0}", dynamoModel.CurrentWorkspace.Name);
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
                tw.WriteLine("endsolid {0}", dynamoModel.CurrentWorkspace.Name);
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                viewModel = null;
                var effectsManager = EffectsManager as DynamoEffectsManager;
                if (effectsManager != null)
                {
                    effectsManager.Dispose();
                    effectsManager = null;
                   
                }
                SelectedMaterial = null;
                WhiteMaterial = null;
                FrozenMaterial = null;
                IsolatedMaterial = null;

                foreach (var sceneItem in SceneItems)
                {
                    sceneItem.Dispose();
                }
                sceneItems.Clear();
                foreach (var item in Element3DDictionary.Values)
                {
                    item.Dispose();
                }
                element3DDictionary.Clear();
                
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// The Model3DComparer is used to sort arrays of Model3D objects. 
    /// After sorting, the target array's objects will be organized
    /// as follows:
    /// 1. All not GeometryModel3D objects
    /// 2. All opaque mesh geometry
    /// 3. All opaque line geometry
    /// 4. All opaque point geometry
    /// 5. All transparent geometry, ordered by distance from the camera.
    /// 6. All text.
    /// </summary>
    [Obsolete("Do not use! This will be moved to a new project in a future version of Dynamo.")]
    public class Element3DComparer : IComparer<Element3D>
    {
        private readonly Vector3 cameraPosition;

        public Element3DComparer(Point3D cameraPosition)
        {
            this.cameraPosition = cameraPosition.ToVector3();
        }

        public int Compare(Element3D x, Element3D y)
        {
            // if at least one of them is not GeometryModel3D
            // we either sort by being GeometryModel3D type (result is 1 or -1) 
            // or don't care about order (result is 0)
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            var textA = x.GetType() == typeof(BillboardTextModel3D);
            var textB = y.GetType() == typeof(BillboardTextModel3D);
            var result = textA.CompareTo(textB);

            // if at least one of them is text
            // we either sort by being text type (result is 1 or -1) 
            // or don't care about order (result is 0)
            if (textA || textB)
            {
                return result;
            }

            // under Isolate Selected Geometry mode, selected geometries will have higher precedence
            // and rendered as closer to the camera compared to unselected geometries
            var selectedA = AttachedProperties.GetIsolationMode(x) &&
                !AttachedProperties.GetShowSelected(x) && !AttachedProperties.IsSpecialRenderPackage(x);
            var selectedB = AttachedProperties.GetIsolationMode(y) &&
                !AttachedProperties.GetShowSelected(y) && !AttachedProperties.IsSpecialRenderPackage(y);
            result = selectedA.CompareTo(selectedB);
            if (result != 0) return result;

            // if only one of transA and transB has transparency, sort by having this property
            var transA = AttachedProperties.GetHasTransparencyProperty(x);
            var transB = AttachedProperties.GetHasTransparencyProperty(y);
            result = transA.CompareTo(transB);
            if (result != 0) return result;

            // if both items has transparency, sort by distance
            if (transA)
            {
                // compare distance
                var boundsA = x.Bounds;
                var boundsB = y.Bounds;
                var cpA = (boundsA.Maximum + boundsA.Minimum) / 2;
                var cpB = (boundsB.Maximum + boundsB.Minimum) / 2;
                var dA = Vector3.DistanceSquared(cpA, cameraPosition);
                var dB = Vector3.DistanceSquared(cpB, cameraPosition);
                result = -dA.CompareTo(dB);
                return result;
            }

            // if both items does not have transparency, sort following next order: mesh, line, point
            var pointA = x is PointGeometryModel3D;
            var pointB = y is PointGeometryModel3D;
            result = pointA.CompareTo(pointB);

            if (pointA || pointB)
            {
                return result;
            }

            var lineA = x is LineGeometryModel3D;
            var lineB = y is LineGeometryModel3D;
            return lineA.CompareTo(lineB);
        }
    }

    internal static class CameraExtensions
    {
        public static CameraData ToCameraData(this PerspectiveCamera camera, string name)
        {
            var camData = new CameraData
            {
                NearPlaneDistance = camera.NearPlaneDistance,
                FarPlaneDistance = camera.FarPlaneDistance,

                Name = name,
                EyeX = camera.Position.X,
                EyeY = camera.Position.Y,
                EyeZ = camera.Position.Z,
                LookX = camera.LookDirection.X,
                LookY = camera.LookDirection.Y,
                LookZ = camera.LookDirection.Z,
                UpX = camera.UpDirection.X,
                UpY = camera.UpDirection.Y,
                UpZ = camera.UpDirection.Z
            };

            return camData;
        }
    }

    internal static class BoundingBoxExtensions
    {
        /// <summary>
        /// Convert a <see cref="BoundingBox"/> to a <see cref="Rect3D"/>
        /// </summary>
        /// <param name="bounds">The <see cref="BoundingBox"/> to be converted.</param>
        /// <returns>A <see cref="Rect3D"/> object.</returns>
        internal static Rect3D ToRect3D(this BoundingBox bounds)
        {
            var min = bounds.Minimum;
            var max = bounds.Maximum;
            var size = new Size3D((max.X - min.X), (max.Y - min.Y), (max.Z - min.Z));
            return new Rect3D(min.ToPoint3D(), size);
        }

        /// <summary>
        /// If a <see cref="GeometryModel3D"/> has more than one point, then
        /// return its bounds, otherwise, return a bounding
        /// box surrounding the point of the supplied size.
        /// 
        /// This extension method is to correct for the Helix toolkit's GeometryModel3D.Bounds
        /// property which does not update correctly as new geometry is added to the GeometryModel3D.
        /// </summary>
        /// <param name="pointGeom">A <see cref="GeometryModel3D"/> object.</param>
        /// <returns>A <see cref="BoundingBox"/> object encapsulating the geometry.</returns>
        internal static BoundingBox Bounds(this GeometryModel3D geom, float defaultBoundsSize = 5.0f)
        {
            if (geom.Geometry.Positions == null || geom.Geometry.Positions.Count == 0)
            {
                return new BoundingBox();
            }

            if (geom.Geometry.Positions.Count > 1)
            {
                return BoundingBox.FromPoints(geom.Geometry.Positions.ToArray());
            }

            var pos = geom.Geometry.Positions.First();
            var min = pos + new Vector3(-defaultBoundsSize, -defaultBoundsSize, -defaultBoundsSize);
            var max = pos + new Vector3(defaultBoundsSize, defaultBoundsSize, defaultBoundsSize);
            return new BoundingBox(min, max);
        }

        public static Vector3 Center(this BoundingBox bounds)
        {
            return (bounds.Maximum + bounds.Minimum)/2;
        }

    }

    internal static class Vector3Extensions
    {
        internal static double DistanceToPlane(this Vector3 point, Vector3 planeOrigin, Vector3 planeNormal)
        {
            return Vector3.Dot(planeNormal, (point - planeOrigin));
        }

        internal static bool IsInvalid(this Vector3 point)
        {
            return float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z);
        }
    }

    internal static class DoubleExtensions
    {
        internal static bool AlmostEqualTo(this double a, double b, double tolerance)
        {
            return Math.Abs(a - b) < tolerance;
        }
    }
}
