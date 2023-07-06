using System.Windows;
using HelixToolkit.Wpf.SharpDX;
#if NET5_0_OR_GREATER 
using HelixToolkit.SharpDX.Core.Core;
#else
using HelixToolkit.Wpf.SharpDX.Core;
#endif

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    internal class DynamoPointLineRenderCore : PointLineRenderCore
    {
        private readonly DynamoRenderCoreDataStore dataCore;
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
                    (geo as DynamoPointGeometryModel3D).SetState(dataCore.GenerateEnumFromState());
                }
                else
                {
                    (geo as DynamoLineGeometryModel3D).SetState(dataCore.GenerateEnumFromState());
                }
            }

        }

    }
}
