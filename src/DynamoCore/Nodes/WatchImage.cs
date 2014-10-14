using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;

using ProtoCore.AST.AssociativeAST;
using Image = System.Windows.Controls.Image;

namespace Dynamo.Nodes
{

    [NodeName("Watch Image")]
    [NodeDescription("Previews an image")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeSearchTags("image")]
    [IsDesignScriptCompatible]
    public class WatchImageCore : NodeModel, IWpfNode
    {
        private Image image;

        public WatchImageCore(WorkspaceModel ws) : base(ws)
        {
            InPortData.Add(new PortData("image", "image"));
            OutPortData.Add(new PortData("image", "image"));

            RegisterAllPorts();
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            var resultAst = new List<AssociativeNode>
            {
                AstFactory.BuildAssignment(AstIdentifierForPreview, inputAstNodes[0])
            };

            return resultAst;
        }

        public void SetupCustomUIElements(dynNodeView nodeUi)
        {
            image = new Image
            {
                MaxWidth = 400,
                MaxHeight = 400,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Name = "image1",
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            this.PropertyChanged += (sender, args) => 
            {
                if (args.PropertyName != "IsUpdated") return;
                var im = GetImageFromMirror();
                nodeUi.Dispatcher.Invoke(new Action<Bitmap>(SetImageSource), new object[] { im });
            };

            nodeUi.PresentationGrid.Children.Add(image);
            nodeUi.PresentationGrid.Visibility = Visibility.Visible;
        }

        private void SetImageSource(System.Drawing.Bitmap bmp)
        {
            if (bmp == null)
                return;

            // how to convert a bitmap to an imagesource http://blog.laranjee.com/how-to-convert-winforms-bitmap-to-wpf-imagesource/ 
            // TODO - watch out for memory leaks using system.drawing.bitmaps in managed code, see here http://social.msdn.microsoft.com/Forums/en/csharpgeneral/thread/4e213af5-d546-4cc1-a8f0-462720e5fcde
            // need to call Dispose manually somewhere, or perhaps use a WPF native structure instead of bitmap?

            var hbitmap = bmp.GetHbitmap();
            var imageSource = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            image.Source =  imageSource;
        }

        private System.Drawing.Bitmap GetImageFromMirror()
        {
            if (this.InPorts[0].Connectors.Count == 0) return null;

            var mirror = this.Workspace.DynamoModel.EngineController.GetMirror(AstIdentifierForPreview.Name);

            if (null == mirror)
                return null;

            var data = mirror.GetData();

            if (data == null || data.IsNull) return null;
            if (data.Data is System.Drawing.Bitmap) return data.Data as System.Drawing.Bitmap;
            return null;
        }

#if ENABLE_DYNAMO_SCHEDULER

        protected override void RequestVisualUpdateCore(int maxTesselationDivisions)
        {
            return; // No visualization update is required for this node type.
        }

#else

        public override void UpdateRenderPackage(int maxTessDivisions)
        {
            //do nothing
            //a watch should not draw its outputs
        }

#endif

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }
    }

}
