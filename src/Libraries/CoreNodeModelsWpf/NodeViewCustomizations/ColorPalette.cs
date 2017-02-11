using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Wpf;

using CoreNodeModelsWpf.Controls;
using CoreNodeModels.Input;

using DSColor = DSCore.Color;
using Xceed.Wpf.Toolkit;


namespace CoreNodeModelsWpf.Nodes
{
    public class ColorPaletteNodeViewCustomization : NotificationObject, INodeViewCustomization<ColorPalette> 
    {
        /// <summary>
        ///     List of standard colors.
        /// </summary>
        private ObservableCollection<ColorItem> cList = new ObservableCollection<ColorItem>()
        {
            new ColorItem(Color.FromArgb(255,48,130,189), "Blue 1"),
            new  ColorItem(Color.FromArgb(255,106,173,213), "Blue 2"),
            new  ColorItem(Color.FromArgb(255,158,202,225), "Blue 3"),
            new  ColorItem(Color.FromArgb(255,199,219,238), "Blue 4"),

            new  ColorItem(Color.FromArgb(255,229,87,37), "Orange 1"),
            new  ColorItem(Color.FromArgb(255,246,140,63), "Orange 2"),
            new  ColorItem(Color.FromArgb(255,229,87,37), "Orange 3"),
            new  ColorItem(Color.FromArgb(255,252,208,162), "Orange 4"),

            new  ColorItem(Color.FromArgb(255,49,163,83), "Green 1"),
            new  ColorItem(Color.FromArgb(255,117,195,118), "Green 2"),
            new  ColorItem(Color.FromArgb(255,162,211,154), "Green 3"),
            new  ColorItem(Color.FromArgb(255,200,228,191), "Green 4"),

            new  ColorItem(Color.FromArgb(255,117,107,177), "Purple 1"),
            new  ColorItem(Color.FromArgb(255,158,154,199), "Purple 2"),
            new  ColorItem(Color.FromArgb(255,189,189,219), "Purple 3"),
            new  ColorItem(Color.FromArgb(255,218,218,234), "Purple 4"),

            new  ColorItem(Color.FromArgb(255,99,99,99), "Grey 1"),
            new  ColorItem(Color.FromArgb(255,150,150,150), "Grey 2"),
            new  ColorItem(Color.FromArgb(255,189,189,189), "Grey 3"),
            new  ColorItem(Color.FromArgb(255,217,217,217), "Grey 4"),
        };
        /// <summary>
        ///     WPF Control.
        /// </summary>
        private ColorPaletteUI ColorPaletteUINode;
        private ColorPalette colorPaletteNode;
        private Color mcolor = Color.FromArgb(255, 0, 0, 0);

        /// <summary>
        ///     List of standard colors.
        /// </summary>
        public ObservableCollection<Xceed.Wpf.Toolkit.ColorItem> ColorList
        {
            get { return cList; }
        }
        public Color MColor
        {
            get { return mcolor; }
            set
            {                
                mcolor = value;              
                colorPaletteNode.dsColor = DSColor.ByARGB(mcolor.A, mcolor.R, mcolor.G, mcolor.B);
                RaisePropertyChanged("MColor");
            }
        }
        /// <summary>
        ///     Customize View.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nodeView"></param>
        public void CustomizeView(ColorPalette model, NodeView nodeView)
        {
            colorPaletteNode = model;
            ColorPaletteUINode = new ColorPaletteUI();
            nodeView.inputGrid.Children.Add(ColorPaletteUINode);
            ColorPaletteUINode.DataContext = this;
        }
        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose() { }
    }
}
