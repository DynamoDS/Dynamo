using System.Windows;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    internal class DynamoPointLineRenderCore : PointLineRenderCore
    {
        private DynamoRenderCoreDataStore dataCore;
        public DynamoPointLineRenderCore()
        {
            dataCore = new DynamoRenderCoreDataStore(SetAffectsRender<bool>);
        }

        internal void SetPropertyData(DependencyPropertyChangedEventArgs args, GeometryModel3D geo)
        {
            dataCore.SetPropertyData(args);

            if (geo is DynamoPointGeometryModel3D || geo is DynamoLineGeometryModel3D)
            {
                if (geo is DynamoPointGeometryModel3D)
                {
                    ((DynamoPointGeometryModel3D) geo).SetState(dataCore.GenerateEnumFromState());
                }
                else
                {
                    ((DynamoLineGeometryModel3D) geo).SetState(dataCore.GenerateEnumFromState());
                }
            }

        }

    }
}
