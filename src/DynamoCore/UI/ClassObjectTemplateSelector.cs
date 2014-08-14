using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Models;

namespace Dynamo.Controls
{
    public class ClassObjectTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ClassObjectTemplate { get; set; }
        public DataTemplate ClassDetailsTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is ClassObject)
                return ClassObjectTemplate;
            if (item is ClassDetails)
                return ClassDetailsTemplate;

            const string message = "Unknown object bound to collection";
            throw new InvalidOperationException(message);
        }
    }
}
