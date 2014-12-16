using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Dynamo.Wpf;

namespace DSCoreNodesUI
{
    public class DummyNodeNodeViewCustomization : INodeViewCustomization<DummyNode>
    {
        public void CustomizeView(DummyNode model, Dynamo.Controls.NodeView nodeView)
        {
            var fileName = "DeprecatedNode.png";
            if (model.NodeNature == DummyNode.Nature.Unresolved)
                fileName = "MissingNode.png";

            var src = @"/DSCoreNodesUI;component/Resources/" + fileName;

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