using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.Controls
{
    public class NodeViewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate DeferredTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var nodeViewModel = item as NodeViewModel;
            //if (nodeViewModel != null && nodeViewModel.NodeCount > 150)
            //{
            //    return DeferredTemplate;
            //}
            return DefaultTemplate;
        }
    }
}
