using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

using Dynamo.Search;

using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Wpf.ViewModels
{
    public class BrowserItemViewModel : NotificationObject
    {
        public ICommand ToggleIsExpandedCommand { get; private set; }
        public BrowserItem Model { get; private set; }
        public ObservableCollection<BrowserItemViewModel> Items { get; set; }

        public BrowserItemViewModel(BrowserItem model)
        {
            this.Model = model;
            this.ToggleIsExpandedCommand = new DelegateCommand((Action) model.Execute);
        }


    }
}
