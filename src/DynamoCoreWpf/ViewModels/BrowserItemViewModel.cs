using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

using Dynamo.Search;
using Dynamo.Search.SearchElements;

using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Wpf.ViewModels
{
    public abstract class BrowserItemViewModel : NotificationObject
    {
        public ICommand ToggleIsExpandedCommand { get; private set; }
        public BrowserItem Model { get; private set; }
        public ObservableCollection<BrowserItemViewModel> Items { get; set; }

        protected BrowserItemViewModel(BrowserItem model)
        {
            Model = model;
            ToggleIsExpandedCommand = new DelegateCommand(Model.Execute);
            Items = new ObservableCollection<BrowserItemViewModel>();

            foreach (var item in Model.Items)
            {
                Items.Add(Wrap(item));
            }

            Model.Items.CollectionChanged += ItemsOnCollectionChanged;
        }

        /// <summary>
        /// Sort this items children and then tell its children and recurse on children
        /// </summary>
        public void RecursivelySort()
        {
            Items = new ObservableCollection<BrowserItemViewModel>(Items.OrderBy(x => x.Model.Name));
            Items.ToList().ForEach(x => x.RecursivelySort());
        }

        private void ItemsOnCollectionChanged(object sender, 
            NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (BrowserItem item in e.NewItems.OfType<BrowserItem>()) {
                        Items.Add(Wrap(item));
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (
                        var vm in
                            from object item in e.OldItems
                            select Items.First(x => x.Model == item))
                    {
                        Items.Remove(vm);
                    }
                    break;
            }
        }

        #region Wrap

        internal static BrowserItemViewModel Wrap(BrowserItem item)
        {
            dynamic itemDyn = item;
            return WrapExplicit(itemDyn);
        }

        internal static BrowserRootElementViewModel WrapExplicit(BrowserRootElement elem)
        {
            return new BrowserRootElementViewModel(elem);
        }

        internal static BrowserInternalElementViewModel WrapExplicit(BrowserInternalElement elem)
        {
            return new BrowserInternalElementViewModel(elem);
        }

        internal static CustomNodeSearchElementViewModel WrapExplicit(CustomNodeSearchElement elem)
        {
            return new CustomNodeSearchElementViewModel(elem);
        }

        internal static NodeSearchElementViewModel WrapExplicit(NodeSearchElement elem)
        {
            return new NodeSearchElementViewModel(elem);
        }

        internal static DSFunctionNodeSearchElementViewModel WrapExplicit(DSFunctionNodeSearchElement elem)
        {
            return new DSFunctionNodeSearchElementViewModel(elem);
        }

        #endregion

    }
}
