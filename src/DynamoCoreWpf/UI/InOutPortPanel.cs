using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.UI.Controls
{
    public class InOutPortPanel : Panel
    {
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            int count = 0;
            double x = 0, y = 0;
            foreach (UIElement child in this.Children)
            {
                var childSize = child.DesiredSize;
                y = count * (childSize.Height == 26 ? childSize.Height :
                    Configurations.CodeBlockPortHeightInPixels);

                child.Arrange(new Rect(x, y, arrangeSize.Width, childSize.Height));
                count = count + 1;
            }

            return base.ArrangeOverride(arrangeSize);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (this.Children.Count <= 0)
                return new Size(0, 0);

            var cumulative = new Size(0, 0);
            foreach (UIElement child in this.Children)
            {
                // Default behavior of getting each child to measure.
                child.Measure(constraint);

                // All children should be stacked from top to bottom, so we 
                // will take the largest child's width as the final width.
                if (cumulative.Width < child.DesiredSize.Width)
                    cumulative.Width = child.DesiredSize.Width;

                // Having one child item stack on top of another.
                cumulative.Height += child.DesiredSize.Height;
            }

            return cumulative;
        }
    }
}
