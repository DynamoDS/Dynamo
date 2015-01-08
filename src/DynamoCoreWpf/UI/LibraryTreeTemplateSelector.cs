using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.Controls
{
    public class LibraryTreeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NestedCategoryTemplate { get; set; }
        public DataTemplate SubclassesTemplate { get; set; }
        public DataTemplate MemberTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is NodeSearchElementViewModel)
                return MemberTemplate;
            // TODO(Vladimir): take a look.
            //if (item is BrowserInternalElementForClassesViewModel)
            //    return SubclassesTemplate;

            if (item is BrowserRootElementViewModel || item is BrowserInternalElementViewModel)
                return NestedCategoryTemplate;

            const string message = "Unknown object bound to collection";
            throw new InvalidOperationException(message);
        }
    }
}
