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
using System.Windows.Media.Animation;
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

        /// <summary>
        /// You can't use dynamic resource references or data binding expressions
        /// to set Storyboard or animation property values. That's because 
        /// everything inside a ControlTemplate must be thread-safe, and the 
        /// timing system must Freeze Storyboard objects to make them thread-safe.
        /// A Storyboard cannot be frozen if it or its child timelines contain 
        /// dynamic resource references or data binding expressions. 
        /// 
        /// See: http://msdn.microsoft.com/en-us/library/ms742868.aspx
        /// 
        /// </summary>
        /// 
        public override void OnApplyTemplate()
        {
            var animationNames = new string[]
            {
                "normalAnimation",
                "hoverOverAnimation",
                "pressedAnimation",
                "checkedAnimation"
            };

            var offsetValues = new double[]
            {
                0.0 * this.Height,
                -1.0 * this.Height,
                -2.0 * this.Height,
                -3.0 * this.Height
            };

            for (int index = 0; index < animationNames.Length; ++index)
            {
                var animation = Template.FindName(animationNames[index], this);
                var doubleAnimation = animation as DoubleAnimation;
                if (doubleAnimation != null)
                    doubleAnimation.To = offsetValues[index];
            }

            base.OnApplyTemplate();
        }
    }
}
