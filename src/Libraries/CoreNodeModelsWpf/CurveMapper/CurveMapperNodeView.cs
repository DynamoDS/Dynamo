using CoreNodeModels;
using Dynamo.Controls;

namespace Dynamo.Wpf.CurveMapper
{
    public class CurveMapperNodeView : INodeViewCustomization<CurveMapperNodeModel>
    {
        private CurveMapperNodeModel curveMapperNodeModel;
        private CurveMapperControl curveMapperControl;

        public void CustomizeView(CurveMapperNodeModel model, NodeView nodeView)
        {
            curveMapperNodeModel = model;
            curveMapperControl = new CurveMapperControl(model, model.DynamicCanvasSize);
            curveMapperControl.DataContext = model;
            curveMapperNodeModel = model;

            nodeView.inputGrid.Children.Add(curveMapperControl);
        }

        public void Dispose()
        {
        }
    }
}
