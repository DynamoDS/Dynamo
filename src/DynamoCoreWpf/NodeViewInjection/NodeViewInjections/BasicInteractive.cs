using System.Windows;
using System.Windows.Controls;

using Dynamo.Controls;
using Dynamo.Wpf;

namespace Dynamo.Wpf
{
    public abstract class BasicInteractiveViewInjection : INodeViewInjection
    {
        public virtual void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add an edit window option to the 
            //main context window
            var editWindowItem = new MenuItem { Header = "Edit...", IsCheckable = false };
            nodeUI.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += editWindowItem_Click;
        }

        public virtual void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            //override in child classes
        }

        public void Dispose()
        {

        }
    }
}