using CoreNodeModels;
using Dynamo.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dynamo.Wpf.CurveMapper
{
    public class CurveMapperNodeView : INodeViewCustomization<CurveMapperNodeModel>
    {
        private CurveMapperNodeModel curveMapperNodeModel;
        private CurveMapperControl curveMapperControl;
        private Canvas graphCanvas;

        public void CustomizeView(CurveMapperNodeModel model, NodeView nodeView)
        {
            curveMapperNodeModel = model;
            curveMapperControl = new CurveMapperControl(model);
            curveMapperControl.DataContext = model;
            curveMapperNodeModel = model;

            nodeView.inputGrid.Children.Add(curveMapperControl);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
