using CoreNodeModels;
using CoreNodeModels.Input;

using Dynamo.Core;
using Dynamo.UI.Commands;

namespace CoreNodeModelsWpf
{
    /// <summary>
    /// View model for <see cref="Controls.CustomSelectionControl"/>, for the Custom Selection node.
    /// </summary>
    public class CustomSelectionViewModel : NotificationObject
    {
        public CustomSelection Model { get; }

        /// <summary>
        /// Add an item to the list. This command is bound to the + button in the GUI.
        /// </summary>
        public DelegateCommand AddCommand { get; }

        /// <summary>
        /// Remove an item from the list. This command is bound to the - button in the GUI.
        /// </summary>
        public DelegateCommand RemoveCommand { get; }

        private void AddItem(object obj)
        {
            Model.Items.Add(new DynamoDropDownItem(string.Empty, string.Empty));
        }

        private void RemoveItem(object parameter)
        {
            if (Model.Items.Count > 1 && parameter is DynamoDropDownItem item)
            {
                Model.Items.Remove(item);
                Model.OnNodeModified();
            }
        }

        /// <summary>
        /// Create a new <see cref="CustomSelectionViewModel"/>. Used by the view in design-time.
        /// </summary>
        public CustomSelectionViewModel()
        {
        }

        /// <summary>
        /// Create a new <see cref="CustomSelectionViewModel"/> with an existing model.
        /// </summary>
        /// <param name="model">The model data.</param>
        public CustomSelectionViewModel(CustomSelection model)
        {
            Model = model;

            AddCommand = new DelegateCommand(AddItem);
            RemoveCommand = new DelegateCommand(RemoveItem);
        }
    }
}