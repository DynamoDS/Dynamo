using Dynamo.Nodes.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.Controls
{
    public class CategoryTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CategoryTemplate { get; set; }
        public DataTemplate AddonsTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is BrowserRootElement)
            {
                if ((item as BrowserRootElement).Name == "Add-ons")
                    return AddonsTemplate;

                return CategoryTemplate;
            }

            const string message = "Unknown object bound to collection";
            throw new InvalidOperationException(message);
        }
    }
}
