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
        public DataTemplate MemberTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is NodeSearchElement)
                return MemberTemplate;

            if (item is BrowserInternalElementForClasses)
                return SubclassesTemplate;

            if (item is BrowserRootElement || item is BrowserInternalElement)
                return NestedCategoryTemplate;

            const string message = "Unknown object bound to collection";
            throw new InvalidOperationException(message);
        }
    }
}
