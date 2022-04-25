using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{
    public class DummyNodeNodeViewCustomization : INodeViewCustomization<DummyNode>
    {
        public void CustomizeView(DummyNode model, Dynamo.Controls.NodeView nodeView)
        {
            var fileName = "DeprecatedNode.png";
            if (model.NodeNature == DummyNode.Nature.Unresolved)
                fileName = "MissingNode.png";

            model.State = ElementState.Warning;

            var src = @"/CoreNodeModelsWpf;component/Resources/" + fileName;

            Image dummyNodeImage = new Image
            {
                Width = 66.0,
                Height = 66.0,
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Source = new BitmapImage(new Uri(src, UriKind.Relative))
            };
            RenderOptions.SetBitmapScalingMode(dummyNodeImage, BitmapScalingMode.HighQuality);

            nodeView.inputGrid.Children.Add(dummyNodeImage);
            model.Warning(model.GetDescription(), true);

            // Grid containing the State overlay Glyphs in Zoomed Out state
            // Remove so only the 'paperclip' icon appears 
            UIElement child = nodeView.grid.FindName("zoomGlyphsGrid") as UIElement;    
            if (child != null) nodeView.grid.Children.Remove(child);
        }

        public void Dispose()
        {

        }
    }
}