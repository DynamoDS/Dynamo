using System.Windows;
using System.Windows.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace Dynamo.UI.Controls
{
    public class DynamoNodeButton : Button
    {
        private string eventName = string.Empty;
        private ModelBase model = null;

        public DynamoNodeButton()
            : base()
        {
            Style = (Style)SharedDictionaryManager.DynamoModernDictionary["SNodeTextButton"];
            Margin = new Thickness(1, 0, 1, 0);
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
            if (null != model && (!string.IsNullOrEmpty(eventName)))
            {
                var command = new DynamoViewModel.ModelEventCommand(model.GUID, eventName);
                DynamoSettings.Controller.DynamoViewModel.ExecuteCommand(command);
            }
        }
    }
}