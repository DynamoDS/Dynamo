using System.Windows;
using System.Windows.Controls;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;

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

        public DynamoNodeButton()
        {
            Style = (Style)SharedDictionaryManager.DynamoModernDictionary["SNodeTextButton"];
            Margin = new System.Windows.Thickness(1, 0, 1, 0);
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
            // needs the "DynCmd.ModelEventCommand" to be sent when user clicks
            // on the button.
            // 
            if (null != this.model && (!string.IsNullOrEmpty(this.eventName)))
            {
                var command = new DynamoModel.ModelEventCommand(model.GUID, eventName);
                this.DynamoViewModel.ExecuteCommand(command);
            }
        }
    }
}