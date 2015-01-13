using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.Controls
{
    public class ClassObjectTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ClassObjectTemplate { get; set; }
        public DataTemplate ClassDetailsTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ClassInformationViewModel)
                return ClassDetailsTemplate;

            if (item is NodeCategoryViewModel)
                return ClassObjectTemplate;

            const string message = "Unknown object bound to collection";
            throw new InvalidOperationException(message);
        }
    }
}
