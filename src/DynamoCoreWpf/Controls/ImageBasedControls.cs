using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Dynamo.UI.Controls
{
    public class ImageCheckBox : CheckBox
    {
        public static readonly DependencyProperty StateImageProperty =
            DependencyProperty.Register("StateImage", typeof(ImageSource),
            typeof(ImageCheckBox), new UIPropertyMetadata(null));

        public static readonly DependencyProperty CheckImageProperty =
            DependencyProperty.Register("CheckImage", typeof(ImageSource),
            typeof(ImageCheckBox), new UIPropertyMetadata(null));

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
            var animationOffsets = new Dictionary<string, double>();
            animationOffsets.Add("hoverOverAnimation", -1.0 * this.Height);
            animationOffsets.Add("pressedAnimation", -2.0 * this.Height);
            animationOffsets.Add("checkedAnimation", -1.0 * this.Height);

            foreach (var offset in animationOffsets)
            {
                var animation = Template.FindName(offset.Key, this);
                var doubleAnimation = animation as DoubleAnimation;
                if (doubleAnimation != null)
                    doubleAnimation.To = offset.Value;
            }

            base.OnApplyTemplate();
        }
    }

    public class ImageButton : Button
    {
        public static readonly DependencyProperty StateImageProperty =
            DependencyProperty.Register("StateImage", typeof(ImageSource),
            typeof(ImageButton), new UIPropertyMetadata(null));

        public ImageSource StateImage
        {
            get { return (ImageSource)GetValue(StateImageProperty); }
            set { SetValue(StateImageProperty, value); }
        }

        /// <summary>
        /// For detailed explaination of why we need this,
        /// see "ImageCheckBox.OnApplyTemplate" method above.
        /// </summary>
        /// 
        public override void OnApplyTemplate()
        {
            var animationOffsets = new Dictionary<string, double>();
            animationOffsets.Add("hoverOverAnimation", -1.0 * this.Height);
            animationOffsets.Add("pressedAnimation", -2.0 * this.Height);
            animationOffsets.Add("disabledAnimation", -3.0 * this.Height);

            foreach(var offset in animationOffsets)
            {
                var animation = Template.FindName(offset.Key, this);
                var doubleAnimation = animation as DoubleAnimation;
                if (doubleAnimation != null)
                    doubleAnimation.To = offset.Value;
            }

            base.OnApplyTemplate();
        }
    }

    public class ImageRepeatButton : RepeatButton
    {
        public static readonly DependencyProperty StateImageProperty =
            DependencyProperty.Register("StateImage", typeof(ImageSource),
            typeof(ImageRepeatButton), new UIPropertyMetadata(null));

        public ImageSource StateImage
        {
            get { return (ImageSource)GetValue(StateImageProperty); }
            set { SetValue(StateImageProperty, value); }
        }

        /// <summary>
        /// For detailed explaination of why we need this,
        /// see "ImageCheckBox.OnApplyTemplate" method above.
        /// </summary>
        /// 
        public override void OnApplyTemplate()
        {
            var animationOffsets = new Dictionary<string, double>();
            animationOffsets.Add("hoverOverAnimation", -1.0 * this.Height);
            animationOffsets.Add("pressedAnimation", -2.0 * this.Height);
            animationOffsets.Add("disabledAnimation", -3.0 * this.Height);

            foreach (var offset in animationOffsets)
            {
                var animation = Template.FindName(offset.Key, this);
                var doubleAnimation = animation as DoubleAnimation;
                if (doubleAnimation != null)
                    doubleAnimation.To = offset.Value;
            }

            base.OnApplyTemplate();
        }
    }
}
