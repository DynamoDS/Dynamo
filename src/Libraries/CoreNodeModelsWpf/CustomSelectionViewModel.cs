using CoreNodeModels;
using CoreNodeModels.Input;

using Dynamo.Core;
using Dynamo.UI.Commands;

namespace CoreNodeModelsWpf
{
    public class CustomSelectionViewModel : NotificationObject
    {
        public CustomSelection Model { get; }

        /// <summary>
        /// This command is bound to the Add button in the GUI
        /// </summary>
        public DelegateCommand AddCommand { get; }

        public DelegateCommand RemoveCommand { get; }

        private void AddItem(object obj)
        {
            Model.Items.Add(new DynamoDropDownItem(string.Empty, string.Empty));
        }

        private void RemoveItem(object parameter)
        {
            if (parameter is DynamoDropDownItem item)
            {
                Model.Items.Remove(item);
            }

            Model.OnNodeModified();
        }

        public CustomSelectionViewModel()
        {
        }

        public CustomSelectionViewModel(CustomSelection model)
        {
            Model = model;

            AddCommand = new DelegateCommand(AddItem);
            RemoveCommand = new DelegateCommand(RemoveItem);
        }
    }
}