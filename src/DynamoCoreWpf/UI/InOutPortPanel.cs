using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Dynamo.Logging;
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
                try
                {
                    var portVm = generator.ItemFromContainer(child) as PortViewModel;
                    var lineIndex = portVm.PortModel.LineIndex;
                    var multiplier = ((lineIndex == -1) ? itemIndex : lineIndex);
                    var portHeight = portVm.PortModel.Height;

                    y = multiplier * portHeight;
                    child.Arrange(new Rect(x, y, arrangeSize.Width, portHeight));
                    itemIndex = itemIndex + 1;
                }
                catch (Exception ex)
                {
                    Analytics.TrackException(ex, true);
                }
            }

            return base.ArrangeOverride(arrangeSize);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var aggregatedExceptions = new List<Exception>();

            if (this.Children.Count <= 0)
                return new Size(0, 0);

            var cumulative = new Size(0, 0);
            foreach (UIElement child in this.Children)
            {
                try
                {
                    // Default behavior of getting each child to measure.
                    child.Measure(constraint);
                }
                catch (XamlParseException e)
                {
                    //aggregate the exceptions as we'll loop over all children in this panel and send one exception to analytics.
                    aggregatedExceptions.Add(e);
                    continue;
                }

                // All children should be stacked from top to bottom, so we 
                // will take the largest child's width as the final width.
                if (cumulative.Width < child.DesiredSize.Width)
                    cumulative.Width = child.DesiredSize.Width;

                // Having one child item stack on top of another.
                cumulative.Height += child.DesiredSize.Height;
            }
            //log to analytics if we recorded any exceptions
            if (aggregatedExceptions.Count > 0)
            {
                var aggException = new AggregateException($"XamlParseException(s) InOutPortPanel MeasureOverride:{aggregatedExceptions.Count}", aggregatedExceptions);
                Analytics.TrackException(aggException, false);
            }


            return cumulative;
        }
    }
}
