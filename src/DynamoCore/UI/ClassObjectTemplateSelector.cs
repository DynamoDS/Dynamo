using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Models;
using Dynamo.Nodes.Search;

namespace Dynamo.Controls
{
    public class ClassObjectTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ClassObjectTemplate { get; set; }
        public DataTemplate ClassDetailsTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is BrowserDetailsElement)
                return ClassDetailsTemplate;

            if (item is BrowserClassElement || item is BrowserInternalElement)
                return ClassObjectTemplate;            

            const string message = "Unknown object bound to collection";
            throw new InvalidOperationException(message);
        }
    }
}
