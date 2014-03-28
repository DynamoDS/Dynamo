using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
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
        private ResultImageUI resultImageUI = new ResultImageUI();
        private System.Windows.Controls.Image image = null;

        public WatchImageCore()
        {
            InPortData.Add(new PortData("image", "image", typeof(System.Drawing.Bitmap)));
            OutPortData.Add(new PortData("image", "image", typeof(System.Drawing.Bitmap)));

            RegisterAllPorts();

            this.PropertyChanged += NodeValueUpdated;
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
            image = new System.Windows.Controls.Image();
            image.Width = 320;
            image.Height = 240;
            image.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            image.Name = "image1";
            image.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            var bindingVal = new System.Windows.Data.Binding("ResultImage")
            {
                Mode = BindingMode.OneWay,
                NotifyOnValidationError = false,
                Source = resultImageUI,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            image.SetBinding(System.Windows.Controls.Image.SourceProperty, bindingVal);

            nodeUi.inputGrid.Children.Add(image);

        }

        private System.Windows.Media.ImageSource ConvertToImageSource(System.Drawing.Bitmap image)
        {
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();

            bitmap.BeginInit();

            var memoryStream = new MemoryStream();
            image.Save(memoryStream, image.RawFormat);

            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();

            return bitmap;
        }

        private void NodeValueUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsUpdated")
                return;

            var im = GetImageFromMirror();
            var bm = ConvertToImageSource(im);

            DispatchOnUIThread(() => { if (bm != null) resultImageUI.ResultImage = bm; });
        }

        private System.Drawing.Bitmap GetImageFromMirror()
        {
            if (this.InPorts[0].Connectors.Count == 0) return null;

            var mirror = dynSettings.Controller.EngineController.GetMirror(AstIdentifierForPreview.Name);

            if (null == mirror)
                return null;

            var data = mirror.GetData();

            if (data == null || data.IsNull) return null;
            if (data.Data is System.Drawing.Bitmap) return data.Data as System.Drawing.Bitmap;
            return null;
        }

        public override void UpdateRenderPackage()
        {
            //do nothing
            //a watch should not draw its outputs
        }

        public class ResultImageUI : INotifyPropertyChanged
        {
            private System.Windows.Media.ImageSource resultImage;
            public System.Windows.Media.ImageSource ResultImage
            {
                get { return resultImage; }

                set
                {
                    resultImage = value;
                    Notify("ResultImage");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, e);
            }

            protected void OnPropertyChanged(string propertyName)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }

            protected void Notify(string propertyName)
            {
                if (this.PropertyChanged != null)
                {
                    try
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    }
                    catch (Exception ex)
                    {
                        DynamoLogger.Instance.Log(ex.Message);
                        DynamoLogger.Instance.Log(ex.StackTrace);
                    }
                }
            }

        }

    }

}
