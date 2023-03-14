using System.Windows;
using System.Windows.Controls;

using Dynamo.Controls;
using Dynamo.Graph;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;

namespace Dynamo.Nodes
{
    public class DynamoNodeButton : Button
    {
        private string eventName = string.Empty;
        private ModelBase model = null;
        private DynamoViewModel dynamoViewModel;
        private DynamoViewModel DynamoViewModel
        {
            get
            {
                if (this.dynamoViewModel != null) return this.dynamoViewModel;

                var f = WpfUtilities.FindUpVisualTree<NodeView>(this);
                if (f != null) this.dynamoViewModel = f.ViewModel.DynamoViewModel;

                return this.dynamoViewModel;
            }
        }

        private Window Owner
        {
            get
            {
                var f = WpfUtilities.FindUpVisualTree<DynamoView>(this);
                if (f != null) return f;

                return null;
            }
        }

        public DynamoNodeButton()
        {
        }

        public DynamoNodeButton(ModelBase model, string eventName)
            : this()
        {
            this.model = model;
            this.eventName = eventName;
            Click += OnDynamoNodeButtonClick;
        }

        private void OnDynamoNodeButtonClick(object sender, RoutedEventArgs e)
        {
            // If this DynamoNodeButton was created with an associated model 
            // and the event name, then the owner of this button (a ModelBase) 
            // needs the "ModelEventCommand" to be sent when user clicks
            // on the button.
            // 
            if (null != this.model && (!string.IsNullOrEmpty(this.eventName)))
            {
                MessageBoxResult result = MessageBoxResult.None;

                if (eventName.Equals("RemoveInPort"))
                {
                    result = MessageBoxService.Show
                    (
                        Owner,
                        Dynamo.Wpf.Properties.Resources.MessageRemovePythonPort,
                        Dynamo.Wpf.Properties.Resources.RemovePythonPortWarningMessageBoxTitle,
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Information
                    );
                }

                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }

                var command = new DynamoModel.ModelEventCommand(model.GUID, eventName);
                this.DynamoViewModel.ExecuteCommand(command);
            }
        }
    }
}
