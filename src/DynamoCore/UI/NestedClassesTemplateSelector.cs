using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Nodes.Search;
using Dynamo.Search.SearchElements;

namespace Dynamo.Controls
{
    public class NestedClassesTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NestedCategoryTemplate { get; set; }
        public DataTemplate SubclassesTemplate { get; set; }
        public DataTemplate CategoryClassDetailsTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is BrowserInternalElementForClasses)
                return SubclassesTemplate;

            if (item is BrowserRootElement)
                if ((item as BrowserRootElement).IsPlaceholder) return CategoryClassDetailsTemplate;

            if ((item is BrowserInternalElement) || (item is BrowserRootElement))
                return NestedCategoryTemplate;

            const string message = "Unknown object bound to collection";
            throw new InvalidOperationException(message);
        }
    }
}
