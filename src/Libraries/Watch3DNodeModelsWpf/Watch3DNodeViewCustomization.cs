using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Controls;
using Dynamo.Wpf;
using Dynamo.Wpf.Rendering;
using Dynamo.Wpf.ViewModels.Watch3D;
using Dynamo.Visualization;
using VMDataBridge;
using Watch3DNodeModels;
using Watch3DNodeModelsWpf.Properties;

namespace Watch3DNodeModelsWpf
{
    public class Watch3DNodeViewCustomization : INodeViewCustomization<Watch3D>
    {
        private Watch3D watch3dModel;
        private Watch3DView watch3DView;
        private HelixWatch3DNodeViewModel watch3DViewModel;

        private void onCameraChanged(object sender ,RoutedEventArgs args)
        {
            var camera = watch3DViewModel.GetCameraInformation();
            watch3dModel.Camera.Name = camera.Name;
            watch3dModel.Camera.EyeX = camera.EyePosition.X;
            watch3dModel.Camera.EyeY = camera.EyePosition.Y;
            watch3dModel.Camera.EyeZ = camera.EyePosition.Z;
            watch3dModel.Camera.LookX = camera.LookDirection.X;
            watch3dModel.Camera.LookY = camera.LookDirection.Y;
            watch3dModel.Camera.LookZ = camera.LookDirection.Z;
            watch3dModel.Camera.UpX = camera.UpDirection.X;
            watch3dModel.Camera.UpY = camera.UpDirection.Y;
            watch3dModel.Camera.UpZ = camera.UpDirection.Z;
        }

        public void CustomizeView(Watch3D model, NodeView nodeView)
        {
            var dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
            watch3dModel = model;

            var renderingTier = (RenderCapability.Tier >> 16);
            if (renderingTier < 2) return;

            var dynamoModel = dynamoViewModel.Model;

            var vmParams = new Watch3DViewModelStartupParams(dynamoModel);
            watch3DViewModel = new HelixWatch3DNodeViewModel(watch3dModel, vmParams);
            watch3DViewModel.Setup(dynamoViewModel,
                dynamoViewModel.RenderPackageFactoryViewModel.Factory);

            if (model.initialCameraData != null)
            {
                try
                {
                    // The deserialization logic is unified between the view model and this node model.
                    // For the node model, we need to supply the deserialization method with the camera node.
                    var cameraNode = model.initialCameraData.ChildNodes.Cast<XmlNode>().FirstOrDefault(innerNode => innerNode.Name.Equals("camera", StringComparison.OrdinalIgnoreCase));
                    var cameraData = watch3DViewModel.DeserializeCamera(cameraNode);
                    watch3DViewModel.SetCameraData(cameraData);
                }
                catch
                {
                    watch3DViewModel.SetCameraData(new CameraData());
                }
            }

            model.Serialized += model_Serialized;
            watch3DViewModel.ViewCameraChanged += onCameraChanged;

            watch3DView = new Watch3DView()
            {
                Width = model.WatchWidth,
                Height = model.WatchHeight,
                DataContext = watch3DViewModel
            };

            // When user sizes a watch node, only view gets resized. The actual 
            // NodeModel does not get updated. This is where the view updates the 
            // model whenever its size is updated. 
            // Updated from (Watch3d)View.SizeChanged to nodeView.SizeChanged - height 
            // and width should correspond to node model and not watch3Dview
            nodeView.SizeChanged += (sender, args) =>
			    model.SetSize(args.NewSize.Width, args.NewSize.Height);

            // set WatchSize in model
            watch3DView.View.SizeChanged += (sender, args) => 
			    model.SetWatchSize(args.NewSize.Width, args.NewSize.Height);

            var mi = new MenuItem { Header = Resources.ZoomToFit };
            mi.Click += mi_Click;

            nodeView.MainContextMenu.Items.Add(mi);

            var backgroundRect = new Rectangle
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsHitTestVisible = false,
            };
            var bc = new BrushConverter();
            var strokeBrush = (Brush)bc.ConvertFrom("#313131");
            backgroundRect.Stroke = strokeBrush;
            backgroundRect.StrokeThickness = 1;
            var backgroundBrush = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            backgroundRect.Fill = backgroundBrush;

            nodeView.PresentationGrid.Children.Add(backgroundRect);
            nodeView.PresentationGrid.Children.Add(watch3DView);
            nodeView.PresentationGrid.Visibility = Visibility.Visible;

            DataBridge.Instance.RegisterCallback(
                model.GUID.ToString(),
                obj =>
                    nodeView.Dispatcher.Invoke(
                        new Action<object>(RenderData),
                        DispatcherPriority.Render,
                        obj));       
        }

        void model_Serialized(XmlElement nodeElement)
        {
            watch3DViewModel.SerializeCamera(nodeElement);
        }

        private IRenderPackage CreateRenderPackageFromGraphicItem(IGraphicItem gItem)
        {
            var factory = new HelixRenderPackageFactory();
            var renderPackage = factory.CreateRenderPackage();
            gItem.Tessellate(renderPackage, factory.TessellationParameters);
            return renderPackage;
        }

        private void RenderData(object data)
        {
            IEnumerable<IRenderPackage> packages = UnpackRenderData(data).Select(CreateRenderPackageFromGraphicItem);
            watch3DViewModel.AddGeometryForRenderPackages(new RenderPackageCache(packages));
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            watch3DView.View.ZoomExtents();
        }

        private static IEnumerable<IGraphicItem> UnpackRenderData(object data)
        {
            var item = data as IGraphicItem;
            if (item != null)
            {
                yield return item;
            }
            else
            {
                var enumerable = data as IEnumerable;
                if (enumerable == null) yield break;
                var graphics = enumerable.Cast<object>().SelectMany(UnpackRenderData);
                foreach (var g in graphics)
                    yield return g;
            }
        }

        public void Dispose()
        {
            if (watch3DViewModel != null)
            {
                watch3DViewModel.ViewCameraChanged -= onCameraChanged;
                watch3DViewModel.Dispose();
            }
            DataBridge.Instance.UnregisterCallback(watch3dModel.GUID.ToString());
        }
    }
}
