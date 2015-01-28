using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

using Autodesk.DesignScript.Interfaces;

using Dynamo.DSEngine;
using Dynamo.ViewModels;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX.Model.Geometry;

using SharpDX;

using Color = SharpDX.Color;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
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
        private Point _rightMousePoint;
        private LineGeometry3D _grid;
        private RenderTechnique renderTechnique;
        private HelixToolkit.Wpf.SharpDX.Camera camera;
        private SharpDX.Color4 selectionColor = new Color4(0,1,1,1);
        private bool showShadows;

        #endregion

        #region public properties

        public LineGeometry3D Grid
        {
            get { return _grid; }
            set
            {
                _grid = value;
                NotifyPropertyChanged("Grid");
            }
        }

        public PointGeometry3D Points { get; set; }

        public LineGeometry3D Lines { get; set; }

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
        
        public Vector3 DirectionalLightDirection { get; private set; }
        
        public Color4 DirectionalLightColor { get; private set; }
        
        public Color4 AmbientLightColor { get; private set; }
        
        public System.Windows.Media.Media3D.Transform3D Model1Transform { get; private set; }
        
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

        public HelixToolkit.Wpf.SharpDX.Camera Camera
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
            this.ShadowMapResolution = new Vector2(2048, 2048);
            this.ShowShadows = false;
            
            // setup lighting            
            this.AmbientLightColor = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

            this.DirectionalLightColor = SharpDX.Color.White;
            this.DirectionalLightDirection = new Vector3(-0.5f, -1, 0);

            this.RenderTechnique = Techniques.RenderPhong;
            this.WhiteMaterial = PhongMaterials.White;

            this.Model1Transform = new System.Windows.Media.Media3D.TranslateTransform3D(0, -0, 0);

            // camera setup
            this.Camera = new HelixToolkit.Wpf.SharpDX.PerspectiveCamera
            {
                Position = new Point3D(10, 10, 10),
                LookDirection = new Vector3D(-10, -10, -10),
                UpDirection = new Vector3D(0, 1, 0)
            };
            
            DrawGrid();
        }

        #endregion

        #region event handlers

        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Watch 3D view unloaded.");

            var vm = DataContext as IWatchViewModel;
            if (vm != null)
            {
                vm.VisualizationManager.RenderComplete -= VisualizationManagerRenderComplete;
                vm.VisualizationManager.ResultsReadyToVisualize -= VisualizationManager_ResultsReadyToVisualize;
            }
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            
            MouseLeftButtonDown += view_MouseButtonIgnore;
            MouseLeftButtonUp += view_MouseButtonIgnore;
            MouseRightButtonUp += view_MouseRightButtonUp;
            PreviewMouseRightButtonDown += view_PreviewMouseRightButtonDown;

            watch_view.InputBindings.Add(new KeyBinding(ViewportCommands.ZoomExtents, Key.E, ModifierKeys.Control));
            watch_view.InputBindings.Add(
                new MouseBinding(
                    ViewportCommands.ZoomExtents, new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.Control)));
            watch_view.InputBindings.Add(
                new MouseBinding(ViewportCommands.Rotate, new MouseGesture(MouseAction.RightClick, ModifierKeys.None)));
            watch_view.InputBindings.Add(
                new MouseBinding(ViewportCommands.Zoom, new MouseGesture(MouseAction.RightClick, ModifierKeys.Control)));
            watch_view.InputBindings.Add(
                new MouseBinding(ViewportCommands.Pan, new MouseGesture(MouseAction.MiddleClick)));
            watch_view.InputBindings.Add(
                new MouseBinding(
                    ViewportCommands.ChangeFieldOfView, new MouseGesture(MouseAction.RightClick, ModifierKeys.Alt)));
            watch_view.InputBindings.Add(
                new MouseBinding(
                    ViewportCommands.ZoomRectangle,
                    new MouseGesture(MouseAction.RightClick, ModifierKeys.Control | ModifierKeys.Shift)));
            watch_view.InputBindings.Add(
                new MouseBinding(
                    ViewportCommands.SetTarget, new MouseGesture(MouseAction.RightDoubleClick, ModifierKeys.Control)));
            watch_view.InputBindings.Add(
                new MouseBinding(
                    ViewportCommands.Reset, new MouseGesture(MouseAction.MiddleDoubleClick, ModifierKeys.Control)));

            var vm = DataContext as IWatchViewModel;
            
            //check this for null so the designer can load the preview
            if (vm != null)
            {
                vm.VisualizationManager.RenderComplete += VisualizationManagerRenderComplete;
                vm.VisualizationManager.ResultsReadyToVisualize += VisualizationManager_ResultsReadyToVisualize;

                var renderingTier = (RenderCapability.Tier >> 16);
                var pixelShader3Supported = RenderCapability.IsPixelShaderVersionSupported(3, 0);
                var pixelShader4Supported = RenderCapability.IsPixelShaderVersionSupported(4, 0);
                var softwareEffectSupported = RenderCapability.IsShaderEffectSoftwareRenderingSupported;
                var maxTextureSize = RenderCapability.MaxHardwareTextureSize;

                vm.ViewModel.Model.Logger.Log("RENDER", string.Format("Rendering Tier: {0}", renderingTier));
                vm.ViewModel.Model.Logger.Log("RENDER", string.Format("Pixel Shader 3 Supported: {0}", pixelShader3Supported));
                vm.ViewModel.Model.Logger.Log("RENDER", string.Format("Pixel Shader 4 Supported: {0}", pixelShader4Supported));
                vm.ViewModel.Model.Logger.Log("RENDER", string.Format("Software Effect Rendering Supported: {0}", softwareEffectSupported));
                vm.ViewModel.Model.Logger.Log("RENDER", string.Format("Maximum hardware texture size: {0}", maxTextureSize));
            }
        }

        /// <summary>
        /// Handler for the visualization manager's ResultsReadyToVisualize event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VisualizationManager_ResultsReadyToVisualize(object sender, VisualizationEventArgs e)
        {
            if (CheckAccess())
                RenderDrawables(e);
            else
            {
                // Scheduler invokes ResultsReadyToVisualize on background thread.
                Dispatcher.BeginInvoke(new Action(() => RenderDrawables(e)));
            }
        }

        /// <summary>
        /// When visualization is complete, the view requests it's visuals. For Full
        /// screen watch, this will be all renderables. For a Watch 3D node, this will
        /// be the subset of the renderables associated with the node.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VisualizationManagerRenderComplete(object sender, RenderCompletionEventArgs e)
        {
            var executeCommand = new Action(delegate
            {
                var vm = (IWatchViewModel)DataContext;
                if (vm.GetBranchVisualizationCommand.CanExecute(e.TaskId))
                    vm.GetBranchVisualizationCommand.Execute(e.TaskId);
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
            _rightMousePoint = e.GetPosition(topControl);
        }

        void view_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if the mouse has moved, and this is a right click, we assume 
            // rotation. handle the event so we don't show the context menu
            // if the user wants the contextual menu they can click on the
            // node sidebar or top bar
            if (e.GetPosition(topControl) != _rightMousePoint)
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

            for(int i= 0; i < 10; i += 1)
            {
                for (int j = 0; j < 10; j += 1)
                {
                    DrawGridPatch(positions, indices, colors, -50 + i * 10, -50 + j * 10);
                }
            }

            // Draw the coordinate axes
            positions.Add(new Vector3());
            indices.Add(positions.Count - 1);
            positions.Add(new Vector3(1,0,0));
            indices.Add(positions.Count - 1);
            colors.Add(Color.Red);
            colors.Add(Color.Red);

            positions.Add(new Vector3());
            indices.Add(positions.Count - 1);
            positions.Add(new Vector3(0, 1, 0));
            indices.Add(positions.Count - 1);
            colors.Add(Color.Blue);
            colors.Add(Color.Blue);

            positions.Add(new Vector3());
            indices.Add(positions.Count - 1);
            positions.Add(new Vector3(0, 0, -1));
            indices.Add(positions.Count - 1);
            colors.Add(Color.Green);
            colors.Add(Color.Green);

            Grid.Positions = positions;
            Grid.Indices = indices;
            Grid.Colors = colors;
        }

        private static void DrawGridPatch(
            Vector3Collection positions, IntCollection indices, Color4Collection colors, int startX, int startY)
        {
            var size = 10;

            for (
                int x = startX; x <= startX + size; x++)
            {
                var v = new Vector3(x, -.001f, startY);
                positions.Add(v);
                indices.Add(positions.Count - 1);
                positions.Add(new Vector3(x, -.001f, startY + size));
                indices.Add(positions.Count - 1);

                if (x%5 == 0)
                {
                    colors.Add(Color.DarkGray);
                    colors.Add(Color.DarkGray);
                }
                else
                {
                    colors.Add(Color.LightGray);
                    colors.Add(Color.LightGray);
                }
            }

            for (int y = startY; y <= startY + size; y++)
            {
                positions.Add(new Vector3(startX, -.001f, y));
                indices.Add(positions.Count - 1);
                positions.Add(new Vector3(startX + size, -.001f, y));
                indices.Add(positions.Count - 1);

                if (y%5 == 0)
                {
                    colors.Add(Color.DarkGray);
                    colors.Add(Color.DarkGray);
                }
                else
                {
                    colors.Add(Color.LightGray);
                    colors.Add(Color.LightGray);
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

            Debug.WriteLine(string.Format("Rendering visuals for {0}", e.Id));

            var sw = new Stopwatch();
            sw.Start();

            Points = null;
            Lines = null;
            Mesh = null;
            Text = null;
            MeshCount = 0;

            var packages = e.Packages
                .Where(rp=>rp.TriangleVertices.Count % 9 == 0);

            var points = InitPointGeometry();
            var lines = InitLineGeometry();
            var text = InitText3D(); 
            var mesh = InitMeshGeometry();

            foreach (var package in packages)
            {
                ConvertPoints(package, points, text);
                ConvertLines(package, lines, text);
                ConvertMeshes(package, mesh);
            }

            sw.Stop();
            Debug.WriteLine(string.Format("RENDER: {0} ellapsed for updating background preview.", sw.Elapsed));

            if (!points.Positions.Any())
                points = null;

            if (!lines.Positions.Any())
                lines = null;

            if (!text.TextInfo.Any())
                text = null;

            if (!mesh.Positions.Any())
                mesh = null;

            Dispatcher.Invoke(new Action<
                PointGeometry3D,
                LineGeometry3D,
                HelixToolkit.Wpf.SharpDX.MeshGeometry3D, 
                BillboardText3D>(SendGraphicsToView), DispatcherPriority.Render,
                               new object[] {
                                   points, 
                                   lines, 
                                   mesh, 
                                   text});
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

        private static HelixToolkit.Wpf.SharpDX.MeshGeometry3D InitMeshGeometry()
        {
            var mesh = new HelixToolkit.Wpf.SharpDX.MeshGeometry3D()
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
            HelixToolkit.Wpf.SharpDX.MeshGeometry3D mesh,
            BillboardText3D text)
        {
            Points = points;
            Lines = lines;
            Mesh = mesh;
            Text = text;

            // Send property changed notifications for everything
            NotifyPropertyChanged(string.Empty);
        }

        private void ConvertPoints(RenderPackage p, PointGeometry3D points, BillboardText3D text)
        {
            int color_idx = 0;

            for (int i = 0; i < p.PointVertices.Count; i += 3)
            {
                var x = (float)p.PointVertices[i];
                var y = (float)p.PointVertices[i + 1];
                var z = (float)p.PointVertices[i + 2];

                // DirectX convention - Y Up
                var pt = new Vector3(x, z, -y);

                if (i == 0 && p.DisplayLabels)
                {
                    text.TextInfo.Add(new TextInfo(CleanTag(p.Tag), pt));
                }

                var ptColor = SharpDX.Color4.Black;
                if (p.PointVertexColors.Count > color_idx + 2)
                {
                    ptColor = new SharpDX.Color4(
                                        (p.PointVertexColors[color_idx] / 255.0f),
                                        (p.PointVertexColors[color_idx + 1] / 255.0f),
                                        (p.PointVertexColors[color_idx + 2] / 255.0f), 1);
                }

                points.Positions.Add(pt);
                points.Indices.Add(points.Positions.Count);

                points.Colors.Add(p.Selected ? selectionColor : ptColor);

                color_idx += 4;
            }

        }

        private void ConvertLines(RenderPackage p, LineGeometry3D geom, BillboardText3D text)
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
                    var point = new Vector3(x1, z1, -y1);

                    if (i == 0 && outerCount == 0 && p.DisplayLabels)
                    {
                        text.TextInfo.Add(new TextInfo(CleanTag(p.Tag), point));
                    }

                    SharpDX.Color4 startColor = SharpDX.Color.Black;

                    if (p.LineStripVertexColors.Count >= color_idx + 2)
                    {
                        startColor = new SharpDX.Color4(
                            (p.LineStripVertexColors[color_idx] / 255.0f),
                            (p.LineStripVertexColors[color_idx + 1] / 255.0f),
                            (p.LineStripVertexColors[color_idx + 2] / 255.0f),
                            1);
                    }

                    if (startColor == SharpDX.Color.White)
                    {
                        startColor = SharpDX.Color.Black;
                    }

                    // Line segments are represented as a 
                    // start point and an end point. Except
                    // where we are starting the curve or ending it,
                    // we duplicate the point.
                    if (i != 0 && i != count - 1)
                    {
                        geom.Indices.Add(geom.Indices.Count);
                        geom.Positions.Add(point);
                        geom.Colors.Add(p.Selected ? selectionColor : startColor);
                    }

                    geom.Indices.Add(geom.Indices.Count);
                    geom.Positions.Add(point);
                    geom.Colors.Add(p.Selected ? selectionColor : startColor);

                    idx += 3;
                    color_idx += 4;
                }

                outerCount++;
            }
        }

        private void ConvertMeshes(RenderPackage p, HelixToolkit.Wpf.SharpDX.MeshGeometry3D mesh)
        {
            // DirectX has a different winding than we store in
            // render packages. Re-wind triangles here...
            int color_idx = 0;
            int pt_idx = mesh.Positions.Count;

            for (int i = 0; i < p.TriangleVertices.Count; i += 9)
            {
                var a = GetVertex(p.TriangleVertices, i);
                var b = GetVertex(p.TriangleVertices, i + 3);
                var c = GetVertex(p.TriangleVertices, i + 6);

                var an = GetVertex(p.TriangleNormals, i);
                var bn = GetVertex(p.TriangleNormals, i + 3);
                var cn = GetVertex(p.TriangleNormals, i + 6);

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

                if (p.Selected)
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

            if (color_idx < p.TriangleVertexColors.Count)
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
