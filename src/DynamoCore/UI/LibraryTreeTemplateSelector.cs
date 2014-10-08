using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Nodes.Search;
using Dynamo.Search.SearchElements;

namespace Dynamo.Controls
{
    public class LibraryTreeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NestedCategoryTemplate { get; set; }
        public DataTemplate SubclassesTemplate { get; set; }
        public DataTemplate CategoryClassDetailsTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is BrowserInternalElementForClasses)
                return SubclassesTemplate;

            var browserRootElement = item as BrowserRootElement;
            if (browserRootElement != null)
            {
                if (browserRootElement.IsPlaceholder)
                    return CategoryClassDetailsTemplate;

                return NestedCategoryTemplate;
            }

            if (item is BrowserInternalElement)
                return NestedCategoryTemplate;

            const string message = "Unknown object bound to collection";
            throw new InvalidOperationException(message);
        }
    }
}
