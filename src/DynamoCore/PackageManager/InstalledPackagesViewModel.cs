using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using Dynamo.Controls;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.ViewModels
{
    public class InstalledPackagesViewModel : NotificationObject
    {
        private ObservableCollection<PackageViewModel> _localPackages = new ObservableCollection<PackageViewModel>();
        public ObservableCollection<PackageViewModel> LocalPackages { get { return _localPackages; } }

        private readonly DynamoViewModel dynamoViewModel;
        private readonly PackageLoader model;

        public InstalledPackagesViewModel(DynamoViewModel dynamoViewModel, PackageLoader model)
        {
            this.model = model;
            this.dynamoViewModel = dynamoViewModel;

            InitializeLocalPackages();
        }

        private void InitializeLocalPackages()
        {
            foreach (var pkg in model.LocalPackages)
            {
                LocalPackages.Add(new PackageViewModel(this.dynamoViewModel, pkg));
            }


            this.model.LocalPackages.CollectionChanged += LocalPackagesOnCollectionChanged;
        }

        private void LocalPackagesOnCollectionChanged(object sender, 
            NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        LocalPackages.Add(new PackageViewModel(dynamoViewModel, item as Package));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        LocalPackages.Remove(_localPackages.ToList().First(x => x.Model == item));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    LocalPackages.Clear();
                    break;
            }

            RaisePropertyChanged("LocalPackages");
        }
    }
}
