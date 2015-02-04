using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.UI.Controls;

namespace Dynamo.Controls
{
    public class HeaderTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SingleHeaderTemplate { get; set; }
        public DataTemplate HeadersListTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            if ((item as IEnumerable<HeaderStripItem>).Count() == 1)
                return SingleHeaderTemplate;
            else
                return HeadersListTemplate;
        }
    }
}
