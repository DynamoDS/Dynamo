using System;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.Wpf.Controls
{
    public class FilterPanel : WrapPanel
    {

        public int MaxItemPerRow
        {
            get { return (int)GetValue(MaxItemPerRowProperty); }
            set { SetValue(MaxItemPerRowProperty, value); }
        }

        public static readonly DependencyProperty MaxItemPerRowProperty =
        DependencyProperty.Register("MaxItemPerRow", typeof(int), typeof(FilterPanel), new UIPropertyMetadata(1));


        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.Children == null || this.Children.Count == 0)
                return finalSize;

            double x = 0, y = 0, currentRowHeight = 0;
            int itemPerRow = 0;

            foreach (UIElement child in this.Children)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var desiredSize = child.DesiredSize;
                if (itemPerRow == MaxItemPerRow)
                {
                    x = 0;
                    y = y + currentRowHeight;
                    currentRowHeight = 0;
                    itemPerRow = 0;
                }

                child.Arrange(new Rect(x, y, desiredSize.Width, desiredSize.Height));
                x = x + desiredSize.Width;

                if (desiredSize.Height > currentRowHeight)
                    currentRowHeight = desiredSize.Height;

                itemPerRow++;
            }

            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double totalWidth = 0.0;
            double totalHeight = 0.0;

            int itemPerRow = 0;
            double rowWidth = 0.0;
            double rowHeight = 0.0;
            foreach (UIElement child in Children)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size childSize = child.DesiredSize;

                rowWidth += childSize.Width;
                rowHeight = rowHeight < childSize.Height ? childSize.Height : rowHeight;

                itemPerRow++;
                if (itemPerRow == MaxItemPerRow)
                {
                    itemPerRow = 0;
                    totalWidth = totalWidth < rowWidth ? rowWidth : totalWidth;
                    totalHeight += rowHeight;
                    rowHeight = 0.0;
                    rowWidth = 0.0;
                }
            }

            return new Size(totalWidth, totalHeight);
        }
    }
}
