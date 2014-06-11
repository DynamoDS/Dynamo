using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dynamo.UI.Controls
{
    public class CanvasCheckBox : CheckBox
    {
        // static CanvasCheckBox()
        // {
        //     DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasCheckBox),
        //         new FrameworkPropertyMetadata(typeof(CanvasCheckBox)));
        // }

        public ImageSource ImageStrip
        {
            get { return (ImageSource)GetValue(ImageStripProperty); }
            set { SetValue(ImageStripProperty, value); }
        }

        public static readonly DependencyProperty ImageStripProperty =
            DependencyProperty.Register("ImageStrip", typeof(ImageSource),
            typeof(CanvasCheckBox), new UIPropertyMetadata(null));
    }
}
