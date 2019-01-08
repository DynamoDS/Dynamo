using System.Windows;
using System.Windows.Controls;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.UI.Controls
{
    public class InOutPortPanel : Panel
    {
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (this.Children.Count <= 0)
            {
                // A port list without any port in it.
                return base.ArrangeOverride(arrangeSize);
            }

            var itemsControl = WpfUtilities.FindUpVisualTree<ItemsControl>(this);
            var generator = itemsControl.ItemContainerGenerator;

            int itemIndex = 0;
            double x = 0, y = 0;
            foreach (UIElement child in this.Children)
            {
                var portVm = generator.ItemFromContainer(child) as PortViewModel;
                var lineIndex = portVm.PortModel.LineIndex;
                var multiplier = ((lineIndex == -1) ? itemIndex : lineIndex);
                var portHeight = portVm.PortModel.Height;

                y = multiplier * portHeight;
                child.Arrange(new Rect(x, y, arrangeSize.Width, portHeight));
                itemIndex = itemIndex + 1;
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
