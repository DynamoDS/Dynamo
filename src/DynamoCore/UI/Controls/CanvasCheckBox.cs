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
        public static readonly DependencyProperty StateImageProperty =
            DependencyProperty.Register("StateImage", typeof(ImageSource),
            typeof(CanvasCheckBox), new UIPropertyMetadata(null));

        public static readonly DependencyProperty CheckImageProperty =
            DependencyProperty.Register("CheckImage", typeof(ImageSource),
            typeof(CanvasCheckBox), new UIPropertyMetadata(null));

        public ImageSource StateImage
        {
            get { return (ImageSource)GetValue(StateImageProperty); }
            set { SetValue(StateImageProperty, value); }
        }

        public ImageSource CheckImage
        {
            get { return (ImageSource)GetValue(CheckImageProperty); }
            set { SetValue(CheckImageProperty, value); }
        }

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
                "hoverOverAnimation",
                "pressedAnimation",
                "checkedAnimation"
            };

            var offsetValues = new double[]
            {
                -1.0 * this.Height,
                -2.0 * this.Height,
                -1.0 * this.Height
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
