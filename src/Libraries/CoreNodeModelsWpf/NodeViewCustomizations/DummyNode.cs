using System;
using System.Windows.Controls;
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

            var src = @"/CoreNodeModelsWpf;component/Resources/" + fileName;

            Image dummyNodeImage = new Image()
            {
                Stretch = System.Windows.Media.Stretch.None,
                Source = new BitmapImage(new Uri(src, UriKind.Relative))
            };

            nodeView.inputGrid.Children.Add(dummyNodeImage);
            model.Warning(model.GetDescription());
        }

        public void Dispose()
        {

        }
    }
}