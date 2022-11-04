using System.Windows;
using System.Windows.Controls;
using Dynamo.Configuration;
using Dynamo.Graph.Annotations;

namespace Dynamo.Wpf.Utilities
{
    /// <summary>
    /// This class will be used for select a specific Style depending if the list item is a Separator or MenuItem
    /// </summary>
    public class GroupStyleItemSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is GroupStyleSeparator)
            {
                return (Style)((FrameworkElement)container).FindResource("GroupStyleSeparatorStyle");
            }
            if (item is GroupStyleItem)
            {
                return (Style)((FrameworkElement)container).FindResource("GroupStyleItemStyle");
            }
            return null;
        }
    }
}
