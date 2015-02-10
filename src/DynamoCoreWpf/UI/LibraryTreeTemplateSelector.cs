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

            if (item is ClassesNodeCategoryViewModel)
                return SubclassesTemplate;

            if (item is NodeCategoryViewModel)
                return NestedCategoryTemplate;

            const string message = "Unknown object bound to collection";
            throw new InvalidOperationException(message);
        }
    }
}
