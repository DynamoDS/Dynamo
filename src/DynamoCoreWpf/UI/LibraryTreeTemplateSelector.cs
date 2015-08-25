using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Wpf.ViewModels;
using Dynamo.Search;

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

    public class LibrarySearchTreeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TopCategoryTemplate { get; set; }
        public DataTemplate CategoryTemplate { get; set; }
        public DataTemplate MemberGroupsTemplate { get; set; }
        public DataTemplate MemberTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is NodeSearchElementViewModel)
                return MemberTemplate;

            if (item is SearchMemberGroup)
                return MemberGroupsTemplate;

            // "Top Result" is no longer a standalone panel on the library view. A SearchCategory 
            // is created based off the first item in search results, and inserted into the results 
            // just like any other SearchCategory objects. The only difference is, "IsTopCategory"
            // property will be set to "true". This is where the right template is selected so that 
            // top result item appears to look different from other categories in results.

            if (item is SearchCategory)
            {
                if ((item as SearchCategory).IsTopCategory)
                {
                    return TopCategoryTemplate;
                }
                else
                {
                    return CategoryTemplate;
                }
            }

            const string message = "Unknown object bound to collection";
            throw new InvalidOperationException(message);
        }
    }
}
